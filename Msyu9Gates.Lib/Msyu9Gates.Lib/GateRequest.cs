namespace Msyu9Gates.Lib
{
    public class GateRequest
    {
        string? Key { get; set; } = null;
        string? KeyID { get; set; } = String.Empty;

        public GateRequest(string? key, string? keyID)
        {
            Key = key;
            KeyID = keyID;
        }
    }
}
