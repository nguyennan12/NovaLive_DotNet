namespace NovaLive.Application.UseCases.Shops;

public static class ShopValidation
{
    public const string PhonePattern = @"^(?:0|\+84)(?:[35789][0-9]{8}|2[0-9]{9})$";
    public static bool IsOptionalHttpUrl(string? value) => value is null ||
        (Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https"
         && uri.UserInfo.Length == 0);
}
