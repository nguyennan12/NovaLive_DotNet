namespace NovaLive.Contracts.V1.Categories;

public record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string? IconUrl,
    int DisplayOrder,
    bool IsVisible);

public record CategoryNodeDto(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string? IconUrl,
    int DisplayOrder,
    List<CategoryNodeDto> Children);
