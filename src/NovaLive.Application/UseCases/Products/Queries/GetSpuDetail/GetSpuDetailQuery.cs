using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;

namespace NovaLive.Application.UseCases.Products.Queries.GetSpuDetail;

public record GetSpuDetailQuery(Guid SpuId, bool IsSellerView = false) : IQuery<SpuDetailResponse>;
