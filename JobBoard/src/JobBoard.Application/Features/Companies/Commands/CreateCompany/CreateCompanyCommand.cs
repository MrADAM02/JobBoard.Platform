using FluentValidation;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;

namespace JobBoard.Application.Features.Companies.Commands.CreateCompany;

public record CreateCompanyCommand(string Name, string? Website, string? Description, string? Location)
    : IRequest<Guid>;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, Guid>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public CreateCompanyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = new Company
        {
            OwnerUserId = _currentUser.UserId!.Value, // [Authorize(Roles = "Employer")] guarantees this is set
            Name = request.Name,
            Website = request.Website,
            Description = request.Description,
            Location = request.Location
        };

        _db.Companies.Add(company);
        await _db.SaveChangesAsync(cancellationToken);

        return company.Id;
    }
}
