using Microsoft.EntityFrameworkCore;
using Msyu9Gates.Data;
using Msyu9Gates.Data.Models;
using Msyu9Gates.Utils;

namespace Msyu9Gates
{
    public class ChapterManager
    {
        ILogger _log;
        IConfiguration _config;
        IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public List<Chapter> Chapters = new List<Chapter>() 
        { 
            new Chapter(ChapterModel.GateChapter.I),
            new Chapter(ChapterModel.GateChapter.II),
            new Chapter(ChapterModel.GateChapter.III)
        };

        public ChapterManager(IConfiguration config, ILogger log, IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _log = log;
            _config = config;
            _dbContextFactory = dbContextFactory;
        }

        public async Task<int> GetChapterId(Chapter chapter)
        {
            try
            {
                using (var db = _dbContextFactory.CreateDbContext())
                {
                    var ch = await db.ChaptersDb.FirstOrDefaultAsync(c => (c.GateId == chapter.GateId && c.Chapter == chapter.Chapter));
                    if (ch != null)
                    {
                        return ch.Id;
                    }
                    else
                    {
                        _log.LogWarning($"Gate {chapter.GateId} - Chaper {chapter.Chapter} not found in the database.");
                        return -1;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to get chapter ID from database: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> UpdateOrAddChapter(Chapter chapter)
        {
            try
            {
                using (var db = _dbContextFactory.CreateDbContext())
                {
                    var chapterIfExists = await db.ChaptersDb.FindAsync(chapter.Id);
                    if (chapterIfExists != null)
                    {
                        chapterIfExists.Id = chapter.Id;

                        chapterIfExists.GateId = chapter.GateId;

                        chapterIfExists.Chapter = chapter.Chapter;

                        if (chapter.IsLocked)
                        {
                            chapterIfExists.IsLocked = chapter.IsLocked;
                            chapterIfExists.DateUnlocked = DateTime.Now;
                        }

                        if (chapter.IsCompleted)
                        {
                            chapterIfExists.IsCompleted = chapter.IsCompleted;
                            chapterIfExists.DateCompleted = DateTime.Now;
                        }  

                        await db.SaveChangesAsync();
                        _log.LogInformation($"Chapter {chapter.Chapter} updated successfully.");
                        return true;
                    }
                    await DbUtils.AddChapterAsync(db, chapter);
                    await db.SaveChangesAsync();
                    _log.LogInformation($"Chapter {chapter.Chapter} added successfully.");
                }
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to add chapter to database: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoadChapters()
        {
            try
            {
                using (var db = _dbContextFactory.CreateDbContext())
                {
                    var allChapters = await DbUtils.GetAllChaptersAsync(db);
                    Chapters.Clear();

                    foreach (var chapter in allChapters)
                    {
                        Chapters.Add(new Chapter(
                            chapter.GateId
                            , chapter.Chapter
                            , chapter.IsLocked
                            , chapter.IsCompleted
                            , chapter.DateUnlocked
                            , chapter.DateCompleted
                            ));
                    }
                }

                _log.LogInformation($"Loaded data for {Chapters.Count} chapters from the database.");
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to load chapters from database: {ex.Message}");
            }
            return false;
        }

        public async Task<bool> DropAllChapters()
        {
            try
            {
                using (var db = _dbContextFactory.CreateDbContext())
                {
                    int affectedRows = await db.Database.ExecuteSqlRawAsync(
                        sql: "DELETE FROM ChaptersDb; DELETE FROM SQLITE_SEQUENCE WHERE NAME='ChaptersDb';"
                        );
                }
                _log.LogInformation("All keys dropped from the database.");
                return true;
            }
            catch (Exception ex)
            {
                _log.LogError($"Failed to drop all chapters from database: {ex.Message}");
            }
            return false;
        }

        public async Task UnlockChapter(Chapter chapter)
        {
            int _id = await this.GetChapterId(chapter);
            int index = this.Chapters.FindIndex(c => c.Id == _id);
            this.Chapters[index].IsLocked = false;
            await this.UpdateOrAddChapter(chapter);
            _log.LogInformation($"Gate {chapter.GateId} Chapter {chapter.Chapter} with ID {_id} has been marked as unlocked.");
        }

        public async Task CompleteChapter(Chapter chapter)
        {
            int _id = await this.GetChapterId(chapter);
            int index = this.Chapters.FindIndex(c => c.Id == _id);
            this.Chapters[index].IsCompleted = true;
            await this.UpdateOrAddChapter(chapter);
            _log.LogInformation($"Gate {chapter.GateId} Chapter {chapter.Chapter} with ID {_id} has been marked as complete.");
        }
    }

    public class Chapter : ChapterModel
    {
        public Chapter(GateChapter chapter)
        {
            this.Chapter = chapter;
            this.IsCompleted = false;
            this.IsLocked = true;            
        }

        public Chapter(int gateId, GateChapter chapter, bool isLocked, bool isCompleted, DateTime? dateUnlocked = null, DateTime? dateCompleted = null)
        {
            this.GateId = gateId;
            this.Chapter = chapter;
            this.IsLocked = isLocked;
            this.IsCompleted = isCompleted;
            this.DateUnlocked = dateUnlocked;
            this.DateCompleted = dateCompleted;
        }
    }
}
