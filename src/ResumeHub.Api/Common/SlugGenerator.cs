using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ResumeHub.Api.Common;

public static partial class SlugGenerator
{
    /// <summary>Normalizes text into a URL-safe slug (lowercase, hyphenated, ASCII).</summary>
    public static string Slugify(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Guid.NewGuid().ToString("n")[..8];

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = NonAlphanumeric().Replace(slug, "-").Trim('-');
        slug = MultiHyphen().Replace(slug, "-");

        return string.IsNullOrEmpty(slug) ? Guid.NewGuid().ToString("n")[..8] : slug;
    }

    [GeneratedRegex("[^a-z0-9]+")]
    private static partial Regex NonAlphanumeric();

    [GeneratedRegex("-{2,}")]
    private static partial Regex MultiHyphen();
}
