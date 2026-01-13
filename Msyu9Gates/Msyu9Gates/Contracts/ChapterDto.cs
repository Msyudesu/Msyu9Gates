namespace Msyu9Gates.Contracts;

public sealed record ChapterDto(
    int Id,
    int GateId,
    int ChapterNumber,
    int DiffiutyLevel,
    bool IsCompleted,
    bool IsLocked,
    DateTimeOffset? DateUnlockedUtc,
    DateTimeOffset? DateCompletedUtc,
    string? Narrative,
    Guid? RouteGuid
);