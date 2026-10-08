namespace NovaLive.Contracts.V1.Categories;

public record CreateCategoryRequest(
    string Name,
    Guid? ParentId,
    string? IconUrl,
    int DisplayOrder);

public record UpdateCategoryRequest(
    string Name,
    Guid? ParentId,
    string? IconUrl,
    int DisplayOrder,
    bool IsVisible);
