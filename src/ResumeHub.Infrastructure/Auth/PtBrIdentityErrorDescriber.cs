using Microsoft.AspNetCore.Identity;

namespace ResumeHub.Infrastructure.Auth;

/// <summary>Portuguese (pt-BR) messages for ASP.NET Identity validation errors.</summary>
public class PtBrIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError DefaultError() => new()
    { Code = nameof(DefaultError), Description = "Ocorreu um erro desconhecido." };

    public override IdentityError DuplicateEmail(string email) => new()
    { Code = nameof(DuplicateEmail), Description = $"O e-mail '{email}' já está em uso." };

    public override IdentityError DuplicateUserName(string userName) => new()
    { Code = nameof(DuplicateUserName), Description = $"O usuário '{userName}' já está em uso." };

    public override IdentityError InvalidEmail(string? email) => new()
    { Code = nameof(InvalidEmail), Description = $"O e-mail '{email}' é inválido." };

    public override IdentityError InvalidUserName(string? userName) => new()
    { Code = nameof(InvalidUserName), Description = $"O usuário '{userName}' é inválido." };

    public override IdentityError PasswordTooShort(int length) => new()
    { Code = nameof(PasswordTooShort), Description = $"A senha deve ter no mínimo {length} caracteres." };

    public override IdentityError PasswordRequiresNonAlphanumeric() => new()
    {
        Code = nameof(PasswordRequiresNonAlphanumeric),
        Description = "A senha deve conter ao menos um caractere especial.",
    };

    public override IdentityError PasswordRequiresDigit() => new()
    { Code = nameof(PasswordRequiresDigit), Description = "A senha deve conter ao menos um número." };

    public override IdentityError PasswordRequiresLower() => new()
    {
        Code = nameof(PasswordRequiresLower),
        Description = "A senha deve conter ao menos uma letra minúscula.",
    };

    public override IdentityError PasswordRequiresUpper() => new()
    {
        Code = nameof(PasswordRequiresUpper),
        Description = "A senha deve conter ao menos uma letra maiúscula.",
    };

    public override IdentityError PasswordRequiresUniqueChars(int uniqueChars) => new()
    {
        Code = nameof(PasswordRequiresUniqueChars),
        Description = $"A senha deve conter ao menos {uniqueChars} caracteres diferentes.",
    };
}
