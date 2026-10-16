using FluentValidation;
using NovaLive.Application.Abstractions.Services;

namespace NovaLive.Application.UseCases.FlashSales.Commands.CreateFlashSaleCampaign;

public sealed class CreateFlashSaleCampaignCommandValidator : AbstractValidator<CreateFlashSaleCampaignCommand>
{
    public CreateFlashSaleCampaignCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("Tên chiến dịch Flash Sale không được để trống.")
            .MaximumLength(200).WithMessage("Tên chiến dịch Flash Sale không được vượt quá 200 ký tự.");

        RuleFor(x => x.Request.StartAt)
            .GreaterThanOrEqualTo(_ => dateTimeProvider.UtcNow.AddMinutes(-5))
            .WithMessage("Thời gian bắt đầu không được trong quá khứ quá 5 phút.");

        RuleFor(x => x.Request.EndAt)
            .GreaterThan(x => x.Request.StartAt)
            .WithMessage("Thời gian kết thúc phải lớn hơn thời gian bắt đầu.");

        RuleFor(x => x.Request.BannerUrl)
            .MaximumLength(500).WithMessage("Đường dẫn banner không được vượt quá 500 ký tự.")
            .When(x => !string.IsNullOrEmpty(x.Request.BannerUrl));
    }
}
