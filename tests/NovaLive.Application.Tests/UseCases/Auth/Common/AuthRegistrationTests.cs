using FluentAssertions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NovaLive.Application.UseCases.Auth.Queries.GetCurrentUser;

namespace NovaLive.Application.Tests.UseCases.Auth.Common;

public sealed class AuthRegistrationTests
{
    [Fact]
    public void AssemblyScanning_RegistersEveryAuthHandlerAndCommandValidator()
    {
        var services = new ServiceCollection().AddApplication();
        var requests = typeof(GetCurrentUserQuery).Assembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith("NovaLive.Application.UseCases.Auth.", StringComparison.Ordinal) == true
                && !type.IsAbstract && type.GetInterfaces().Any(contract => contract.IsGenericType
                    && contract.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .ToArray();
        requests.Should().HaveCount(10);
        foreach (var request in requests)
        {
            services.Should().ContainSingle(service => service.ServiceType.IsGenericType
                && service.ServiceType.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)
                && service.ServiceType.GenericTypeArguments[0] == request);
            if (request == typeof(GetCurrentUserQuery)) continue;
            services.Should().ContainSingle(service => service.ServiceType == typeof(IValidator<>).MakeGenericType(request));
        }
    }
}
