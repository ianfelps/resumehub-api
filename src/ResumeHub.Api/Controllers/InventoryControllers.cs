using Microsoft.AspNetCore.Mvc;
using ResumeHub.Application.Dtos;
using ResumeHub.Application.Services;

namespace ResumeHub.Api.Controllers;

[Route("api/experiences")]
public class ExperiencesController(IExperienceService service)
    : OwnedCrudController<ExperienceRequest, ExperienceRequest, ExperienceResponse>(service);

[Route("api/projects")]
public class ProjectsController(IProjectService service)
    : OwnedCrudController<ProjectRequest, ProjectRequest, ProjectResponse>(service);

[Route("api/skills")]
public class SkillsController(ISkillService service)
    : OwnedCrudController<SkillRequest, SkillRequest, SkillResponse>(service);

[Route("api/languages")]
public class LanguagesController(ILanguageService service)
    : OwnedCrudController<LanguageRequest, LanguageRequest, LanguageResponse>(service);

[Route("api/education")]
public class EducationController(IEducationService service)
    : OwnedCrudController<EducationRequest, EducationRequest, EducationResponse>(service);

[Route("api/courses")]
public class CoursesController(ICourseService service)
    : OwnedCrudController<CourseRequest, CourseRequest, CourseResponse>(service);
