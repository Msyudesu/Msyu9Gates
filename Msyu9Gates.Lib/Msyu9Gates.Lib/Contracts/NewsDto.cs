using static Msyu9Gates.Lib.Utils;

namespace Msyu9Gates.Lib.Contracts;

public sealed record NewsDto(

    int Id,
    DateTimeOffset? PublishedAtUtc,
    string Body,
    NewsType Type
);