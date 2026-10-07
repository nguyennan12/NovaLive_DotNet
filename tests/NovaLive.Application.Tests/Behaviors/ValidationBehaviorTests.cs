using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using NovaLive.Application.Common.Behaviors;
using NovaLive.Domain.Common;

namespace NovaLive.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public record TestCommand(string Name) : IRequest<Result>;

    [Fact]
    public async Task Handle_WhenNoValidationErrors_ShouldCallNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestCommand>>();
        validatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var behavior = new ValidationBehavior<TestCommand, Result>([validatorMock.Object]);
        var request = new TestCommand("Valid Name");
        var nextMock = new Mock<RequestHandlerDelegate<Result>>();
        nextMock.Setup(n => n(It.IsAny<CancellationToken>())).ReturnsAsync(Result.Success());

        // Act
        var result = await behavior.Handle(request, nextMock.Object, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        nextMock.Verify(n => n(It.IsAny<CancellationToken>()), Times.Once);
    }
}
