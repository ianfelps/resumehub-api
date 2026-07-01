using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeHub.Application.Account;

namespace ResumeHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/account")]
public class AccountController(IAccountService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<AccountResponse>> Get()
        => Ok(await service.GetAsync());

    [HttpPut]
    public async Task<ActionResult<AccountResponse>> Update(UpdateAccountRequest dto)
        => Ok(await service.UpdateAsync(dto));

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest dto)
    {
        await service.ChangePasswordAsync(dto);
        return NoContent();
    }
}
