using System.Text.Json;

namespace FinTrack.Core.Domain.Entities;

public class Transaction : Entity
{
    public required Guid AccountId { get; init; }
    public Guid? CategoryId { get; set; }
    public required DateOnly Date { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public string? Notes { get; set; }
    public string[] Tags { get; set; } = [];
    public JsonDocument? RawData { get; set; }
    public string? DuplicateHash { get; init; }

    public Account? Account { get; init; }
    public Category? Category { get; set; }
}
