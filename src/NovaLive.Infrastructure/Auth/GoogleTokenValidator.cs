using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using NovaLive.Application.Abstractions.Auth;
namespace NovaLive.Infrastructure.Auth;

public sealed class GoogleTokenValidator(IOptions<GoogleOptions> options) : IGoogleTokenValidator
{
    public bool Enabled => options.Value.Enabled;
    public async Task<GoogleIdentity?> ValidateAsync(string idToken, CancellationToken cancellationToken)
    {
        if (!Enabled) return null;
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken,
                new GoogleJsonWebSignature.ValidationSettings { Audience = [options.Value.ClientId] })
                .WaitAsync(cancellationToken);
            return payload.EmailVerified && !string.IsNullOrWhiteSpace(payload.Email)
                ? new(payload.Email.Trim().ToLowerInvariant(), payload.Name ?? payload.Email) : null;
        }
        catch (InvalidJwtException) { return null; }
    }
}

