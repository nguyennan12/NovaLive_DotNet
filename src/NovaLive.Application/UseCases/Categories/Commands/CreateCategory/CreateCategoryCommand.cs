using NovaLive.Application.Abstractions.Auth;
using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Categories;
using NovaLive.Domain.Rbac;

namespace NovaLive.Application.UseCases.Categories.Commands.CreateCategory;

public record CreateCategoryCommand(CreateCategoryRequest Request)
    : ICommand<CategoryResponse>, ITransactionalCommand, IRequirePermission
{
    public string RequiredPermission => PermissionCodes.Categories.ManageAll;
}
