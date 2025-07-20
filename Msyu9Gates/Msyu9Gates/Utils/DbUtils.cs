

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
    }
}
