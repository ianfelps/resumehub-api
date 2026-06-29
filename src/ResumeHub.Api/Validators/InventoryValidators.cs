using FluentValidation;
using ResumeHub.Api.Dtos;

namespace ResumeHub.Api.Validators;

public class ExperienceRequestValidator : AbstractValidator<ExperienceRequest>
{
    public ExperienceRequestValidator()
    {
        RuleFor(x => x.Company).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Role).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("EndDate must be on or after StartDate.");
    }
}

public class ProjectRequestValidator : AbstractValidator<ProjectRequest>
{
    public ProjectRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(4000);
        RuleFor(x => x.Url).MaximumLength(500);
        RuleFor(x => x.RepoUrl).MaximumLength(500);
        RuleFor(x => x.Highlights).MaximumLength(4000);
    }
}

public class SkillRequestValidator : AbstractValidator<SkillRequest>
{
    public SkillRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Level).IsInEnum();
    }
}

public class LanguageRequestValidator : AbstractValidator<LanguageRequest>
{
    public LanguageRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Proficiency).IsInEnum();
    }
}

public class EducationRequestValidator : AbstractValidator<EducationRequest>
{
    public EducationRequestValidator()
    {
        RuleFor(x => x.Institution).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Degree).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Field).MaximumLength(200);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .When(x => x.EndDate.HasValue)
            .WithMessage("EndDate must be on or after StartDate.");
    }
}

public class CourseRequestValidator : AbstractValidator<CourseRequest>
{
    public CourseRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Provider).MaximumLength(200);
        RuleFor(x => x.CertificateUrl).MaximumLength(500);
    }
}
