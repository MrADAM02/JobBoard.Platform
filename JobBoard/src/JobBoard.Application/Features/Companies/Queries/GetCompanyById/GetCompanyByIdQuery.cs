using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Companies.Queries.GetCompanyById;

public record GetCompanyByIdQuery(Guid Id) : IRequest<CompanyDto>;

public record CompanyDto(Guid Id, string Name, string? LogoUrl, string? Website, string? Description, string? Location);

public class GetCompanyByIdQueryHandler : IRequestHandler<GetCompanyByIdQuery, CompanyDto>
{
    private readonly IApplicationDbContext _db;

    public GetCompanyByIdQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<CompanyDto> Handle(GetCompanyByIdQuery request, CancellationToken cancellationToken)
    {
        var company = await _db.Companies.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Company), request.Id);

        return new CompanyDto(company.Id, company.Name, company.LogoUrl, company.Website, company.Description, company.Location);
    }
}
