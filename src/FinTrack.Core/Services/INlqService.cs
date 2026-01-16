using FinTrack.Core.Features.Nlq;

namespace FinTrack.Core.Services;

public interface INlqService
{
    Task<NlqResponse> ExecuteQueryAsync(
        Guid profileId,
        string question,
        CancellationToken ct = default);
}
