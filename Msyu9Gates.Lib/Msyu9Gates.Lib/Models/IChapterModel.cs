using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Lib.Models;

public interface IChapterModel
{
    [Key]
    public int Id { get; set; }
    public int GateId { get; set; }
    public int ChapterNumber { get; set; }
    public int DiffiutyLevel { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? DateUnlocked { get; set; }
    public DateTime? DateCompleted { get; set; }
    public string? Narrative { get; set; }
    public Guid RouteGuid { get; set; }
}
