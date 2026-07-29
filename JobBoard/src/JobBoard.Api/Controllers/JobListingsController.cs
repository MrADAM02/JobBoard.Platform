using JobBoard.Application.Common.Models;
using JobBoard.Application.Features.Jobs.Commands.CloseJobListing;
using JobBoard.Application.Features.Jobs.Commands.CreateJobListing;
using JobBoard.Application.Features.Jobs.Commands.DeleteJobListing;
using JobBoard.Application.Features.Jobs.Commands.PublishJobListing;
using JobBoard.Application.Features.Jobs.Commands.RecordJobView;
using JobBoard.Application.Features.Jobs.Commands.UpdateJobListing;
using JobBoard.Application.Features.Jobs.Queries.GetJobListingById;
using JobBoard.Application.Features.Jobs.Queries.GetJobListings;
using JobBoard.Application.Features.Jobs.Queries.GetJobLocations;
using JobBoard.Application.Features.Jobs.Queries.GetMyJobListings;
using JobBoard.Application.Features.Jobs.Queries.GetSimilarJobs;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

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
    [OutputCache(PolicyName = "PublicReads")]
    public async Task<ActionResult<PaginatedList<JobListingSummaryDto>>> GetAll(
        [FromQuery] string? keyword, [FromQuery] string? location, [FromQuery] JobType? jobType,
        [FromQuery] bool? remoteOnly, [FromQuery] decimal? minSalary,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
    {
        var query = new GetJobListingsQuery(keyword, location, jobType, remoteOnly, minSalary, pageNumber, pageSize);
        return Ok(await _mediator.Send(query));
    }

    // Public - backs the selectable location filter on the /jobs listing page.
    [HttpGet("locations")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "PublicReads")]
    public async Task<ActionResult<List<string>>> GetLocations()
        => Ok(await _mediator.Send(new GetJobLocationsQuery()));

    // Public - backs the SSR-rendered /jobs/[id] detail page.
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "PublicReads")]
    public async Task<ActionResult<JobListingDetailDto>> GetById(Guid id)
        => Ok(await _mediator.Send(new GetJobListingByIdQuery(id)));

    // Public - backs the "similar jobs" section on the job detail page.
    [HttpGet("{id:guid}/similar")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "PublicReads")]
    public async Task<ActionResult<List<JobListingSummaryDto>>> GetSimilar(Guid id)
        => Ok(await _mediator.Send(new GetSimilarJobsQuery(id)));

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

    // The other half of "save as draft" - lets an employer publish a listing
    // that was created with PublishImmediately: false.
    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Publish(Guid id)
    {
        await _mediator.Send(new PublishJobListingCommand(id));
        return NoContent();
    }

    // Employers delete their own listings; admins can moderate/remove any
    // listing - the ownership-vs-admin check itself lives in the command handler.
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Employer,Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _mediator.Send(new DeleteJobListingCommand(id));
        return NoContent();
    }

    // Public - fired client-side from the job detail page on mount, not part of
    // the SSR fetch, so crawler/bot hits don't inflate the view count.
    [HttpPost("{id:guid}/view")]
    [AllowAnonymous]
    public async Task<IActionResult> RecordView(Guid id)
    {
        await _mediator.Send(new RecordJobViewCommand(id));
        return NoContent();
    }
}
