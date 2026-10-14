using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Categories;

namespace NovaLive.Application.UseCases.Categories.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id, bool IncludeInvisible = false) : IQuery<CategoryResponse>;
