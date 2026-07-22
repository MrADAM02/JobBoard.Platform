using JobBoard.Application.Features.Companies.Commands.CreateCompany;
using JobBoard.Application.Features.Companies.Commands.UpdateCompany;
using JobBoard.Application.Features.Companies.Queries.GetCompanyById;
using JobBoard.Application.Features.Companies.Queries.GetMyCompany;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api/companies")]
public class CompaniesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CompaniesController(IMediator mediator) => _mediator = mediator;

    [HttpGet("mine")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetMine()
        => Ok(await _mediator.Send(new GetMyCompanyQuery()));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
        => Ok(await _mediator.Send(new GetCompanyByIdQuery(id)));

    [HttpPost]
    [Authorize(Roles = "Employer")]
    public async Task<ActionResult<Guid>> Create(CreateCompanyCommand command)
        => Ok(await _mediator.Send(command));

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> Update(Guid id, UpdateCompanyCommand command)
    {
        if (id != command.Id) return BadRequest("Route id and body id must match.");
        await _mediator.Send(command);
        return NoContent();
    }
}
