using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Categories.Queries.GetCategoriesTree;

public sealed class GetCategoriesTreeQueryHandler(
    ICategoryRepository categoryRepository,
    ISpuRepository spuRepository)
    : IQueryHandler<GetCategoriesTreeQuery, IReadOnlyList<CategoryNodeDto>>
{
    public async Task<Result<IReadOnlyList<CategoryNodeDto>>> Handle(
        GetCategoriesTreeQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Đọc toàn bộ bảng Categories sắp xếp theo display_order ASC
        var categories = await categoryRepository.GetAllAsync(request.IncludeInvisible, cancellationToken);
        if (categories.Count == 0)
        {
            return Result.Success<IReadOnlyList<CategoryNodeDto>>([]);
        }

        // 2. Tính toán số lượng sản phẩm ProductCount cho từng danh mục
        var categoryIds = categories.Select(c => c.Id);
        var productCounts = await spuRepository.GetProductCountsByCategoryIdsAsync(categoryIds, cancellationToken);

        // 3. Build cấu trúc cây JSON n-cấp (Parent-Child hierarchy) trong bộ nhớ
        var lookup = categories
            .Select(c => new CategoryNodeDto(
                c.Id,
                c.Name,
                c.Slug,
                c.ParentId,
                c.IconUrl,
                c.DisplayOrder,
                productCounts.GetValueOrDefault(c.Id, 0),
                []))
            .ToLookup(c => c.ParentId);

        var allCategoryIds = categories.Select(c => c.Id).ToHashSet();

        // Root nodes: ParentId null hoặc ParentId không nằm trong danh sách danh mục hiện có
        var rootCandidates = categories
            .Where(c => !c.ParentId.HasValue || !allCategoryIds.Contains(c.ParentId.Value))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name);

        var tree = rootCandidates
            .Select(c =>
            {
                var node = new CategoryNodeDto(
                    c.Id,
                    c.Name,
                    c.Slug,
                    c.ParentId,
                    c.IconUrl,
                    c.DisplayOrder,
                    productCounts.GetValueOrDefault(c.Id, 0),
                    []);
                return BuildNodeWithRollup(node, lookup);
            })
            .ToList();

        return tree;
    }

    private static CategoryNodeDto BuildNodeWithRollup(
        CategoryNodeDto node,
        ILookup<Guid?, CategoryNodeDto> lookup)
    {
        var children = lookup[node.Id]
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(child => BuildNodeWithRollup(child, lookup))
            .ToList();

        var totalProductCount = node.ProductCount + children.Sum(c => c.ProductCount);

        return node with
        {
            ProductCount = totalProductCount,
            Children = children
        };
    }
}
