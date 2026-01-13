namespace Msyu9Gates.Contracts;

public sealed record KeyDto(
    int Id,
    int GateId,
    int ChapterId,
    int KeyNumber,
    string? KeyValue,
    bool Discovered,
    DateTimeOffset? DateDiscoveredUtc
);   