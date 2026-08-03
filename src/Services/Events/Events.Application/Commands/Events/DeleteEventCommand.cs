using Events.Application.Common.Models;
using Events.Domain.Interfaces;
using MediatR;

namespace Events.Application.Commands.Events;

public record DeleteEventCommand(int Id) : IRequest<ApiResponse<bool>>;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, ApiResponse<bool>>
{
    private readonly IEventRepository _eventRepository;

    public DeleteEventCommandHandler(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var @event = await _eventRepository.GetByIdAsync(request.Id, cancellationToken);
        if (@event is null)
        {
            return ApiResponse<bool>.Fail("رویداد یافت نشد");
        }

        await _eventRepository.DeleteAsync(@event);
        return ApiResponse<bool>.Ok(true, "رویداد حذف شد");
    }
}
