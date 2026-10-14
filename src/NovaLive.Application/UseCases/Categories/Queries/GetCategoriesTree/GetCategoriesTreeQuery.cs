using NovaLive.Application.Common.Messaging;
using NovaLive.Contracts.V1.Categories;

namespace NovaLive.Application.UseCases.Categories.Queries.GetCategoriesTree;

public record GetCategoriesTreeQuery(bool IncludeInvisible = false) : IQuery<IReadOnlyList<CategoryNodeDto>>;
