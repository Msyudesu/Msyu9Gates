using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Msyu9Gates.Data;
using Msyu9Gates.Lib;
using Msyu9Gates.Utils;

namespace Msyu9Gates
{
    internal class Gate
    {
        internal string? Name { get; set; }
        private Dictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();
        internal Difficulty GateDifficulty { get; set; } = Difficulty.None;
        internal AttemptHistory history = new AttemptHistory();
        private IConfiguration _config;

        internal enum Difficulty
        {
            None,       /// <summary>No difficulty set / not applicable. Nothing to solve</summary> 
            Easy,       /// <summary>[ 1 - 8  HOURS  ]  Easiest difficulty level. Very simple puzzles, entry-level cryptography (easy pencil/paper solves), or riddles</summary> 
            Medium,     /// <summary>[ 1 - 3  DAYS   ]  Average difficulty level. Moderately challenging puzzles, stronger basic cryptography (solvable with pencil/paper), or riddles</summary> 
            Challenge,  /// <summary>[ 3 - 7  DAYS   ]  Intended as a challenge, but not too hard. Complex/Multitep puzzles, intermediate cryptography (solvable with modern cryptanalysis), or riddles</summary> 
            Hard,       /// <summary>[ 1 - 4  WEEKS  ]  Very difficult. Fragmented puzzles with gate-keeping, advaned cryptography (requires specialized knowledge or tools), meta-riddles requiring specific domain/community knowledge</summary> 
            Extreme,    /// <summary>[ 1 - 3  MONTHS ]  Extremely difficult, requires special knowledge, tools and/or skills. Close to but not entirely unfair or impossible.</summary> 
            Msyu,       /// <summary>[ 3+     MONTHS ]  Borderline impossible, the type of challenge Msyu is notorius for crafting as punishment</summary> 
        }

        internal enum GateKeyType
        { 
            A = 1,
            B = 2,
            C = 3
        }

        internal Gate(IConfiguration config)
        {
            _config = config;
        }

        internal string GetDifficult() => GateDifficulty switch
        {
            Difficulty.None => "None",
            Difficulty.Easy => "Easy",
            Difficulty.Medium => "Medium",
            Difficulty.Challenge => "Challenge",
            Difficulty.Hard => "Hard",
            Difficulty.Extreme => "Extremely Hard",
            Difficulty.Msyu => "Msyu's Usual, Utterly Unfair Insanity",
            _ => "Unknown"
        };

        internal int GetNumberOfKeys() => Keys.Count;

        internal GateResponse CheckKey(string _key, string? subKeyID = null)
        {
            string _correctKey = _config.GetValue<string>($"Keys:{subKeyID}") ?? "";

            var response = new GateResponse(key: _key, keyID: subKeyID, success: false);

            if (!string.IsNullOrWhiteSpace(_key) && !string.IsNullOrWhiteSpace(_correctKey))
            {
                this.history.SaveAttempt(_key);
                if (_key.Equals(_config.GetValue<string>($"Keys:{subKeyID}")))
                {
                    CheckSpecialConditions(_key);
                    response.Success = true;
                }
                response.Success = false;
            }
            response.Message = $"Key was blank or invalid -> {_key}";
            return response;
        }

        private void CheckSpecialConditions(string _key)
        {
            // Implement any special conditions for the gate here
            switch(_key)
            {
                case "0005": // Gate 2C
                    GateFlags.Gate2C_ClueEnabled = true;
                    break;                
                default:                    
                    break;
            }
        }
    }
}
