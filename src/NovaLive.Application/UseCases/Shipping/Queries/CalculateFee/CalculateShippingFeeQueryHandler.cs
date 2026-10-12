using Microsoft.EntityFrameworkCore;
using NovaLive.Application.Abstractions.Persistence;
using NovaLive.Application.Abstractions.ThirdParty;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Shipping;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Shipping.Queries.CalculateFee;

public sealed class CalculateShippingFeeQueryHandler(
    IAppDbContext dbContext,
    IShippingProvider shippingProvider)
    : IQueryHandler<CalculateShippingFeeQuery, ShippingFeeResponse>
{
    public async Task<Result<ShippingFeeResponse>> Handle(CalculateShippingFeeQuery query, CancellationToken cancellationToken)
    {
        var req = query.Request;

        // 1. Lấy thông tin kho xuất hàng của Shop
        var warehouse = await dbContext.ShopAddresses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == req.WarehouseAddressId, cancellationToken);

        if (warehouse is null)
        {
            return ShippingErrors.WarehouseNotFound;
        }

        if (warehouse.ShopId != req.ShopId)
        {
            return ShippingErrors.InvalidShopWarehouse;
        }

        if (!warehouse.DistrictId.HasValue || string.IsNullOrWhiteSpace(warehouse.WardCode))
        {
            return ShippingErrors.IncompleteWarehouseAddress;
        }

        // 2. Lấy thông tin địa chỉ giao hàng của Buyer
        var recipientAddress = await dbContext.UserAddresses
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == req.ShippingAddressId, cancellationToken);

        if (recipientAddress is null)
        {
            return ShippingErrors.RecipientAddressNotFound;
        }

        if (!recipientAddress.DistrictId.HasValue || string.IsNullOrWhiteSpace(recipientAddress.WardCode))
        {
            return ShippingErrors.IncompleteRecipientAddress;
        }

        // 3. Tính tổng khối lượng (WeightGram)
        var totalWeight = req.WeightGram ?? 0;
        if (totalWeight <= 0 && req.Items != null && req.Items.Count > 0)
        {
            totalWeight = req.Items.Sum(i => i.Quantity * i.WeightGram);
        }

        if (totalWeight <= 0)
        {
            totalWeight = 500; // Mặc định 500 gram nếu không có thông tin
        }

        // 4. Gọi ShippingProvider tính cước
        var calcModel = new ShippingFeeCalculationModel(
            FromDistrictId: warehouse.DistrictId.Value,
            FromWardCode: warehouse.WardCode,
            ToDistrictId: recipientAddress.DistrictId.Value,
            ToWardCode: recipientAddress.WardCode,
            WeightGram: totalWeight,
            LengthCm: req.LengthCm ?? 10,
            WidthCm: req.WidthCm ?? 10,
            HeightCm: req.HeightCm ?? 10,
            InsuranceValue: req.InsuranceValue ?? 0);

        var quoteResult = await shippingProvider.CalculateFeeAsync(calcModel, cancellationToken);
        if (!quoteResult.IsSuccess || quoteResult.Value is null)
        {
            return quoteResult.Error;
        }

        var quote = quoteResult.Value;

        var response = new ShippingFeeResponse(
            Provider: quote.ProviderCode,
            ServiceName: "Giao hàng chuẩn GHN",
            ShippingFee: quote.TotalFee,
            EstimatedDeliveryDate: quote.ExpectedDeliveryDate ?? DateTime.UtcNow.AddDays(3));

        return response;

    }
}
