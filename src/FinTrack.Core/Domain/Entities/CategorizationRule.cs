namespace FinTrack.Core.Domain.Entities;

public class CategorizationRule : Entity
{
    public required Guid ProfileId { get; init; }
    public required string Name { get; set; }
    public int Priority { get; set; }
    public required string RuleToml { get; set; }
    public bool IsActive { get; set; } = true;

    public Profile? Profile { get; init; }
}
