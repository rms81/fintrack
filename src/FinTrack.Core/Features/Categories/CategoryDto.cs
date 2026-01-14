namespace FinTrack.Core.Features.Categories;

public record CategoryDto(
    Guid Id,
    string Name,
    string Icon,
    string Color,
    Guid? ParentId,
    int TransactionCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateCategoryRequest(
    string Name,
    string Icon = "folder",
    string Color = "#6B7280",
    Guid? ParentId = null);

public record UpdateCategoryRequest(
    string Name,
    string Icon,
    string Color,
    Guid? ParentId);

public record CreateCategory(Guid ProfileId, string Name, string Icon, string Color, Guid? ParentId);
public record GetCategories(Guid ProfileId);
public record GetCategory(Guid Id);
public record UpdateCategory(Guid Id, string Name, string Icon, string Color, Guid? ParentId);
public record DeleteCategory(Guid Id);
public record DeleteCategoryResult(bool Success);
