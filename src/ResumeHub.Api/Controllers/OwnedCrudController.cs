using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeHub.Application.Services;

namespace ResumeHub.Api.Controllers;

/// <summary>
/// Generic REST controller for user-scoped inventory items.
/// Derived controllers only declare their route and DTO types.
/// </summary>
[ApiController]
[Authorize]
public abstract class OwnedCrudController<TCreate, TUpdate, TResponse>(
    IOwnedCrudService<TCreate, TUpdate, TResponse> service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TResponse>>> GetAll()
        => Ok(await service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TResponse>> GetById(Guid id)
        => Ok(await service.GetByIdAsync(id));

    [HttpPost]
    public async Task<ActionResult<TResponse>> Create(TCreate dto)
    {
        var created = await service.CreateAsync(dto);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TResponse>> Update(Guid id, TUpdate dto)
        => Ok(await service.UpdateAsync(id, dto));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}
