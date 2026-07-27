using JobBoard.Application.Common.Models;
using JobBoard.Application.Features.Jobs.Commands.SaveJob;
using JobBoard.Application.Features.Jobs.Commands.UnsaveJob;
using JobBoard.Application.Features.Jobs.Queries.GetJobListings;
using JobBoard.Application.Features.Jobs.Queries.GetMySavedJobIds;
using JobBoard.Application.Features.Jobs.Queries.GetMySavedJobs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api/saved-jobs")]
[Authorize(Roles = "Candidate")]
public class SavedJobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SavedJobsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<PaginatedList<JobListingSummaryDto>>> GetMine(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetMySavedJobsQuery(pageNumber, pageSize)));

    [HttpGet("ids")]
    public async Task<ActionResult<List<Guid>>> GetMineIds()
        => Ok(await _mediator.Send(new GetMySavedJobIdsQuery()));

    [HttpPost("{jobId:guid}")]
    public async Task<IActionResult> Save(Guid jobId)
    {
        await _mediator.Send(new SaveJobCommand(jobId));
        return NoContent();
    }

    [HttpDelete("{jobId:guid}")]
    public async Task<IActionResult> Unsave(Guid jobId)
    {
        await _mediator.Send(new UnsaveJobCommand(jobId));
        return NoContent();
    }
}
