using System;
using System.Threading;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    DizhuPromptAI_Freeout_V1 dai = new DizhuPromptAI_Freeout_V1();
    public static string TranIntListToString(List<int> intList)
    {
        StringBuilder sb = new StringBuilder("[");
        for(int i = 0; i < intList.Count; ++i){
            sb.Append(intList[i].ToString());
            if(i != intList.Count - 1)
                sb.Append(", ");
        }
        sb.Append("]");
        return sb.ToString();
    }
    // Start is called before the first frame update
    

    public void SortTest()
    {
        // CardTipTest.PrintRecommondedCards();
        List<int> intList = new List<int>{0, 1, 2, 3, 4, 5};
        // 从小到大
        intList.Sort();
        Debug.Log("default sort:" + TranIntListToString(intList));
        intList.Sort(delegate(int left, int right){
            // 从小到大
            return left - right;
        });
        Debug.Log("left - right sort:" + TranIntListToString(intList));
        intList.Sort(delegate(int left, int right){
            // 从大到小
            return right - left;
        });
        Debug.Log("right - left sort:" + TranIntListToString(intList));
    }
    public void SetTest()
    {
        List<string> strList = new List<string>{"test", "test", "test0", "test1"};
        Debug.Log("original");
        PrintStrList(strList);
        List<string> strSet = CopyTool.TransStrListToSet(strList);
        Debug.Log("after");
        PrintStrList(strSet);
    }
    public void PrintStrList(List<string> strList){
        foreach(string str in strList){
            Debug.Log("str:" + str);
        }
    }
    public void TimeSpanTest()
    {
        string creatTime = "2019-10-30 10:16:29";

        DateTime creatDateTime;
        DateTime.TryParse(creatTime, out creatDateTime);
        DateTime nowDT = DateTime.Now;
        DateTime cTc = Convert.ToDateTime(string.Format("{0}-{1}-{2}", creatDateTime.Year, creatDateTime.Month, creatDateTime.Day));
        DateTime nTc = Convert.ToDateTime(string.Format("{0}-{1}-{2}", nowDT.Year, nowDT.Month, nowDT.Day));
        int days = (nTc - cTc).Days;
        Debug.Log("days=" + days);
    }
    public void CommaTest(){
        StringBuilder sb = new StringBuilder("{");
        sb.Append("\"");
        Debug.Log("sb.ToString()=" + sb.ToString());
    }
    public void CombinationTest()
    {
        List<string> list = new List<string>{"a", "bb", "ccc"};
        int count = 2;
        int process = 0;
        List<string[]> ret = PermutationAndCombination<string>.GetCombination(list.ToArray(), count);
        // List<List<string>> comList = new List<List<string>>();
        // foreach(string str in list){
        //     List<string> inList = new List<string>();
        //     inList.Add(str);
        //     inList.AddRange(ret[process]);
        // }
        foreach(string[] strArr in ret){
            StringBuilder sb = new StringBuilder("[");
            foreach(string str in strArr){
                sb.Append(str);
            }
            sb.Append("]");
            Debug.Log("comb=" + sb.ToString());
        }
    }
    public void CopyTest(){
        DeepCopyTest dc1 = new DeepCopyTest();
        DeepCopyTest dc2 = (DeepCopyTest)CardRecomDeepCopy.DeepCopy(dc1);
        dc2.i = 9;
        dc2.test2.value = 999;
        Debug.Log("dc1.name=" + dc1.ClassName() + ", dc1.i=" + dc1.i + ", t2.value=" + dc1.test2.value + ", t3=" + dc1.t3);
        Debug.Log("dc2.name=" + dc2.ClassName() + ", dc2.i=" + dc2.i + ", t2.value=" + dc2.test2.value + ", t3=" + dc2.t3);
    }
    // public void StrSortTest()
    // {
    //     List<List<List<string>>> exhaustion = new List<List<List<string>>>{
    //         new List<List<string>>{
    //             new List<string>{"Q", "7", "6"},
    //             new List<string>{"8", "K", "3", "4"},
    //             new List<string>{"8", "A", "3"},
    //             new List<string>{"8", "A", "3"}
    //         },
    //         new List<List<string>>{
    //             new List<string>{"Q", "7", "6"},
    //             new List<string>{"8", "K", "3", "4"},
    //             new List<string>{"8", "A", "3"},
    //             new List<string>{"8", "A", "3"}
    //         }
    //     };
    //     List<List<List<string>>> exhaustionT = OrderTest.uniqueShunzi(exhaustion);
    //     OrderTest._uniqueShunzi(exhaustionT);
    //     Debug.Log("count1=" + exhaustion.Count + ", count2=" + exhaustionT.Count);
    // }
    public void StreakTest()
    {
        Dictionary<string, int> groups = new Dictionary<string, int>{
            {"3", 1},
            {"4", 1},
            {"5", 1},
            {"6", 1},
            {"7", 1},
            {"8", 1},
            {"9", 1}
        };
        // DizhuDeckAnalyzer dda = new DizhuDeckAnalyzer();
        // List<List<List<string>>> strLLL = dda._streakExhaustion(groups);
        // strLLL = dda._uniqueShunzi(strLLL);
        // Debug.Log("strLLL.Count=" + strLLL.Count);
        // foreach(var strLL in strLLL){
        //     foreach(var strL in strLL){
        //         MyPrint.PrintList(strL);
        //     }
        // }
    }

    public void DeepCopyTest2()
    {
        Dictionary<string, Test2> groups = new Dictionary<string, Test2>{
            // {"3", 1},
            // {"4", 1},
            // {"5", 1},
            // {"6", 1},
            // {"7", 1},
            // {"8", 1},
            {"9", new Test2()}
        };
        // CardRecomDeepCopy.DeepCopy(groups);
        Dictionary<string, Test2> groups2 = new Dictionary<string, Test2>(groups);
        groups2["9"].value = 99;
        Debug.Log("v1=" + groups["9"].value);
        Debug.Log("v2=" + groups2["9"].value);
    }
    public void Test6(){
        // List<string> topCards = new List<string>{"3", "4", "5", "6", "7"};
        // List<string> myDeckCards = new List<string>{"5", "6", "7", "8", "9", "10"};
        // List<List<string>> ret = dpaf._drudgery_DP_34567(topCards, myDeckCards);
        // Debug.Log("ret=" + MyPrint.GetTLLStr(ret));
    }
    public void Test7(){
        // List<string> topCards = new List<string>{"3", "4", "5", "6", "7", "3", "4", "5", "6", "7"};
        // List<string> myDeckCards = new List<string>{"5", "6", "7", "8", "9", "10", "5", "6", "7", "8", "9", "10"};
        // List<List<string>> ret = dpaf._drudgery_DP_JJQQKK(topCards, myDeckCards);
        // Debug.Log("ret=" + MyPrint.GetTLLStr(ret));
    }
    public void Test8(){
        // List<string> topCards = new List<string>{"3", "4", "5", "3", "4", "5", "3", "4", "5"};
        // List<string> myDeckCards = new List<string>{"5", "6", "7", "8", "5", "6", "7", "8",  "5", "6", "7", "8"};
        // List<List<string>> ret = dpaf._drudgery_DP_QQQKKK(topCards, myDeckCards);
        // Debug.Log("ret=" + MyPrint.GetTLLStr(ret));
    }
    public void Test9(){
        Dictionary<string, string> dict = new Dictionary<string, string>{
            {"key1", "value1"},
            {"key2", "value2"}
        };
        string ret = StringTool.JsonDumpsStrStrDcit(dict);
        Debug.Log("ret=" + ret);
    }

    public void Test10(){
        DizhuPromptAI_Freeout_V1 AI = new DizhuPromptAI_Freeout_V1();
        Dictionary<string, int> intMap = new Dictionary<string, int>{
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
            {"K", 12},
            {"A", 0},
            {"2", 1},
            {"joker", 52},
            {"JOKER", 53}
        };
        // List<string> rC = new List<string>{"3", "4", "4", "5", "5", "6", "6", "7", "7", "8", "8", "8", "9", "9", "10", "10", "A", "J", "K", "Q"};
        // List<string> rC = new List<string>{"3", "4", "4", "5", "5", "6", "6", "7", "7"};
        // List<string> rC = AI._transToReadableCards(new List<int>{5, 18, 31, 44, 29, 26, 14, 41, 27, 0, 11, 32, 50, 47, 16, 53, 38});
        // List<string> rC = AI._transToReadableCards(new List<int>{9,22,35,48,7,1,40,18,2,39,13,8,16,41,45,47,25,29,52,26});
        // List<string> rC = AI._transToReadableCards(new List<int>{35,29,42,13,40,10,23,12,22,30,25,3,34,43,52,49,33,48,36,32});
        // List<string> rC = AI._transToReadableCards(new List<int>{15,18,32,34,7,29,3,23,22,26,21,38,31,0,37,51,50,9,45,25});
        List<string> rC = AI._transToReadableCards(new List<int>{6,19,32,45,36,16,15,42,18,50,4,41,28,3,49,51,48,2,27,0});
        // List<string> rC = new List<string>{"3", "4", "5", "5", "6", "7", "7", "8", "8", "10", "10", "10", "J", "Q", "Q", "K", "A", "2", "2", "2"};
        List<int> ic = new List<int>();
        foreach(var i in rC){
            ic.Add(intMap[i]);
        }
        // List<List<int>> x = AI.AI_promptCards(new List<int>{50, 37, 24, 11});
        DateTime time1 = DateTime.Now;
        List<List<int>> ret = AI.AI_promptCards(ic);
        DateTime time2 = DateTime.Now;
        Debug.Log("cost:" + (time2 - time1).TotalMilliseconds);
        Debug.Log("over"); 
        // new DizhuDeckAnalyzer().exhaustionDivision_prompt(rC);
    }
    public void Test11(){
        Dictionary<string, int> oriDic = new Dictionary<string, int>{{"0", 0}, {"1", 1}};
        Dictionary<string, int> newDic = new Dictionary<string, int>(oriDic);
        oriDic["0"] = 100;
        newDic["1"] = 111;
        Debug.Log("ori:" + oriDic["0"] + ", " + oriDic["1"]);
        Debug.Log("ori:" + newDic["0"] + ", " + newDic["1"]);
    }
    public void Test12(){
        List<List<string>> results = new List<List<string>>{
            new List<string>{"5", "5"},
            new List<string>{"4", "4"},
            new List<string>{"7", "7"},
            new List<string>{"6", "6"},
            new List<string>{"3"},
            new List<string>{"5", "5", "6", "6", "7", "7"},
            new List<string>{"4", "4", "5", "5", "6", "6", "7", "7"},
            new List<string>{"4", "4", "5", "5", "6", "6"}
        };
        List<string> myDeckCards = new List<string>{"3", "4", "4", "5", "5", "6", "6", "7", "7"};
        int ret = 3;
        List<List<string>> shunzi = new List<List<string>>();
        new DizhuPromptAI_Freeout_V1()._getTipsData(results, myDeckCards, ret, shunzi);
    }

    public void Test13(){
        // List<string> deckCards = new List<string>{"3", "4", "4", "5", "5", "6", "6", "7", "7"};
        List<string> deckCards = new List<string>{"4","4","6","7","Q","K","K","A","A"};
        // new DizhuDeckAnalyzer().exhaustionDivision_prompt(deckCards);
        List<BaseDeckPattern> patterns = new List<BaseDeckPattern>{
            new DeckPattern_joker_JOKER(),
            new DeckPattern_AAAA(),
            new DeckPattern_KKK(),
            new DeckPattern_QQQKKK(),
            new DeckPattern_QQ(),
            new DeckPattern_SINGLE()
        };
        new DizhuDeckAnalyzer().elementaryDivision(deckCards);
    }

    public void Test15(){
        Dictionary<string, List<List<string>>> _division = new Dictionary<string, List<List<string>>>{
            {"3", new List<List<string>>{ new List<string>{"9"}, new List<string>{"K"}, new List<string>{"joker"}}},
            {"34567", new List<List<string>>{new List<string>{"6","7","8","9","10"}}},
            {"KKK", new List<List<string>>{new List<string>{"10","10","10"}, new List<string>{"A","A","A"}}},
            {"QQ", new List<List<string>>{new List<string>{"3","3"}, new List<string>{"4","4"}, new List<string>{"2","2"}}},
            };
        new DizhuDeckAnalyzer().aggressiveDivision(_division, true);
    }

    public void Test16(){
        // _getTipsScore方法检测
        DizhuPromptAI_Freeout_V1 dai = new DizhuPromptAI_Freeout_V1();
        // [(['3', '4', '5', '6', '7'], 15050000), 
        // (['3', '4', '5', '6', '7', '8'], 15060000), 
        // (['5'], 30000000), 
        // (['7'], 50000000), 
        // (['8'], 60000000), 
        // (['8', '8'], 61000000), 
        // (['10', '10'], 81000000), 
        // (['10', 'J', 'Q', 'K', 'A'], 85050000), 
        // (['Q'], 100000000), 
        // (['5', '2', '2', '2'], 132030000), 
        // (['7', '2', '2', '2'], 132050000), 
        // (['8', '2', '2', '2'], 132060000), 
        // (['Q', '2', '2', '2'], 132100000), 
        // (['8', '8', '2', '2', '2'], 133060600), 
        // (['10', '10', '2', '2', '2'], 133080800)]
        // List<List<string>> cards = new List<List<string>>{
        //     new List<string>{"8", "8"},
        //     new List<string>{"10", "10"},
        //     new List<string>{"10", "J", "Q", "K", "A"},
        //     new List<string>{"Q"},
        //     new List<string>{"5", "2", "2", "2"},
        //     new List<string>{"7", "2", "2", "2"},
        //     new List<string>{"8", "2", "2", "2"},
        //     new List<string>{"Q", "2", "2", "2"},
        //     new List<string>{"8", "8", "2", "2", "2"},
        //     new List<string>{"10", "10", "2", "2", "2"},
        //     new List<string>{"3", "4", "5", "6", "7"},
        //     new List<string>{"3", "4", "5", "6", "7", "8"},
        //     new List<string>{"5"},
        //     new List<string>{"7"},
        //     new List<string>{"8"}
        // };
        List<List<string>> cards = new List<List<string>>{
            new List<string>{"3", "4", "5", "6", "6", "6", "7", "7", "7", "8", "8", "8"},
            new List<string>{"3", "4", "6", "6", "6", "7", "7", "7", "8", "8", "8", "K"}
        };
        // List<int> scores = new List<int>();
        // for(int i = 0; i < cards.Count; ++i){
        //     scores.Add(dai._getTipsScore(cards[i]));
        // }
        // int count = scores.Count;
        int score1 = dai._getTipsScore(cards[0]);
        int score2 = dai._getTipsScore(cards[1]);

        cards.Sort(delegate (List<string> left, List<string> right)
        {
            return dai._getTipsScore(left) - dai._getTipsScore(right);
        });
        int i;
    }
    public void Test17(){
        CardTipsTestByServerNumFree.PrintRecommondedCards();
        Debug.Log("free succeed");
    }
    public void Test18(){
        // 测试_exhaustionMatchPatterns方法
        List<string> myDeckCards = new List<string>{"10", "10", "10", "10", "8", "2", "2", "6", "3", "A", "A", "9", "4", "3", "7", "9", "K", "4", "joker", "A"};
        List<List<string>> singles = new List<List<string>>{
            new List<string>{"joker"},
            new List<string>{"8"},
            new List<string>{"K"},
            new List<string>{"7"},
            new List<string>{"6"}
        };
        List<List<string>> pairs = new List<List<string>>{
            new List<string>{"2", "2"},
            new List<string>{"4", "4"},
            new List<string>{"3", "3"},
            new List<string>{"9", "9"}
        };
        List<List<string>> tips = dai._exhaustionMatchPatterns(myDeckCards, singles, pairs);
        int i;
    }
    public void Test19(){
        // 测试_drudgery_DP_QQQKKK45方法
    }
    // 测试exhaustionDivision_removeShunzi
    public void Test20(){
        List<string> deckCards = new List<string>{"7", "7", "7", "7", "10", "8", "2", "A", "9", "Q", "J", "10", "Q", "J", "K"};
        List<List<List<string>>> shunzi = 
        new List<List<List<string>>>{
            new List<List<string>>{
                new List<string>{"8", "9", "10", "J", "Q"},
                new List<string>{"10", "J", "Q", "K", "A"}
                }, 
            new List<List<string>>{
                new List<string>{"10", "J", "Q", "K", "A"},
                new List<string>{"8", "9", "10", "J", "Q"}
                } 
        };
        List<Division> ret = new DizhuDeckAnalyzer().exhaustionDivision_removeShunzi(deckCards, shunzi);
    }
    public void Test21()
    {
        // 测试_greedyMerge
        Dictionary<string, List<List<string>>> division = new Dictionary<string, List<List<string>>>{
            {"QQ", new List<List<string>>{new List<string>{"4","4"},new List<string>{"K","K"},new List<string>{"A","A"}}},
            {"3", new List<List<string>>{new List<string>{"6"},new List<string>{"7"},new List<string>{"Q"}}},
            {"34567", new List<List<string>>{new List<string>{"10","6","7","8","9"},new List<string>{"10","9","J","K","Q"}}}
        };
        new DizhuDeckAnalyzer()._greedyMerge(division);
        int i = 10;
        i += 1;
    }

    private void Test22(){
        DateTime be = DateTime.Now;
        Thread.Sleep(1000);

        Debug.Log((DateTime.Now - be).TotalMilliseconds);
        Debug.Log((DateTime.Now - be).Milliseconds);
    }
    // 测试 _updateShunzi
    private void Test23(){
        List<string> tips = new List<string>{"3", "3"};
        List<List<string>> shunzi = new List<List<string>>{
            new List<string>{"8","9","10","J","Q"},
            new List<string>{"10","J","Q","K","A"}
        };
        List<string> cards = new List<string>{"7", "7", "7", "7", "3", "10", "8", "3", "2", "A", "9", "Q", "J", "10", "Q", "J", "K"};
        Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);

        List<List<List<string>>> ret = new DizhuPromptAI_Freeout_V1()._updateShunzi(tips, shunzi, groups);
    }

    private void Test24(){
        Debug.Log("Test24");
        CardTipsTestByServerNumFollow.PrintRecommondedCards();
        Debug.Log("follow succeed");
    }

    private void Test25(){
        DizhuPromptAI_Follow_V1 AI = new DizhuPromptAI_Follow_V1();
        List<int> topCards = new List<int>{30};
        List<int> myCards = new List<int>{6, 40, 17, 5, 15, 14, 32, 12, 4, 27, 35, 29, 52, 16, 11, 28, 36, 0, 48};
        List<List<int>> ret = AI.AI_promptCards(topCards, myCards);
        Debug.Log("before return");
    }
    void Start()
    {
        // SortTest();
        // SetTest();
        // TimeSpanTest();
        // CommaTest();
        // CombinationTest();
        // CopyTest();
        // StrSortTest(); 
        // StreakTest();
        // DeepCopyTest2();
        // Test20();
        // Test23();
        for(int i = 0; i < 5; ++i){
            Test10();
        }
    }
}

public class DeepCopyTest
{
    public int i = 0;
    public Test2 test2 = new Test2();
    public Test2 t3;
    public string ClassName(){
        return "DeepCopyTest";
    }
}
public class Test2
{
    public int value = 0;
}

public class OrderTest
{
    // public static List<List<List<string>>> uniqueShunzi(List<List<List<string>>> exhaustion)
    // {
    //     // exhuastion: 一手牌可用顺子的全排列组合
    //     // return: 排列组合去重
    //     List<List<List<string>>> tokens = new List<List<List<string>>>();
    //     List<string> strTokens = new List<string>();
    //     foreach(List<List<string>> ex in exhaustion){
    //         ex.Sort(delegate(List<string> left, List<string> right){
    //             if(left[0] != right[0]){
    //                 // 先按顺子第一张牌的大小排序
    //                 return CardTransfer.CARD_SCORE_MAP[left[0]] - CardTransfer.CARD_SCORE_MAP[right[0]];
    //             }
    //             else{
    //                 // 如果第一张牌一样，按长度排序
    //                 return left.Count - right.Count;
    //             }
    //         });
    //         List<string> tokenStrL = new List<string>();
    //         foreach(List<string> strL in ex){
    //             tokenStrL.Add(string.Join("-", strL));
    //         }
    //         strTokens.Add(string.Join("#", tokenStrL));
    //     }
    //     List<string> setTokens = CopyTool.TransStrListToSet(strTokens);
    //     List<List<string>> tokerLL = new List<List<string>>();
    //     foreach(string tk in setTokens){
    //         tokerLL.Add(new List<string>(tk.Split('#')));
    //     }
    //     foreach(List<string> strL in tokerLL){
    //         List<List<string>> tempTLL = new List<List<string>>();
    //         foreach(string str in strL){
    //             List<string> tokenL = new List<string>(str.Split('-'));
    //             tempTLL.Add(tokenL);
    //         }
    //         tokens.Add(tempTLL);
    //     }
    //     return tokens;
    // }

    // public static List<List<string>> _uniqueShunzi(List<List<List<string>>> exhaustion)
    // {
    //     // exhuastion: 一手牌可用顺子的全排列组合
    //     // return: 排列组合去重
    //     List<List<List<string>>> tokens = new List<List<List<string>>>();
    //     foreach(List<List<string>> ex in exhaustion){
    //         ex.Sort(delegate(List<string> left, List<string> right){
    //             if(left[0] != right[0]){
    //                 // 先按顺子第一张牌的大小排序
    //                 return CardTransfer.CARD_SCORE_MAP[left[0]] - CardTransfer.CARD_SCORE_MAP[right[0]];
    //             }
    //             else{
    //                 // 如果第一张牌一样，按长度排序
    //                 return left.Count - right.Count;
    //             }
    //         });
    //         // PrintStrLL(ex); 
    //     }
    //     return null;
    // }

    public static void PrintStrLL(List<List<string>> strLL)
    {
        Debug.Log("PrintStrLL");
        StringBuilder sbOut = new StringBuilder("[");
        foreach(var strL in strLL){
            StringBuilder sb = new StringBuilder("[");
            foreach(var str in strL){
                sb.Append(str);
                sb.Append(",");
            }
            sb.Append("]");
            sbOut.Append(sb.ToString());
            sbOut.Append(", ");
        }
        // Debug.Log("strLL:" + sbOut.ToString());
        List<string> tokenStrL = new List<string>();
        foreach (List<string> strL in strLL)
        {
            tokenStrL.Add(string.Join("-", strL));
        }
        Debug.Log(string.Join("#", tokenStrL));
    }

    public static void PrintStrL(List<string> strL){
        StringBuilder sb = new StringBuilder();
    }
}

public static class MyPrint
{
    public static string GetListStr<T>(List<T> strL){
        StringBuilder sb = new StringBuilder("[");
        int count = 0;
        foreach(var str in strL){
            sb.Append(str.ToString());
            if(count != strL.Count - 1){
                sb.Append(", ");
            }
        }
        sb.Append("]");
        return sb.ToString();
    }

    public static string GetTLLStr<T>(List<List<T>> tLL){
        StringBuilder sbo = new StringBuilder("[");
        int counto = 0;
        foreach(List<T> tL in tLL){
            StringBuilder sbi = new StringBuilder("[");
            int counti = 0;
            foreach(T t in tL){
                sbi.Append(t.ToString());
                if(counti != tL.Count - 1){
                    sbi.Append(", ");
                }
                ++counti;
            }
            sbi.Append("]");
            sbo.Append(sbi.ToString());
            if(counto != tLL.Count - 1){
                sbo.Append(", ");
            }
        }
        sbo.Append("]");
        return sbo.ToString();
    }

    public static string GetTLLLStr<T>(List<List<List<T>>> tLLL){
        StringBuilder sb = new StringBuilder("[");
        int count = 0;
        foreach(List<List<T>> tLL in tLLL){
            sb.Append(GetTLLStr(tLL));
            if(count != tLLL.Count - 1){
                sb.Append(", ");
            }
        }
        sb.Append("]");
        return sb.ToString();
    }

    public static string PrintBoolList(List<bool> strL){
        StringBuilder sb = new StringBuilder("[");
        int count = 0;
        foreach(var bl in strL){
            sb.Append(bl.ToString());
            if(count != strL.Count - 1){
                sb.Append(", ");
            }
        }
        sb.Append("]");
        Debug.Log(sb.ToString());
        return sb.ToString();
    }

    public static string GetDLLTStr<T>(Dictionary<string, List<List<T>>> dict)
    {
        // {'AAAA': [['Q', 'Q', 'Q', 'Q']]}
        StringBuilder sb = new StringBuilder("{");
        int countO = 0;
        foreach(string key in dict.Keys){
            sb.Append("\"");
            sb.Append(key);
            sb.Append("\"");
            sb.Append(": ");
            sb.Append("[");
            int countI = 0;
            foreach(List<T> vL in dict[key]){
                sb.Append("[");
                int countII = 0;
                foreach(T v in vL){
                    sb.Append(v.ToString());
                    if(vL.Count - 1 != countII)
                        sb.Append(", ");
                    ++countII;
                }
                sb.Append("]");
                if(dict[key].Count - 1 != countI)
                    sb.Append(", ");
                ++countI;
            }
            sb.Append("]");
            if(dict.Count - 1 != countO)
                sb.Append(", ");
            ++countO;
        }
        sb.Append("}");
        return sb.ToString();
    }

    public static string GetStrIntDicStr(Dictionary<string, int> dict){
        StringBuilder sb = new StringBuilder("{");
        int count = 0;
        foreach(string key in dict.Keys){
            sb.Append("\"");
            sb.Append(key);
            sb.Append("\"");
            sb.Append(": ");
            sb.Append(dict[key].ToString()); 
            if(count != dict.Count - 1){
                sb.Append(", ");
            }
        }
        sb.Append("}");
        return sb.ToString();
    }

    public static string GetPatterNames(List<BaseDeckPattern> bdkList){
        string ret = "";
        foreach(var bdk in bdkList){
            ret += ", ";
            ret += bdk.patternName();
        }
        return ret;
    }
}
