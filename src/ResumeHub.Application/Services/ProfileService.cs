using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ResumeHub.Application.Abstractions;
using ResumeHub.Application.Common;
using ResumeHub.Application.Dtos;
using ResumeHub.Domain.Enums;
using ResumeHub.Domain.Entities;
using System.Text;
using System.Text.RegularExpressions;

namespace ResumeHub.Application.Services;

public interface IProfileService
{
    Task<IReadOnlyList<ProfileResponse>> GetAllAsync();
    Task<ProfileResponse> GetByIdAsync(Guid id);
    Task<ProfileResponse> CreateAsync(ProfileRequest dto);
    Task<ProfileResponse> UpdateAsync(Guid id, ProfileRequest dto);
    Task DeleteAsync(Guid id);
    Task SetItemsAsync(Guid id, ProfileItemsRequest dto);
    Task<ProfileItemsResponse> GetItemsAsync(Guid id);
    Task<PublicResumeResponse> GetPublicBySlugAsync(string slug);
    Task<ProfilePdfResult> GeneratePdfAsync(Guid id);
}

public record ProfilePdfResult(string FileName, byte[] Content, int PageCount);

public class ProfileService(IApplicationDbContext db, ICurrentUser currentUser) : IProfileService
{
    public async Task<IReadOnlyList<ProfileResponse>> GetAllAsync()
    {
        var profiles = await db.Profiles
            .Where(p => p.UserId == currentUser.Id)
            .OrderByDescending(p => p.UpdatedAt)
            .AsNoTracking()
            .ToListAsync();
        return profiles.Select(ToResponse).ToList();
    }

    public async Task<ProfileResponse> GetByIdAsync(Guid id)
        => ToResponse(await FindOwnedAsync(id));

    public async Task<ProfileResponse> CreateAsync(ProfileRequest dto)
    {
        var slug = await EnsureUniqueSlugAsync(
            SlugGenerator.Slugify(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug),
            excludeProfileId: null);

        var profile = new Profile
        {
            UserId = currentUser.Id,
            Name = dto.Name,
            Slug = slug,
            Headline = dto.Headline,
            Summary = dto.Summary,
            IsPublic = dto.IsPublic,
            Theme = NormalizeTheme(dto.Theme),
            AccentColor = NormalizeAccent(dto.AccentColor)
        };

        db.Profiles.Add(profile);
        await db.SaveChangesAsync();
        return ToResponse(profile);
    }

    public async Task<ProfileResponse> UpdateAsync(Guid id, ProfileRequest dto)
    {
        var profile = await FindOwnedAsync(id);

        var desiredSlug = SlugGenerator.Slugify(string.IsNullOrWhiteSpace(dto.Slug) ? dto.Name : dto.Slug);
        if (desiredSlug != profile.Slug)
            profile.Slug = await EnsureUniqueSlugAsync(desiredSlug, excludeProfileId: profile.Id);

        profile.Name = dto.Name;
        profile.Headline = dto.Headline;
        profile.Summary = dto.Summary;
        profile.IsPublic = dto.IsPublic;
        profile.Theme = NormalizeTheme(dto.Theme);
        profile.AccentColor = NormalizeAccent(dto.AccentColor);

        await db.SaveChangesAsync();
        return ToResponse(profile);
    }

    public async Task DeleteAsync(Guid id)
    {
        var profile = await FindOwnedAsync(id);
        db.Profiles.Remove(profile);
        await db.SaveChangesAsync();
    }

    public async Task SetItemsAsync(Guid id, ProfileItemsRequest dto)
    {
        var profile = await db.Profiles
            .Include(p => p.Experiences)
            .Include(p => p.Projects)
            .Include(p => p.Skills)
            .Include(p => p.Languages)
            .Include(p => p.Education)
            .Include(p => p.Courses)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id)
            ?? throw new NotFoundException($"Perfil '{id}' não encontrado.");

        await ValidateOwnedIdsAsync(dto);

        profile.Experiences = Map(dto.Experiences, s => new ProfileExperience
        { ProfileId = id, ExperienceId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Projects = Map(dto.Projects, s => new ProfileProject
        { ProfileId = id, ProjectId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Skills = Map(dto.Skills, s => new ProfileSkill
        { ProfileId = id, SkillId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Languages = Map(dto.Languages, s => new ProfileLanguage
        { ProfileId = id, LanguageId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Education = Map(dto.Education, s => new ProfileEducation
        { ProfileId = id, EducationId = s.Id, DisplayOrder = s.DisplayOrder });
        profile.Courses = Map(dto.Courses, s => new ProfileCourse
        { ProfileId = id, CourseId = s.Id, DisplayOrder = s.DisplayOrder });

        await db.SaveChangesAsync();
    }

    public async Task<ProfileItemsResponse> GetItemsAsync(Guid id)
    {
        var profile = await db.Profiles
            .AsNoTracking()
            .Include(p => p.Experiences)
            .Include(p => p.Projects)
            .Include(p => p.Skills)
            .Include(p => p.Languages)
            .Include(p => p.Education)
            .Include(p => p.Courses)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id)
            ?? throw new NotFoundException($"Perfil '{id}' não encontrado.");

        return new ProfileItemsResponse(
            profile.Experiences.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.ExperienceId, x.DisplayOrder)).ToList(),
            profile.Projects.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.ProjectId, x.DisplayOrder)).ToList(),
            profile.Skills.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.SkillId, x.DisplayOrder)).ToList(),
            profile.Languages.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.LanguageId, x.DisplayOrder)).ToList(),
            profile.Education.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.EducationId, x.DisplayOrder)).ToList(),
            profile.Courses.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProfileItemSelection(x.CourseId, x.DisplayOrder)).ToList());
    }

    public async Task<PublicResumeResponse> GetPublicBySlugAsync(string slug)
    {
        var profile = await db.Profiles
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Experiences).ThenInclude(x => x.Experience)
            .Include(p => p.Projects).ThenInclude(x => x.Project)
            .Include(p => p.Skills).ThenInclude(x => x.Skill)
            .Include(p => p.Languages).ThenInclude(x => x.Language)
            .Include(p => p.Education).ThenInclude(x => x.Education)
            .Include(p => p.Courses).ThenInclude(x => x.Course)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublic)
            ?? throw new NotFoundException($"Perfil público '{slug}' não encontrado.");

        return new PublicResumeResponse(
            profile.Name,
            profile.Summary,
            profile.Theme,
            profile.AccentColor,
            new PublicOwner(
                profile.User?.FullName,
                profile.Headline ?? profile.User?.Headline,
                profile.User?.Location,
                profile.User?.ShowEmailOnResume == true ? profile.User?.Email : null,
                profile.User?.PhoneNumber,
                profile.User?.LinkedInUrl,
                profile.User?.GitHubUrl,
                profile.User?.WebsiteUrl),
            profile.Experiences.OrderBy(x => x.DisplayOrder)
                .Select(x => new ExperienceResponse(x.Experience!.Id, x.Experience.Company,
                    x.Experience.Role, x.Experience.Location, x.Experience.StartDate,
                    x.Experience.EndDate, x.Experience.Description)).ToList(),
            profile.Projects.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProjectResponse(x.Project!.Id, x.Project.Name,
                    x.Project.Description, x.Project.Url, x.Project.RepoUrl, x.Project.Highlights)).ToList(),
            profile.Skills.OrderBy(x => x.DisplayOrder)
                .Select(x => new SkillResponse(x.Skill!.Id, x.Skill.Name, x.Skill.Category, x.Skill.Level)).ToList(),
            profile.Languages.OrderBy(x => x.DisplayOrder)
                .Select(x => new LanguageResponse(x.Language!.Id, x.Language.Name, x.Language.Proficiency)).ToList(),
            profile.Education.OrderBy(x => x.DisplayOrder)
                .Select(x => new EducationResponse(x.Education!.Id, x.Education.Institution,
                    x.Education.Degree, x.Education.Field, x.Education.StartDate, x.Education.EndDate)).ToList(),
            profile.Courses.OrderBy(x => x.DisplayOrder)
                .Select(x => new CourseResponse(x.Course!.Id, x.Course.Name, x.Course.Provider,
                    x.Course.CompletionDate, x.Course.CertificateUrl)).ToList());
    }

    public async Task<ProfilePdfResult> GeneratePdfAsync(Guid id)
    {
        var resume = await GetOwnedResumeAsync(id);
        var bytes = BuildPdf(resume);
        return new ProfilePdfResult($"{SlugGenerator.Slugify(resume.Name)}.pdf", bytes, CountPdfPages(bytes));
    }

    // ---- helpers ----

    private async Task<PublicResumeResponse> GetOwnedResumeAsync(Guid id)
    {
        var profile = await db.Profiles
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Experiences).ThenInclude(x => x.Experience)
            .Include(p => p.Projects).ThenInclude(x => x.Project)
            .Include(p => p.Skills).ThenInclude(x => x.Skill)
            .Include(p => p.Languages).ThenInclude(x => x.Language)
            .Include(p => p.Education).ThenInclude(x => x.Education)
            .Include(p => p.Courses).ThenInclude(x => x.Course)
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id)
            ?? throw new NotFoundException($"Perfil '{id}' nao encontrado.");

        return new PublicResumeResponse(
            profile.Name,
            profile.Summary,
            profile.Theme,
            profile.AccentColor,
            new PublicOwner(
                profile.User?.FullName,
                profile.Headline ?? profile.User?.Headline,
                profile.User?.Location,
                profile.User?.ShowEmailOnResume == true ? profile.User?.Email : null,
                profile.User?.PhoneNumber,
                profile.User?.LinkedInUrl,
                profile.User?.GitHubUrl,
                profile.User?.WebsiteUrl),
            profile.Experiences.OrderBy(x => x.DisplayOrder)
                .Select(x => new ExperienceResponse(x.Experience!.Id, x.Experience.Company,
                    x.Experience.Role, x.Experience.Location, x.Experience.StartDate,
                    x.Experience.EndDate, x.Experience.Description)).ToList(),
            profile.Projects.OrderBy(x => x.DisplayOrder)
                .Select(x => new ProjectResponse(x.Project!.Id, x.Project.Name,
                    x.Project.Description, x.Project.Url, x.Project.RepoUrl, x.Project.Highlights)).ToList(),
            profile.Skills.OrderBy(x => x.DisplayOrder)
                .Select(x => new SkillResponse(x.Skill!.Id, x.Skill.Name, x.Skill.Category, x.Skill.Level)).ToList(),
            profile.Languages.OrderBy(x => x.DisplayOrder)
                .Select(x => new LanguageResponse(x.Language!.Id, x.Language.Name, x.Language.Proficiency)).ToList(),
            profile.Education.OrderBy(x => x.DisplayOrder)
                .Select(x => new EducationResponse(x.Education!.Id, x.Education.Institution,
                    x.Education.Degree, x.Education.Field, x.Education.StartDate, x.Education.EndDate)).ToList(),
            profile.Courses.OrderBy(x => x.DisplayOrder)
                .Select(x => new CourseResponse(x.Course!.Id, x.Course.Name, x.Course.Provider,
                    x.Course.CompletionDate, x.Course.CertificateUrl)).ToList());
    }

    private static byte[] BuildPdf(PublicResumeResponse resume)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(42);
                page.DefaultTextStyle(x => x.FontSize(9.5f).LineHeight(1.25f));
                page.Content().Column(column =>
                {
                    Header(column, resume);
                    Markdown(column, resume.Summary);
                    Section(column, "Experiência", resume.Experiences, (section, e) =>
                    {
                        Entry(section, $"{e.Role} - {e.Company}", FormatPeriod(e.StartDate, e.EndDate), e.Location);
                        Markdown(section, e.Description);
                    });
                    Section(column, "Projetos", resume.Projects, (section, p) =>
                    {
                        Entry(section, p.Name, null, p.Url);
                        Markdown(section, p.Description);
                        Markdown(section, p.Highlights);
                    });
                    Section(column, "Formação", resume.Education, (section, e) =>
                        Entry(section, e.Degree, FormatPeriod(e.StartDate, e.EndDate),
                            string.Join(" - ", new[] { e.Institution, e.Field }.Where(NotBlank))));
                    SkillsSection(column, resume.Skills);
                    Section(column, "Idiomas", resume.Languages, (section, l) =>
                        section.Item().Text($"{l.Name} - {LanguageLabel(l.Proficiency)}").FontSize(9));
                    Section(column, "Cursos", resume.Courses, (section, c) =>
                        Entry(section, c.Name,
                            c.CompletionDate.HasValue ? MonthYear(c.CompletionDate.Value) : null, c.Provider));
                });
            });
        });

        document = document.WithMetadata(new DocumentMetadata
        {
            Title = resume.Owner.Headline ?? resume.Name,
            Author = resume.Owner.FullName ?? resume.Name,
            Keywords = string.Join(", ", resume.Skills.Select(s => s.Name)),
            Subject = resume.Summary ?? string.Empty
        });

        return WithCategory(document.GeneratePdf(), "Currículo");
    }

    // QuestPDF's DocumentMetadata has no Category field, so inject a /Category
    // entry into the PDF's Info dictionary after generation. Only runs when the
    // file uses a classic (uncompressed) xref table + trailer, which we can
    // safely rebuild; otherwise the PDF is returned untouched to avoid corruption.
    private static byte[] WithCategory(byte[] pdf, string category)
    {
        var text = Encoding.Latin1.GetString(pdf);

        var trailerIdx = text.LastIndexOf("trailer", StringComparison.Ordinal);
        var xrefIdx = text.LastIndexOf("\nxref", StringComparison.Ordinal);
        if (trailerIdx < 0 || xrefIdx < 0) return pdf; // cross-reference stream: bail

        // Find the trailer's /Info reference, e.g. "/Info 3 0 R".
        var infoRef = Regex.Match(text, @"/Info\s+(\d+)\s+(\d+)\s+R");
        if (!infoRef.Success) return pdf;

        var objNum = infoRef.Groups[1].Value;
        var gen = infoRef.Groups[2].Value;

        // Locate the Info object body: "<objNum> <gen> obj << ... >>".
        var objStart = Regex.Match(text, $@"(?m)^{objNum}\s+{gen}\s+obj\b");
        if (!objStart.Success) return pdf;

        var dictEnd = text.IndexOf(">>", objStart.Index, StringComparison.Ordinal);
        if (dictEnd < 0 || dictEnd > xrefIdx) return pdf;

        var escaped = category.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
        var withCategory = text[..dictEnd] + $"/Category ({escaped})" + text[dictEnd..];

        // The insertion shifted every object offset after it, so rebuild the
        // xref table and repoint startxref.
        return RebuildXref(withCategory);
    }

    // Regenerates the classic xref table and startxref pointer from the object
    // offsets present in the text. Caller guarantees a classic xref/trailer.
    private static byte[] RebuildXref(string text)
    {
        var trailerIdx = text.LastIndexOf("trailer", StringComparison.Ordinal);
        var xrefIdx = text.LastIndexOf("\nxref", StringComparison.Ordinal);

        var objs = Regex.Matches(text, @"(?m)^(\d+)\s+(\d+)\s+obj\b")
            .Select(m => (Num: int.Parse(m.Groups[1].Value), Offset: m.Index))
            .OrderBy(o => o.Num)
            .ToList();

        var maxNum = objs.Max(o => o.Num);
        var offsets = new int[maxNum + 1];
        foreach (var o in objs) offsets[o.Num] = o.Offset;

        var newXrefOffset = xrefIdx + 1; // skip the leading newline

        var sb = new StringBuilder();
        sb.Append("xref\n");
        sb.Append($"0 {maxNum + 1}\n");
        sb.Append("0000000000 65535 f \n");
        for (var i = 1; i <= maxNum; i++)
            sb.Append($"{offsets[i]:D10} 00000 n \n");

        // Point startxref (inside the trailer) at the rebuilt table.
        var trailer = Regex.Replace(text[trailerIdx..], @"startxref\s+\d+", $"startxref\n{newXrefOffset}");

        return Encoding.Latin1.GetBytes(text[..(xrefIdx + 1)] + sb + trailer);
    }

    // Counts page objects in the generated PDF. QuestPDF's PDF backend writes
    // uncompressed page dictionaries, so each page shows up as "/Type /Page"
    // (distinct from the single "/Type /Pages" tree node).
    private static int CountPdfPages(byte[] pdf)
    {
        var text = Encoding.Latin1.GetString(pdf);
        var count = Regex.Matches(text, @"/Type\s*/Page(?![sA-Za-z])").Count;
        return Math.Max(1, count);
    }

    private static void Header(ColumnDescriptor column, PublicResumeResponse resume)
    {
        column.Item().Text(resume.Owner.FullName ?? resume.Name)
            .FontSize(20)
            .Bold()
            .FontColor(Colors.Grey.Darken4);

        if (NotBlank(resume.Owner.Headline))
            column.Item().PaddingTop(2).Text(resume.Owner.Headline!).FontSize(10.5f).SemiBold();

        var contacts = new[]
        {
            resume.Owner.Location,
            resume.Owner.PhoneNumber,
            resume.Owner.Email,
            resume.Owner.LinkedInUrl,
            resume.Owner.GitHubUrl,
            resume.Owner.WebsiteUrl
        }.Where(NotBlank);

        var contactLine = string.Join("  |  ", contacts);
        if (NotBlank(contactLine))
            column.Item().PaddingTop(4).Text(contactLine).FontSize(8.5f).FontColor(Colors.Grey.Darken1);

        column.Item().PaddingTop(10).LineHorizontal(0.75f).LineColor(Colors.Grey.Lighten2);
    }

    private static void Section<T>(
        ColumnDescriptor column,
        string title,
        IReadOnlyList<T> items,
        Action<ColumnDescriptor, T> content)
    {
        if (items.Count == 0) return;

        SectionHeader(column, title);
        foreach (var item in items)
        {
            column.Item().PaddingTop(8).Column(section => content(section, item));
        }
    }

    private static void SectionHeader(ColumnDescriptor column, string title)
    {
        column.Item().PaddingTop(14).Text(title.ToUpperInvariant())
            .FontSize(9)
            .Bold()
            .FontColor(Colors.Blue.Darken2)
            .LetterSpacing(0.8f);

        column.Item().PaddingTop(3).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
    }

    // Skills use a two-column grid to make better use of the page width.
    private static void SkillsSection(ColumnDescriptor column, IReadOnlyList<SkillResponse> skills)
    {
        if (skills.Count == 0) return;

        SectionHeader(column, "Habilidades");

        var half = (skills.Count + 1) / 2;
        var left = skills.Take(half).ToList();
        var right = skills.Skip(half).ToList();

        column.Item().PaddingTop(8).Row(row =>
        {
            row.RelativeItem().Column(col => { foreach (var s in left) SkillLine(col, s); });
            row.ConstantItem(18);
            row.RelativeItem().Column(col => { foreach (var s in right) SkillLine(col, s); });
        });
    }

    private static void SkillLine(ColumnDescriptor column, SkillResponse skill)
        => column.Item().PaddingBottom(2).Text($"{skill.Name} - {SkillLevelLabel(skill.Level)}").FontSize(9);

    private static void Entry(ColumnDescriptor column, string title, string? period, string? subtitle)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem().Text(title).FontSize(10).Bold().FontColor(Colors.Grey.Darken4);
            if (NotBlank(period))
                row.AutoItem().Text(period!).FontSize(8).FontColor(Colors.Grey.Darken1);
        });

        if (NotBlank(subtitle))
            column.Item().PaddingTop(1).Text(subtitle!).FontSize(8.5f).FontColor(Colors.Grey.Darken1);
    }

    // Renders a lightweight subset of Markdown (headings, bullet lists, bold,
    // italic, inline code and links) into QuestPDF rich text.
    private static void Markdown(ColumnDescriptor column, string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return;

        var lines = raw.Replace("\r\n", "\n").Replace("\r", "\n").Trim().Split('\n');
        var paragraph = new List<string>();

        void FlushParagraph()
        {
            if (paragraph.Count == 0) return;
            var text = string.Join(" ", paragraph);
            paragraph.Clear();
            column.Item().PaddingTop(4).Text(line =>
            {
                line.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken2));
                RenderInline(line, text);
            });
        }

        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.Length == 0) { FlushParagraph(); continue; }

            var heading = Regex.Match(trimmed, @"^#{1,6}\s+(.*)$");
            var bullet = Regex.Match(trimmed, @"^[-+*]\s+(.*)$");

            if (heading.Success)
            {
                FlushParagraph();
                column.Item().PaddingTop(6).Text(text =>
                {
                    text.DefaultTextStyle(x => x.FontSize(10.5f).SemiBold().FontColor(Colors.Grey.Darken3));
                    RenderInline(text, heading.Groups[1].Value);
                });
            }
            else if (bullet.Success)
            {
                FlushParagraph();
                var content = bullet.Groups[1].Value;
                column.Item().PaddingTop(2).Row(row =>
                {
                    row.ConstantItem(12).Text("•").FontSize(9).FontColor(Colors.Grey.Darken2);
                    row.RelativeItem().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9).FontColor(Colors.Grey.Darken2));
                        RenderInline(text, content);
                    });
                });
            }
            else
            {
                paragraph.Add(trimmed);
            }
        }

        FlushParagraph();
    }

    // Parses inline Markdown spans left-to-right and emits styled QuestPDF spans.
    private static void RenderInline(TextDescriptor text, string content)
    {
        var plain = new StringBuilder();
        void FlushPlain()
        {
            if (plain.Length == 0) return;
            text.Span(plain.ToString());
            plain.Clear();
        }

        var i = 0;
        while (i < content.Length)
        {
            var rest = content.Substring(i);
            Match m;

            if ((m = Regex.Match(rest, @"^\[([^\]]+)\]\(([^)]+)\)")).Success)
            {
                FlushPlain();
                text.Hyperlink(m.Groups[1].Value, m.Groups[2].Value)
                    .FontColor(Colors.Blue.Medium).Underline();
            }
            else if ((m = Regex.Match(rest, @"^\*\*([^*]+)\*\*|^__([^_]+)__")).Success)
            {
                FlushPlain();
                text.Span(m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value).Bold();
            }
            else if ((m = Regex.Match(rest, @"^\*([^*]+)\*|^_([^_]+)_")).Success)
            {
                FlushPlain();
                text.Span(m.Groups[1].Success ? m.Groups[1].Value : m.Groups[2].Value).Italic();
            }
            else if ((m = Regex.Match(rest, @"^`([^`]+)`")).Success)
            {
                FlushPlain();
                text.Span(m.Groups[1].Value).FontFamily("Courier New");
            }
            else
            {
                plain.Append(content[i]);
                i++;
                continue;
            }

            i += m.Length;
        }

        FlushPlain();
    }

    private static readonly string[] PtMonths =
    {
        "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho",
        "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro"
    };

    private static string MonthYear(DateOnly date) => $"{PtMonths[date.Month - 1]} de {date.Year}";

    private static string FormatPeriod(DateOnly start, DateOnly? end)
        => $"{MonthYear(start)} - {(end.HasValue ? MonthYear(end.Value) : "Atual")}";

    private static string SkillLevelLabel(SkillLevel level) => level switch
    {
        SkillLevel.Beginner => "Iniciante",
        SkillLevel.Intermediate => "Intermediário",
        SkillLevel.Advanced => "Avançado",
        SkillLevel.Expert => "Especialista",
        _ => level.ToString()
    };

    private static string LanguageLabel(LanguageProficiency level) => level switch
    {
        LanguageProficiency.Basic => "Básico",
        LanguageProficiency.Intermediate => "Intermediário",
        LanguageProficiency.Advanced => "Avançado",
        LanguageProficiency.Fluent => "Fluente",
        LanguageProficiency.Native => "Nativo",
        _ => level.ToString()
    };

    private static bool NotBlank(string? value) => !string.IsNullOrWhiteSpace(value);

    private static List<T> Map<T>(List<ProfileItemSelection>? items, Func<ProfileItemSelection, T> map)
        => items?.Select(map).ToList() ?? [];

    private async Task ValidateOwnedIdsAsync(ProfileItemsRequest dto)
    {
        await AssertOwnedAsync(db.Experiences, dto.Experiences, "experiência");
        await AssertOwnedAsync(db.Projects, dto.Projects, "projetos");
        await AssertOwnedAsync(db.Skills, dto.Skills, "habilidades");
        await AssertOwnedAsync(db.Languages, dto.Languages, "idiomas");
        await AssertOwnedAsync(db.Education, dto.Education, "formação");
        await AssertOwnedAsync(db.Courses, dto.Courses, "cursos");
    }

    private async Task AssertOwnedAsync<TEntity>(
        DbSet<TEntity> set, List<ProfileItemSelection>? selections, string label)
        where TEntity : OwnedEntity
    {
        if (selections is null || selections.Count == 0) return;

        var ids = selections.Select(s => s.Id).Distinct().ToList();
        var ownedCount = await set.CountAsync(e => ids.Contains(e.Id) && e.UserId == currentUser.Id);
        if (ownedCount != ids.Count)
            throw new NotFoundException(
                $"Um ou mais itens de {label} não foram encontrados no seu inventário.");
    }

    private async Task<string> EnsureUniqueSlugAsync(string baseSlug, Guid? excludeProfileId)
    {
        var slug = baseSlug;
        var suffix = 1;
        while (await db.Profiles.AnyAsync(p =>
            p.Slug == slug && (excludeProfileId == null || p.Id != excludeProfileId)))
        {
            slug = $"{baseSlug}-{++suffix}";
        }
        return slug;
    }

    private async Task<Profile> FindOwnedAsync(Guid id)
        => await db.Profiles.FirstOrDefaultAsync(p => p.Id == id && p.UserId == currentUser.Id)
            ?? throw new NotFoundException($"Perfil '{id}' não encontrado.");

    private static ProfileResponse ToResponse(Profile p) =>
        new(p.Id, p.Name, p.Slug, p.Headline, p.Summary, p.IsPublic,
            p.Theme, p.AccentColor, p.CreatedAt, p.UpdatedAt);

    private static string NormalizeTheme(string? theme)
        => theme == "light" ? "light" : "dark";

    private static string NormalizeAccent(string? accent)
        => accent is not null && System.Text.RegularExpressions.Regex.IsMatch(
            accent, "^#(?:[0-9a-fA-F]{3}|[0-9a-fA-F]{6})$")
            ? accent
            : "#5b8cff";
}
