using FluentValidation;
using ResumeHub.Application.Account;

namespace ResumeHub.Application.Validators;

public class UpdateAccountRequestValidator : AbstractValidator<UpdateAccountRequest>
{
    public UpdateAccountRequestValidator()
    {
        RuleFor(x => x.FullName).MaximumLength(200);
        RuleFor(x => x.Headline).MaximumLength(200);
        RuleFor(x => x.Location).MaximumLength(200);
        RuleFor(x => x.PhoneNumber).MaximumLength(40);
        RuleFor(x => x.LinkedInUrl).MaximumLength(300).MustBeHttpUrl();
        RuleFor(x => x.GitHubUrl).MaximumLength(300).MustBeHttpUrl();
        RuleFor(x => x.WebsiteUrl).MaximumLength(300).MustBeHttpUrl();
    }
}

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().StrongPassword();
    }
}

public class DeleteAccountRequestValidator : AbstractValidator<DeleteAccountRequest>
{
    public DeleteAccountRequestValidator()
    {
        RuleFor(x => x.Password).NotEmpty();
    }
}
