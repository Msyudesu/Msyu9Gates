using Msyu9Gates.Lib.Contracts;
using Msyu9Gates.Contracts;
using Microsoft.EntityFrameworkCore;
using Msyu9Gates.Data.Models;

namespace Msyu9Gates.Data.Utils;

public static class NewsDbUtils
{
    public static async Task<List<NewsDto>> GetAllNewsAsync(ApplicationDbContext db, CancellationToken ct)
    {
        var newsModels = await db.NewsDb
            .AsNoTracking()
            .OrderByDescending(n => n.Id)
            .ToListAsync(ct);
        return newsModels.Select(n => n.ToDto()).ToList();
    }

    public static async Task<NewsDto?> GetNewsByIdAsync(ApplicationDbContext db, int newsId, CancellationToken ct)
    {
        var newsModel = await db.NewsDb
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == newsId, ct);
        return newsModel?.ToDto();
    }

    public static async Task<NewsDto> SaveNewsAsync(ApplicationDbContext db, NewsDto newsDto, CancellationToken ct)
    {
        var newsModel = new News(newsDto.Body, newsDto.Type);
        newsModel.PublishedAtUtc = newsDto.PublishedAtUtc;
        await db.NewsDb.AddAsync(newsModel, ct);
        await db.SaveChangesAsync(ct);
        return newsModel.ToDto();
    }

    public static async Task<NewsDto?> UpdateNewsAsync(ApplicationDbContext db, NewsDto newsDto, CancellationToken ct)
    {
        var newsModel = await db.NewsDb.FirstOrDefaultAsync(n => n.Id == newsDto.Id, ct);
        if (newsModel == null)
        {
            return null;
        }
        newsModel.Body = newsDto.Body;
        newsModel.Type = newsDto.Type;
        newsModel.PublishedAtUtc = newsDto.PublishedAtUtc;
        db.NewsDb.Update(newsModel);
        await db.SaveChangesAsync(ct);
        return newsModel.ToDto();
    }

    public static async Task DeleteNewsAsync(ApplicationDbContext db, int newsId, CancellationToken ct)
    {
        var newsModel = await db.NewsDb.FirstOrDefaultAsync(n => n.Id == newsId, ct);
        if (newsModel != null)
        {
            db.NewsDb.Remove(newsModel);
            await db.SaveChangesAsync(ct);
        }
    }
}
