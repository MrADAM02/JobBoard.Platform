using JobBoard.Application.Features.Candidates.Commands.UpdateCandidateProfile;
using JobBoard.Application.Features.Candidates.Commands.UploadResume;
using JobBoard.Application.Features.Candidates.Queries.GetMyCandidateProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api/candidates")]
[Authorize(Roles = "Candidate")]
public class CandidatesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CandidatesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
        => Ok(await _mediator.Send(new GetMyCandidateProfileQuery()));

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateCandidateProfileCommand command)
    {
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("me/resume")]
    [RequestSizeLimit(UploadResumeCommandValidator.MaxFileSizeBytes)]
    public async Task<ActionResult<string>> UploadResume(IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var command = new UploadResumeCommand(stream, file.FileName, file.ContentType, file.Length);
        return Ok(await _mediator.Send(command, cancellationToken));
    }
}
