using FinTrack.Core.Domain.Entities;

namespace FinTrack.Core.Services;

public interface ICategorySeeder
{
    IEnumerable<Category> GetDefaultCategories(Guid profileId);
}
