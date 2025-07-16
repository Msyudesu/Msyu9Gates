namespace Msyu9Gates.Data.Models
{
    public class KeyModel
    {
        public int Id { get; set; }
        public string? KeyValue { get; set; }
        public bool Discovered { get; set; } = false;
        public DateTime DateDiscovered { get; set; }
    }
}
