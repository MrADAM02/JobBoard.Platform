using JobBoard.Application.Common.Models;
using JobBoard.Application.Features.Admin.Commands.ToggleUserActive;
using JobBoard.Application.Features.Admin.Queries.GetAllJobListings;
using JobBoard.Application.Features.Admin.Queries.GetPlatformStats;
using JobBoard.Application.Features.Admin.Queries.GetUsers;
using JobBoard.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator) => _mediator = mediator;

    [HttpGet("stats")]
    public async Task<ActionResult<PlatformStatsDto>> GetStats()
        => Ok(await _mediator.Send(new GetPlatformStatsQuery()));

    [HttpGet("users")]
    public async Task<ActionResult<PaginatedList<UserSummaryDto>>> GetUsers(
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetUsersQuery(pageNumber, pageSize)));

    [HttpPut("users/{id:guid}/active")]
    public async Task<IActionResult> ToggleUserActive(Guid id)
    {
        await _mediator.Send(new ToggleUserActiveCommand(id));
        return NoContent();
    }

    [HttpGet("jobs")]
    public async Task<ActionResult<PaginatedList<AdminJobListingDto>>> GetJobs(
        [FromQuery] JobStatus? status, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetAllJobListingsQuery(status, pageNumber, pageSize)));
}
