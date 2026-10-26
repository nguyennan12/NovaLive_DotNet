using FluentValidation;

namespace NovaLive.Application.UseCases.Products.Commands.RebuildProductSearchVector;

public sealed class RebuildProductSearchVectorCommandValidator : AbstractValidator<RebuildProductSearchVectorCommand>
{
    public RebuildProductSearchVectorCommandValidator()
    {
        RuleFor(command => command.SpuId).NotEmpty().WithMessage("Mã sản phẩm không hợp lệ.");
    }
}
