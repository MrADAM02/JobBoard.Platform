using JobBoard.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Notifications.Queries.GetMyNotifications;

// Backs the nav bell dropdown - most recent 20, newest first. The frontend
// derives the unread badge count client-side from IsRead rather than a
// separate endpoint, since the full list is already small and cheap to fetch.
public record GetMyNotificationsQuery : IRequest<List<NotificationDto>>;

public record NotificationDto(Guid Id, string Type, string Message, bool IsRead, DateTime CreatedAt);

public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, List<NotificationDto>>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyNotificationsQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<List<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        return await _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == _currentUser.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(20)
            .Select(n => new NotificationDto(n.Id, n.Type, n.Message, n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
