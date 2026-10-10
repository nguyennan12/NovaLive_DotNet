namespace NovaLive.Domain.Rbac;

public static class PermissionCodes
{
    public static class Auth
    {
        public const string Register = "auth:register";
        public const string Login = "auth:login";
        public const string Logout = "auth:logout";
    }

    public static class Users
    {
        public const string ViewOwnProfile = "users:view_own_profile";
        public const string UpdateOwnProfile = "users:update_own_profile";
        public const string ManageOwnAddresses = "users:manage_own_addresses";
        public const string ManageAll = "users:manage_all";
    }

    public static class Shops
    {
        public const string Register = "shops:register";
        public const string ViewPublic = "shops:view_public";
        public const string ManageOwn = "shops:manage_own";
        public const string Follow = "shops:follow";
        public const string ApproveKyc = "shops:approve_kyc";
        public const string BanUnban = "shops:ban_unban";
    }

    public static class Wallets
    {
        public const string ViewOwn = "wallets:view_own";
        public const string RequestPayout = "wallets:request_payout";
        public const string ApprovePayout = "wallets:approve_payout";
    }

    public static class Categories
    {
        public const string ViewPublic = "categories:view_public";
        public const string ManageAll = "categories:manage_all";
    }

    public static class Products
    {
        public const string ViewPublic = "products:view_public";
        public const string ViewOwn = "products:view_own";
        public const string CreateOwn = "products:create_own";
        public const string UpdateOwn = "products:update_own";
        public const string DeleteOwn = "products:delete_own";
        public const string ModerateAll = "products:moderate_all";
    }

    public static class Inventory
    {
        public const string ManageOwn = "inventory:manage_own";
    }

    public static class Carts
    {
        public const string ManageOwn = "carts:manage_own";
    }

    public static class Orders
    {
        public const string Checkout = "orders:checkout";
        public const string ViewOwnBuy = "orders:view_own_buy";
        public const string CancelOwnBuy = "orders:cancel_own_buy";
        public const string ViewOwnSell = "orders:view_own_sell";
        public const string FulfillmentOwn = "orders:fulfillment_own";
        public const string CancelOwnSell = "orders:cancel_own_sell";
        public const string ViewAllPlatform = "orders:view_all_platform";
    }

    public static class Payments
    {
        public const string Initiate = "payments:initiate";
        public const string ViewStatus = "payments:view_status";
    }

    public static class Discounts
    {
        public const string ViewPublic = "discounts:view_public";
        public const string CreateOwnShop = "discounts:create_own_shop";
        public const string CreatePlatform = "discounts:create_platform";
    }

    public static class FlashSales
    {
        public const string ViewPublic = "flashsales:view_public";
        public const string RegisterOwn = "flashsales:register_own";
        public const string ManageAll = "flashsales:manage_all";
    }

    public static class Shipping
    {
        public const string CalculateFee = "shipping:calculate_fee";
        public const string PrintOwnLabel = "shipping:print_own_label";
        public const string TrackOrder = "shipping:track_order";
    }

    public static class Returns
    {
        public const string RequestOwn = "returns:request_own";
        public const string RespondOwn = "returns:respond_own";
    }

    public static class Disputes
    {
        public const string ArbitrateAll = "disputes:arbitrate_all";
    }

    public static class Livestreams
    {
        public const string ViewPublic = "livestreams:view_public";
        public const string StartOwn = "livestreams:start_own";
        public const string PinOwnProduct = "livestreams:pin_own_product";
        public const string TerminateAll = "livestreams:terminate_all";
    }

    public static class Reviews
    {
        public const string CreateOwn = "reviews:create_own";
        public const string UpdateOwn = "reviews:update_own";
        public const string ReplyOwnShop = "reviews:reply_own_shop";
        public const string HideAll = "reviews:hide_all";
    }

    public static class Reports
    {
        public const string ViewOwnShop = "reports:view_own_shop";
        public const string ViewAllPlatform = "reports:view_all_platform";
    }

    public static class Roles
    {
        public const string ManageAll = "roles:manage_all";
    }
}
