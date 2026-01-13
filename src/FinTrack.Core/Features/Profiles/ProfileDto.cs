using FinTrack.Core.Domain.Enums;

namespace FinTrack.Core.Features.Profiles;

public record ProfileDto(
    Guid Id,
    string Name,
    ProfileType Type,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateProfileRequest(string Name, ProfileType Type = ProfileType.Personal);

public record UpdateProfileRequest(string Name, ProfileType Type);

// Command/Query records (no handlers - handlers are in Host)
public record CreateProfile(string Name, ProfileType Type = ProfileType.Personal);
public record GetProfiles;
public record GetProfile(Guid Id);
public record UpdateProfile(Guid Id, string Name, ProfileType Type);
public record DeleteProfile(Guid Id);
public record DeleteProfileResult(bool Success);
