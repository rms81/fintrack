namespace FinTrack.Core.Domain.Entities;

public class Account : Entity
{
    public required Guid ProfileId { get; init; }
    public required string Name { get; set; }
    public string? BankName { get; set; }
    public string Currency { get; set; } = "EUR";

    public Profile? Profile { get; init; }
}
