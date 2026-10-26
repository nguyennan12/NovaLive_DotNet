using FluentValidation;
namespace NovaLive.Application.UseCases.Returns.Commands.ConfirmReturnReceived;
public sealed class ConfirmReturnReceivedCommandValidator : AbstractValidator<ConfirmReturnReceivedCommand>
{
    public ConfirmReturnReceivedCommandValidator() { RuleFor(c => c.Id).NotEmpty(); }
}
