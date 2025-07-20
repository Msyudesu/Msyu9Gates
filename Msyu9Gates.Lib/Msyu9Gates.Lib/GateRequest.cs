namespace Msyu9Gates.Lib
{
    public class GateRequest
    {
        public string? Key { get; set; } = null;
        public int Chapter { get; set; }
        public int Gate { get; set; } = 0;

        public GateRequest(string? key, int chapter, int gate)
        {
            Key = key;
            Chapter = chapter;
            Gate = gate;
        }
    }
}
