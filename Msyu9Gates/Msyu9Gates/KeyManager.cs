using Microsoft.EntityFrameworkCore;
using Msyu9Gates.Data;
using Msyu9Gates.Data.Models;
using Msyu9Gates.Utils;

namespace Msyu9Gates
{
    public class KeyManager
    {
        ILogger _log;
        IConfiguration _config;
        IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public List<Key> Keys = new List<Key>();

        public KeyManager(IConfiguration config, ILogger log, IDbContextFactory<ApplicationDbContext> dbContextFactory) 
        {
            _config = config;
            _log = log;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<int> GetKeyId(string KeyValue)
        {
            try
            {
                using(var db = _dbContextFactory.CreateDbContext())
                {
                    var key = await db.KeysDb.FirstOrDefaultAsync(k => k.KeyValue == KeyValue);
                    if (key != null)
                    {
                        return key.Id;
                    }
                    else
                    {
                        _log.LogWarning($"Key with value '{KeyValue}' not found in the database.");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to get key ID from database: {ex.Message}");
                return -1;
            }
        }
        
        public async Task<bool> UpdateOrAddKey(Key key)
        {
            try
            {
                using (var db = _dbContextFactory.CreateDbContext())
                {
                    var keyIfExists = await db.KeysDb.FindAsync(key.Id);
                    if (keyIfExists != null)
                    {
                        keyIfExists.Id = key.Id;
                        keyIfExists.KeyValue = key.KeyValue;
                        keyIfExists.Discovered = key.Discovered;
                        if (keyIfExists.Discovered)
                            keyIfExists.DateDiscovered = DateTime.Now;

                        await db.SaveChangesAsync();
                        return true;
                    }
                    await DbUtils.AddKeyAsync(db, (KeyModel)key);
                    await db.SaveChangesAsync();
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to add key to database: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoadKeys()
        {
            try
            {
                using (var db = _dbContextFactory.CreateDbContext())
                {
                    var allKeys = await DbUtils.GetAllKeysAsync(db);
                    Keys.Clear();
                    
                    foreach (var key in allKeys)
                    {
                        Keys.Add(new Key(key.Id, key.KeyValue ?? string.Empty, key.DateDiscovered, key.Discovered));
                    }
                }

                _log.LogInformation($"Loaded {Keys.Count} keys from the database.");
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to load keys from database: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> DropAllKeys()
        {
            try
            {
                using (var db = _dbContextFactory.CreateDbContext())
                {
                    int affectedRows = await db.Database.ExecuteSqlRawAsync(
                        sql: "DELETE FROM KeysDb; DELETE FROM SQLITE_SEQUENCE WHERE NAME='KeysDb';"
                        );
                }
                _log.LogInformation("All keys dropped from the database.");
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to drop all keys from database: {ex.Message}");
            }
            return false;
        }

        public async Task DiscoverKey(string keyValue)
        {
            int _id = await this.GetKeyId(keyValue);
            int index = this.Keys.FindIndex(k => k.Id == _id);
            this.Keys[index].Discovered = true;
            await this.UpdateOrAddKey(this.Keys[index]);
            _log.LogInformation($"Key '{keyValue}' with ID {_id} has been marked as discovered.");
        }
    }

    public class Key : KeyModel
    {
        public Key(int id, string keyValue, DateTime dateDiscovered, bool discovered = false)
        {
            Id = id;
            KeyValue = keyValue;
            Discovered = discovered;
            DateDiscovered = dateDiscovered;
        }
    }
}
