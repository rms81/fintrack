# Categories Module

## Overview
Categories organize transactions into meaningful groups. They are profile-scoped and support hierarchical organization via parent-child relationships.

## Domain Model

### Category Entity
```csharp
public class Category : BaseEntity
{
    public Guid ProfileId { get; init; }
    public required string Name { get; set; }
    public string Icon { get; set; } = "folder";
    public string Color { get; set; } = "#6B7280";
    public Guid? ParentId { get; set; }
    
    // Navigation
    public Profile Profile { get; init; } = null!;
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}
```

## Database

### Table: categories
```sql
CREATE TABLE categories (
    id uuid PRIMARY KEY DEFAULT uuidv7(),
    profile_id uuid NOT NULL REFERENCES profiles(id) ON DELETE CASCADE,
    name varchar(100) NOT NULL,
    icon varchar(50) NOT NULL DEFAULT 'folder',
    color char(7) NOT NULL DEFAULT '#6B7280',
    parent_id uuid REFERENCES categories(id) ON DELETE SET NULL,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX ix_categories_profile_id ON categories(profile_id);
CREATE INDEX ix_categories_parent_id ON categories(parent_id);
CREATE UNIQUE INDEX ix_categories_profile_name ON categories(profile_id, name);
```

## Endpoints

### GET /api/profiles/{profileId}/categories
List all categories for a profile (flat or tree structure).

**Query Parameters:**
- `tree=true` - Return hierarchical structure

**Response (flat):** `200 OK`
```json
[
  {
    "id": "0193a1d1-...",
    "name": "Food & Dining",
    "icon": "utensils",
    "color": "#FF6B6B",
    "parentId": null,
    "transactionCount": 145
  },
  {
    "id": "0193a1d2-...",
    "name": "Restaurants",
    "icon": "utensils",
    "color": "#FF6B6B",
    "parentId": "0193a1d1-...",
    "transactionCount": 89
  }
]
```

### POST /api/profiles/{profileId}/categories
Create new category.

**Request:**
```json
{
  "name": "Subscriptions",
  "icon": "repeat",
  "color": "#8B5CF6",
  "parentId": null
}
```

### PUT /api/categories/{id}
Update category.

### DELETE /api/categories/{id}
Delete category (transactions become uncategorized).

## Default Categories

Created automatically for new profiles:

| Name | Icon | Color | Purpose |
|------|------|-------|---------|
| Food & Dining | utensils | #FF6B6B | Restaurants, groceries |
| Transportation | car | #4ECDC4 | Fuel, public transit, rideshare |
| Shopping | shopping-bag | #45B7D1 | Retail purchases |
| Bills & Utilities | file-text | #96CEB4 | Recurring bills |
| Entertainment | film | #FFEAA7 | Movies, games, events |
| Income | dollar-sign | #26DE81 | Salary, freelance |
| Other | more-horizontal | #A0A0A0 | Uncategorized |

## React Components

```typescript
// src/features/categories/CategoryPicker.tsx
export function CategoryPicker({ 
  value, 
  onChange 
}: { 
  value?: string; 
  onChange: (id: string) => void 
}) {
  const { currentProfile } = useProfileContext();
  const { data: categories } = useCategories(currentProfile?.id);
  
  return (
    <Select value={value} onValueChange={onChange}>
      <SelectTrigger>
        <SelectValue placeholder="Select category" />
      </SelectTrigger>
      <SelectContent>
        {categories?.map(cat => (
          <SelectItem key={cat.id} value={cat.id}>
            <span className="flex items-center gap-2">
              <CategoryIcon name={cat.icon} color={cat.color} />
              {cat.name}
            </span>
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}
```

## Business Rules

1. Category names unique within profile
2. Max 2 levels of nesting (parent → child)
3. Deleting category sets transactions to uncategorized (null)
4. Categories cannot be moved between profiles
5. Default categories created on profile creation
