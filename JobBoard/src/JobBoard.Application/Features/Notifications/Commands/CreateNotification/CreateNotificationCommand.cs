using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;

namespace JobBoard.Application.Features.Notifications.Commands.CreateNotification;

// Internal side-effect command, not exposed on any controller - called directly
// from other handlers (ApplyToJob, UpdateApplicationStatus) via IMediator.Send.
public record CreateNotificationCommand(Guid UserId, string Type, string Message) : IRequest;

public class CreateNotificationCommandHandler : IRequestHandler<CreateNotificationCommand>
{
    private readonly IApplicationDbContext _db;

    public CreateNotificationCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = request.UserId,
            Type = request.Type,
            Message = request.Message
        });

        await _db.SaveChangesAsync(cancellationToken);
    }
}
