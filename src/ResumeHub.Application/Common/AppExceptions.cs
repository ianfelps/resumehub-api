namespace ResumeHub.Application.Common;

/// <summary>Resource not found / not owned by the caller — surfaced as 404.</summary>
public class NotFoundException(string message) : Exception(message);

/// <summary>Request is well-formed but violates a business rule — surfaced as 409/400.</summary>
public class ConflictException(string message) : Exception(message);
