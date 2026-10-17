using NovaLive.Domain.Orders;
namespace NovaLive.Application.UseCases.Returns;
public sealed record SellerReturnContext(OrderReturn Request, SubOrder SubOrder, Guid SellerId);
