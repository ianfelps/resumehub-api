using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResumeHub.Api.Dtos;
using ResumeHub.Api.Services;

namespace ResumeHub.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public")]
public class PublicController(IProfileService service) : ControllerBase
{
    /// <summary>Assembled, publicly visible resume for a profile slug.</summary>
    [HttpGet("{slug}")]
    public async Task<ActionResult<PublicResumeResponse>> GetBySlug(string slug)
        => Ok(await service.GetPublicBySlugAsync(slug));
}
