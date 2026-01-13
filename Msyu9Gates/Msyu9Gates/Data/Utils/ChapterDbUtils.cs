using Microsoft.EntityFrameworkCore;

using Msyu9Gates.Lib.Models;
using Msyu9Gates.Contracts;

namespace Msyu9Gates.Data.Utils;

public static class ChapterDbUtils
{
    public static async Task<List<ChapterDto>> GetAllChaptersAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var chapters = await db.ChaptersDb.AsNoTracking().OrderBy(c => c.GateId).ThenBy(c => c.ChapterNumber).ToListAsync(ct);
        return chapters.Select(c => c.ToDto()).ToList();
    }

    public static async Task<List<ChapterDto>> GetChaptersByGateIdAsync(ApplicationDbContext db, int gateId, CancellationToken ct)
    {
        var chapters = await db.ChaptersDb.AsNoTracking().Where(c => c.GateId == gateId).OrderBy(c => c.ChapterNumber).ToListAsync(ct);
        return chapters.Select(c => c.ToDto()).ToList();
    }

    public static async Task<ChapterDto?> GetChapterAsync(ApplicationDbContext db, int gateId, int chapterNumber, CancellationToken ct)
    {
        var chapter = await db.ChaptersDb.AsNoTracking().Where(c => c.GateId == gateId && c.ChapterNumber == chapterNumber).FirstOrDefaultAsync(ct);
        return chapter?.ToDto();
    }

    public static async Task<ChapterDto> SaveChapterAsync(ApplicationDbContext db, ChapterDto chapterDto, CancellationToken ct)
    {
        var chapterModel = await db.ChaptersDb.Where(c => c.GateId == chapterDto.GateId && c.ChapterNumber == chapterDto.ChapterNumber).FirstOrDefaultAsync(ct);
        if (chapterModel is null)
        {
            chapterModel = new ChapterModel();
            chapterModel.ToModel(chapterDto);
            db.ChaptersDb.Add(chapterModel);
        }
        else
        {
            chapterModel.ToModel(chapterDto);
            db.ChaptersDb.Update(chapterModel);
        }
        await db.SaveChangesAsync(ct);
        return chapterModel.ToDto();
    }
}