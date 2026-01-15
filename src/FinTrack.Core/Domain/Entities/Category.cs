namespace FinTrack.Core.Domain.Entities;

public class Category : Entity
{
    public required Guid ProfileId { get; init; }
    public required string Name { get; set; }
    public string Icon { get; set; } = "folder";
    public string Color { get; set; } = "#6B7280";
    public int SortOrder { get; set; }
    public Guid? ParentId { get; set; }

    public Profile? Profile { get; init; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; init; } = new List<Category>();
    public ICollection<Transaction> Transactions { get; init; } = new List<Transaction>();
}
