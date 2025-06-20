using Msyu9Gates.Data;

namespace Msyu9Gates.Utils
{
    public static class Gate2Utils
    {
        private static int correctCount = 0;

        public static string CheckKey(IConfiguration config, string key)
        {
            string _key = config.GetValue<string>("Keys:0002") ?? "";
            correctCount = 0;

            if ((!String.IsNullOrWhiteSpace(key) && !String.IsNullOrWhiteSpace(_key))
                && key.Length == _key?.Length)
            {
                if (key == _key) { return "Key is correct."; }

                for (int i = 0; i <= key.Length - 1; i++)
                { 
                    if (key[i] == _key[i])
                    {
                        correctCount++;
                    }   
                }
                Gate2Data.Gate2AttemptLog.Add($"{key} -- {correctCount} / {_key.Length}");
                return $"Key is incorrect. {correctCount} / {_key.Length} characters are correct.";
            }
            Gate2Data.Gate2AttemptLog.Add($"{key}");
            return $"Key is incorrect.";
        }
    }
}
