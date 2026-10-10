namespace NovaLive.Application.Abstractions.Auth;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class RequirePermissionAttribute(params string[] permissions) : Attribute
{
    public IReadOnlyCollection<string> Permissions { get; } = permissions;
}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public sealed class AuthorizeRoleAttribute(params string[] roles) : Attribute
{
    public IReadOnlyCollection<string> Roles { get; } = roles;
}
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class RequireAuthenticatedAttribute : Attribute;

