using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Helpers;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Common;
using NovaLive.Domain.Products;

namespace NovaLive.Application.UseCases.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : ICommandHandler<CreateCategoryCommand, CategoryResponse>
{
    public async Task<Result<CategoryResponse>> Handle(
        CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;

        // 1. Kiểm tra danh mục cha nếu có chỉ định
        if (request.ParentId.HasValue)
        {
            var parentExists = await categoryRepository.ExistsAsync(request.ParentId.Value, cancellationToken);
            if (!parentExists)
            {
                return CategoryErrors.ParentNotFound;
            }
        }

        // 2. Tạo hoặc chuẩn hóa Slug
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.GenerateSlug(request.Name)
            : SlugHelper.GenerateSlug(request.Slug);

        // 3. Kiểm tra trùng Slug
        var existingCategory = await categoryRepository.GetBySlugAsync(slug, cancellationToken);
        if (existingCategory is not null)
        {
            return CategoryErrors.SlugAlreadyExists;
        }

        // 4. Khởi tạo Category Entity
        var category = new Category(
            id: Guid.NewGuid(),
            name: request.Name.Trim(),
            slug: slug,
            parentId: request.ParentId,
            iconUrl: request.IconUrl?.Trim(),
            displayOrder: request.DisplayOrder,
            isVisible: true);

        await categoryRepository.AddAsync(category, cancellationToken);

        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Slug,
            category.ParentId,
            category.IconUrl,
            category.DisplayOrder,
            category.IsVisible,
            ProductCount: 0);
    }
}
