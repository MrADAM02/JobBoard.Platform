using JobBoard.Application.Features.Applications.Commands.ApplyToJob;
using JobBoard.Application.Features.Applications.Commands.UpdateApplicationStatus;
using JobBoard.Application.Features.Applications.Queries.GetApplicationsForJob;
using JobBoard.Application.Features.Applications.Queries.GetMyApplications;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ApplicationsController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    [Authorize(Roles = "Candidate")]
    public async Task<ActionResult<Guid>> Apply(ApplyToJobCommand command)
        => Ok(await _mediator.Send(command));

    [HttpGet("mine")]
    [Authorize(Roles = "Candidate")]
    public async Task<IActionResult> GetMine()
        => Ok(await _mediator.Send(new GetMyApplicationsQuery()));

    [HttpGet("job/{jobListingId:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetForJob(Guid jobListingId)
        => Ok(await _mediator.Send(new GetApplicationsForJobQuery(jobListingId)));

    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateApplicationStatusCommand command)
    {
        if (id != command.ApplicationId) return BadRequest("Route id and body id must match.");
        await _mediator.Send(command);
        return NoContent();
    }
}
