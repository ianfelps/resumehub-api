using ResumeHub.Application.Abstractions;
using ResumeHub.Application.Common;
using ResumeHub.Domain.Entities;

namespace ResumeHub.Application.Account;

public interface IAccountService
{
    Task<AccountResponse> GetAsync();
    Task<AccountResponse> UpdateAsync(UpdateAccountRequest dto);
    Task ChangePasswordAsync(ChangePasswordRequest dto);
    Task DeleteAsync(DeleteAccountRequest dto);
}

public class AccountService(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
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

        await db.SaveChangesAsync();
        return ToResponse(user);
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest dto)
    {
        var user = await FindUserAsync();

        if (passwordHasher.Verify(user.PasswordHash, dto.CurrentPassword) == PasswordVerify.Failed)
            throw new ConflictException("Senha atual incorreta.");

        user.PasswordHash = passwordHasher.Hash(dto.NewPassword);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(DeleteAccountRequest dto)
    {
        var user = await FindUserAsync();

        if (passwordHasher.Verify(user.PasswordHash, dto.Password) == PasswordVerify.Failed)
            throw new ConflictException("Senha incorreta.");

        // FK cascade removes the whole inventory, profiles and their join rows.
        db.Users.Remove(user);
        await db.SaveChangesAsync();
    }

    private async Task<User> FindUserAsync()
        => await db.Users.FindAsync(currentUser.Id)
            ?? throw new NotFoundException("Usuário não encontrado.");

    private static AccountResponse ToResponse(User u)
        => new(u.Email, u.FullName, u.Headline, u.Location,
            u.PhoneNumber, u.ShowEmailOnResume,
            u.LinkedInUrl, u.GitHubUrl, u.WebsiteUrl);
}
