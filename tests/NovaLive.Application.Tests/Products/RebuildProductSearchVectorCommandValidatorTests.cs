using FluentAssertions;
using NovaLive.Application.UseCases.Products.Commands.RebuildProductSearchVector;

namespace NovaLive.Application.Tests.Products;

public sealed class RebuildProductSearchVectorCommandValidatorTests
{
    [Fact]
    public async Task Validate_EmptySpuId_Fails()
    {
        var validator = new RebuildProductSearchVectorCommandValidator();

        var result = await validator.ValidateAsync(new RebuildProductSearchVectorCommand(Guid.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle().Which.PropertyName.Should().Be("SpuId");
    }

    [Fact]
    public async Task Validate_ValidSpuId_Succeeds()
    {
        var validator = new RebuildProductSearchVectorCommandValidator();

        var result = await validator.ValidateAsync(new RebuildProductSearchVectorCommand(Guid.NewGuid()));

        result.IsValid.Should().BeTrue();
    }
}
