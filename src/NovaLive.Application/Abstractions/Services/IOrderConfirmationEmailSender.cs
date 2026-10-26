namespace NovaLive.Application.Abstractions.Services;

public interface IOrderConfirmationEmailSender
{
    Task SendAsync(string email, string html, CancellationToken ct = default);
}
