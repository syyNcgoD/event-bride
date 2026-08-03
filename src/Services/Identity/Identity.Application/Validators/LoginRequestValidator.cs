using FluentValidation;

namespace Identity.Application.Validators;

public class LoginRequestValidator : AbstractValidator<DTOs.LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty().WithMessage("نام کاربری یا ایمیل الزامی است");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است");
    }
}
