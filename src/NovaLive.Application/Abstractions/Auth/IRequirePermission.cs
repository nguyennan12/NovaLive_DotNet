namespace NovaLive.Application.Abstractions.Auth;

public interface IRequirePermission
{
    string RequiredPermission { get; }
}
