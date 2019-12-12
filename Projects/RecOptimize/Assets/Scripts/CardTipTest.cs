using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using UnityEngine;
using Tuyoo;

public class CardTipTest
{
    // 输出提示牌型文件路径
    private const string outputPath = "/Users/tugame/RecommendCardTest/tipTest/unityOutput.txt";
    // 输入牌型文件路径
    private const string inputPath = "/Users/tugame/RecommendCardTest/tipTest/input.txt";
    private bool _inited = false;

    public static List<CardInfo> GenCardList(int[] serverNums=null)
    {
        List<CardInfo> cardList;
        // 从服务器发过来的牌，0~53
        int[] _serverNums = {47,38,10,36,12,15,35,29,32,24,0,7,21,53,8,20,27,34,23,30};
        if(null != serverNums)
        {
            _serverNums = serverNums;
        }
        List<int> serverCards = new List<int>(_serverNums);
        // 转换成我当前的手牌
        cardList = CardCommon.GetCardListSorted(serverCards);
        return cardList;
    }

    public static List<List<CardInfo>> GetRecommendedCards(List<CardInfo> myCards, bool forward=true, List<CardInfo> topCards=null)
    {
        // Debug.LogError("myCards.Count=" + myCards.Count);
        // 主动出牌提示
        if (forward)
        {
            // return CardUtil.AIFindActiveCards(myCards);
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
            List<string> rC = new List<string>();
            foreach (CardInfo ci in myCards)
            {
                rC.Add(ci.serverNum.ToString());
            }
            DizhuPromptAI_Freeout_V1 AI = new DizhuPromptAI_Freeout_V1();
            List<int> ic = new List<int>();
            foreach (var i in rC)
            {
                ic.Add(intMap[i]);
            }
            List<List<int>> ret = AI.AI_promptCards(ic);
            return null;
        }
        else
        {
            Debug.Assert(null != topCards);
            CardType cardType = CardUtil.CheckCardType(topCards, out int lv);
            return CardUtil.AIFindWinCards(myCards, topCards, cardType);
        }
    }

    public static int[] GetServerCards(string lineStr)
    {
        List<int> cardList = new List<int>();
        List<string> numStrList = new List<string>(lineStr.Split(','));
        for(int i = 0; i < numStrList.Count; ++i)
        {
            cardList.Add(int.Parse(numStrList[i]));
        }
        return cardList.ToArray();
    }

    public static void PrintRecommondedCards()
    {
        // 主动出牌
        int groupCount = 0;
        FileStream fs = new FileStream(inputPath, FileMode.Open);
        StreamReader fsReader = new StreamReader(fs);
        StreamWriter fsWriter = new StreamWriter(outputPath);
        string lineStr;
        while(null != (lineStr = fsReader.ReadLine()))
        {
            ++groupCount;
            List<CardInfo> cardList = GenCardList(GetServerCards(lineStr));
            List<List<CardInfo>> recCards = GetRecommendedCards(cardList);
            WriteToFile(fsWriter, recCards, groupCount);
        }
        fs.Close();
        fsWriter.Close();
    }

    /// <summary>
    /// 把生成的只能提示结果写入到文件
    /// </summary>
    /// <param name="recCards"></param>
    public static void WriteToFile(StreamWriter fsWriter, List<List<CardInfo>> recCards, int groupCount)
    {
        Debug.Log("----------outCardList forwardly begin----------");
        Debug.Log("outCardList the count of tips is: " + recCards.Count);
        fsWriter.WriteLine("group" + groupCount.ToString());
        fsWriter.Write("[ ");
        for(int i = 0; i < recCards.Count; ++i)
        {
            if(0 != i){
                fsWriter.Write("  ");
            }
            StringBuilder sb = new StringBuilder();
            // sb.Append("outCardList:");
            sb.Append("[ ");
            for(int j = 0; j < recCards[i].Count; ++j)
            {
                if(0 != j)
                    sb.Append(", ");
                sb.Append("'");
                sb.Append(ConvertServerCardToReadable(recCards[i][j].serverNum));
                sb.Append("'");
            }
            sb.Append(" ]");
            if(i != recCards.Count - 1){
                sb.Append(",");
            }
            // Debug.Log(sb.ToString());
            if(i != recCards.Count - 1){
                fsWriter.WriteLine(sb.ToString());
            }
            else{
                fsWriter.Write(sb.ToString());
            }
        }
        fsWriter.Write(" ]\n");
        Debug.Log("----------outCardList forwardly end----------");
    }

    public static string ConvertServerCardToReadable(int serverNum)
    {
        int[] _cardToPoint = {11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             13, 14};
        string[] _pointToHumanCard = {"3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A", "2", "w", "W"};
        return _pointToHumanCard[_cardToPoint[serverNum]];
    }
}

// 主动出牌测试
public class CardTipsTestByServerNumFree
{
    // 输出提示牌型文件路径
    private const string outputPath = "/Users/tugame/RecommendCardTest/tipTest/freeout/unityOutput.txt";
    // 输入牌型文件路径
    private const string inputPath = "/Users/tugame/RecommendCardTest/tipTest/freeout/input.txt";
    private const string timeOutputPath = "/Users/tugame/RecommendCardTest/tipTest/freeout/timeSpend.txt";
    private bool _inited = false;

    public static List<List<string>> GetRecommendedCards(List<int> myCards, bool forward=true, List<CardInfo> topCards=null)
    {
        // 主动出牌提示
        if (forward)
        {
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
            DizhuPromptAI_Freeout_V1 AI = new DizhuPromptAI_Freeout_V1();
            List<string> rC = AI._transToReadableCards(myCards);
            List<int> ic = new List<int>();
            foreach (var i in rC)
            {
                ic.Add(intMap[i]);
            }
            List<List<int>> ret = AI.AI_promptCards(ic);
            List<List<string>> strRet = new List<List<string>>();
            foreach(var r in ret){
                List<string> rdeckCards = AI._transToReadableCards(r);
                strRet.Add(rdeckCards);
            }
            return strRet;
        }
        else
        {
            Debug.Assert(null != topCards);
            CardType cardType = CardUtil.CheckCardType(topCards, out int lv);
            return null;
        }
    }

    public static void PrintRecommondedCards()
    {
        // 主动出牌
        int groupCount = 0;
        FileStream fs = new FileStream(inputPath, FileMode.Open);
        StreamReader fsReader = new StreamReader(fs);
        StreamWriter fsWriter = new StreamWriter(outputPath);
        StreamWriter timeWriter = new StreamWriter(timeOutputPath);
        string lineStr;
        while(null != (lineStr = fsReader.ReadLine()))
        {
            ++groupCount;
            DateTime beforeTime = DateTime.Now;
            List<List<string>> recCards = GetRecommendedCards(GetServerCards(lineStr));
            double timeInterval = (DateTime.Now - beforeTime).TotalMilliseconds;
            RecordTime(timeWriter, timeInterval);
            WriteToFile(fsWriter, recCards, groupCount);
        }
        fs.Close();
        fsWriter.Close();
        timeWriter.Close();
    }

    public static void RecordTime(StreamWriter timeWriter, double timeInterval){
        timeWriter.WriteLine(timeInterval);
    }

    /// <summary>
    /// 把生成的只能提示结果写入到文件
    /// </summary>
    /// <param name="recCards"></param>
    public static void WriteToFile(StreamWriter fsWriter, List<List<string>> recCards, int groupCount)
    {
        // Debug.Log("----------outCardList forwardly begin----------");
        // Debug.Log("outCardList the count of tips is: " + recCards.Count);
        fsWriter.WriteLine("group" + groupCount.ToString());
        fsWriter.Write("[ ");
        for(int i = 0; i < recCards.Count; ++i)
        {
            if(0 != i){
                fsWriter.Write("  ");
            }
            StringBuilder sb = new StringBuilder();
            // sb.Append("outCardList:");
            sb.Append("[ ");
            for(int j = 0; j < recCards[i].Count; ++j)
            {
                if(0 != j)
                    sb.Append(", ");
                sb.Append("'");
                sb.Append(recCards[i][j]);
                sb.Append("'");
            }
            sb.Append(" ]");
            if(i != recCards.Count - 1){
                sb.Append(",");
            }
            // Debug.Log(sb.ToString());
            if(i != recCards.Count - 1){
                fsWriter.WriteLine(sb.ToString());
            }
            else{
                fsWriter.Write(sb.ToString());
            }
        }
        fsWriter.Write(" ]\n");
        // Debug.Log("----------outCardList forwardly end----------");
    }

    public static List<int> GetServerCards(string lineStr)
    {
        List<int> cardList = new List<int>();
        List<string> numStrList = new List<string>(lineStr.Split(','));
        for(int i = 0; i < numStrList.Count; ++i)
        {
            cardList.Add(int.Parse(numStrList[i]));
        }
        return cardList;
    }

    public static string ConvertServerCardToReadable(int serverNum)
    {
        int[] _cardToPoint = {11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             13, 14};
        string[] _pointToHumanCard = {"3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A", "2", "w", "W"};
        return _pointToHumanCard[_cardToPoint[serverNum]];
    }
}

// 跟牌测试
public class CardTipsTestByServerNumFollow
{
    // 输出提示牌型文件路径
    private const string outputPath = "/Users/tugame/RecommendCardTest/tipTest/follow/unityOutputFollow.txt";
    // 输入牌型文件路径
    private const string inputPath = "/Users/tugame/RecommendCardTest/tipTest/follow/input.txt";
    private const string timeOutputPath = "/Users/tugame/RecommendCardTest/tipTest/follow/timeSpendFollow.txt";
    private bool _inited = false;

    public static List<List<string>> GetRecommendedCards(List<int> myCards, List<int> topCards)
    {
        // 跟牌提示
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
        DizhuPromptAI_Follow_V1 AI = new DizhuPromptAI_Follow_V1();
        List<string> myCardsStr = AI._transToReadableCards(myCards);
        List<string> topCardsStr = AI._transToReadableCards(topCards);
        List<int> myCardsTrans = new List<int>();
        List<int> topCardsTrans = new List<int>();
        foreach (var i in myCardsStr)
        {
            myCardsTrans.Add(intMap[i]);
        }
        foreach (var i in topCardsStr)
        {
            topCardsTrans.Add(intMap[i]);
        }
        List<List<int>> ret = AI.AI_promptCards(topCardsTrans, myCardsTrans);
        List<List<string>> strRet = new List<List<string>>();
        foreach (var r in ret)
        {
            List<string> rdeckCards = AI._transToReadableCards(r);
            strRet.Add(rdeckCards);
        }
        return strRet;
    }

    public static void RecordTime(StreamWriter timeWriter, double timeInterval){
        timeWriter.WriteLine(timeInterval);
    }

    /// <summary>
    /// 把生成的只能提示结果写入到文件
    /// </summary>
    /// <param name="recCards"></param>
    public static void WriteToFile(StreamWriter fsWriter, List<List<string>> recCards, int groupCount)
    {
        // Debug.Log("----------outCardList forwardly begin----------");
        // Debug.Log("outCardList the count of tips is: " + recCards.Count);
        fsWriter.WriteLine("group" + groupCount.ToString());
        // fsWriter.Write("[\n");
        for(int i = 0; i < recCards.Count; ++i)
        {
            // if(0 != i){
            //     fsWriter.Write("  ");
            // }
            StringBuilder sb = new StringBuilder();
            // sb.Append("outCardList:");
            sb.Append("[");
            for(int j = 0; j < recCards[i].Count; ++j)
            {
                if(0 != j)
                    sb.Append(",");
                sb.Append(" '");
                sb.Append(recCards[i][j]);
                sb.Append("'");
                if(recCards[i].Count - 1== j){
                    sb.Append(" ");
                }
            }
            sb.Append("]");
            // if(i != recCards.Count - 1){
            //     sb.Append(",");
            // }
            // Debug.Log(sb.ToString());
            if(i != recCards.Count - 1){
                fsWriter.WriteLine(sb.ToString());
            }
            else{
                fsWriter.Write(sb.ToString());
            }
        }
        if (0 != recCards.Count)
        {
            fsWriter.Write("\n");
        }
        // fsWriter.Write("\n]\n");
        // fsWriter.Write("\n");
        // Debug.Log("----------outCardList forwardly end----------");
    }

    public static List<int> GetServerCards(string lineStr)
    {
        List<int> cardList = new List<int>();
        List<string> numStrList = new List<string>(lineStr.Split(','));
        for(int i = 0; i < numStrList.Count; ++i)
        {
            cardList.Add(int.Parse(numStrList[i]));
        }
        return cardList;
    }

    public static string ConvertServerCardToReadable(int serverNum)
    {
        int[] _cardToPoint = {11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             11, 12, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10,
                             13, 14};
        string[] _pointToHumanCard = {"3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A", "2", "w", "W"};
        return _pointToHumanCard[_cardToPoint[serverNum]];
    }

    public static void PrintRecommondedCards()
    {
        // 主动出牌
        int groupCount = 0;
        FileStream fs = new FileStream(inputPath, FileMode.Open);
        StreamReader fsReader = new StreamReader(fs);
        StreamWriter fsWriter = new StreamWriter(outputPath);
        StreamWriter timeWriter = new StreamWriter(timeOutputPath);
        string lineStr;
        while(null != (lineStr = fsReader.ReadLine()))
        {
            ++groupCount;
            string topCards = lineStr;
            string myCards = fsReader.ReadLine();
            DateTime beforeTime = DateTime.Now;
            List<List<string>> recCards = GetRecommendedCards(GetServerCards(myCards), GetServerCards(topCards));
            double timeInterval = (DateTime.Now - beforeTime).TotalMilliseconds;
            RecordTime(timeWriter, timeInterval);
            WriteToFile(fsWriter, recCards, groupCount);
        }
        fs.Close();
        fsWriter.Close();
        timeWriter.Close();
    }
}
