using System.Security.Cryptography;
namespace NovaLive.Domain.Orders;
public static class OrderCodeGenerator
{
    public static string Generate(DateTimeOffset now) =>
        $"NOVA-{now:yyyyMMdd}-{RandomNumberGenerator.GetString("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", 4)}";
}
