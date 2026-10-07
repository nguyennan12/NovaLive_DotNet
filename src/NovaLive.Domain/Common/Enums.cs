namespace NovaLive.Domain.Common;

public enum AccountStatus
{
    Unverified = 0,
    Active = 1,
    Locked = 2,
    Suspended = 3,
    Deleted = 4
}

public enum UserGender
{
    Male = 0,
    Female = 1,
    Other = 2,
    Unspecified = 3
}

public enum OtpType
{
    EmailVerify = 0,
    PhoneVerify = 1,
    ForgotPassword = 2,
    Login2FA = 3
}

public enum ShopStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2,
    Banned = 3,
    Closed = 4
}

public enum VerificationStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

public enum WalletTxType
{
    EscrowHold = 0,
    EscrowRelease = 1,
    EscrowRefund = 2,
    CodCommissionDeduct = 3,
    PayoutLock = 4,
    PayoutWithdrawal = 5,
    PayoutFailedUnlock = 6,
    PenaltyDeduct = 7,
    ManualAdjustment = 8
}

public enum RbacAction
{
    Create = 0,
    Read = 1,
    Update = 2,
    Delete = 3,
    Approve = 4,
    Export = 5,
    Override = 6,
    Suspend = 7
}

public enum ProductStatus
{
    Draft = 0,
    Active = 1,
    Inactive = 2,
    Banned = 3
}

public enum InventoryChangeType
{
    Import = 0,
    SaleConfirmed = 1,
    ReturnIn = 2,
    ManualAdjust = 3,
    ReserveAdd = 4,
    ReserveRelease = 5
}

public enum ParentOrderPaymentStatus
{
    Pending = 0,
    Paid = 1,
    PartiallyRefunded = 2,
    FullyRefunded = 3,
    Cancelled = 4
}

public enum OrderSource
{
    Online = 0,
    Livestream = 1,
    FlashSale = 2
}

public enum SubOrderStatus
{
    PendingPayment = 0,
    Confirmed = 1,
    Processing = 2,
    Shipping = 3,
    Delivered = 4,
    Completed = 5,
    Cancelled = 6,
    ReturnRequested = 7,
    Returned = 8,
    Refunded = 9
}

public enum OrderItemStatus
{
    Active = 0,
    Returned = 1,
    PartialReturn = 2
}

public enum OrderRole
{
    Buyer = 0,
    Seller = 1,
    Admin = 2,
    System = 3
}

public enum ReturnReason
{
    WrongItem = 0,
    Defective = 1,
    DamagedInShipping = 2,
    NotAsDescribed = 3,
    ChangeOfMind = 4
}

public enum ReturnStatus
{
    Pending = 0,
    SellerApproved = 1,
    SellerRejected = 2,
    AdminDispute = 3,
    AdminApproved = 4,
    AdminRejected = 5,
    Completed = 6
}

public enum DiscountType
{
    PercentCart = 0,
    FixedCart = 1,
    PercentShip = 2,
    FreeShip = 3
}

public enum DiscountAppliesTo
{
    AllProducts = 0,
    SpecificSpus = 1,
    SpecificCategories = 2
}

public enum FlashSaleCampaignStatus
{
    Scheduled = 0,
    Active = 1,
    Ended = 2,
    Cancelled = 3
}

public enum FlashSaleItemStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Ended = 3
}

public enum PaymentMethod
{
    MoMo = 0,
    VietQR = 1,
    COD = 2
}

public enum PaymentStatus
{
    Pending = 0,
    Success = 1,
    Failed = 2,
    Expired = 3,
    Refunded = 4
}

public enum EscrowStatus
{
    PendingCapture = 0,
    Holding = 1,
    Released = 2,
    Disputed = 3,
    Refunded = 4,
    PartialRefund = 5
}

public enum PayoutStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}

public enum ShippingProvider
{
    GHN = 0,
    GHTK = 1,
    ViettelPost = 2
}

public enum ShippingStatus
{
    ReadyToPick = 0,
    Picking = 1,
    Delivering = 2,
    Delivered = 3,
    Failed = 4,
    Returned = 5,
    Cancelled = 6
}

public enum LivestreamStatus
{
    Scheduled = 0,
    Live = 1,
    Ended = 2,
    Cancelled = 3
}

public enum ReviewMediaType
{
    image = 0,
    video = 1
}
