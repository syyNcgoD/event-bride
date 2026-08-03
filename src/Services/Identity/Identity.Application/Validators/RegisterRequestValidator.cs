using FluentValidation;

namespace Identity.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<DTOs.RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("نام الزامی است")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("نام خانوادگی الزامی است")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل الزامی است")
            .EmailAddress().WithMessage("ایمیل معتبر نیست");

        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("نام کاربری الزامی است")
            .MinimumLength(3).WithMessage("نام کاربری حداقل ۳ کاراکتر است")
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("رمز عبور الزامی است")
            .MinimumLength(6).WithMessage("رمز عبور حداقل ۶ کاراکتر است")
            .MaximumLength(100)
            .Matches(@"[A-Z]").WithMessage("رمز عبور باید حداقل یک حرف بزرگ داشته باشد")
            .Matches(@"[a-z]").WithMessage("رمز عبور باید حداقل یک حرف کوچک داشته باشد")
            .Matches(@"[0-9]").WithMessage("رمز عبور باید حداقل یک عدد داشته باشد");
    }
}
