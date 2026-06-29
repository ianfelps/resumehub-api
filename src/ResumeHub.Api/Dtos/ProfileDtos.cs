namespace ResumeHub.Api.Dtos;

public record ProfileRequest(string Name, string? Slug, string? Summary, bool IsPublic);

public record ProfileResponse(
    Guid Id, string Name, string Slug, string? Summary, bool IsPublic,
    DateTime CreatedAt, DateTime UpdatedAt);

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

// ---- Public assembled resume ----

public record PublicOwner(string? FullName, string? Headline, string? Location);

public record PublicResumeResponse(
    string Name,
    string? Summary,
    PublicOwner Owner,
    IReadOnlyList<ExperienceResponse> Experiences,
    IReadOnlyList<ProjectResponse> Projects,
    IReadOnlyList<SkillResponse> Skills,
    IReadOnlyList<LanguageResponse> Languages,
    IReadOnlyList<EducationResponse> Education,
    IReadOnlyList<CourseResponse> Courses);
