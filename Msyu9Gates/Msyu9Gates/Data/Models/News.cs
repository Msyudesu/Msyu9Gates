using static Msyu9Gates.Lib.Utils;

namespace Msyu9Gates.Data.Models;

public class News
{
    public int Id { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    public string Body { get; set; } = string.Empty;
    public NewsType Type { get; set; }
    
    public News(string Body, NewsType Type)
    {
        this.Body = Body;
        this.Type = Type;
        PublishedAtUtc = DateTimeOffset.UtcNow;
    }
}    
