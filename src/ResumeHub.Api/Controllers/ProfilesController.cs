using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeHub.Application.Dtos;
using ResumeHub.Application.Services;

namespace ResumeHub.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/profiles")]
public class ProfilesController(IProfileService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProfileResponse>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProfileResponse>> GetById(Guid id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<ProfileResponse>> Create(ProfileRequest dto)
        => StatusCode(StatusCodes.Status201Created, await service.CreateAsync(dto));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProfileResponse>> Update(Guid id, ProfileRequest dto)
        => Ok(await service.UpdateAsync(id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/items")]
    public async Task<ActionResult<ProfileItemsResponse>> GetItems(Guid id)
        => Ok(await service.GetItemsAsync(id));

    [HttpPut("{id:guid}/items")]
    public async Task<IActionResult> SetItems(Guid id, ProfileItemsRequest dto)
    {
        await service.SetItemsAsync(id, dto);
        return NoContent();
    }
}
