using ResumeHub.Domain.Enums;

namespace ResumeHub.Api.Dtos;

// Experience
public record ExperienceRequest(
    string Company, string Role, string? Location,
    DateOnly StartDate, DateOnly? EndDate, string? Description);
public record ExperienceResponse(
    Guid Id, string Company, string Role, string? Location,
    DateOnly StartDate, DateOnly? EndDate, string? Description);

// Project
public record ProjectRequest(
    string Name, string? Description, string? Url, string? RepoUrl, string? Highlights);
public record ProjectResponse(
    Guid Id, string Name, string? Description, string? Url, string? RepoUrl, string? Highlights);

// Skill
public record SkillRequest(string Name, SkillCategory Category, SkillLevel Level);
public record SkillResponse(Guid Id, string Name, SkillCategory Category, SkillLevel Level);

// Language
public record LanguageRequest(string Name, LanguageProficiency Proficiency);
public record LanguageResponse(Guid Id, string Name, LanguageProficiency Proficiency);

// Education
public record EducationRequest(
    string Institution, string Degree, string? Field, DateOnly StartDate, DateOnly? EndDate);
public record EducationResponse(
    Guid Id, string Institution, string Degree, string? Field, DateOnly StartDate, DateOnly? EndDate);

// Course
public record CourseRequest(
    string Name, string? Provider, DateOnly? CompletionDate, string? CertificateUrl);
public record CourseResponse(
    Guid Id, string Name, string? Provider, DateOnly? CompletionDate, string? CertificateUrl);
