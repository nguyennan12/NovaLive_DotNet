using FluentAssertions;
using NovaLive.Domain.Common;

namespace NovaLive.Domain.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_ShouldReturnSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldReturnFailureResultWithError()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TEST_ERROR");
        result.Error.Message.Should().Be("Test error message");
    }

    [Fact]
    public void GenericSuccess_ShouldContainValue()
    {
        // Act
        var result = Result<string>.Success("Hello NovaLive");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Hello NovaLive");
    }
}
