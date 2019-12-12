using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanReadableCardHelper 
{
    public static Dictionary<string, string> _READABLE_CARD_MAP = new Dictionary<string, string>{
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

    public static Dictionary<string, int> _REVERSE_MAP = new Dictionary<string, int>{
        {"A", 0},
        {"2", 1},
        {"3", 2},
        {"4", 3},
        {"5", 4},
        {"6", 5},
        {"7", 6},
        {"8", 7},
        {"9", 8},
        {"10", 9},
        {"J", 10},
        {"Q", 11},
        {"K", 12}
    };

    public static string readableOne(int card)
    {
        return _READABLE_CARD_MAP[card.ToString()];
    }

    public static List<string> readableRange(string minReadable, string maxReadable)
    {
        Debug.Assert(_REVERSE_MAP.ContainsKey(minReadable));
        Debug.Assert(_REVERSE_MAP.ContainsKey(maxReadable));
        List<string> ret = new List<string>();
        int minIdx = _REVERSE_MAP[minReadable];
        int maxIdx = _REVERSE_MAP[maxReadable];
        if(maxIdx < minIdx){
            return ret;
        }
        else{
            for(int i = minIdx; i < maxIdx + 1; ++i){
                ret.Add(_READABLE_CARD_MAP[i.ToString()]);
            }
            return ret;
        }
    }

    public static List<string> readableList(List<int> cardList)
    {
        List<string> ret = new List<string>();
        // foreach(int x in cardList){
        //     ret.Add(_READABLE_CARD_MAP[x.ToString()]);
        // }
        for(int i = 0; i < cardList.Count; ++i){
            ret.Add(_READABLE_CARD_MAP[cardList[i].ToString()]);
        }
        return ret;
    }
}
