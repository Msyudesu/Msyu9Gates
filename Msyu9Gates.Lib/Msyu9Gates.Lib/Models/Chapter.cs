using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Lib.Models;

public class Chapter
{
    [Key] public int Id { get; set; }
    public int GateId { get; set; }
    public int ChapterNumber { get; set; }
    public int DifficultyLevel { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsLocked { get; set; } = true;
    public DateTimeOffset? DateUnlockedUtc { get; set; } = null;
    public DateTimeOffset? DateCompletedUtc { get; set; } = null;
    public string? Narrative { get; set; } = string.Empty;
    public Guid? RouteGuid { get; set; } = null;

    public ICollection<GateKey> Keys { get; set; } = new List<GateKey>();
    public ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
}
