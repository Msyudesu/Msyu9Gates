namespace Msyu9Gates.Lib
{
    public class GateRequest
    {
        public string? Key { get; set; } = null;
        public string? KeyID { get; set; } = String.Empty;
        public int Gate { get; set; } = 0;

        public GateRequest(string? key, string? keyID, int gate)
        {
            Key = key;
            KeyID = keyID;
            Gate = gate;
        }
    }
}
