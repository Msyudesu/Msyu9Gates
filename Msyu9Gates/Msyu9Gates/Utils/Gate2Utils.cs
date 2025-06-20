using Msyu9Gates.Data;

namespace Msyu9Gates.Utils
{
    public static class Gate2Utils
    {
        public static string Check2AKeyIsCorrect(IConfiguration config, string key)
        {
            string _key = config.GetValue<string>("Keys:0002") ?? "";
            int _correctCount = 0;

            if ((!String.IsNullOrWhiteSpace(key) && !String.IsNullOrWhiteSpace(_key))
                && key.Length == _key?.Length)
            {
                if (key == _key) { return "Key is correct."; }

                for (int i = 0; i <= key.Length - 1; i++)
                {
                    if (key[i] == _key[i])
                    {
                        _correctCount++;
                    }
                }
                Gate2Data.Gate2AttemptLog.Add($"{key} -- {_correctCount} / {_key.Length}");
                return $"Key is incorrect. {_correctCount} / {_key.Length} characters are correct.";
            }
            Gate2Data.Gate2AttemptLog.Add($"{key}");
            return $"Key is incorrect.";
        }

        public static string Check2BKeyIsCorrect(IConfiguration config, string key)
        {
            Gate2Data.Gate2B_AttemptLog.Add(key);
            if (key.Equals(config.GetValue<string>("Keys:0003")))
                return "Success";
            return "Failure";
        }
    }       
}
