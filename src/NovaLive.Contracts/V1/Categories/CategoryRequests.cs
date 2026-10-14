namespace NovaLive.Contracts.V1.Categories;

public record CreateCategoryRequest(
    string Name,
    Guid? ParentId = null,
    string? IconUrl = null,
    int DisplayOrder = 0,
    string? Slug = null);

public record UpdateCategoryRequest(
    string Name,
    Guid? ParentId = null,
    string? IconUrl = null,
    int DisplayOrder = 0,
    bool IsVisible = true,
    string? Slug = null);
