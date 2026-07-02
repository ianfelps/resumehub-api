using Microsoft.AspNetCore.Identity;
using ResumeHub.Application.Common;
using ResumeHub.Domain.Entities;

namespace ResumeHub.Application.Account;

public interface IAccountService
{
    Task<AccountResponse> GetAsync();
    Task<AccountResponse> UpdateAsync(UpdateAccountRequest dto);
    Task ChangePasswordAsync(ChangePasswordRequest dto);
}

public class AccountService(
    UserManager<ApplicationUser> userManager,
    ICurrentUser currentUser) : IAccountService
{
    public async Task<AccountResponse> GetAsync()
        => ToResponse(await FindUserAsync());

    public async Task<AccountResponse> UpdateAsync(UpdateAccountRequest dto)
    {
        var user = await FindUserAsync();
        user.FullName = dto.FullName;
        user.Headline = dto.Headline;
        user.Location = dto.Location;
        user.PhoneNumber = dto.PhoneNumber;
        user.ShowEmailOnResume = dto.ShowEmailOnResume;
        user.LinkedInUrl = dto.LinkedInUrl;
        user.GitHubUrl = dto.GitHubUrl;
        user.WebsiteUrl = dto.WebsiteUrl;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new ConflictException(string.Join("; ", result.Errors.Select(e => e.Description)));

        return ToResponse(user);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest dto)
    {
        var user = await FindUserAsync();
        var result = await userManager.ChangePasswordAsync(
            user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw new ConflictException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    private async Task<ApplicationUser> FindUserAsync()
        => await userManager.FindByIdAsync(currentUser.Id.ToString())
            ?? throw new NotFoundException("Usuário não encontrado.");

    private static AccountResponse ToResponse(ApplicationUser u)
        => new(u.Email ?? string.Empty, u.FullName, u.Headline, u.Location,
            u.PhoneNumber, u.ShowEmailOnResume,
            u.LinkedInUrl, u.GitHubUrl, u.WebsiteUrl);
}
