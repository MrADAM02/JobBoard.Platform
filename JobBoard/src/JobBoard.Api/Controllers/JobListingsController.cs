using JobBoard.Application.Common.Models;
using JobBoard.Application.Features.Jobs.Commands.CloseJobListing;
using JobBoard.Application.Features.Jobs.Commands.CreateJobListing;
using JobBoard.Application.Features.Jobs.Commands.DeleteJobListing;
using JobBoard.Application.Features.Jobs.Commands.UpdateJobListing;
using JobBoard.Application.Features.Jobs.Queries.GetJobListingById;
using JobBoard.Application.Features.Jobs.Queries.GetJobListings;
using JobBoard.Application.Features.Jobs.Queries.GetMyJobListings;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api/jobs")]
public class JobListingsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobListingsController(IMediator mediator) => _mediator = mediator;

    // Public - backs the SSR-rendered /jobs listing page on the Nuxt frontend.
    [HttpGet("mine")]
    [Authorize(Roles = "Employer")]
    public async Task<ActionResult<PaginatedList<MyJobListingDto>>> GetMine(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetMyJobListingsQuery(pageNumber, pageSize)));

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<PaginatedList<JobListingSummaryDto>>> GetAll(
        [FromQuery] string? keyword, [FromQuery] string? location, [FromQuery] JobType? jobType,
        [FromQuery] bool? remoteOnly, [FromQuery] decimal? minSalary,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetJobListingsQuery(keyword, location, jobType, remoteOnly, minSalary, pageNumber, pageSize);
        return Ok(await _mediator.Send(query));
    }

    // Public - backs the SSR-rendered /jobs/[id] detail page.
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<ActionResult<JobListingDetailDto>> GetById(Guid id)
        => Ok(await _mediator.Send(new GetJobListingByIdQuery(id)));

    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<ActionResult<Guid>> Create(CreateJobListingCommand command)
        => Ok(await _mediator.Send(command));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Update(Guid id, UpdateJobListingCommand command)
    {
        if (id != command.Id) return BadRequest("Route id and body id must match.");
        await _mediator.Send(command);
        return NoContent();
    }

    [HttpPost("{id:guid}/close")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Close(Guid id)
    {
        await _mediator.Send(new CloseJobListingCommand(id));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteJobListingCommand(id));
        return NoContent();
    }
}
