using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Products;

namespace NovaLive.Application.UseCases.Products.Queries.GetPublicProductDetail;

public record GetPublicProductDetailQuery(Guid SpuId) : IQuery<PublicSpuDetailResponse>;
