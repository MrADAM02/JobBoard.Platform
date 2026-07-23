using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;

namespace JobBoard.Application.Features.Companies.Commands.UploadCompanyLogo;

public record UploadCompanyLogoCommand(Guid CompanyId, Stream FileStream, string FileName, string ContentType, long FileSizeBytes)
    : IRequest<string>;

public class UploadCompanyLogoCommandValidator : AbstractValidator<UploadCompanyLogoCommand>
{
    public static readonly string[] AllowedContentTypes = ["image/png", "image/jpeg", "image/svg+xml"];

    public const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB

    public UploadCompanyLogoCommandValidator()
    {
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Logo must be a PNG, JPEG, or SVG image.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("Logo must be 2 MB or smaller.");
    }
}

public class UploadCompanyLogoCommandHandler : IRequestHandler<UploadCompanyLogoCommand, string>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public UploadCompanyLogoCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _db = db;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<string> Handle(UploadCompanyLogoCommand request, CancellationToken cancellationToken)
    {
        var company = _db.Companies.FirstOrDefault(c => c.Id == request.CompanyId)
            ?? throw new NotFoundException(nameof(Company), request.CompanyId);

        if (company.OwnerUserId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("You do not own this company profile.");
        }

        var url = await _fileStorage.SaveAsync(request.FileStream, request.FileName, request.ContentType, cancellationToken);

        if (!string.IsNullOrEmpty(company.LogoUrl))
        {
            await _fileStorage.DeleteAsync(company.LogoUrl, cancellationToken);
        }

        company.LogoUrl = url;
        company.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return url;
    }
}
