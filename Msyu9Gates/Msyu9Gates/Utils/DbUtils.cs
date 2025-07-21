

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Msyu9Gates.Data;
using Msyu9Gates.Data.Models;

namespace Msyu9Gates.Utils
{
    public static class DbUtils
    {
        // Create
        public static async Task<KeyModel> AddKeyAsync(ApplicationDbContext db, KeyModel key)
        {
            db.KeysDb.Add(key);
            await db.SaveChangesAsync();
            return key;
        }

        // Read (by Id)
        public static async Task<KeyModel?> GetKeyByIdAsync(ApplicationDbContext db, int id)
        {
            return await db.KeysDb.FindAsync(id);
        }

        // Read (all)
        public static async Task<List<KeyModel>> GetAllKeysAsync(ApplicationDbContext db)
        {
            return await db.KeysDb.ToListAsync();
        }

        // Update
        public static async Task<bool> UpdateKeyAsync(ApplicationDbContext db, KeyModel key)
        {
            db.KeysDb.Update(key);
            return await db.SaveChangesAsync() > 0;
        }

        // Delete
        public static async Task<bool> DeleteKeyAsync(ApplicationDbContext db, int id)
        {
            var key = await db.KeysDb.FindAsync(id);
            if (key == null)
                return false;

            db.KeysDb.Remove(key);
            return await db.SaveChangesAsync() > 0;
        }

        // Gates


        // Chapters
        public async static Task<ChapterModel> AddChapterAsync(ApplicationDbContext db, ChapterModel chapter)
        {
            db.ChaptersDb.Add(chapter);
            await db.SaveChangesAsync();
            return chapter;
        }
        public async static Task<ChapterModel?> GetChapterByIdAsync(ApplicationDbContext db, int id)
        {
            return await db.ChaptersDb.FindAsync(id);
        }

        public async static Task<List<ChapterModel>> GetAllChaptersAsync(ApplicationDbContext db)
        {
            return await db.ChaptersDb.ToListAsync();
        }

        public async static Task<bool> UpdateChapterAsync(ApplicationDbContext db, ChapterModel chapter)
        {
            db.ChaptersDb.Update(chapter);
            return await db.SaveChangesAsync() > 0;
        }

        public async static Task<bool> DeleteChapterAsync(ApplicationDbContext db, int id)
        {
            var chapter = await db.ChaptersDb.FindAsync(id);
            if (chapter == null)
                return false;
            db.ChaptersDb.Remove(chapter);
            return await db.SaveChangesAsync() > 0;
        }
    }
}
