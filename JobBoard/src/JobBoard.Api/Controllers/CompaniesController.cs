using JobBoard.Application.Features.Companies.Commands.CreateCompany;
using JobBoard.Application.Features.Companies.Commands.UpdateCompany;
using JobBoard.Application.Features.Companies.Commands.UploadCompanyLogo;
using JobBoard.Application.Features.Companies.Queries.GetCompanies;
using JobBoard.Application.Features.Companies.Queries.GetCompanyById;
using JobBoard.Application.Features.Companies.Queries.GetEmployerAnalytics;
using JobBoard.Application.Features.Companies.Queries.GetMyCompany;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

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

    [HttpGet("mine/analytics")]
    [Authorize(Roles = "Employer")]
    public async Task<IActionResult> GetMineAnalytics()
        => Ok(await _mediator.Send(new GetEmployerAnalyticsQuery()));

    [HttpGet]
    [AllowAnonymous]
    [OutputCache(PolicyName = "PublicReads")]
    public async Task<IActionResult> GetAll([FromQuery] string? keyword, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        => Ok(await _mediator.Send(new GetCompaniesQuery(keyword, pageNumber, pageSize)));

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [OutputCache(PolicyName = "PublicReads")]
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

    [HttpPost("{id:guid}/logo")]
    [Authorize(Roles = "Employer")]
    [RequestSizeLimit(UploadCompanyLogoCommandValidator.MaxFileSizeBytes)]
    public async Task<ActionResult<string>> UploadLogo(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        await using var stream = file.OpenReadStream();
        var command = new UploadCompanyLogoCommand(id, stream, file.FileName, file.ContentType, file.Length);
        return Ok(await _mediator.Send(command, cancellationToken));
    }
}
