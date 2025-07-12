namespace Msyu9Gates.Data
{
    public class AttemptHistory
    {
        private List<string> Log { get; set; } = new List<string>();

        public AttemptHistory()
        {
            ClearHistory();
        }

        public void SaveAttempt(string attempt)
        {
            if (!string.IsNullOrWhiteSpace(attempt))
            {
                Log.Add($"[{DateTime.UtcNow}] {attempt}");
            }
        }

        public List<string> GetAttempts() => Log;
        public void ClearHistory()
        {
            Log.Clear();
            Log.Add($"[{DateTime.UtcNow}] - History was Reset");
        }
    }
}
