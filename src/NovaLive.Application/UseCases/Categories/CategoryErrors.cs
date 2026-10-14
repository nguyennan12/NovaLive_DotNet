using NovaLive.Domain.Common;

namespace NovaLive.Application.UseCases.Categories;

public static class CategoryErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Category.NotFound", "Không tìm thấy danh mục yêu cầu.");

    public static readonly Error ParentNotFound =
        Error.NotFound("Category.ParentNotFound", "Không tìm thấy danh mục cha được chỉ định.");

    public static readonly Error CannotBeOwnParent =
        Error.Validation("Category.CannotBeOwnParent", "Danh mục không thể là danh mục cha của chính nó.");

    public static readonly Error CircularReference =
        Error.Validation("Category.CircularReference", "Không thể chọn danh mục con làm danh mục cha (tránh vòng lặp vô tận).");

    public static readonly Error SlugAlreadyExists =
        Error.Conflict("Category.SlugAlreadyExists", "Đường dẫn (slug) của danh mục đã tồn tại trong hệ thống.");

    public static readonly Error CannotDeleteCategoryWithChildrenOrProducts =
        Error.Validation("Category.CannotDelete", "Không thể xóa danh mục đang có sản phẩm hoặc danh mục con.");
}
