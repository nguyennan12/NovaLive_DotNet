using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler(
    ICategoryRepository categoryRepository,
    ISpuRepository spuRepository)
    : IQueryHandler<GetCategoryByIdQuery, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(request.Id, request.IncludeInvisible, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound;
        }

        var productCounts = await spuRepository.GetProductCountsByCategoryIdsAsync([category.Id], cancellationToken);
        var productCount = productCounts.GetValueOrDefault(category.Id, 0);

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Slug,
            category.ParentId,
            category.IconUrl,
            category.DisplayOrder,
            category.IsVisible,
            productCount);
    }
}
