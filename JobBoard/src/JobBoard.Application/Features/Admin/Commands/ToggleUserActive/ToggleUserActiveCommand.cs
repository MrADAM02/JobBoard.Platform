using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;

namespace JobBoard.Application.Features.Admin.Commands.ToggleUserActive;

public record ToggleUserActiveCommand(Guid UserId) : IRequest;

public class ToggleUserActiveCommandHandler : IRequestHandler<ToggleUserActiveCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ToggleUserActiveCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(ToggleUserActiveCommand request, CancellationToken cancellationToken)
    {
        // An admin locking out their own only account would need direct DB access to undo -
        // simplest guard is to just never allow it via this command.
        if (request.UserId == _currentUser.UserId)
        {
            throw new ValidationException("You cannot deactivate your own account.");
        }

        var user = _db.Users.FirstOrDefault(u => u.Id == request.UserId)
            ?? throw new NotFoundException(nameof(User), request.UserId);

        user.IsActive = !user.IsActive;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
