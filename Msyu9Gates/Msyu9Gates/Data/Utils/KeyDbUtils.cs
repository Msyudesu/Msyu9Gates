using Microsoft.EntityFrameworkCore;

using Msyu9Gates.Lib.Models;
using Msyu9Gates.Contracts;

namespace Msyu9Gates.Data.Utils;

public static class KeyDbUtils
{
    public static async Task<List<KeyDto>> GetAllKeysAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var keys = await db.KeysDb.AsNoTracking().OrderBy(k => k.GateId).ThenBy(k => k.ChapterId).ThenBy(k => k.KeyNumber).ToListAsync(ct);
        return keys.Select(k => k.ToDto()).ToList();
    }

    public static async Task<List<KeyDto>> GetKeysByGateIdAsync(ApplicationDbContext db, int gateId, CancellationToken ct)
    {
        var keys = await db.KeysDb.AsNoTracking().Where(k => k.GateId == gateId).OrderBy(k => k.ChapterId).ThenBy(k => k.KeyNumber).ToListAsync(ct);
        return keys.Select(k => k.ToDto()).ToList();
    }

    public static async Task<KeyDto?> GetKeyByGateChapterAsync(ApplicationDbContext db, int gateId, int chapterId, CancellationToken ct)
    {
        var key = await db.KeysDb.AsNoTracking().Where(k => k.GateId == gateId && k.ChapterId == chapterId).FirstOrDefaultAsync(ct);
        return key?.ToDto();
    }

    public static async Task<KeyDto?> GetUnlockedKeys(ApplicationDbContext db, CancellationToken ct)
    {
        var key = await db.KeysDb.AsNoTracking().Where(k => k.Discovered).FirstOrDefaultAsync(ct);
        return key?.ToDto();
    }

    public static async Task<KeyDto> SaveKeyAsync(ApplicationDbContext db, KeyDto keyDto, CancellationToken ct)
    {
        var keyModel = await db.KeysDb.Where(k => k.GateId == keyDto.GateId && k.ChapterId == keyDto.ChapterId && k.KeyNumber == keyDto.KeyNumber).FirstOrDefaultAsync(ct);
        if (keyModel is null)
        {
            keyModel = new KeyModel();
            keyModel.ApplyFromDto(keyDto);
            await db.KeysDb.AddAsync(keyModel, ct);
        }
        else
        {
            keyModel.ApplyFromDto(keyDto);
            db.KeysDb.Update(keyModel);
        }
        await db.SaveChangesAsync(ct);
        return keyModel.ToDto();
    }
}
