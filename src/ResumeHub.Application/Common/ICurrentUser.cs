namespace ResumeHub.Application.Common;

/// <summary>
/// Port for the authenticated caller's identity. Implemented in the Api layer,
/// which reads it from the request's JWT claims.
/// </summary>
public interface ICurrentUser
{
    Guid Id { get; }
}
