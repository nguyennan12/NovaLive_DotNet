namespace NovaLive.Contracts.V1.Categories;

public record CategoryResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string? IconUrl,
    int DisplayOrder,
    bool IsVisible,
    int ProductCount = 0);

public record CategoryNodeDto(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string? IconUrl,
    int DisplayOrder,
    int ProductCount,
    List<CategoryNodeDto> Children);
