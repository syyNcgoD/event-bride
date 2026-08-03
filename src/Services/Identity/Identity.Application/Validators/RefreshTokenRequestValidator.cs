using FluentValidation;

namespace Identity.Application.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<DTOs.RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh Token الزامی است");
    }
}
