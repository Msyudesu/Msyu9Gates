using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Lib.Models;

public class ChapterModel
{
    [Key] public int Id { get; set; }
    public int GateId { get; set; }
    public int ChapterNumber { get; set; }
    public int DiffiutyLevel { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsLocked { get; set; } = true;
    public DateTimeOffset? DateUnlockedUtc { get; set; } = null;
    public DateTimeOffset? DateCompletedUtc { get; set; } = null;
    public string? Narrative { get; set; } = string.Empty;
    public Guid? RouteGuid { get; set; } = null;

    public ICollection<KeyModel> Keys { get; set; } = new List<KeyModel>();
    public ICollection<AttemptModel> Attempts { get; set; } = new List<AttemptModel>();
}
