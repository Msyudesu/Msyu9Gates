namespace Msyu9Gates.Contracts
{
    public sealed record GateDto(
        int Id,
        int GateNumber,
        int GateOverallDifficultyLevel,
        bool IsCompleted,
        bool IsLocked,
        DateTimeOffset? DateUnlocked,
        DateTimeOffset? DateCompleted,
        string? Narrative,
        string? Conclusion
    );
}
