using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewFollow : MonoBehaviour
{
    
}
public class TestTopCard
{
    public static List<List<int>> topCardsL = new List<List<int>>{
        new List<int>{2},
        new List<int>{2,15},
        new List<int>{2,15,28},
        new List<int>{2,15,28,3},
        new List<int>{2,15,28,3,16},
        new List<int>{2,3,4,5,6},
        new List<int>{2,3,4,15,16,17},
        new List<int>{2,15,28,3,16,29},
        new List<int>{2,15,28,3,16,29,4,5},
        new List<int>{2,15,28,3,16,29,4,17,5,18},
        new List<int>{2,15,28,41},
        new List<int>{2,15,28,41,3,4},
        new List<int>{2,15,28,41,3,16,4,17}
    };
    public static int groupCount = 0;
}
public class FollowTest
{
    // 输出提示牌型文件路径
    private const string outputPath = "/Users/tugame/RecommendCardTest/tipTest/follow/newFollowResult.txt";
    private const string timeOutputPath = "/Users/tugame/RecommendCardTest/tipTest/follow/newFollowTime.txt";

    public static void RecordTime(StreamWriter timeWriter, double timeInterval){
        timeWriter.WriteLine(timeInterval);
    }

    public static void RecordCards(StreamWriter fsWriter, List<string> topCards, List<string> myCards)
    {
        fsWriter.WriteLine("topCards:" + string.Join(",", topCards));
        fsWriter.WriteLine("myCards:" + string.Join(",", myCards));
    }
    /// <summary>
    /// 把生成的只能提示结果写入到文件
    /// </summary>
    /// <param name="recCards"></param>
    public static void WriteToFile(StreamWriter fsWriter, List<List<string>> allTips)
    {
        foreach(List<string> tip in allTips){
            fsWriter.WriteLine(string.Join(",", tip));
        }
        fsWriter.WriteLine();
    }

    public static List<int> GenMyCard(){
        List<int> myCards = new List<int>();
        List<int> allCards = new List<int>();
        for(int i = 0; i < 54; ++i){
            allCards.Add(i);
        }
        System.Random r1 = new System.Random();
        while(myCards.Count < 19){      // 随机生成19张牌
            int index = r1.Next(0, allCards.Count);
            myCards.Add(allCards[index]);
            allCards.RemoveAt(index);
        }
        return myCards;
    }
    public static void PrintRecommondedCards()
    {
        // 主动出牌
        StreamWriter fsWriter = new StreamWriter(outputPath);
        StreamWriter timeWriter = new StreamWriter(timeOutputPath);
        int testCount = 1000;
        for (int i = 0; i < testCount; ++i)
        {
            ++TestTopCard.groupCount;
            fsWriter.WriteLine("=============group" + TestTopCard.groupCount.ToString());
            List<int> myCards = GenMyCard();
            foreach (List<int> topCards in TestTopCard.topCardsL)
            {
                DateTime beforeTime = DateTime.Now;
                List<List<int>> recCards = FollowTipsHandler.Instance.GetAllTips(myCards, topCards);
                double timeInterval = (DateTime.Now - beforeTime).TotalMilliseconds;
                CheckCardsRepeat(recCards);
                RecordTime(timeWriter, timeInterval);
                RecordCards(fsWriter, TransCards(topCards), TransCards(myCards));
                WriteToFile(fsWriter, TransIntToStr(recCards));
            }
        }
        fsWriter.Close();
        timeWriter.Close();
    }
    public static void CheckCardsRepeat(List<List<int>> cardTips){
        Dictionary<int, int> cardGroup = new Dictionary<int, int>();
        foreach(List<int> tips in cardTips){
            cardGroup.Clear();
            foreach(int card in tips){
                if(cardGroup.ContainsKey(card)){
                    Debug.LogError("this card has already existed");
                }
                cardGroup[card] = 1;
            }
        }
    }
    public static List<string> TransCards(List<int> cards){
        List<string> ret = HumanReadableCardHelper.readableList(cards);
        ret.Sort(delegate(string left, string right){
            return CardTransferNew.cardValue[left] - CardTransferNew.cardValue[right];
        });
        return ret;
    }
    public static List<List<string>> TransIntToStr(List<List<int>> recCards){
        List<List<string>> ret = new List<List<string>>();
        foreach(List<int> cards in recCards){
            ret.Add(HumanReadableCardHelper.readableList(cards));
        }
        return ret;
    }
}
