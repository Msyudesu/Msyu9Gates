using Microsoft.EntityFrameworkCore;

using Msyu9Gates.Contracts;
using Msyu9Gates.Lib.Models;

namespace Msyu9Gates.Data.Utils;

public static class GateDbUtils
{
    public static async Task<List<GateDto>> GetAllGatesAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var gates = await db.GatesDb.AsNoTracking().OrderBy(g => g.GateNumber).ToListAsync(ct);
        return gates.Select(g => g.ToDto()).ToList();
    }

    public static async Task<GateDto?> GetGateByNumberAsync(ApplicationDbContext db, int gateNumber, CancellationToken ct)
    {
        var gate = await db.GatesDb.AsNoTracking().Where(g => g.GateNumber == gateNumber).FirstOrDefaultAsync(ct);
        return gate?.ToDto();
    }

    public static async Task<GateDto> SaveGateAsync(ApplicationDbContext db, GateDto gateDto, CancellationToken ct)
    {
        var gateModel = await db.GatesDb.Where(g => g.GateNumber == gateDto.GateNumber).FirstOrDefaultAsync(ct);
        if (gateModel is null)
        {
            gateModel = new GateModel();
            gateModel.ApplyFromDto(gateDto);
            db.GatesDb.Add(gateModel);
        }
        else
        {
            gateModel.ApplyFromDto(gateDto);
            db.GatesDb.Update(gateModel);
        }
        await db.SaveChangesAsync(ct);
        return gateModel.ToDto();
    }
}
