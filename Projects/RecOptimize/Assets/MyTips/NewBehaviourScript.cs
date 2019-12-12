using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CardTransferNew
{
    public static string[] intToStr = {
        "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K",
        "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K",
        "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K",
        "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K",
        "w", "W"
    };
    public static Dictionary<string, int> cardValue = new Dictionary<string, int>{
        {"3", 3},
        {"4", 4},
        {"5", 5},
        {"6", 6},
        {"7", 7},
        {"8", 8},
        {"9", 9},
        {"10", 10},
        {"J", 11},
        {"Q", 12},
        {"K", 13},
        {"A", 14},
        {"2", 15},
        {"w", 16},
        {"W", 17}
    };
    public static List<string> STREAKABLE = new List<string> { "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
    public static List<string> streakCards = new List<string> { "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" };
    public static void Init(){
    }
}

public class SortedCards
{
    private static SortedCards _instance = null;
    public static SortedCards Instance{
        get{
            return null == _instance ? _instance = new SortedCards() : _instance;
        }
    }
    public List<string> myCards;
    private List<string> _singles;
    public List<string> singles{
        get{ return _singles;}
        set{
            _singles = value;
            _singles.Sort(CardValueComp);
            // 检测是否有火箭
            if(_singles.Count >= 2 && "W" == _singles[_singles.Count - 1] && "w" == _singles[_singles.Count - 2]){
                rocket.Add("w");
                rocket.Add("W");
            }
        }
    }
    private List<string> _pairs;
    public List<string> pairs{
        get{ return _pairs;}
        set{
            // 生成双顺
            _pairs = value;
            _pairs.Sort(CardValueComp);
            Gen23Shunzi(shuangshun, _pairs, 3);
        }
    }
    private List<string> _threes;
    public List<string> threes{
        get{return _threes;}
        set{
            _threes = value;
            _threes.Sort(CardValueComp);
            Gen23Shunzi(sanshun, _threes, 2);
        }
    }
    private List<string> _bombs;
    public List<string> bombs{
        get{return _bombs;}
        set{
            _bombs = value;
            _bombs.Sort(CardValueComp);
        }
    }
    public List<string> rocket;
    public List<string> shunzi;             // 只计算最小的顺子
    public List<string> shuangshun;        // 只计算最小的双顺
    public List<string> sanshun;            // 只计算最小的三顺
    private int singleCheckTimes;
    private int pairCheckTiems;
    private int threeCheckTimes;
    private int shuangshunCheckTimes;
    private int sanshunCheckTimes;
    private int bombCheckTimes;
    private int sandai1CheckTimes;
    private int sandai2CheckTimes;
    private int feijidai21CheckTimes;
    private int feijidai22CheckTiems;
    private List<List<string>> tipsQueue;       // 一次生成多个提示时只返回一个提示，其他的放在该队列里
    private SortedCards(){
        RenewSortedCards();
    }
    private void Gen23Shunzi(List<string> dest, List<string> src, int validCount){
        // 生成连对或飞机
        if(null == src || src.Count < validCount){
            return;
        }
        int start, end;
        List<int> cardsNumList = new List<int>();
        for(int i = 0; i < pairs.Count; ++i){
            cardsNumList.Add(1);
        }
        CardTipHandler.Instance.GenShunziIndex(cardsNumList, out start, out end);
        if(-1 != start && -1 != end && end - start >= validCount - 1){
            for(int i = start; i <= end; ++i){
                dest.Add(src[i]);
            }
            for(int i = end; i >= start; --i){
                src.RemoveAt(i);
            }
        }
    }

    private void SetMinCard(List<string> pattern, out string minCard){
        minCard = (null != pattern && 0 != pattern.Count) ? pattern[0] : null;
    }

    /// <summary>
    /// 重置分类结果供下次使用
    /// </summary>
    public void RenewSortedCards(){
        shunzi = new List<string>();
        shuangshun = new List<string>();
        sanshun = new List<string>();
        singleCheckTimes = 0;
        pairCheckTiems = 0;
        threeCheckTimes = 0;
        shuangshunCheckTimes = 0;
        sanshunCheckTimes = 0;
        bombCheckTimes = 0;
        sandai1CheckTimes = 0;
        sandai2CheckTimes = 0;
        feijidai21CheckTimes = 0;
        feijidai22CheckTiems = 0;
    }
    public int CardValueComp(string left, string right){
        return CardTransferNew.cardValue[left] - CardTransferNew.cardValue[right];
    }
    public bool RightCardBigger(string leftCard, string rightCard){
        return CardTransferNew.cardValue[rightCard] > CardTransferNew.cardValue[leftCard];
    }

    public List<string> GenNextTip(){
        List<string> ret = new List<string>();
        if(0 != tipsQueue.Count){
            ret = tipsQueue[0];
            tipsQueue.RemoveAt(0);
            return ret;
        }
        // string minSingle, minPair, minThree, minShunzi, minShuangshun, minSanshun, minBomb;
        List<string>[] patterns = new List<string>[7]{singles, pairs, threes, shunzi, shuangshun, sanshun, bombs};
        string[] mins = new string[7];
        for(int i = 0; i < 7; ++i){
            SetMinCard(patterns[i], out mins[i]);
        }
        List<int> minsIndex = new List<int>{0, 1, 2, 3, 4, 5, 6};
        minsIndex.Sort(delegate(int left, int right){
            if(null == mins[left]){
                return -1;
            }
            else if(null == mins[right]){
                return 1;
            }
            else{
                return CardTransferNew.cardValue[mins[left]] - CardTransferNew.cardValue[mins[right]];
            }
        });
        if(null != mins[minsIndex[0]]){
            switch(minsIndex[0]){
                case 0:             // 单张
                    if(0 == singleCheckTimes){
                        ret.Add(singles[0]);
                        singles.RemoveAt(0);
                        if(null != patterns[2]){
                            tipsQueue.Add(new List<string>{threes[0], threes[0], threes[0], ret[0]});
                            threes.RemoveAt(0);
                        }
                        else if(null != sanshun){
                            if(singles.Count >= sanshun.Count){
                                List<string> inList = new List<string>();
                                for(int i = 0; i < sanshun.Count; ++i){
                                    inList.Add(sanshun[i]);
                                    inList.Add(sanshun[i]);
                                    inList.Add(sanshun[i]);
                                    inList.Add(singles[i]);
                                }
                            }
                        }
                    }
                    break;
                case 1:             // 对子
                    break;
                case 2:             // 三张
                    break;
                case 3:             // 顺子
                    break;
                case 4:             // 连对
                    break;
                case 5:             // 飞机
                    break;
                case 6:             // 炸弹
                    break;
            }
        }

        // 只有无其他牌型时才推荐出炸弹
        if(null != bombs && bombs.Count * 4 == myCards.Count){
            ret.Add(bombs[0]);
            ret.Add(bombs[0]);
            ret.Add(bombs[0]);
            ret.Add(bombs[0]);
        }
        return ret;
    }
    private List<string> Sandai1or2(string minThree, string minSingle, string minPair){
        if (null != minSingle)
        {
            if (null != minPair)
            {
                if (RightCardBigger(minSingle, minPair))
                {
                    if (RightCardBigger(minSingle, "2"))
                    {
                        return new List<string> { minThree, minThree, minThree, minSingle };
                    }
                    else
                    {
                        return new List<string> { minThree, minThree, minThree };
                    }
                }
            }
        }
        return null;
    }
}
public class CardTipHandler
{
    private static CardTipHandler _instance = null;
    public static CardTipHandler Instance{
        get{
            return null == _instance ? _instance = new CardTipHandler() : _instance;
        }
    }

    private List<int> _myCardsInt;          // 我的手牌，范围是0~53
    private List<int> myCardsInt{
        get{return _myCardsInt;}
        set{
            _myCardsInt = value;
            myCardsStr.Clear();
            foreach(int card in value){
                myCardsStr.Add(CardTransferNew.intToStr[card]);
            }
            SortedCards.Instance.myCards = myCardsStr;
        }
    }       
    private List<string> myCardsStr = new List<string>();     // 我的手牌，范围是'3'~'2'及大小王
    private Dictionary<string, int> cardGroup;               // 记录同大小手牌的数量
    private List<List<string>> allTips;   // 已生成的提示
    private bool allTipsChecked;        // 是否已生成所有的提示
    private int getTipProgress = 0;     // 如果以生成所有的提示，再试图获取提示时从allTips中选取，此变量表示当前提示索引
    private CardTipHandler(){
        allTips = new List<List<string>>();
    }

    // 把牌中所有的单张，对子，三张，四张找出来
    private void CalCardsCount(){
        List<string> singles = new List<string>();
        List<string> pairs = new List<string>();
        List<string> threes = new List<string>();
        List<string> bombs = new List<string>();
        cardGroup = GroupCard(myCardsStr);
        foreach(string key in cardGroup.Keys){
            switch(cardGroup[key]){
                case 1:
                    singles.Add(key);
                    break;
                case 2:
                    pairs.Add(key);
                    break;
                case 3:
                    threes.Add(key);
                    break;
                case 4:
                    bombs.Add(key);
                    break;
                default:
                    throw new System.Exception("牌的数量不对");
            }
        }
        SortedCards.Instance.singles = singles;
        SortedCards.Instance.pairs = pairs;
        SortedCards.Instance.threes = threes;
        SortedCards.Instance.bombs = bombs;
    }

    private List<string> GenNexTip(){
        // 生成下一条提示
        List<string> nextTip = SortedCards.Instance.GenNextTip();
        if(null != nextTip){
            allTips.Add(nextTip);
            allTipsChecked = allTips.Count >= 10;
        }
        else{
            allTipsChecked = true;
        }
        // 无更多可用提示或者已生成大于10条提示时不再生成新提示
        return nextTip;
    }

    private List<string> GetNextTipStr(){
        if(allTipsChecked){
            if(getTipProgress == allTips.Count){
                getTipProgress = 0;
            }
            return allTips[getTipProgress++];
        }
        else{
            return GenNexTip();
        }
    }

    // 生成提示所需要的基本牌型结构 start
    // 生成有效的顺子，是否有效的判断规则暂定为(1.不拆炸弹, 2.使用顺子后剩余牌张数要少于使用前)
    private void GenValidShunzi(){
        List<int> cardsNumList = new List<int>();
        List<string> streakCards = CardTransferNew.streakCards;
        for(int i = 0; i < streakCards.Count; ++i){
            cardsNumList.Add(0);
        }
        for(int i = 0; i < streakCards.Count; ++i){
            int count = 0;
            cardGroup.TryGetValue(streakCards[i], out count);
            cardsNumList[i] = count;
        }
        int start = -1, end = -1;
        GenShunziIndex(cardsNumList, out start, out end); 
        GenValidShunzi(cardsNumList, start, end);
    }
    public void GenShunziIndex(List<int> cardsNumList, out int start, out int end){
        start = -1;
        end = -1;
        for(int i = 0; i < cardsNumList.Count; ++i){
            // 暂时只找最小的最长的顺子
            if(0 != cardsNumList[i]){
                if(-1 == start){
                    start = i;
                }
                end = i;
            }
            else{
                if(end - start >= 4){
                    break;
                }
                else{
                    start = -1;
                    end = -1;
                }
            }
        }
    }
    public void GenValidShunzi(List<int> cardsNumList, int start, int end){
        if(-1 != start && -1 != end && end - start >= 4){
            // 找到了顺子
            int cardCountUsingShunzi = 0;
            int cardCountNoUsingShunzi = 0;
            for(int i = start; i <= end; ++i){
                cardCountUsingShunzi += cardsNumList[i] - 1;
                if(1 == cardsNumList[i]){
                    ++cardCountNoUsingShunzi;
                }
            }
            if(cardCountUsingShunzi > cardCountNoUsingShunzi){
                int subStart = start;
                int subEnd = end;
                if(cardsNumList[subEnd] > 1){
                    --subEnd;
                }
                else if(cardsNumList[subStart] > 1){
                    ++subStart;
                }
                GenValidShunzi(cardsNumList, subEnd, subStart);
            }
            else{
                // 顺子不能拆 >=2 个三张
                int breakCount = 0;
                List<int> breakPos = new List<int>();
                for(int i = start; i <= end; ++i){
                    if(SortedCards.Instance.threes.Contains(CardTransferNew.streakCards[i])){
                        breakPos.Add(i);
                        ++breakCount;
                    }
                    if(SortedCards.Instance.sanshun.Contains(CardTransferNew.streakCards[i])){
                        breakPos.Add(i);
                        ++breakCount;
                    }
                }
                if(breakCount >= 2){
                    if (2 == breakCount)
                    {
                        if (breakPos[0] - start >= 4)
                        {
                            GenValidShunzi(cardsNumList, start, breakPos[0]);
                        }
                        if(breakPos[1] - breakPos[0] >= 4){
                            GenValidShunzi(cardsNumList, breakPos[0], breakPos[1]);
                        }
                        if(end - breakPos[1] >= 4){
                            GenValidShunzi(cardsNumList, breakPos[1], end);
                        }
                    }
                }
                else
                {
                    List<string> shun = SortedCards.Instance.shunzi;
                    for (int i = start; i < end; ++i)
                    {
                        // 有效的顺子
                        shun.Add(CardTransferNew.streakCards[i]);
                    }
                    List<string> singles = SortedCards.Instance.singles;
                    List<string> pairs = SortedCards.Instance.pairs;
                    List<string> threes = SortedCards.Instance.threes;
                    // 从其他牌中移除顺子使用的牌
                    foreach(string card in shun){
                        singles.Remove(card);
                        if(pairs.Remove(card)){
                            singles.Add(card);
                        }
                        if(threes.Remove(card)){
                            pairs.Add(card);
                        }
                    }
                }
            }
        }
    }
    private void GenBaseCardsPattern(){
        CalCardsCount();
        GenValidShunzi();
    }
    // 生成提示所需要的基本牌型结构 end 

    // 外部接口 start
    /// <summary>
    /// 获取下一条提示
    /// </summary>
    /// <param name="myCards">当前手牌</param>
    /// <returns></returns>
    public List<int> GetNextTip(){
        
        return TransTipToInt(GetNextTipStr());
    }
    /// <summary>
    /// 设置我的手牌，第一次点击提示时调用
    /// </summary>
    /// <param name="myCards"></param>
    /// <returns>第一条提示</returns>
    public List<int> SetMyCards(List<int> myCards){
        SortedCards.Instance.RenewSortedCards();
        myCardsInt = myCards;
        GenBaseCardsPattern();
        return GetNextTip();
    }
    // 外部接口 end 

    // 工具方法 start
    /// <summary>
    /// 计算每张牌的数量
    /// </summary>
    /// <param name="myCards"></param>
    /// <returns></returns>
    public static Dictionary<string, int> GroupCard(List<string> myCards){
        Dictionary<string, int> cardDic = new Dictionary<string, int>();
        for(int i = 0; i < myCards.Count; ++i){
            if(cardDic.ContainsKey(myCards[i])){
                ++cardDic[myCards[i]];
            }
            else{
                cardDic[myCards[i]] = 1;
            }
        }
        return cardDic;
    }

    public List<int> TransTipToInt(List<string> tipStr){
        List<int> tipInt = new List<int>();
        foreach(int card in this.myCardsInt){
            if(tipStr.Contains(CardTransferNew.intToStr[card])){
                tipInt.Add(card);
            }
        }
        return tipInt;
    }
    // 工具方法 end
}
