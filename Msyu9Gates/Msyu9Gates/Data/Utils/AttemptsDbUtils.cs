using Microsoft.EntityFrameworkCore;

using Msyu9Gates.Contracts;
using Msyu9Gates.Lib.Models;

namespace Msyu9Gates.Data.Utils;

public static class AttemptsDbUtils
{
    public static async Task<int> GetTotalAttemptsByPlayerForChapterAsync (ApplicationDbContext db, int userId, int chapterId, CancellationToken ct)
    {
        return await db.AttemptsDb.AsNoTracking()
            .Where(a => a.UserId == userId && a.ChapterId == chapterId)
            .CountAsync(ct);
    }

    public static async Task<AttemptDto> SaveAttemptAsync(ApplicationDbContext db, AttemptDto attemptDto, CancellationToken ct)
    {
        var attemptModel = new AttemptModel();
        attemptModel.ApplyFromDto(attemptDto);
        await db.AttemptsDb.AddAsync(attemptModel, ct);
        await db.SaveChangesAsync(ct);
        return attemptModel.ToDto();
    }
}
