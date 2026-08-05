using Booking.Application.DTOs;
using FluentValidation;

namespace Booking.Application.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("ایمیل الزامی است")
            .EmailAddress().WithMessage("ایمیل معتبر نیست");

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("حداقل یک بلیط لازم است");

        RuleForEach(x => x.Items)
            .SetValidator(new OrderItemRequestValidator());
    }
}

public class OrderItemRequestValidator : AbstractValidator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.TicketTypeId)
            .GreaterThan(0).WithMessage("نوع بلیط نامعتبر است");

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 10).WithMessage("تعداد بلیط باید بین ۱ تا ۱۰ باشد");
    }
}

public class ConfirmPaymentRequestValidator : AbstractValidator<ConfirmPaymentRequest>
{
    public ConfirmPaymentRequestValidator()
    {
        RuleFor(x => x.PaymentMethod)
            .NotEmpty().WithMessage("روش پرداخت الزامی است")
            .MaximumLength(50);
    }
}