using static Msyu9Gates.Gate;

namespace Msyu9Gates.Data.Models
{
    public class GateModel
    {
        public int Id { get; set; }
        private Dictionary<string, string> Keys { get; set; } = new Dictionary<string, string>();
        public Difficulty GateDifficulty { get; set; } = Difficulty.None;
        public AttemptHistory history = new AttemptHistory();

        public enum Difficulty
        {
            None,       /// <summary>No difficulty set / not applicable. Nothing to solve</summary> 
            Easy,       /// <summary>[ 1 - 8  HOURS  ]  Easiest difficulty level. Very simple puzzles, entry-level cryptography (easy pencil/paper solves), or riddles</summary> 
            Medium,     /// <summary>[ 1 - 3  DAYS   ]  Average difficulty level. Moderately challenging puzzles, stronger basic cryptography (solvable with pencil/paper), or riddles</summary> 
            Challenge,  /// <summary>[ 3 - 7  DAYS   ]  Intended as a challenge, but not too hard. Complex/Multitep puzzles, intermediate cryptography (solvable with modern cryptanalysis), or riddles</summary> 
            Hard,       /// <summary>[ 1 - 4  WEEKS  ]  Very difficult. Fragmented puzzles with gate-keeping, advaned cryptography (requires specialized knowledge or tools), meta-riddles requiring specific domain/community knowledge</summary> 
            Extreme,    /// <summary>[ 1 - 3  MONTHS ]  Extremely difficult, requires special knowledge, tools and/or skills. Close to but not entirely unfair or impossible.</summary> 
            Msyu,       /// <summary>[ 3+     MONTHS ]  Borderline impossible, the type of challenge Msyu is notorius for crafting as punishment</summary> 
        }

        /// <summary>
        /// Identify which Key is being used for the stage of the gate.
        /// Options: A, B, C.
        /// </summary>
        public enum GateKeyType
        {
            A = 1,
            B = 2,
            C = 3
        }
    }
}
