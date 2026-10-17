using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Commands.SendComment;

public sealed class SendLivestreamCommentCommandValidator : AbstractValidator<SendLivestreamCommentCommand>
{
    public SendLivestreamCommentCommandValidator()
    {
        RuleFor(x => x.SessionId)
            .NotEmpty()
            .WithMessage("Mã phiên livestream không được để trống.");

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .WithMessage("Tên người bình luận không được để trống.")
            .MaximumLength(100)
            .WithMessage("Tên người bình luận tối đa 100 ký tự.");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Nội dung bình luận không được để trống.")
            .MaximumLength(500)
            .WithMessage("Nội dung bình luận tối đa 500 ký tự.");
    }
}
