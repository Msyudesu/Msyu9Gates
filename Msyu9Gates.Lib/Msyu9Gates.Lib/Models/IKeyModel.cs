using System.ComponentModel.DataAnnotations;

namespace Msyu9Gates.Lib.Models;

public interface IKeyModel
{
    [Key]
    public int Id { get; set; }
    public int GateId { get; set; }
    public int ChapterId { get; set; }
    public int KeyNumber { get; set; }
    public string? KeyValue { get; set; }
    public bool Discovered { get; set; }
    public DateTime DateDiscovered { get; set; }
}
