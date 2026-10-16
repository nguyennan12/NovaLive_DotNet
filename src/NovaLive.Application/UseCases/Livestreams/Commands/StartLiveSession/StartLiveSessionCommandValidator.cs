using FluentValidation;

namespace NovaLive.Application.UseCases.Livestreams.Commands.StartLiveSession;

public sealed class StartLiveSessionCommandValidator : AbstractValidator<StartLiveSessionCommand>
{
    public StartLiveSessionCommandValidator()
    {
        RuleFor(x => x.Request.Title)
            .NotEmpty().WithMessage("Tiêu đề livestream không được để trống.")
            .MinimumLength(5).WithMessage("Tiêu đề livestream phải có ít nhất 5 ký tự.")
            .MaximumLength(300).WithMessage("Tiêu đề livestream không được vượt quá 300 ký tự.");

        When(x => !string.IsNullOrWhiteSpace(x.Request.ThumbnailUrl), () =>
        {
            RuleFor(x => x.Request.ThumbnailUrl)
                .MaximumLength(500).WithMessage("Đường dẫn ảnh bìa không được vượt quá 500 ký tự.")
                .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                .WithMessage("Đường dẫn ảnh bìa không hợp lệ.");
        });
    }
}
