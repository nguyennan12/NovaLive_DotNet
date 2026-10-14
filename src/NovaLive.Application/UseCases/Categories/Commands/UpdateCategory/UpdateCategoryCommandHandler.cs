using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Helpers;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ISpuRepository spuRepository)
    : ICommandHandler<UpdateCategoryCommand, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetByIdAsync(command.Id, includeInvisible: true, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound;
        }

        var request = command.Request;

        // 1. Kiểm tra parent_id không được là chính danh mục hiện tại (chống vòng lặp vô tận)
        if (request.ParentId.HasValue)
        {
            if (request.ParentId.Value == category.Id)
            {
                return CategoryErrors.CannotBeOwnParent;
            }

            var parentCategory = await categoryRepository.GetByIdAsync(request.ParentId.Value, includeInvisible: true, cancellationToken);
            if (parentCategory is null)
            {
                return CategoryErrors.ParentNotFound;
            }

            // Kiểm tra chống chu trình cha - con (Circular Reference): ParentId mới không được là hậu duệ của category hiện tại
            var allCategories = await categoryRepository.GetAllAsync(includeInvisible: true, cancellationToken);
            var categoryMap = allCategories.ToDictionary(c => c.Id);

            var currentAncestorId = request.ParentId;
            while (currentAncestorId.HasValue)
            {
                if (currentAncestorId.Value == category.Id)
                {
                    return CategoryErrors.CircularReference;
                }

                if (categoryMap.TryGetValue(currentAncestorId.Value, out var ancestor))
                {
                    currentAncestorId = ancestor.ParentId;
                }
                else
                {
                    break;
                }
            }
        }

        // 2. Tạo hoặc chuẩn hóa Slug
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(request.Name)
            : SlugHelper.GenerateSlug(request.Slug);

        // 3. Kiểm tra trùng Slug với danh mục khác
        if (!string.Equals(slug, category.Slug, StringComparison.OrdinalIgnoreCase))
        {
            var existingCategory = await categoryRepository.GetBySlugAsync(slug, cancellationToken);
            if (existingCategory is not null && existingCategory.Id != category.Id)
            {
                return CategoryErrors.SlugAlreadyExists;
            }
        }

        // 4. Cập nhật qua Domain method
        category.Update(
            name: request.Name.Trim(),
            slug: slug,
            parentId: request.ParentId,
            iconUrl: request.IconUrl?.Trim(),
            displayOrder: request.DisplayOrder,
            isVisible: request.IsVisible);

        categoryRepository.Update(category);

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
