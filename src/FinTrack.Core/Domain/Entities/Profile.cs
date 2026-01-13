using FinTrack.Core.Domain.Enums;

namespace FinTrack.Core.Domain.Entities;

public class Profile : Entity
{
    public required Guid UserId { get; init; }
    public required string Name { get; set; }
    public ProfileType Type { get; set; } = ProfileType.Personal;

    public User? User { get; init; }
    public ICollection<Account> Accounts { get; init; } = new List<Account>();
}
