using Microsoft.EntityFrameworkCore;
using Msyu9Gates.Data;
using Msyu9Gates.Data.Models;
using Msyu9Gates.Lib;
using Msyu9Gates.Utils;

namespace Msyu9Gates
{
    public class GateManager : GateModel
    {        
        private IConfiguration _config;
        private ILogger _log;
        private IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private KeyManager _keyManager;

        public int GateId { get; set; } = 1; // Default to Gate I

        public GateManager(IConfiguration config, ILogger log, IDbContextFactory<ApplicationDbContext> dbContextFactory, KeyManager keyManager, int gateId)
        {
            this.GateId = gateId;

            _config = config;
            _log = log;
            _dbContextFactory = dbContextFactory;

            _keyManager = keyManager;

            this.Keys = new List<string>();
            this.AttemptHistories = new Dictionary<int, AttemptHistory>
            {
                { 1, new AttemptHistory() },
                { 2, new AttemptHistory() },
                { 3, new AttemptHistory() }
            };
        }

        /// <summary>
        /// Retrieves a string representation of the current difficulty level.
        /// </summary>
        /// <returns>A string that describes the difficulty level. Possible values include "None", "Easy", "Medium", 
        /// "Challenge", "Hard", "Extremely Hard", "Msyu's Usual, Utterly Unfair Insanity", or "Unknown"  if the
        /// difficulty level is not recognized.</returns>
        public string GetDifficulty() => GateDifficulty switch
        {
            Difficulty.None => "None",
            Difficulty.Easy => "Easy",
            Difficulty.Medium => "Medium",
            Difficulty.Challenging => "Challenging",
            Difficulty.Hard => "Hard",
            Difficulty.Extreme => "Extremely Hard",
            Difficulty.Msyu => "Msyu's Usual, Utterly Unfair Insanity",
            _ => "Unknown"
        };

        public List<string> GetHistory(int chapter) => AttemptHistories![chapter].GetAttempts();

        public void ResetHistory(int chapter) => AttemptHistories![chapter]!.ClearHistory();

        public GateResponse CheckKey(string _key, int _chapter)
        {
            string _correctKey = _config.GetValue<string>($"Keys:{Keys![_chapter - 1]}") ?? "";

            var response = new GateResponse(key: _key, chapter: _chapter, success: false);

            if (!string.IsNullOrWhiteSpace(_key) && !string.IsNullOrWhiteSpace(_correctKey))
            {
                this.AttemptHistories![_chapter].SaveAttempt(_key);
                if (_key.Equals(_correctKey))
                {
                    CheckSpecialConditions(_key);

                    _keyManager.DiscoverKey(_key).GetAwaiter().GetResult();

                    response.Success = true;
                    return response;
                }
                response.Message = $"Key was valid but incorrect -> \"{_key}\".\nTry again.";
                response.Success = false;
                return response;
            }
            response.Errors.Add($"Key was blank or invalid -> {_key}");
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
