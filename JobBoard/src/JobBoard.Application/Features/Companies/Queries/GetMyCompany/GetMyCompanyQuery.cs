using JobBoard.Application.Common.Interfaces;
using JobBoard.Application.Features.Companies.Queries.GetCompanyById;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Companies.Queries.GetMyCompany;

// Lets the frontend discover the current employer's own CompanyId after login -
// CreateJobListingCommand needs it, and there's no other way to look it up
// without already knowing it. Returns null (not a 404) when the employer
// hasn't created a company yet, since that's an expected first-login state.
public record GetMyCompanyQuery : IRequest<CompanyDto?>;

public class GetMyCompanyQueryHandler : IRequestHandler<GetMyCompanyQuery, CompanyDto?>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public GetMyCompanyQueryHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<CompanyDto?> Handle(GetMyCompanyQuery request, CancellationToken cancellationToken)
    {
        var company = await _db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.OwnerUserId == _currentUser.UserId, cancellationToken);

        return company is null
            ? null
            : new CompanyDto(company.Id, company.Name, company.LogoUrl, company.Website, company.Description, company.Location);
    }
}
