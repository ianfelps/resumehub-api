using FluentValidation;
using ResumeHub.Application.Dtos;

namespace ResumeHub.Application.Validators;

public class ProfileRequestValidator : AbstractValidator<ProfileRequest>
{
    public ProfileRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).MaximumLength(120);
        RuleFor(x => x.Headline).MaximumLength(200);
        // Summary feeds the Markdown/PDF renderer — cap it to bound render cost and storage.
        RuleFor(x => x.Summary).MaximumLength(8000);
        RuleFor(x => x.Theme).MaximumLength(50);
        RuleFor(x => x.AccentColor).MaximumLength(30);
    }
}

public class ProfileItemsRequestValidator : AbstractValidator<ProfileItemsRequest>
{
    private const int MaxItemsPerSection = 200;

    public ProfileItemsRequestValidator()
    {
        RuleFor(x => x.Experiences!).Must(BeWithinLimit).When(x => x.Experiences is not null);
        RuleFor(x => x.Projects!).Must(BeWithinLimit).When(x => x.Projects is not null);
        RuleFor(x => x.Skills!).Must(BeWithinLimit).When(x => x.Skills is not null);
        RuleFor(x => x.Languages!).Must(BeWithinLimit).When(x => x.Languages is not null);
        RuleFor(x => x.Education!).Must(BeWithinLimit).When(x => x.Education is not null);
        RuleFor(x => x.Courses!).Must(BeWithinLimit).When(x => x.Courses is not null);
    }

    private static bool BeWithinLimit(List<ProfileItemSelection> items)
        => items.Count <= MaxItemsPerSection;
}
