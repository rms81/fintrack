namespace FinTrack.Core.Services;

public interface ICurrentUser
{
    string? Id { get; }
    string? Email { get; }
    string? DisplayName { get; }
    bool IsAuthenticated { get; }
}
