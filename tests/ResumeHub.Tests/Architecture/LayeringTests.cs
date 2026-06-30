using System.Reflection;
using NetArchTest.Rules;
using ResumeHub.Application.Abstractions;
using ResumeHub.Domain.Entities;
using ResumeHub.Infrastructure.Persistence;
using Xunit;

namespace ResumeHub.Tests.Architecture;

/// <summary>
/// Enforces the dependency rule of the layered architecture:
/// Domain ← Application ← Infrastructure ← Api.
/// </summary>
public class LayeringTests
{
    private static readonly Assembly Domain = typeof(OwnedEntity).Assembly;
    private static readonly Assembly Application = typeof(IApplicationDbContext).Assembly;
    private static readonly Assembly Infrastructure = typeof(ResumeHubDbContext).Assembly;

    private const string ApplicationNs = "ResumeHub.Application";
    private const string InfrastructureNs = "ResumeHub.Infrastructure";
    private const string ApiNs = "ResumeHub.Api";

    [Fact]
    public void Domain_should_not_depend_on_other_layers()
    {
        var result = Types.InAssembly(Domain)
            .Should()
            .NotHaveDependencyOnAny(ApplicationNs, InfrastructureNs, ApiNs)
            .GetResult();

        AssertSuccess(result);
    }

    [Fact]
    public void Application_should_not_depend_on_infrastructure_or_api()
    {
        var result = Types.InAssembly(Application)
            .Should()
            .NotHaveDependencyOnAny(InfrastructureNs, ApiNs)
            .GetResult();

        AssertSuccess(result);
    }

    [Fact]
    public void Infrastructure_should_not_depend_on_api()
    {
        var result = Types.InAssembly(Infrastructure)
            .Should()
            .NotHaveDependencyOn(ApiNs)
            .GetResult();

        AssertSuccess(result);
    }

    private static void AssertSuccess(TestResult result)
    {
        var failing = result.FailingTypeNames is null
            ? string.Empty
            : string.Join(", ", result.FailingTypeNames);
        Assert.True(result.IsSuccessful, $"Offending types: {failing}");
    }
}
