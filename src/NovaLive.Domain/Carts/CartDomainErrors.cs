using NovaLive.Domain.Common;

namespace NovaLive.Domain.Carts;

public static class CartDomainErrors
{
    public static readonly Error InvalidQuantity = Error.Validation("Carts.InvalidQuantity", "Số lượng phải lớn hơn 0 và không vượt giới hạn số nguyên.");
    public static readonly Error InvalidPrice = Error.Validation("Carts.InvalidPrice", "Giá sản phẩm không được âm.");
    public static readonly Error ItemNotFound = Error.NotFound("Carts.ItemNotFound", "Không tìm thấy sản phẩm trong giỏ hàng.");
}
