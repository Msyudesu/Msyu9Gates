namespace Msyu9Gates.Lib.Contracts;

public sealed record AttemptDto(
    DateTimeOffset AttemptedAtUtc,
    int UserId,
    int GateId,
    int ChapterId,
    string? AttemptValue
);