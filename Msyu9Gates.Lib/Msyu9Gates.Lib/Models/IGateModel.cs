using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Lib.Models;

public interface IGateModel
{
    [Key]
    public int Id { get; set; }
    public int GateNumber { get; set; }
    public int GateOverallDifficultyLevel { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? DateUnlocked { get; set; }
    public DateTime? DateCompleted { get; set; }
}
