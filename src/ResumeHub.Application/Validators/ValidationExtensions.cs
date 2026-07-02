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
}
