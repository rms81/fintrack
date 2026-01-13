namespace FinTrack.Core.Domain.Entities;

public class User : Entity
{
    public required string ExternalId { get; init; }
    public required string Email { get; set; }
    public string? DisplayName { get; set; }

    public ICollection<Profile> Profiles { get; init; } = new List<Profile>();
}
