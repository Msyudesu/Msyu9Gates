using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Data.Models
{
    public class ChapterModel
    {
        [Key]
        public int Id { get; set; }
        public int GateId { get; set; }
        public GateChapter Chapter { get; set; }
        public bool IsCompleted { get; set; } = false;
        public bool IsLocked { get; set; } = true;
        public DateTime? DateUnlocked { get; set; }
        public DateTime? DateCompleted { get; set; }

        /// <summary>
        /// Identify which Key is being used for the stage of the gate.
        /// Options: I, II, III.
        /// </summary>
        public enum GateChapter
        {
            I = 1,
            II = 2,
            III = 3
        }
    }
}
