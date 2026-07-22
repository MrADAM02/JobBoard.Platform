using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;

namespace JobBoard.Application.Features.Companies.Commands.UpdateCompany;

public record UpdateCompanyCommand(Guid Id, string Name, string? Website, string? Description, string? Location)
    : IRequest;

public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public UpdateCompanyCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = _db.Companies.FirstOrDefault(c => c.Id == request.Id)
            ?? throw new NotFoundException(nameof(Company), request.Id);

        if (company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException();
        }

        company.Name = request.Name;
        company.Website = request.Website;
        company.Description = request.Description;
        company.Location = request.Location;
        company.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
