using FinTrack.Core.Domain.Entities;
using FinTrack.Core.Services;

namespace FinTrack.Infrastructure.Services;

public class CategorySeeder : ICategorySeeder
{
    private static readonly DefaultCategory[] DefaultCategories =
    [
        new("Food", "utensils", "#EF4444", 10, ["Groceries", "Restaurants", "Delivery"]),
        new("Transport", "car", "#F97316", 20, ["Fuel", "Public Transport", "Rideshare", "Parking"]),
        new("Shopping", "shopping-bag", "#EAB308", 30, ["Clothing", "Electronics", "Home Goods", "Shopping Other"]),
        new("Housing", "home", "#22C55E", 40, ["Rent", "Utilities", "Home Insurance", "Maintenance"]),
        new("Entertainment", "film", "#14B8A6", 50, ["Streaming", "Games", "Events", "Hobbies"]),
        new("Health", "heart-pulse", "#06B6D4", 60, ["Medical", "Pharmacy", "Gym"]),
        new("Subscriptions", "credit-card", "#3B82F6", 70, ["Software", "Services", "Memberships"]),
        new("Travel", "plane", "#8B5CF6", 80, ["Flights", "Hotels", "Car Rental"]),
        new("Income", "trending-up", "#10B981", 90, ["Salary", "Freelance", "Investments", "Income Other"]),
        new("Transfer", "arrow-left-right", "#6B7280", 100, []),
        new("Uncategorized", "folder", "#9CA3AF", 110, [])
    ];

    public IEnumerable<Category> GetDefaultCategories(Guid profileId)
    {
        var categories = new List<Category>();
        var parentLookup = new Dictionary<string, Guid>();

        // Create parent categories first with explicit IDs
        foreach (var def in DefaultCategories)
        {
            var parentId = Guid.NewGuid();
            var parent = new Category
            {
                Id = parentId,
                ProfileId = profileId,
                Name = def.Name,
                Icon = def.Icon,
                Color = def.Color,
                SortOrder = def.SortOrder,
                ParentId = null
            };
            categories.Add(parent);
            parentLookup[def.Name] = parentId;
        }

        // Create subcategories
        foreach (var def in DefaultCategories)
        {
            var parentId = parentLookup[def.Name];
            var parentCategory = categories.First(c => c.Id == parentId);
            var subcategorySortOrder = 1;

            foreach (var subName in def.Subcategories)
            {
                var subcategory = new Category
                {
                    Id = Guid.NewGuid(),
                    ProfileId = profileId,
                    Name = subName,
                    Icon = parentCategory.Icon,
                    Color = parentCategory.Color,
                    SortOrder = parentCategory.SortOrder + subcategorySortOrder++,
                    ParentId = parentId
                };
                categories.Add(subcategory);
            }
        }

        return categories;
    }

    private record DefaultCategory(
        string Name,
        string Icon,
        string Color,
        int SortOrder,
        string[] Subcategories);
}
