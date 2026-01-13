namespace FinTrack.Core.Features.Accounts;

public record AccountDto(
    Guid Id,
    Guid ProfileId,
    string Name,
    string? BankName,
    string Currency,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateAccountRequest(string Name, string? BankName = null, string Currency = "EUR");

public record UpdateAccountRequest(string Name, string? BankName, string Currency);
