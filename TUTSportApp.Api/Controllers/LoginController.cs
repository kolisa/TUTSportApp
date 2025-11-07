using MediatR;
using Microsoft.AspNetCore.Mvc;
using TUTSportApp.Application.Features.Auth.Commands;

namespace TUTSportApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public  class LoginController : ControllerBase
    {
        private readonly ISender _sender;

        public LoginController(ISender sender)
        {
            _sender = sender;
        }

        // POST /api/login/login  (or adjust route as needed)
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(
            [FromBody] LoginCommand request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var result = await _sender.Send(request, cancellationToken)
                                      .ConfigureAwait(false);

            if (result.IsSuccess)
            {
                return Ok(result);
            }

                return Unauthorized(result.Error);
        }
    }
}

