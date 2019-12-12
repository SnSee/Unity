using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndividualAiUtil
{
    public static Dictionary<string, string> _READABLE_CARD_MAP = new Dictionary<string, string>
    {
        {"0", "A"},
        {"1", "2"},
        {"2", "3"},
        {"3", "4"},
        {"4", "5"},
        {"5", "6"},
        {"6", "7"},
        {"7", "8"},
        {"8", "9"},
        {"9", "10"},
        {"10", "J"},
        {"11", "Q"},
        {"12", "K"},
        {"13", "A"},
        {"14", "2"},
        {"15", "3"},
        {"16", "4"},
        {"17", "5"},
        {"18", "6"},
        {"19", "7"},
        {"20", "8"},
        {"21", "9"},
        {"22", "10"},
        {"23", "J"},
        {"24", "Q"},
        {"25", "K"},
        {"26", "A"},
        {"27", "2"},
        {"28", "3"},
        {"29", "4"},
        {"30", "5"},
        {"31", "6"},
        {"32", "7"},
        {"33", "8"},
        {"34", "9"},
        {"35", "10"},
        {"36", "J"},
        {"37", "Q"},
        {"38", "K"},
        {"39", "A"},
        {"40", "2"},
        {"41", "3"},
        {"42", "4"},
        {"43", "5"},
        {"44", "6"},
        {"45", "7"},
        {"46", "8"},
        {"47", "9"},
        {"48", "10"},
        {"49", "J"},
        {"50", "Q"},
        {"51", "K"},
        {"52", "w"},
        {"53", "W"}
    };

    public static Dictionary<string, int> CARD_SCORE_MAP = new Dictionary<string, int>()
    {
        {"3", 1},
        {"4", 2},
        {"5", 3},
        {"6", 4},
        {"7", 5},
        {"8", 6},
        {"9", 7},
        {"1", 8},
        {"J", 9},
        {"Q", 10},
        {"K", 11},
        {"A", 12},
        {"2", 13},
        {"joker", 14},
        {"JOKER", 15}
    };
    public static bool _VERBOSE = false;
    public static bool _VERBOSE1 = false;

}
