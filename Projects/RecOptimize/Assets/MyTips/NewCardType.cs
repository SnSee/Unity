using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* TODO 顺子(CardTypeABCDE)及之前的牌的排序算法支持癞子，之后的暂不支持 查找算法暂时都不支持癞子*/
// TODO 定义三顺的个数，如AAABBB, AAABBBCCC, AAABBBCCCDDD
public enum CARD_TYPE
{
    A,
    AA,
    AAA,
    AAAB,
    AAABB,
    ABCDE,
    AAABBB,
    AABBCC,
    AAABBBCD,
    AAABBBCCDD,
    // AAAA,
    AAAABC,
    AAAABBCC,
    AAAAPureLaize,          // 纯癞子炸弹
    AAAALaizi,              // 癞子炸弹
    AAAANormal,             // 硬炸
    AAAANormalLaizi,        // 软炸
    ROCKET
}
public class AllCardInfo
{
    public bool isLaizi;
    public string laiZiReadableNum;            // 如果是癞子，本次使用癞子代表的值，和readableNum范围一样
    public int serverNum;           // 0~53
    public string readableNum;      // 正常牌可读值 "3"~"2"及"w","W"
    public AllCardInfo(bool isLaizi, int serverNum)
    {
        this.isLaizi = isLaizi;
        this.serverNum = serverNum;
        this.readableNum = CardTransferNew.intToStr[serverNum];
    }
}
public class FollowTipsHandler
{
    public static FollowTipsHandler _instance = null;
    public static FollowTipsHandler Instance
    {
        get { return null == _instance ? _instance = new FollowTipsHandler() : _instance; }
    }
    private Dictionary<CARD_TYPE, CardTypeBase> cardTypes;
    private FollowTipsHandler()
    {

    }
    private List<AllCardInfo> TransIntCardToAllCardInfo(List<int> cardsInt)
    {
        List<AllCardInfo> cardInfos = new List<AllCardInfo>();
        foreach (int intCard in cardsInt)
        {
            cardInfos.Add(new AllCardInfo(false, intCard));
        }
        return cardInfos;
    }
    private List<List<int>> TransAllCardInfoToInt(List<List<AllCardInfo>> cardInfoLL)
    {
        List<List<int>> ret = new List<List<int>>();
        foreach (List<AllCardInfo> cardInfoL in cardInfoLL)
        {
            List<int> inList = new List<int>();
            foreach (AllCardInfo cardInfo in cardInfoL)
            {
                inList.Add(cardInfo.serverNum);
            }
            ret.Add(inList);
        }
        return ret;
    }
    /// <summary>
    /// 根据牌型获取牌型对象
    /// </summary>
    /// <param name="cardType"></param>
    /// <returns></returns>
    private CardTypeBase GetCardTypeObj(List<AllCardInfo> topCards)
    {
        return CardTypeBase.GetCardTypeObj(topCards);
    }
    /// <summary>
    /// 初始化
    /// </summary>
    public void InitCardTipsHelper()
    {
        CardTypeBase.InitCardTypes();
    }
    /// <summary>
    /// 获取提示
    /// </summary>
    /// <param name="myCards">我的手牌，每张牌范围是0~53</param>
    /// <param name="topCards">要压制的牌，</param>
    /// <returns></returns>
    public List<List<int>> GetAllTips(List<int> myCards, List<int> topCards)
    {
#if !UNITY_EDITOR
        try
        {
#endif
        List<AllCardInfo> myCardsInfo = TransIntCardToAllCardInfo(myCards);
        List<AllCardInfo> topCardsInfo = TransIntCardToAllCardInfo(topCards);
        List<List<AllCardInfo>> cardInfos = GetSortedTips(myCardsInfo, topCardsInfo);
        return TransAllCardInfoToInt(cardInfos);
#if !UNITY_EDITOR
        }
        catch(System.Exception e){
            Debug.LogError("error occur when get follow tips, err=" + e.Message);
            return null;
        }
#endif
    }
    private List<List<AllCardInfo>> GetSortedTips(List<AllCardInfo> myCards, List<AllCardInfo> topCards)
    {
        CardTypeBase cardTypeObj = GetCardTypeObj(topCards);
        if (null == cardTypeObj)
        {
            // 不支持的牌型，返回所有能用的炸弹和火箭
            return CardTypeBase.GetBombAndRocket(myCards);
        }
        cardTypeObj.SetMyCardsAndTopCards(myCards, topCards);
        return cardTypeObj.GetSortedTips();
    }
}
public class CardTypeBase
{
    # region 变量
    // protected const string STR_WANG = "wang";
    // protected const string STR_LAIZI = "laizi";
    protected List<AllCardInfo> myCards, topCards;
    protected CARD_TYPE topCardType;
    protected Dictionary<string, List<AllCardInfo>> myCardGroup;
    protected Dictionary<string, List<AllCardInfo>> topCardGroup;
    private List<string> myNormalbombCard;
    private List<List<AllCardInfo>> myNormalBombInfo;   // 硬炸
    protected List<AllCardInfo> wangCard;       // 火箭，长度为0或2 
    private static Dictionary<CARD_TYPE, CardTypeBase> _cardTypes = null;
    private static Dictionary<CARD_TYPE, CardTypeBase> cardTypes
    {
        get
        {
            if (null == _cardTypes)
            {
                InitCardTypes();
            }
            return _cardTypes;
        }
    }
    # endregion
    public static void InitCardTypes()
    {
        _cardTypes = new Dictionary<CARD_TYPE, CardTypeBase>();
        _cardTypes[CARD_TYPE.A] = new CardTypeA();
        _cardTypes[CARD_TYPE.AA] = new CardTypeAA();
        _cardTypes[CARD_TYPE.AAA] = new CardTypeAAA();
        _cardTypes[CARD_TYPE.AAAB] = new CardTypeAAAB();
        _cardTypes[CARD_TYPE.AAABB] = new CardTypeAAABB();
        _cardTypes[CARD_TYPE.ABCDE] = new CardTypeABCDE();
        _cardTypes[CARD_TYPE.AABBCC] = new CardTypeAABBCC();
        _cardTypes[CARD_TYPE.AAABBB] = new CardTypeAAABBB();
        _cardTypes[CARD_TYPE.AAABBBCD] = new CardTypeAAABBBCD();
        _cardTypes[CARD_TYPE.AAABBBCCDD] = new CardTypeAAABBBCCDD();
        _cardTypes[CARD_TYPE.AAAABC] = new CardTypeAAAABC();
        _cardTypes[CARD_TYPE.AAAABBCC] = new CardTypeAAAABBCC();
        _cardTypes[CARD_TYPE.AAAAPureLaize] = new CardTypeAAAAPureLaizi();
        _cardTypes[CARD_TYPE.AAAALaizi] = new CardTypeAAAALaizi();
        _cardTypes[CARD_TYPE.AAAANormal] = new CardTypeAAAANormal();
        _cardTypes[CARD_TYPE.AAAANormalLaizi] = new CardTypeAAAANormalLaizi();
        _cardTypes[CARD_TYPE.ROCKET] = new CardTypeRocket();
    }
    # region 排序规则
    protected void SortTips(List<List<AllCardInfo>> tips)
    {
        tips.Sort(Priority0);
    }
    private int Priority0(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 按未使用炸弹，使用炸弹，使用拆炸弹的牌进行排序
        bool breakL = BreakNormalBombOrRocket(left);
        bool breakR = BreakNormalBombOrRocket(right);
        if (breakL && !breakR)
        {
            return 1;
        }
        else if (breakR && !breakL)
        {
            return -1;
        }
        else if (!breakL && !breakR)
        {
            // 都没拆
            return Priority1(left, right);
        }
        else
        {
            // 都拆了
            // 炸弹在前，拆炸弹的在后
            bool isBombL = IsNormalBomb(left) || IsRocket(left);
            bool isBombR = IsNormalBomb(right) || IsRocket(right);
            if (isBombL && !isBombR)
            {
                return -1;
            }
            else if (isBombR && !isBombL)
            {
                return 1;
            }
            else if (!isBombL && !isBombR)
            {
                // 都不是炸弹
                // 拆火箭的在后
                bool breakRoL = BreakRocket(left);
                bool breakRoR = BreakRocket(right);
                if (breakRoL && !breakRoR)
                {
                    return 1;
                }
                else if (breakRoR && !breakRoL)
                {
                    return -1;
                }
                else if (breakRoL && breakRoR)
                {
                    // 都拆火箭了，小王在前，大王在后
                    return WingsBreakFoursPeriority(left, right);

                }
                else
                {
                    // 都没拆火箭
                    return WingsBreakFoursPeriority(left, right);
                }
            }
            else
            {
                // 都是炸弹
                bool isRocketL = IsRocket(left);
                bool isRocketR = IsRocket(right);
                if (isRocketL)
                {
                    return 1;
                }
                else if (isRocketR)
                {
                    return -1;
                }
                else
                {
                    return CardTransferNew.cardValue[left[0].readableNum] - CardTransferNew.cardValue[right[0].readableNum];
                }
            }
        }
    }
    private int WingsBreakFoursPeriority(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        bool breakL = WingsBreakFours(left);
        bool breakR = WingsBreakFours(right);
        if (breakL && !breakR)
        {
            return 1;
        }
        else if (breakR && !breakL)
        {
            return -1;
        }
        return Priority1(left, right);
    }
    private bool WingsBreakFours(List<AllCardInfo> cardInfos)
    {
        Dictionary<string, List<AllCardInfo>> cardGroup = GroupCards(cardInfos);
        foreach (string card in cardGroup.Keys)
        {
            if (4 == cardGroup[card].Count)
            {
                continue;
            }
            if (myNormalbombCard.Contains(card))
            {
                return true;
            }
        }
        return false;
    }
    private int Priority1(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 按使用癞子牌从小到大
        int laiziCountL = GetLaiziCount(left);
        int laiziCountR = GetLaiziCount(right);
        if (laiziCountL == laiziCountR)
            return Priority4(left, right);
        else
            return laiziCountL - laiziCountR;
    }
    protected virtual int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 按各个牌型确定
        throw new System.Exception("Virtual function was not overrided");
    }
    protected int SubPriority1(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 是否拆对子，不拆的在前
        bool breakL = BreakPair(left);
        bool breakR = BreakPair(right);
        if (breakL && !breakR)
        {
            return 1;
        }
        else if (breakR && !breakL)
        {
            return -1;
        }
        else
        {
            return SortOnValueCommon(left, right);
        }
    }
    protected int SubPriority2(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 是否拆三张，不拆的在前
        bool breakL = BreakThrees(left);
        bool breakR = BreakThrees(right);
        if (breakL && !breakR)
        {
            return 1;
        }
        else if (breakR && !breakL)
        {
            return -1;
        }
        else
        {
            return SubPriority1(left, right);
        }
    }
    protected virtual int SortOnValueCommon(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 按各个牌型牌值从小到大排序
        int cardValueL = GetCardValue(left);
        int cardValueR = GetCardValue(right);
        return cardValueL - cardValueR;
    }
    # endregion
    # region 工具方法
    public Dictionary<string, List<AllCardInfo>> GroupCards(List<AllCardInfo> cards, bool myCard = false)
    {
        Dictionary<string, List<AllCardInfo>> group = new Dictionary<string, List<AllCardInfo>>();
        foreach (AllCardInfo cardInfo in cards)
        {
            if (group.ContainsKey(cardInfo.readableNum))
            {
                group[cardInfo.readableNum].Add(cardInfo);
            }
            else
            {
                group[cardInfo.readableNum] = new List<AllCardInfo> { cardInfo };
            }
        }
        return group;
    }
    public Dictionary<string, List<AllCardInfo>> GroupCardsWithOutSpecial(List<AllCardInfo> cards)
    {
        Dictionary<string, List<AllCardInfo>> group = new Dictionary<string, List<AllCardInfo>>();
        foreach (AllCardInfo cardInfo in cards)
        {
            if (group.ContainsKey(cardInfo.readableNum))
            {
                group[cardInfo.readableNum].Add(cardInfo);
            }
            else
            {
                group[cardInfo.readableNum] = new List<AllCardInfo> { cardInfo };
            }
        }
        return group;
    }
    protected int GetLaiziCount(List<AllCardInfo> cardInfos)
    {
        // 获取牌型中癞子的数量
        int count = 0;
        foreach (AllCardInfo card in cardInfos)
        {
            if (card.isLaizi)
            {
                ++count;
            }
        }
        return count;
    }
    private bool BreakRocket(List<AllCardInfo> cardInfos)
    {
        if (2 == wangCard.Count)
        {
            foreach (AllCardInfo cardInfo in cardInfos)
            {
                if (cardInfo.serverNum == 52 || cardInfo.serverNum == 53)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool BreakNormalBombOrRocket(List<AllCardInfo> cardInfos)
    {
        // 获取牌型是否拆了硬炸或王炸(炸弹及王炸本身算拆了)
        if (2 == wangCard.Count || 0 != myNormalBombInfo.Count)
        {
            foreach (AllCardInfo card in cardInfos)
            {
                if (52 == card.serverNum || 53 == card.serverNum || myNormalbombCard.Contains(card.readableNum))
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool IsRocket(List<AllCardInfo> cardInfos)
    {
        // 是否是王炸
        if (2 != cardInfos.Count || 2 != wangCard.Count)
        {
            return false;
        }
        return 52 == cardInfos[0].serverNum && 53 == cardInfos[1].serverNum || 53 == cardInfos[0].serverNum && 52 == cardInfos[1].serverNum;
    }
    private bool IsNormalBomb(List<AllCardInfo> cardInfos)
    {
        // 是否是硬炸
        if (0 == myNormalBombInfo.Count || 4 != cardInfos.Count)
        {
            return false;
        }
        if (cardInfos[0].readableNum != cardInfos[1].readableNum)
        {
            return false;
        }
        if (cardInfos[2].readableNum != cardInfos[3].readableNum)
        {
            return false;
        }
        return cardInfos[0].readableNum == cardInfos[2].readableNum;
    }
    protected virtual int GetCardValue(List<AllCardInfo> cardInfo)
    {
        throw new System.Exception("Virtual function was not overrided");
    }
    protected bool BreakPair(List<AllCardInfo> cardInfos)
    {
        // 是否拆对子
        Dictionary<string, List<AllCardInfo>> cardGroup = GroupCards(cardInfos);
        foreach (string card in cardGroup.Keys)
        {
            if (2 == cardGroup[card].Count)
            {
                continue;
            }
            if (myCardGroup[card].Count == 2)
            {
                return true;
            }
        }
        return false;
    }
    protected bool BreakThrees(List<AllCardInfo> cardInfos)
    {
        // 是否拆三张
        Dictionary<string, List<AllCardInfo>> cardGroup = GroupCards(cardInfos);
        foreach (string card in cardGroup.Keys)
        {
            if (3 == cardGroup[card].Count)
            {
                continue;
            }
            if (myCardGroup[card].Count == 3)
            {
                return true;
            }
        }
        return false;
    }
    protected bool SecBigger(string fir, string sec)
    {
        return CardTransferNew.cardValue[sec] > CardTransferNew.cardValue[fir];
    }
    protected int SortFunc(AllCardInfo left, AllCardInfo right)
    {
        return CardTransferNew.cardValue[left.readableNum] - CardTransferNew.cardValue[right.readableNum];
    }
    protected bool CardListHasCommon(List<AllCardInfo> cardInfos1, List<AllCardInfo> cardInfos2)
    {
        // 两个列表含有相同牌值元素
        foreach (AllCardInfo card1 in cardInfos1)
        {
            foreach (AllCardInfo card2 in cardInfos2)
            {
                if (card1.readableNum == card2.readableNum)
                {
                    return true;
                }
            }
        }
        return false;
    }
    protected bool CardListHasCommon(List<AllCardInfo> cardInfos1, AllCardInfo[] cardInfos2)
    {
        // 两个列表含有相同牌值元素
        foreach (AllCardInfo card1 in cardInfos1)
        {
            foreach (AllCardInfo card2 in cardInfos2)
            {
                if (card1.readableNum == card2.readableNum)
                {
                    return true;
                }
            }
        }
        return false;
    }
    protected List<List<AllCardInfo>> CombineMainAndSub(List<List<AllCardInfo>> mains, List<List<AllCardInfo>> subs)
    {
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        foreach (List<AllCardInfo> main in mains)
        {
            foreach (List<AllCardInfo> sub in subs)
            {
                if (!CardListHasCommon(main, sub))
                {
                    List<AllCardInfo> toAdd = new List<AllCardInfo>(main);
                    toAdd.AddRange(sub);
                    ret.Add(toAdd);
                }
            }
        }
        return ret;
    }
    private List<List<AllCardInfo>> RemoveSameTip(List<List<AllCardInfo>> oriTips)
    {
        List<List<AllCardInfo>> uniqueTips = new List<List<AllCardInfo>>();
        // 按之前的查找及组合方法重复的提示必定相邻
        StringBuilder sb = new StringBuilder();
        string lastTipStr = "";
        foreach (List<AllCardInfo> tipsL in oriTips)
        {
            sb.Clear();
            string thisTipStr;
            foreach (AllCardInfo cardInfo in tipsL)
            {
                sb.Append(cardInfo.readableNum);
            }
            thisTipStr = sb.ToString();
            if (lastTipStr != thisTipStr)
            {
                uniqueTips.Add(tipsL);
            }
            lastTipStr = thisTipStr;
        }
        return uniqueTips;
    }
    protected List<List<AllCardInfo>> CombineMainAndSub(List<List<AllCardInfo>> mains, List<AllCardInfo[]> subs)
    {
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        foreach (List<AllCardInfo> main in mains)
        {
            foreach (AllCardInfo[] sub in subs)
            {
                if (!CardListHasCommon(main, sub))
                {
                    List<AllCardInfo> toAdd = new List<AllCardInfo>(main);
                    toAdd.AddRange(sub);
                    ret.Add(toAdd);
                }
            }
        }
        return RemoveSameTip(ret);
    }
    protected List<List<AllCardInfo>> GetBombAndRocket()
    {
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        foreach (List<AllCardInfo> bomb in myNormalBombInfo)
        {
            ret.Add(bomb);
        }
        if (2 == wangCard.Count)
        {
            ret.Add(wangCard);
        }
        return ret;
    }
    # endregion
    # region 外部接口
    public void SetMyCardsAndTopCards(List<AllCardInfo> myCards, List<AllCardInfo> topCards)
    {
        this.myCards = myCards;
        this.topCards = topCards;
        myCardGroup = GroupCards(myCards, true);
        topCardGroup = GroupCards(topCards);
        List<List<AllCardInfo>> myNormalBombInfo = new List<List<AllCardInfo>>();
        List<string> myNormalbombCard = new List<string>();
        this.wangCard = new List<AllCardInfo>();
        if (myCardGroup.ContainsKey("w") && myCardGroup.ContainsKey("W"))
        {
            this.wangCard.Add(myCardGroup["w"][0]);
            this.wangCard.Add(myCardGroup["W"][0]);
        }
        foreach (string card in myCardGroup.Keys)
        {
            if (4 == myCardGroup[card].Count)
            {
                myNormalBombInfo.Add(myCardGroup[card]);
                myNormalbombCard.Add(card);
            }
        }
        this.myNormalBombInfo = myNormalBombInfo;
        this.myNormalbombCard = myNormalbombCard;
    }
    public List<List<AllCardInfo>> GetSortedTips()
    {
        List<List<AllCardInfo>> ret = GetValidTips();
        SortTips(ret);
        return ret;
    }
    protected virtual List<List<AllCardInfo>> GetValidTips()
    {
        throw new System.Exception("Virtual function was not overrided");
    }
    public static CardTypeBase GetCardTypeObj(List<AllCardInfo> cardInfos)
    {
        Dictionary<string, List<AllCardInfo>> cardGroup = cardTypes[CARD_TYPE.A].GroupCardsWithOutSpecial(cardInfos);
        int threeCount = 0;
        switch (cardInfos.Count)
        {
            case 1:     // 单张
                return cardTypes[CARD_TYPE.A];
            case 2:     // 对子或火箭
                if (1 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.AA];
                }
                return cardTypes[CARD_TYPE.ROCKET];
            case 3:     // 三张
                return cardTypes[CARD_TYPE.AAA];
            case 4:     // 炸弹或三带一
                if (1 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.AAAANormal];
                }
                return cardTypes[CARD_TYPE.AAAB];
            case 5:     // 三带对或顺子
                if (5 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.ABCDE];
                }
                return cardTypes[CARD_TYPE.AAABB];
            case 6:     // 飞机或连对或顺子或四代二
                if (6 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.ABCDE];
                }
                else if (2 == cardGroup.Count)
                {
                    foreach (string card in cardGroup.Keys)
                    {
                        if (3 == cardGroup[card].Count)
                        {
                            ++threeCount;
                        }
                    }
                    if (2 == threeCount)
                    {
                        return cardTypes[CARD_TYPE.AAABBB];
                    }
                    else
                    {
                        return cardTypes[CARD_TYPE.AAAABC];
                    }
                }
                else if (3 == cardGroup.Count)
                {
                    foreach (string card in cardGroup.Keys)
                    {
                        if (4 == cardGroup[card].Count)
                        {
                            return cardTypes[CARD_TYPE.AAAABC];
                        }
                    }
                    return cardTypes[CARD_TYPE.AABBCC];
                }
                else
                {
                    return null;
                }
            case 7:     // 顺子
                return cardTypes[CARD_TYPE.ABCDE];
            case 8:     // 顺子，连对，飞机带2单，四带2对(两个炸弹顺子算四带2对)
                if (8 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.ABCDE];
                }
                else if (3 == cardGroup.Count || 2 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.AAAABBCC];
                }
                else if (4 == cardGroup.Count)
                {
                    foreach (string card in cardGroup.Keys)
                    {
                        if (3 == cardGroup[card].Count)
                        {
                            return cardTypes[CARD_TYPE.AAABBBCD];
                        }
                    }
                    return cardTypes[CARD_TYPE.AABBCC];
                }
                else
                {
                    return null;
                }
            case 9:     // 顺子，三飞机
                if (9 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.ABCDE];
                }
                else if (3 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.AAABBB];
                }
                else
                {
                    return null;
                }
            case 10:    // 顺子，连对，飞机带两对
                if (10 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.ABCDE];
                }
                else if (4 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.AAABBBCCDD];
                }
                else if (5 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.AABBCC];
                }
                else
                {
                    return null;
                }
            case 11:    // 顺子
                if (11 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.ABCDE];
                }
                else
                {
                    return null;
                }
            case 12:    // 连对，飞机，飞机带三单，三个炸弹顺子
                foreach (string card in cardGroup.Keys)
                {
                    if (3 == cardGroup[card].Count)
                    {
                        ++threeCount;
                    }
                }
                if (4 == threeCount)
                {
                    return cardTypes[CARD_TYPE.AAABBB];
                }
                else if (3 == threeCount)
                {
                    return cardTypes[CARD_TYPE.AAABBBCD];
                }
                if (6 == cardGroup.Count)
                {
                    return cardTypes[CARD_TYPE.AABBCC];
                }
                else
                {
                    return null;
                }
            default:
                // 超出12张不再处理
                return null;
        }
    }
    /// <summary>
    /// 获取所有的炸弹和火箭
    /// </summary>
    /// <param name="myCards"></param>
    /// <returns></returns>
    public static List<List<AllCardInfo>> GetBombAndRocket(List<AllCardInfo> myCards)
    {
        Dictionary<string, List<AllCardInfo>> cardGroup = cardTypes[CARD_TYPE.A].GroupCards(myCards);
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        List<AllCardInfo> wangCard = new List<AllCardInfo>();
        foreach (string card in cardGroup.Keys)
        {
            int cardCount = cardGroup[card].Count;
            if (52 == cardGroup[card][0].serverNum || 53 == cardGroup[card][0].serverNum)
            {
                wangCard.Add(cardGroup[card][0]);
            }
            if (4 == cardCount)
            {
                // 炸弹
                ret.Add(cardGroup[card]);
            }
        }
        ret.Sort(delegate (List<AllCardInfo> left, List<AllCardInfo> right)
        {
            return CardTransferNew.cardValue[left[0].readableNum] - CardTransferNew.cardValue[right[0].readableNum];
        });
        if (2 == wangCard.Count)
        {
            ret.Add(wangCard);
        }
        return ret;
    }
    # endregion
}
public class CardTypeA : CardTypeBase
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (GetCardValue(myCardGroup[card]) > GetCardValue(topCards))
            {
                // 单张
                ret.Add(new List<AllCardInfo> { myCardGroup[card][0] });
            }
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 是否拆三张
        return SubPriority2(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfo)
    {
        if (cardInfo[0].isLaizi)
        {
            return CardTransferNew.cardValue[cardInfo[0].laiZiReadableNum];
        }
        return CardTransferNew.cardValue[cardInfo[0].readableNum];
    }
}
public class CardTypeAA : CardTypeBase
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        int topCardValue = GetCardValue(topCards);
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (cardCount >= 2)
            {
                if (CardTransferNew.cardValue[card] > CardTransferNew.cardValue[topCards[0].readableNum])
                {
                    ret.Add(new List<AllCardInfo> { myCardGroup[card][0], myCardGroup[card][1] });
                }
            }
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 第4优先级，是否拆三张
        return SubPriority2(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        int laiziCount = GetLaiziCount(cardInfos);
        string card = null;
        switch (laiziCount)
        {
            case 0:
                card = cardInfos[0].readableNum;
                break;
            case 1:
                if (cardInfos[0].isLaizi)
                {
                    card = cardInfos[1].readableNum;
                }
                else
                {
                    card = cardInfos[0].readableNum;
                }
                break;
            case 2:
                card = GetBiggerLaiziCard(cardInfos[0], cardInfos[1]);
                break;
        }
        return CardTransferNew.cardValue[card];
    }
    public string GetBiggerLaiziCard(AllCardInfo v1, AllCardInfo v2)
    {
        if (CardTransferNew.cardValue[v1.laiZiReadableNum] > CardTransferNew.cardValue[v2.laiZiReadableNum])
        {
            return v1.laiZiReadableNum;
        }
        return v2.laiZiReadableNum;
    }
}
public class CardTypeAAA : CardTypeBase
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (cardCount >= 3)
            {
                if (CardTransferNew.cardValue[card] > CardTransferNew.cardValue[topCards[0].readableNum])
                {
                    ret.Add(new List<AllCardInfo> { myCardGroup[card][0], myCardGroup[card][1], myCardGroup[card][2] });
                }
            }
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 牌值从小到大
        return SortOnValueCommon(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        int laiziCount = GetLaiziCount(cardInfos);
        string card = null;
        switch (laiziCount)
        {
            case 0:
                card = cardInfos[0].readableNum;
                break;
            case 1:
            case 2:
                card = GetNormalCard(cardInfos);
                break;
            case 3:
                card = GetBiggestLaiziCard(cardInfos);
                break;
        }
        return CardTransferNew.cardValue[card];
    }
    private string GetNormalCard(List<AllCardInfo> cardInfos)
    {
        foreach (AllCardInfo card in cardInfos)
        {
            if (card.isLaizi)
            {
                continue;
            }
            return card.readableNum;
        }
        return null;
    }
    private string GetBiggestLaiziCard(List<AllCardInfo> cardInfos)
    {
        string card = cardInfos[0].laiZiReadableNum;
        for (int i = 1; i < cardInfos.Count; ++i)
        {
            if (CardTransferNew.cardValue[cardInfos[i].laiZiReadableNum] > CardTransferNew.cardValue[card])
            {
                card = cardInfos[i].laiZiReadableNum;
            }
        }
        return card;
    }
}
public class CardTypeWithWings : CardTypeBase
{
    // 带翅膀的组合牌型
    protected int SortOnValueUnique(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        int cardValueL = GetCardValue(left);
        int cardValueR = GetCardValue(right);
        int mainValueL = cardValueL / 100;      // 三带一中三个牌的牌值
        int mainValueR = cardValueR / 100;
        if (mainValueL != mainValueR)
        {
            return mainValueL - mainValueR;
        }
        else
        {
            int subValueL = cardValueL % 100;       // 三带一中被带的牌的牌值
            int subValueR = cardValueR % 100;       // 三带一中被带的牌的牌值
            // 是否拆三张
            return SubPriority2(left, right);
        }
    }
}
public class CardTypeAAAB : CardTypeWithWings
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        int topMainCardValue = GetTopMainCardValue();
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> threes = new List<List<AllCardInfo>>();
        List<AllCardInfo> singles = new List<AllCardInfo>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (cardCount >= 3)
            {
                if (CardTransferNew.cardValue[card] > topMainCardValue)
                {
                    threes.Add(new List<AllCardInfo> { myCardGroup[card][0], myCardGroup[card][1], myCardGroup[card][2] });
                }
            }
            if (cardCount >= 1)
            {
                singles.Add(myCardGroup[card][0]);
            }
        }
        foreach (List<AllCardInfo> thr in threes)
        {
            foreach (AllCardInfo sin in singles)
            {
                if (thr[0].readableNum == sin.readableNum)
                {
                    continue;
                }
                ret.Add(new List<AllCardInfo> { thr[0], thr[1], thr[2], sin });
            }
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        return SortOnValueUnique(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        string mainCard, subCard;
        GetMainAndSubCard(cardInfos, out mainCard, out subCard);
        return CardTransferNew.cardValue[mainCard] * 100 + CardTransferNew.cardValue[subCard];
    }
    private void GetMainAndSubCard(List<AllCardInfo> cardInfos, out string mainCard, out string subCard)
    {
        int laiziCount = GetLaiziCount(cardInfos);
        mainCard = null;
        subCard = null;
        Dictionary<string, List<AllCardInfo>> cardGroup = GroupCards(cardInfos);
        switch (laiziCount)
        {
            case 0:
                foreach (string card in cardGroup.Keys)
                {
                    if (3 == cardGroup[card].Count)
                    {
                        mainCard = card;
                    }
                    else
                    {
                        subCard = card;
                    }
                }
                break;
            case 1:
                foreach (string card in cardGroup.Keys)
                {
                    if (2 == cardGroup[card].Count)
                    {
                        mainCard = card;
                    }
                    else
                    {
                        subCard = card;
                    }
                }
                break;
            case 2:
                foreach (string card in cardGroup.Keys)
                {
                    if (null == mainCard)
                    {
                        mainCard = card;
                    }
                    else
                    {
                        if (CardTransferNew.cardValue[card] > CardTransferNew.cardValue[mainCard])
                        {
                            subCard = mainCard;
                            mainCard = card;
                        }
                        else
                        {
                            subCard = card;
                        }
                    }
                }
                break;
        }
    }
    private int GetTopMainCardValue()
    {
        foreach (string card in topCardGroup.Keys)
        {
            if (3 == topCardGroup[card].Count)
            {
                return CardTransferNew.cardValue[card];
            }
        }
        throw new System.Exception("wrong card count");
    }
}
public class CardTypeAAABB : CardTypeWithWings
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        int topMainCardValue = GetTopMainCardValue();
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> threes = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> pairs = new List<List<AllCardInfo>>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (cardCount >= 3)
            {
                if (CardTransferNew.cardValue[card] > topMainCardValue)
                {
                    threes.Add(new List<AllCardInfo> { myCardGroup[card][0], myCardGroup[card][1], myCardGroup[card][2] });
                }
            }
            if (cardCount >= 2)
            {
                pairs.Add(new List<AllCardInfo> { myCardGroup[card][0], myCardGroup[card][1] });
            }
        }
        if (threes.Count > 0 && pairs.Count >= 1)
        {
            ret.AddRange(CombineMainAndSub(threes, pairs));
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 是否拆三张
        return SortOnValueUnique(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        string mainCard, subCard;
        GetMainAndSubCard(cardInfos, out mainCard, out subCard);
        return CardTransferNew.cardValue[mainCard] * 100 + CardTransferNew.cardValue[subCard];
    }
    private void GetMainAndSubCard(List<AllCardInfo> cardInfos, out string mainCard, out string subCard)
    {
        int laiziCount = GetLaiziCount(cardInfos);
        mainCard = null;
        subCard = null;
        string ambiguousCard1 = null, ambiguousCard2 = null;
        Dictionary<string, List<AllCardInfo>> cardGroup = GroupCards(cardInfos);
        switch (laiziCount)
        {
            case 0:
                foreach (string card in cardGroup.Keys)
                {
                    if (3 == cardGroup[card].Count)
                    {
                        mainCard = card;
                    }
                    else
                    {
                        subCard = card;
                    }
                }
                break;
            case 1:
                foreach (string card in cardGroup.Keys)
                {
                    if (3 == cardGroup[card].Count)
                    {
                        mainCard = card;
                    }
                    else if (2 == cardGroup[card].Count)
                    {
                        if (null == ambiguousCard1)
                        {
                            ambiguousCard1 = card;
                        }
                        else
                        {
                            ambiguousCard2 = card;
                        }
                    }
                    else if (1 == cardGroup[card].Count)
                    {
                        subCard = card;
                    }
                }
                if (null != mainCard)
                {
                    break;
                }
                if (CardTransferNew.cardValue[ambiguousCard1] > CardTransferNew.cardValue[ambiguousCard2])
                {
                    mainCard = ambiguousCard1;
                    subCard = ambiguousCard2;
                }
                else
                {
                    mainCard = ambiguousCard2;
                    subCard = ambiguousCard1;
                }
                break;
            case 2:
                foreach (string card in cardGroup.Keys)
                {
                    if (null == mainCard)
                    {
                        mainCard = card;
                    }
                    else
                    {
                        if (CardTransferNew.cardValue[card] > CardTransferNew.cardValue[mainCard])
                        {
                            subCard = mainCard;
                            mainCard = card;
                        }
                        else
                        {
                            subCard = card;
                        }
                    }
                }
                break;
        }
    }
    private int GetTopMainCardValue()
    {
        foreach (string card in topCardGroup.Keys)
        {
            if (3 == topCardGroup[card].Count)
            {
                return CardTransferNew.cardValue[card];
            }
        }
        throw new System.Exception("wrong card count");
    }
}
public class CardTypeABCDE : CardTypeBase
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        List<string> streak = CardTransferNew.STREAKABLE;
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        string minTopCard, maxTopCard;
        GetMinMaxTopCard(out minTopCard, out maxTopCard);
        if ("A" == maxTopCard)
        {
            return ret;
        }
        List<AllCardInfo> hasCardInfo = new List<AllCardInfo>();
        ret.AddRange(GetBombAndRocket());
        for (int i = 0; i < streak.Count; ++i)
        {
            List<AllCardInfo> cardInfo;
            myCardGroup.TryGetValue(streak[i], out cardInfo);
            if (null != cardInfo)
            {
                hasCardInfo.Add(cardInfo[0]);
            }
            else
            {
                hasCardInfo.Add(null);
            }
        }
        List<int> validShunziStartIndex = new List<int>();
        int startIndex = streak.IndexOf(minTopCard) + 1;
        int validStreakCount = 0;
        int indexA = streak.IndexOf("A");
        while (indexA - startIndex + 1 >= topCards.Count)
        {
            if (validStreakCount == topCards.Count)
            {
                validShunziStartIndex.Add(startIndex);
                ++startIndex;
                validStreakCount = 0;
                continue;
            }
            if (null == hasCardInfo[startIndex + validStreakCount])
            {
                startIndex = startIndex + validStreakCount + 1;
                validStreakCount = 0;
            }
            else
            {
                ++validStreakCount;
            }
        }
        for (int i = 0; i < validShunziStartIndex.Count; ++i)
        {
            List<AllCardInfo> inList = new List<AllCardInfo>();
            for (int j = validShunziStartIndex[i]; j < validShunziStartIndex[i] + topCards.Count; ++j)
            {
                inList.Add(hasCardInfo[j]);
            }
            ret.Add(inList);
        }
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 按值从小到大排序
        return SortOnValueCommon(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        // 由于是接上家的牌，所以顺子的张数是固定的
        return CardTransferNew.cardValue[GetMinCard(cardInfos)];
    }
    private string GetMinCard(List<AllCardInfo> cardInfos)
    {
        // 获取顺子中最小的牌
        int laiziCount = GetLaiziCount(cardInfos);
        int normalCount = cardInfos.Count - laiziCount;     // 非癞子牌的张数
        string minNormalCard = null, maxNormalCard = null;
        GetNormalMinMaxCard(cardInfos, out minNormalCard, out maxNormalCard);
        List<string> st = CardTransferNew.STREAKABLE;
        int minNormalIndex = st.IndexOf(minNormalCard);
        int maxNormalIndex = st.IndexOf(maxNormalCard);
        if (maxNormalCard == "A")
        {
            // 非癞子牌最大牌是A
            int index = CardTransferNew.STREAKABLE.Count - cardInfos.Count;
            return CardTransferNew.STREAKABLE[index];
        }

        // 癞子全在顺子的中间
        if (maxNormalIndex - minNormalIndex + 1 == cardInfos.Count)
        {
            return minNormalCard;
        }

        int indexA = st.IndexOf("A");
        int maxToADis = indexA - maxNormalIndex;        // 最大普通牌到A的距离

        // 既有癞子在中间，又有癞子在两边
        int inLaiziCount = maxNormalIndex - minNormalIndex + 1 - normalCount;   // 在中间的癞子数量
        int sideLaiziCount = laiziCount - inLaiziCount;             // 在两边的癞子数量
        return GetMinCardBaseOnSizeLaiziCount(laiziCount, maxToADis, minNormalIndex);
    }
    private string GetMinCardBaseOnSizeLaiziCount(int sideLaiziCount, int maxToADis, int minNormalIndex)
    {
        // 根据在两侧的癞子数量获取顺子中最小的牌
        // maxToADis: 最大普通牌到A的距离
        // 最小牌在从3到A的顺子中的索引
        List<string> st = CardTransferNew.STREAKABLE;
        if (sideLaiziCount <= maxToADis)
        {
            // 如果有在两边的癞子牌，那癞子全在最大牌后边
            return st[minNormalIndex];
        }
        else
        {
            int spilledLaiziCount = sideLaiziCount - maxToADis;     // 将顺子数到A后剩余的癞子数量
            int index = minNormalIndex - spilledLaiziCount;
            return st[index];
        }
    }
    private void GetNormalMinMaxCard(List<AllCardInfo> cardInfos, out string minNormalCard, out string maxNormalCard)
    {
        // 获取非癞子牌的最大最小牌
        minNormalCard = null;
        maxNormalCard = null;
        foreach (AllCardInfo cardInfo in cardInfos)
        {
            if (cardInfo.isLaizi)
            {
                continue;
            }
            if (null == minNormalCard)
            {
                minNormalCard = cardInfo.readableNum;
            }
            else
            {
                if (CardTransferNew.cardValue[cardInfo.readableNum] < CardTransferNew.cardValue[minNormalCard])
                {
                    minNormalCard = cardInfo.readableNum;
                }
            }
            if (null == maxNormalCard)
            {
                maxNormalCard = cardInfo.readableNum;
            }
            else
            {
                if (CardTransferNew.cardValue[cardInfo.readableNum] > CardTransferNew.cardValue[maxNormalCard])
                {
                    maxNormalCard = cardInfo.readableNum;
                }
            }
        }
    }
    private void GetMinMaxTopCard(out string minCard, out string maxCard)
    {
        topCards.Sort(SortFunc);
        minCard = topCards[0].readableNum;
        maxCard = topCards[topCards.Count - 1].readableNum;
    }
}
public class CardTypeAABBCC : CardTypeBase
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        string minTopCard = GetMinCard();
        List<string> streak = CardTransferNew.STREAKABLE;
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> hasCardInfo = new List<List<AllCardInfo>>();
        for (int i = 0; i < streak.Count; ++i)
        {
            List<AllCardInfo> cardInfo;
            myCardGroup.TryGetValue(streak[i], out cardInfo);
            if (null != cardInfo && cardInfo.Count >= 2)
            {
                hasCardInfo.Add(new List<AllCardInfo> { cardInfo[0], cardInfo[1] });
            }
            else
            {
                hasCardInfo.Add(null);
            }
        }
        List<int> validLianduiStartIndex = new List<int>();
        int startIndex = streak.IndexOf(minTopCard) + 1;
        int validStreakCount = 0;
        int indexA = streak.IndexOf("A");
        while (indexA - startIndex + 1 >= topCards.Count / 2)
        {
            if (topCards.Count / 2 == validStreakCount)
            {
                validLianduiStartIndex.Add(startIndex);
                ++startIndex;
                validStreakCount = 0;
                continue;
            }
            if (null == hasCardInfo[startIndex + validStreakCount])
            {
                startIndex = startIndex + validStreakCount + 1;
                validStreakCount = 0;
            }
            else
            {
                ++validStreakCount;
            }
        }
        for (int i = 0; i < validLianduiStartIndex.Count; ++i)
        {
            List<AllCardInfo> inList = new List<AllCardInfo>();
            for (int j = 0; j < topCards.Count / 2; ++j)
            {
                inList.AddRange(hasCardInfo[validLianduiStartIndex[i] + j]);
            }
            ret.Add(inList);
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 不拆三张 
        return SubPriority2(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        string minMain = GetMinCard(cardInfos);
        return CardTransferNew.cardValue[minMain];
    }
    private string GetMinCard(List<AllCardInfo> cardInfos)
    {
        string minCard = cardInfos[0].readableNum;
        foreach (AllCardInfo card in cardInfos)
        {
            if (SecBigger(minCard, card.readableNum))
            {
                minCard = card.readableNum;
            }
        }
        return minCard;
    }
    private string GetMinCard()
    {
        string minCard = topCards[0].readableNum;
        foreach (string card in topCardGroup.Keys)
        {
            if (SecBigger(card, minCard))
            {
                minCard = card;
            }
        }
        return minCard;
    }
}
public class CardTypeAAABBB : CardTypeBase
{
    protected int threeCount;
    protected List<List<AllCardInfo>> GetMain()
    {
        // 获取飞机部分
        // threeCount: 飞机中有几个三张(如333444有两个三张，threeCount就传2)
        string minTopCard = GetMinTopMainCard(out threeCount);
        List<string> streak = CardTransferNew.STREAKABLE;
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> hasCardInfo = new List<List<AllCardInfo>>();
        for (int i = 0; i < streak.Count; ++i)
        {
            List<AllCardInfo> cardInfo;
            myCardGroup.TryGetValue(streak[i], out cardInfo);
            if (null != cardInfo && cardInfo.Count >= 3)
            {
                hasCardInfo.Add(new List<AllCardInfo> { cardInfo[0], cardInfo[1], cardInfo[2] });
            }
            else
            {
                hasCardInfo.Add(null);
            }
        }
        List<int> validFeijiStartIndex = new List<int>();
        int startIndex = streak.IndexOf(minTopCard) + 1;
        int validStreakCount = 0;
        int indexA = streak.IndexOf("A");
        while (indexA - startIndex + 1 >= threeCount)
        {
            if (threeCount == validStreakCount)
            {
                validFeijiStartIndex.Add(startIndex);
                ++startIndex;
                validStreakCount = 0;
                continue;
            }
            if (null == hasCardInfo[startIndex + validStreakCount])
            {
                startIndex = startIndex + validStreakCount + 1;
                validStreakCount = 0;
            }
            else
            {
                ++validStreakCount;
            }
        }
        for (int i = 0; i < validFeijiStartIndex.Count; ++i)
        {
            List<AllCardInfo> inList = new List<AllCardInfo>();
            for (int j = 0; j < threeCount; ++j)
            {
                inList.AddRange(hasCardInfo[validFeijiStartIndex[i] + j]);
            }
            ret.Add(inList);
        }
        return ret;
    }
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        List<List<AllCardInfo>> ret = GetMain();
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        // 牌值从小到大
        return SortOnValueCommon(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        string minMain = GetMinMainCard(cardInfos);
        return CardTransferNew.cardValue[minMain];
    }
    private string GetMinMainCard(List<AllCardInfo> cardInfos)
    {
        string minCard = cardInfos[0].readableNum;
        // 获取飞机中较小的牌
        foreach (AllCardInfo card in cardInfos)
        {
            if (SecBigger(minCard, card.readableNum))
            {
                minCard = card.readableNum;
            }
        }
        return minCard;
    }
    private string GetMinTopMainCard(out int threeCount)
    {
        string minCard = null;
        threeCount = 0;
        foreach (string card in topCardGroup.Keys)
        {
            if (3 == topCardGroup[card].Count)
            {
                ++threeCount;
                if (null == minCard)
                {
                    minCard = card;
                }
                else if (SecBigger(card, minCard))
                {
                    minCard = card;
                }
            }
        }
        return minCard;
    }
    protected int SortOnValueUnique(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        int cardValueL = GetCardValue(left);
        int cardValueR = GetCardValue(right);
        int mainValueL = cardValueL / 100;      // 三带一中三个牌的牌值
        int mainValueR = cardValueR / 100;
        if (mainValueL != mainValueR)
        {
            return mainValueL - mainValueR;
        }
        else
        {
            int subValueL = cardValueL % 100;       // 三带一中被带的牌的牌值
            int subValueR = cardValueR % 100;       // 三带一中被带的牌的牌值
            // 是否拆三张
            return SubPriority2(left, right);
        }
    }
}
public class CardTypeAAABBBCD : CardTypeAAABBB
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> feiji = GetMain();
        List<AllCardInfo> singles = new List<AllCardInfo>();
        List<List<AllCardInfo>> pairs = new List<List<AllCardInfo>>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (cardCount == 1)
            {
                singles.Add(myCardGroup[card][0]);
            }
            else if (cardCount >= 2)
            {
                singles.Add(myCardGroup[card][0]);
                pairs.Add(new List<AllCardInfo> { myCardGroup[card][0], myCardGroup[card][1] });
            }
        }
        // 飞机和单张组合
        List<AllCardInfo[]> sinComp = PermutationAndCombination<AllCardInfo>.GetCombination(singles.ToArray(), threeCount);
        if (feiji.Count > 0 && singles.Count >= threeCount)
        {
            ret.AddRange(CombineMainAndSub(feiji, sinComp));
        }
        if (feiji.Count > 0 && pairs.Count >= 1)
        {
            ret.AddRange(CombineMainAndSub(feiji, pairs));
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        return SortOnValueUnique(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        int maxMainCardValue = -1, subCardValue1 = -1, subCardValue2 = -1;
        Dictionary<string, List<AllCardInfo>> cardGroup = GroupCards(cardInfos);
        foreach (string card in cardGroup.Keys)
        {
            switch (cardGroup[card].Count)
            {
                case 1:
                    if (-1 == subCardValue1)
                    {
                        subCardValue1 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    }
                    else
                    {
                        subCardValue2 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    }
                    break;
                case 2:
                    subCardValue1 = subCardValue2 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    break;
                case 3:
                    if (maxMainCardValue < CardTransferNew.cardValue[cardGroup[card][0].readableNum])
                    {
                        maxMainCardValue = CardTransferNew.cardValue[cardGroup[card][0].readableNum] * 100;
                    }
                    break;
                default:
                    throw new System.Exception("wrong card count");
            }
        }
        return maxMainCardValue + subCardValue1 + subCardValue2;
    }
}
public class CardTypeAAABBBCCDD : CardTypeAAABBB
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> feiji = GetMain();
        List<List<AllCardInfo>> pairs = new List<List<AllCardInfo>>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (cardCount >= 2)
            {
                pairs.Add(new List<AllCardInfo> { myCardGroup[card][0], myCardGroup[card][1] });
            }
        }
        // 飞机和单张组合
        List<List<AllCardInfo>[]> pairComb = PermutationAndCombination<List<AllCardInfo>>.GetCombination(pairs.ToArray(), threeCount);
        List<List<AllCardInfo>> transPairComb = new List<List<AllCardInfo>>();
        foreach (var pairLA in pairComb)
        {
            List<AllCardInfo> tempL = new List<AllCardInfo>();
            foreach (var pairA in pairLA)
            {
                tempL.AddRange(pairA);
            }
            transPairComb.Add(tempL);
        }
        if (feiji.Count > 0 && pairs.Count >= threeCount)
        {
            ret.AddRange(CombineMainAndSub(feiji, transPairComb));
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        return SortOnValueUnique(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        int maxMainCardValue = -1, subCardValue1 = -1, subCardValue2 = -1;
        Dictionary<string, List<AllCardInfo>> cardGroup = GroupCards(cardInfos);
        foreach (string card in cardGroup.Keys)
        {
            switch (cardGroup[card].Count)
            {
                case 2:
                    if (-1 == subCardValue1)
                    {
                        subCardValue1 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    }
                    else
                    {
                        subCardValue2 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    }
                    break;
                case 3:
                    if (maxMainCardValue < CardTransferNew.cardValue[cardGroup[card][0].readableNum])
                    {
                        maxMainCardValue = CardTransferNew.cardValue[cardGroup[card][0].readableNum] * 100;
                    }
                    break;
                default:
                    throw new System.Exception("wrong card count");
            }
        }
        return maxMainCardValue + subCardValue1 + subCardValue2;
    }
    private string GetMinTopMainCard()
    {
        string minCard = null;
        foreach (string card in topCardGroup.Keys)
        {
            if (3 == topCardGroup[card].Count)
            {
                if (null == minCard)
                {
                    minCard = card;
                }
                else if (SecBigger(card, minCard))
                {
                    return card;
                }
            }
        }
        return minCard;
    }
}
public class CardTypeAAAA : CardTypeBase
{
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        return SortOnValueCommon(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        return CardTransferNew.cardValue[cardInfos[0].readableNum];
    }
}
public class CardTypeAAAABC : CardTypeWithWings
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        int topMainCardValue = GetTopMainCardValue();
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> fours = new List<List<AllCardInfo>>();
        List<AllCardInfo> singles = new List<AllCardInfo>();
        List<List<AllCardInfo>> pairs = new List<List<AllCardInfo>>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (4 == cardCount)
            {
                if (CardTransferNew.cardValue[card] > topMainCardValue)
                {
                    fours.Add(myCardGroup[card]);
                }
            }
            if (cardCount == 1)
            {
                singles.Add(myCardGroup[card][0]);
            }
            else if (cardCount >= 2)
            {
                singles.Add(myCardGroup[card][0]);
                pairs.Add(new List<AllCardInfo> { myCardGroup[card][0], myCardGroup[card][1] });
            }
        }
        List<AllCardInfo[]> sinComb = PermutationAndCombination<AllCardInfo>.GetCombination(singles.ToArray(), 2);
        if (fours.Count > 0 && singles.Count >= 2)
        {
            ret.AddRange(CombineMainAndSub(fours, sinComb));
        }
        if (fours.Count > 0 && pairs.Count >= 1)
        {
            ret.AddRange(CombineMainAndSub(fours, pairs));
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        return SortOnValueUnique(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        int mainCardValue = -1, subCardValue1 = -1, subCardValue2 = -1;
        Dictionary<string, List<AllCardInfo>> cardGroup = GroupCards(cardInfos);
        foreach (string card in cardGroup.Keys)
        {
            switch (cardGroup[card].Count)
            {
                case 1:
                    if (-1 == subCardValue1)
                    {
                        subCardValue1 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    }
                    else
                    {
                        subCardValue2 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    }
                    break;
                case 2:
                    subCardValue1 = subCardValue2 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    break;
                case 4:
                    mainCardValue = CardTransferNew.cardValue[cardGroup[card][0].readableNum] * 100;
                    break;
                default:
                    throw new System.Exception("wrong card count");
            }
        }
        return mainCardValue + subCardValue1 + subCardValue2;
    }
    private int GetTopMainCardValue()
    {
        foreach (string card in topCardGroup.Keys)
        {
            if (4 == topCardGroup[card].Count)
            {
                return CardTransferNew.cardValue[card];
            }
        }
        return -1;
    }
}
public class CardTypeAAAABBCC : CardTypeWithWings
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        int topMainCardValue = GetTopMainCardValue();
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> fours = new List<List<AllCardInfo>>();
        List<List<AllCardInfo>> pairs = new List<List<AllCardInfo>>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (4 == cardCount)
            {
                if (CardTransferNew.cardValue[card] > topMainCardValue)
                {
                    fours.Add(myCardGroup[card]);
                }
            }
            if (cardCount >= 2)
            {
                pairs.Add(new List<AllCardInfo> { myCardGroup[card][0], myCardGroup[card][1] });
            }
        }
        List<List<AllCardInfo>[]> pairComb = PermutationAndCombination<List<AllCardInfo>>.GetCombination(pairs.ToArray(), 2);
        List<List<AllCardInfo>> transPairComb = new List<List<AllCardInfo>>();
        foreach (List<AllCardInfo>[] pairC in pairComb)
        {
            List<AllCardInfo> tempL = new List<AllCardInfo>();
            tempL.AddRange(pairC[0]);
            tempL.AddRange(pairC[1]);
            transPairComb.Add(tempL);
        }
        if (fours.Count > 0 && pairs.Count >= 2)
        {
            ret.AddRange(CombineMainAndSub(fours, transPairComb));
        }
        ret.AddRange(GetBombAndRocket());
        return ret;
    }
    protected override int Priority4(List<AllCardInfo> left, List<AllCardInfo> right)
    {
        return SortOnValueUnique(left, right);
    }
    protected override int GetCardValue(List<AllCardInfo> cardInfos)
    {
        int mainCardValue = -1, subCardValue1 = -1, subCardValue2 = -1;
        Dictionary<string, List<AllCardInfo>> cardGroup = GroupCards(cardInfos);
        foreach (string card in cardGroup.Keys)
        {
            switch (cardGroup[card].Count)
            {
                case 2:
                    if (-1 == subCardValue1)
                    {
                        subCardValue1 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    }
                    else
                    {
                        subCardValue2 = CardTransferNew.cardValue[cardGroup[card][0].readableNum];
                    }
                    break;
                case 4:
                    mainCardValue = CardTransferNew.cardValue[cardGroup[card][0].readableNum] * 100;
                    break;
                default:
                    throw new System.Exception("wrong card count");
            }
        }
        return mainCardValue + subCardValue1 + subCardValue2;
    }
    private int GetTopMainCardValue()
    {
        foreach (string card in topCardGroup.Keys)
        {
            if (4 == topCardGroup[card].Count)
            {
                return CardTransferNew.cardValue[card];
            }
        }
        return -1;
    }
}
public class CardTypeRocket : CardTypeBase
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        return new List<List<AllCardInfo>>();
    }
}
// 纯癞子炸
public class CardTypeAAAAPureLaizi : CardTypeAAAA
{

}
// 癞子炸
public class CardTypeAAAALaizi : CardTypeAAAA
{

}
// 硬炸
public class CardTypeAAAANormal : CardTypeAAAA
{
    protected override List<List<AllCardInfo>> GetValidTips()
    {
        List<List<AllCardInfo>> ret = new List<List<AllCardInfo>>();
        foreach (string card in myCardGroup.Keys)
        {
            int cardCount = myCardGroup[card].Count;
            if (4 == cardCount)
            {
                // 炸弹
                if (CardTransferNew.cardValue[card] > CardTransferNew.cardValue[topCards[0].readableNum])
                {
                    ret.Add(myCardGroup[card]);
                }
            }
        }
        if (2 == wangCard.Count)
        {
            ret.Add(wangCard);
        }
        return ret;
    }
}
// 软炸
public class CardTypeAAAANormalLaizi : CardTypeAAAA
{

}

public static class CardTipUtil
{
    /// <summary>
    /// 从列表中选出两个元素，不考虑顺序
    /// </summary>
    /// <param name="tList"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static List<List<T>> Get2FromN<T>(List<T> tList)
    {
        List<List<T>> ret = new List<List<T>>();
        for (int i = 0; i < tList.Count; ++i)
        {
            for (int j = i + 1; j < tList.Count; ++j)
            {
                ret.Add(new List<T> { tList[i], tList[j] });
            }
        }
        return ret;
    }
}