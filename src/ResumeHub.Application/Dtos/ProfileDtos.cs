namespace ResumeHub.Application.Dtos;

public record ProfileRequest(
    string Name, string? Slug, string? Headline, string? Summary, bool IsPublic,
    string? Theme, string? AccentColor);

public record ProfileResponse(
    Guid Id, string Name, string Slug, string? Headline, string? Summary, bool IsPublic,
    string Theme, string AccentColor, DateTime CreatedAt, DateTime UpdatedAt);

/// <summary>A single inventory item selected into a profile, with its display order.</summary>
public record ProfileItemSelection(Guid Id, int DisplayOrder);

/// <summary>Full replacement of a profile's selected items, per inventory type.</summary>
public record ProfileItemsRequest(
    List<ProfileItemSelection>? Experiences,
    List<ProfileItemSelection>? Projects,
    List<ProfileItemSelection>? Skills,
    List<ProfileItemSelection>? Languages,
    List<ProfileItemSelection>? Education,
    List<ProfileItemSelection>? Courses);

/// <summary>A profile's currently selected items, per inventory type (read-back).</summary>
public record ProfileItemsResponse(
    IReadOnlyList<ProfileItemSelection> Experiences,
    IReadOnlyList<ProfileItemSelection> Projects,
    IReadOnlyList<ProfileItemSelection> Skills,
    IReadOnlyList<ProfileItemSelection> Languages,
    IReadOnlyList<ProfileItemSelection> Education,
    IReadOnlyList<ProfileItemSelection> Courses);

// ---- Public assembled resume ----

public record PublicOwner(
    string? FullName, string? Headline, string? Location,
    string? Email, string? PhoneNumber,
    string? LinkedInUrl, string? GitHubUrl, string? WebsiteUrl);

public record PublicResumeResponse(
    string Name,
    string? Summary,
    string Theme,
    string AccentColor,
    PublicOwner Owner,
    IReadOnlyList<ExperienceResponse> Experiences,
    IReadOnlyList<ProjectResponse> Projects,
    IReadOnlyList<SkillResponse> Skills,
    IReadOnlyList<LanguageResponse> Languages,
    IReadOnlyList<EducationResponse> Education,
    IReadOnlyList<CourseResponse> Courses);
