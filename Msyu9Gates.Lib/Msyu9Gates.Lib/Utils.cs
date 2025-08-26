namespace Msyu9Gates.Lib;

public class Utils
{
    public static string ToRoman(int number)
    {
        if (number < 1 || number > 3999) return number.ToString();
        var numerals = new[]
        {
        (1000, "M"), (900, "CM"), (500, "D"), (400, "CD"),
        (100, "C"), (90, "XC"), (50, "L"), (40, "XL"),
        (10, "X"), (9, "IX"), (5, "V"), (4, "IV"), (1, "I")
    };
        var result = "";
        foreach (var (value, numeral) in numerals)
        {
            while (number >= value)
            {
                result += numeral;
                number -= value;
            }
        }
        return result;
    }

    public enum GateNumber
    {
        I = 1,
        II = 2,
        III = 3,
        IV = 4,
        V = 5,
        VI = 6,
        VII = 7,
        VIII = 8,
        IX = 9
    }

    public enum ChapterNumber
    {
        I = 1,
        II = 2,
        III = 3,
        IV = 4,
        V = 5,
        VI = 6,
        VII = 7,
        VIII = 8,
        IX = 9,
        X = 10,
        XI = 11,
        XII = 12,
        XIII = 13,
        XIV = 14,
        XV = 15,
        XVI = 16,
        XVII = 17,
        XVIII = 18,
        XIX = 19,
        XX = 20
    }

    public enum Difficulty
    {
        None,           /// <summary>No difficulty set / not applicable. Nothing to solve</summary> 
        Easy,           /// <summary>[ 1 - 8  HOURS  ]  Easiest difficulty level. Very simple puzzles, entry-level cryptography (easy pencil/paper solves), or riddles</summary> 
        Medium,         /// <summary>[ 1 - 3  DAYS   ]  Average difficulty level. Moderately challenging puzzles, stronger basic cryptography (solvable with pencil/paper), or riddles</summary> 
        Challenging,    /// <summary>[ 3 - 7  DAYS   ]  Intended as a challenge, but not too hard. Complex/Multitep puzzles, intermediate cryptography (solvable with modern cryptanalysis), or riddles</summary> 
        Hard,           /// <summary>[ 1 - 4  WEEKS  ]  Very difficult. Fragmented puzzles with gate-keeping, advaned cryptography (requires specialized knowledge or tools), meta-riddles requiring specific domain/community knowledge</summary> 
        Extreme,        /// <summary>[ 1 - 3  MONTHS ]  Extremely difficult, requires special knowledge, tools and/or skills. Close to but not entirely unfair or impossible.</summary> 
        Msyu,           /// <summary>[ 3+     MONTHS ]  Borderline impossible, the type of challenge Msyu is notorius for crafting as punishment</summary> 
    }
}
