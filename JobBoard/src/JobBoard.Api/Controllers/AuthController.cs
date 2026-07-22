using JobBoard.Application.Features.Auth.Commands.Login;
using JobBoard.Application.Features.Auth.Commands.RefreshToken;
using JobBoard.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace JobBoard.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator) => _mediator = mediator;

    [HttpPost("register")]
    public async Task<ActionResult<AuthResultDto>> Register(RegisterCommand command)
        => Ok(await _mediator.Send(command));

    [HttpPost("login")]
    public async Task<ActionResult<AuthResultDto>> Login(LoginCommand command)
        => Ok(await _mediator.Send(command));

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResultDto>> Refresh(RefreshTokenCommand command)
        => Ok(await _mediator.Send(command));
}
