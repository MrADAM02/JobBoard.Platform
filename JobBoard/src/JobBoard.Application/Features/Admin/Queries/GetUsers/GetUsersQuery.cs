using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Common.Models;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Admin.Queries.GetUsers;

public record GetUsersQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedList<UserSummaryDto>>;

public record UserSummaryDto(Guid Id, string Email, UserRole Role, bool IsActive, DateTime CreatedAt);

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PaginatedList<UserSummaryDto>>
{
    private readonly IApplicationDbContext _db;

    public GetUsersQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<PaginatedList<UserSummaryDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var query = _db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserSummaryDto(u.Id, u.Email, u.Role, u.IsActive, u.CreatedAt));

        return await PaginatedList<UserSummaryDto>.CreateAsync(query, request.PageNumber, request.PageSize);
    }
}
