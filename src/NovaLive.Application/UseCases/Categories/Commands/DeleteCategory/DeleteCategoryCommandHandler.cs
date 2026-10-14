using NovaLive.Application.Abstractions.Persistence.Repositories;
using NovaLive.Application.Common.Messaging;
using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ISpuRepository spuRepository)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(
        DeleteCategoryCommand command,
        CancellationToken cancellationToken)
    {
        // 1. Kiểm tra danh mục tồn tại
        var category = await categoryRepository.GetByIdAsync(command.Id, includeInvisible: true, cancellationToken);
        if (category is null)
        {
            return CategoryErrors.NotFound;
        }

        // 2. Ràng buộc: Kiểm tra nếu danh mục đang có Categories con
        var hasChildren = await categoryRepository.HasChildrenAsync(command.Id, cancellationToken);
        if (hasChildren)
        {
            return CategoryErrors.CannotDeleteCategoryWithChildrenOrProducts;
        }

        // 3. Ràng buộc: Kiểm tra nếu danh mục đang chứa Spus
        var hasProducts = await spuRepository.ExistsByCategoryIdAsync(command.Id, cancellationToken);
        if (hasProducts)
        {
            return CategoryErrors.CannotDeleteCategoryWithChildrenOrProducts;
        }

        // 4. Xóa danh mục
        categoryRepository.Delete(category);

        return Result.Success();
    }
}
