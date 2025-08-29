namespace Msyu9Gates.Lib;

public class GateResponse
{
    public string? Key { get; set; } = null;
    public int Chapter { get; set; }
    public string? Message { get; set; } = String.Empty;
    public bool Success { get; set; } = false;
    public List<string> Errors { get; set; } = new List<string>();

    public GateResponse(string? key, int chapter, bool success, string? message = null)
    {
        Key = key;
        Chapter = chapter;
        Message = message;
        Success = success;
    }
}
