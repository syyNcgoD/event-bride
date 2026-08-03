using Events.Application.DTOs;
using FluentValidation;

namespace Events.Application.Validators;

public class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان رویداد الزامی است")
            .MaximumLength(200);

        RuleFor(x => x.VenueId)
            .GreaterThan(0).WithMessage("مکان رویداد الزامی است");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("دسته‌بندی رویداد الزامی است");

        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow).WithMessage("تاریخ شروع باید در آینده باشد");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("تاریخ پایان باید بعد از شروع باشد");

        RuleFor(x => x.TicketTypes)
            .NotEmpty().WithMessage("حداقل یک نوع بلیط لازم است");

        RuleForEach(x => x.TicketTypes)
            .SetValidator(new CreateTicketTypeRequestValidator());
    }
}

public class CreateTicketTypeRequestValidator : AbstractValidator<CreateTicketTypeRequest>
{
    public CreateTicketTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام نوع بلیط الزامی است")
            .MaximumLength(100);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("قیمت نمی‌تواند منفی باشد");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("تعداد بلیط باید بیشتر از صفر باشد");

        RuleFor(x => x.MaxPerOrder)
            .GreaterThan(0).WithMessage("حداکثر تعداد در هر سفارش باید بیشتر از صفر باشد");

        RuleFor(x => x.SaleStart)
            .GreaterThanOrEqualTo(DateTime.UtcNow.AddMinutes(-1)).WithMessage("تاریخ شروع فروش نامعتبر است");

        RuleFor(x => x.SaleEnd)
            .GreaterThan(x => x.SaleStart).WithMessage("تاریخ پایان فروش باید بعد از شروع باشد");
    }
}

public class UpdateEventRequestValidator : AbstractValidator<UpdateEventRequest>
{
    public UpdateEventRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("عنوان رویداد الزامی است")
            .MaximumLength(200);

        RuleFor(x => x.VenueId)
            .GreaterThan(0).WithMessage("مکان رویداد الزامی است");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("دسته‌بندی رویداد الزامی است");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate).WithMessage("تاریخ پایان باید بعد از شروع باشد");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("وضعیت رویداد نامعتبر است");
    }
}
