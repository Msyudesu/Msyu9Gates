using Msyu9Gates.Lib.Models;

namespace Msyu9Gates.Contracts;

public static class AttemptsMappingExtensions
{
    public static AttemptDto ToDto(this AttemptModel attempt) => 
        new AttemptDto(
            AttemptedAtUtc: DateTimeOffset.UtcNow,
            UserId: attempt.UserId,
            GateId: attempt.GateId,
            ChapterId: attempt.ChapterId,
            AttemptValue: attempt.AttemptValue
        );
    

    public static void ApplyFromDto(this AttemptModel attempt, AttemptDto dto)
    {
        attempt.AttemptedAtUtc = dto.AttemptedAtUtc;
        attempt.UserId = dto.UserId;
        attempt.GateId = dto.GateId;
        attempt.ChapterId = dto.ChapterId;
        attempt.AttemptValue = dto.AttemptValue;
    }
}
