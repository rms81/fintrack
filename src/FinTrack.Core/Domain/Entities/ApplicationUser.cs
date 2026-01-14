using Microsoft.AspNetCore.Identity;

namespace FinTrack.Core.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public ICollection<Profile> Profiles { get; set; } = [];
}
