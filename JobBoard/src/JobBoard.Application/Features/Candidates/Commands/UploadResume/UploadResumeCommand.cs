using FluentValidation;
using JobBoard.Application.Common.Exceptions;
using JobBoard.Application.Common.Interfaces;
using JobBoard.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobBoard.Application.Features.Candidates.Commands.UploadResume;

// FileStream/FileName/ContentType/FileSizeBytes rather than IFormFile - Application
// stays free of ASP.NET-specific types; the controller does the IFormFile -> these
// primitives translation.
public record UploadResumeCommand(Stream FileStream, string FileName, string ContentType, long FileSizeBytes)
    : IRequest<string>;

public class UploadResumeCommandValidator : AbstractValidator<UploadResumeCommand>
{
    public static readonly string[] AllowedContentTypes =
    [
        "application/pdf",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    public const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UploadResumeCommandValidator()
    {
        RuleFor(x => x.ContentType)
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Resume must be a PDF or Word document.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxFileSizeBytes)
            .WithMessage("Resume must be 5 MB or smaller.");
    }
}

public class UploadResumeCommandHandler : IRequestHandler<UploadResumeCommand, string>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IFileStorageService _fileStorage;

    public UploadResumeCommandHandler(IApplicationDbContext db, ICurrentUserService currentUser, IFileStorageService fileStorage)
    {
        _db = db;
        _currentUser = currentUser;
        _fileStorage = fileStorage;
    }

    public async Task<string> Handle(UploadResumeCommand request, CancellationToken cancellationToken)
    {
        var profile = await _db.CandidateProfiles
            .FirstOrDefaultAsync(c => c.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(CandidateProfile), _currentUser.UserId!);

        var url = await _fileStorage.SaveAsync(request.FileStream, request.FileName, request.ContentType, cancellationToken);

        // Best-effort cleanup of the previous file - a failure here shouldn't block the upload.
        if (!string.IsNullOrEmpty(profile.ResumeUrl))
        {
            await _fileStorage.DeleteAsync(profile.ResumeUrl, cancellationToken);
        }

        profile.ResumeUrl = url;
        profile.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return url;
    }
}
