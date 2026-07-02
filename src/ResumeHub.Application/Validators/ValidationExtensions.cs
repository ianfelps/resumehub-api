using FluentValidation;

namespace ResumeHub.Application.Validators;

public static class ValidationExtensions
{
    /// <summary>
    /// Restricts a URL field to absolute http/https URLs. Blocks dangerous schemes
    /// (javascript:, data:, file:, …) that would otherwise be stored and later emitted
    /// as clickable links in the public resume / generated PDF. Empty/null passes —
    /// combine with NotEmpty when the field is required.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> MustBeHttpUrl<T>(
        this IRuleBuilder<T, string?> rule)
        => rule.Must(BeHttpUrl)
            .WithMessage("URL inválida: use um endereço http:// ou https://.");

    public static bool BeHttpUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    /// <summary>
    /// Enforces the password policy: 8–128 chars, with at least one letter, one digit
    /// and one special character. Each rule yields its own message so the API and the
    /// client checklist stay in sync.
    /// </summary>
    public static IRuleBuilderOptions<T, string> StrongPassword<T>(
        this IRuleBuilder<T, string> rule)
        => rule
            .MinimumLength(8).WithMessage("A senha deve ter pelo menos 8 caracteres.")
            .MaximumLength(128).WithMessage("A senha deve ter no máximo 128 caracteres.")
            .Matches("[A-Za-z]").WithMessage("A senha deve conter ao menos uma letra.")
            .Matches("[0-9]").WithMessage("A senha deve conter ao menos um número.")
            .Matches("[^A-Za-z0-9]").WithMessage("A senha deve conter ao menos um caractere especial.");

    /// <summary>Same policy as <see cref="StrongPassword"/>, as a plain predicate.</summary>
    public static bool IsStrongPassword(string? value)
        => !string.IsNullOrEmpty(value)
            && value.Length is >= 8 and <= 128
            && value.Any(char.IsLetter)
            && value.Any(char.IsDigit)
            && value.Any(c => !char.IsLetterOrDigit(c));
}
