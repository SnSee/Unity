using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate List<List<string>> CardsType1(List<string> topCards, List<string> myDeckCards);
public delegate List<List<string>> CardsType2(List<string> topCards, List<string> myDeckCards, List<List<string>> singles, List<List<string>> pairs);
public delegate List<List<string>> CardsType3(Dictionary<string, object> topScore, List<string> myDeckCards, List<List<string>> singles, List<List<string>> pairs);

public class PopUpTipsItem
{
    public string itemKey;
    public Dictionary<string, object> itemValue;
    public PopUpTipsItem(string key, Dictionary<string, object> value){
        this.itemKey = key;
        this.itemValue = value;
    }
}
public class CardsTypeContainer
{
    public List<List<string>> singles;
    public List<List<string>> pairs;
    public List<List<string>> bombs;
    public CardsTypeContainer(List<List<string>> single, List<List<string>> pair, List<List<string>> bombs){
        this.singles = single;
        this.pairs = pair;
        this.bombs = bombs;
    }
}
public class DizhuPromptAI_Freeout_V1 
{
    # region
    public bool _VERBOSE = false;
    public bool _VERBOSE1 = false;
    DizhuDeckAnalyzer deckAnalyzer;
    Dictionary<string, CardsType1> funcMap1;
    Dictionary<string, CardsType2> funcMap2;
    Dictionary<string, CardsType3> funcMap3;
    List<string> streakable = new List<string>{"3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"};
    # endregion
    public DizhuPromptAI_Freeout_V1()
    {
        deckAnalyzer = new DizhuDeckAnalyzer();
        funcMap1 = new Dictionary<string, CardsType1>();
        funcMap2 = new Dictionary<string, CardsType2>();
        funcMap3 = new Dictionary<string, CardsType3>();
        funcMap1[deckAnalyzer.DP_34567.patternName()] = _drudgery_DP_34567;
        funcMap2[deckAnalyzer.DP_JJQQKK.patternName()] = _drudgery_DP_JJQQKK;
        funcMap2[deckAnalyzer.DP_QQQKKK.patternName()] = _drudgery_DP_QQQKKK;
        funcMap3[deckAnalyzer.DP_KKKQ.patternName()] = _drudgery_DP_KKKQ;
        funcMap3[deckAnalyzer.DP_KKKJJ.patternName()] = _drudgery_DP_KKKJJ;
        funcMap3[deckAnalyzer.DP_QQQKKK45.patternName()] = _drudgery_DP_QQQKKK45;
        funcMap3[deckAnalyzer.DP_QQQKKK6677.patternName()] = _drudgery_DP_QQQKKK6677;
    }

    public List<List<int>> AI_promptCards(List<int> myDeckCards){
        List<string> rdeckCards = _transToReadableCards(myDeckCards);
        List<List<string>> results = _doGetAllPromptsFreely(rdeckCards);
        List<List<int>> retResults = new List<List<int>>();
        if(_VERBOSE){
        }
        foreach(var x in results){
            List<int> intCards = _transToIntCards(myDeckCards, x);
            retResults.Add(intCards);
        }
        return retResults;
    }

    private string TransStrListToStr(List<string> strL){
        StringBuilder sb = new StringBuilder("[");
        int count = 0;
        foreach(var str in strL){
            sb.Append(str);
            if(count != strL.Count - 1){
                sb.Append(", ");
            }
        }
        sb.Append("]");
        return sb.ToString();
    }
    private bool HasThisBomb(List<List<string>> bombs, List<string> thisbomb){
        string thisbombStr = TransStrListToStr(thisbomb);
        foreach(List<string> bL in bombs){
            if(thisbombStr == TransStrListToStr(bL)){
                return true;
            }
        }
        return false;
    }
    public List<List<string>> _doGetAllPromptsFreely(List<string> myDeckCards){
        // 牌型，依次单张，对子，三带1，三带2，三带，顺子，连对，飞机，炸弹，王炸（没有跳过
        // :param myDeckCards:[] 手牌
        // :return: [[]] 提示牌
        List<Division> divisionList = deckAnalyzer.exhaustionDivision_prompt(myDeckCards);
        int minEntropy = _getMinEntroy(divisionList);       // 牌组里最小的熵，熵越大越混乱
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        CardsTypeContainer ctc = _getAllPrimaryElements(groups);         // 得到最基本的牌型
        List<List<string>> singles = ctc.singles;
        List<List<string>> pairs = ctc.pairs;
        List<List<string>> bombs = ctc.bombs;
        List<List<string>> tipsResults = new List<List<string>>();
        List<List<string>> shunzi = new List<List<string>>();
        // 炸弹牌，不管跟啥牌，先把能用的炸弹全找出来
        foreach(var div in divisionList){
            if(_getEntropy(div) > minEntropy){
                // 熵比最小值大 不要 没有计算的必要
                continue;
            }
            foreach(var pt in div.patterns.Keys){
                # region
                //     {"payloads", new List<object>()}        // [pattern, cards, score]
                //     List<object> payload = new List<object>{pt, pc, score};
                //     BaseDeckPattern pt
                //     List<string> pc
                //     Dictionary<string, object> score  = new Dictionary<string, object>{
                // {"streak", streak},
                // {"condition", length},
                // {"value", maxScore}
                // _tuple_ == tuple_ == payload; 
                // tuple_[0] == payload[0] == pt;
                // tuple_[1] == payload[1] == pc;
                # endregion
                foreach(var _tuple_ in (List<object>)(div.patterns[pt]["payloads"])){
                    List<object> tuple_ = (List<object>)_tuple_;
                    if(!HasThisBomb(bombs, (List<string>)tuple_[1])){
                        tipsResults.Add((List<string>)tuple_[1]);
                    }
                    if(deckAnalyzer.DP_34567 == (BaseDeckPattern)tuple_[0]){
                        shunzi.Add((List<string>)tuple_[1]);
                    }
                    else if(deckAnalyzer.DP_SINGLE == (BaseDeckPattern)tuple_[0]){
                        singles.Add((List<string>)tuple_[1]);
                    }
                    else if(deckAnalyzer.DP_QQ == (BaseDeckPattern)tuple_[0]){
                        pairs.Add((List<string>)tuple_[1]);
                    }
                }
            }
        }
        singles = _unique(singles);
        pairs = _unique(pairs);
        shunzi = _unique(shunzi);

        tipsResults.AddRange(singles);
        tipsResults.AddRange(pairs);
        tipsResults.AddRange(_exhaustionMatchPatterns(myDeckCards, singles, pairs));
        if(0 == tipsResults.Count && 0 == bombs.Count){
            return new List<List<string>>();
        }
        if(0 != tipsResults.Count){
            foreach(List<string> item in tipsResults){
                item.Sort(CardSortFunc);
            }
            tipsResults = _unique(tipsResults);
        }
        if(0 != bombs.Count){
            tipsResults.AddRange(bombs);
        }
        if(tipsResults.Count <= 1){
            return tipsResults;
        }
        else{
            // 如果枚举所有出牌情况大于1 则算牌值 排序
            Dictionary<string, Dictionary<string, object>> tipValueMap = _getTipsData(tipsResults, myDeckCards, minEntropy + 1, shunzi);
            tipsResults = _resortOnTipsEntroyAndValue(tipValueMap, tipsResults);
            if(null != tipsResults && 0 != tipsResults.Count){
                foreach(var item in tipsResults){
                    item.Sort(CardSortFunc);
                    tipsResults = _unique(tipsResults);
                }
            }
            tipsResults.Sort(delegate(List<string> left, List<string> right){
                return _getTipsScore(left) - _getTipsScore(right);
            });
        }
        if(null != bombs && 0 != bombs.Count){
            tipsResults = _removeBreakBombCards(tipsResults, bombs);
        }
        return tipsResults;
    }

    // 去除已有顺子中不好的顺子
    public List<List<string>> ValidShunzi(List<string> myDeckCards, List<List<string>> oriShunzi){
        List<List<string>> validShunzi = new List<List<string>>();
        Dictionary<string, int> group = BaseDeckPattern.groupby(myDeckCards);
        foreach(List<string> shun in oriShunzi){
            int badCountBefore = 0;
            int badCountAfter = 0;
            foreach(string card in shun){
                if(1 == group[card]){
                    ++badCountBefore;
                }
                else{
                    badCountAfter += group[card] - 1;
                }
            }
            if(badCountAfter <= badCountBefore){
                validShunzi.Add(shun);
            }
        }
        return validShunzi;
    }

    // 去除单张中包含在顺子里的
    private List<List<string>> ValidSingles(List<List<string>> shunzi, List<List<string>> oriSingles){
        List<string> inShunzi = new List<string>();
        foreach(List<string> shun in shunzi){
            foreach(string card in shun){
                inShunzi.Add(card);
            }
        }
        List<List<string>> validSingles = new List<List<string>>();
        foreach(List<string> single in oriSingles){
            if(!inShunzi.Contains(single[0])){
                validSingles.Add(single);
            }
        }
        return validSingles;
    }

    public List<List<string>> _removeBreakBombCards(List<List<string>> tipsResults, List<List<string>> bombs)
    {
        // 拿走拆炸弹的牌
        // :param tipsResults: [[]]
        // :param bombs: [[]]
        // :return: [[]]
        List<List<string>> newResult = new List<List<string>>();
        foreach(var item in tipsResults){
            if(0 == _bombBrokenCheck(item, tipsResults, bombs)){
                newResult.Add(item);
            }
        }
        tipsResults = newResult;
        return tipsResults;
    }

    private bool LLHasL(List<List<string>> strLL, List<string> strL){
        // strLL是否含有值和strL相同的元素
        string strLstr = StringTool.TransStrListToStr(strL);
        foreach(var s in strLL){
            if(strLstr == StringTool.TransStrListToStr(s)){
                return true;
            }
        }
        return false;
    }

    public int _bombBrokenCheck(List<string> tips, List<List<string>> tipsResults, List<List<string>> bombsList)
    {
        // tipsResults是熵最小的提示牌 包括炸弹
        // 如果tipsResults里拆炸弹、不拆炸弹都存在 只保留不拆炸弹
        // :param tips: []
        // :param tipsResults: [] 
        // :param bombsList: []
        // :return: int 1是拆了 0没拆
        BaseDeckPattern tips_pattern = deckAnalyzer.getPatternByCards(tips, false);
        if(tips_pattern == deckAnalyzer.DP_AAAA || tips_pattern == deckAnalyzer.DP_joker_JOKER){
            return 0;
        }
        else{
            foreach(var b in bombsList){
                bool has = LLHasL(tipsResults, b);
                foreach(var t in tips){
                    if((has && b.Contains(t)) || 
                        (tips_pattern == deckAnalyzer.DP_AAAA4455 || tips_pattern == deckAnalyzer.DP_AAAA45)){
                            return 1;
                        }
                }
            }
            return 0;
        }
    }

    public CardsTypeContainer _getAllPrimaryElements(Dictionary<string, int> groups){
        // 拿到所有基本的单张、对子、炸弹-
        // :param groups: 牌的dict
        List<List<string>> single = new List<List<string>>();
        List<List<string>> pair = new List<List<string>>();
        List<List<string>> bombs = new List<List<string>>();
        if(groups.ContainsKey("joker") && groups.ContainsKey("JOKER")){
            bombs.Add(new List<string>{"joker", "JOKER"});
            groups.Remove("joker");
            groups.Remove("JOKER");
        }
        foreach(var item in groups.Keys){
            if(1 == groups[item]){
                single.Add(new List<string>{item});
            }
            else if(2 == groups[item]){
                pair.Add(new List<string>{item, item});
            }
            else if(4 == groups[item]){
                bombs.Add(new List<string>{item, item, item, item});
            }
        }
        return new CardsTypeContainer(single, pair, bombs);
    }

    public List<List<string>> _exhaustionMatchPatterns(List<string> myDeckCards, List<List<string>> singles, List<List<string>> pairs)
    {
        // Brute Force 所有牌型
        // :param myDeckCards: []
        // :param singles: []
        // :param pairs: []
        // :return: []
        if(myDeckCards.Count < 4){
            return new List<List<string>>();
        }
        List<List<string>> tipsResults = new List<List<string>>();
        List<BaseDeckPattern> _checkSeq = new List<BaseDeckPattern>{
            deckAnalyzer.DP_KKKQ,
            deckAnalyzer.DP_KKKJJ,
            deckAnalyzer.DP_JJQQKK,
            deckAnalyzer.DP_QQQKKK,
            deckAnalyzer.DP_QQQKKK45,
            deckAnalyzer.DP_QQQKKK6677,
        };
        List<BaseDeckPattern> checkSeq = new List<BaseDeckPattern>();
        int startIndex = 0, endIndex = 0;
        if(myDeckCards.Count < 5){
            endIndex = 1;
        }
        else if(myDeckCards.Count < 6){
            endIndex = 2;
        }
        else if(myDeckCards.Count < 7){
            endIndex = 4;
        }
        else if(myDeckCards.Count < 9){
            endIndex = 5;
        }
        else{
            endIndex = _checkSeq.Count;
        }
        for(int i = startIndex; i < endIndex; ++i){
            checkSeq.Add(_checkSeq[i]);
        }

        foreach(var i in checkSeq){
            if(i == deckAnalyzer.DP_QQQKKK){
                List<string> topCards = new List<string>{"3", "3", "3", "4", "4", "4"};
                CardsType2 func = funcMap2[i.patternName()];
                tipsResults.AddRange(func(topCards, myDeckCards, singles, pairs));
            }
            else if(i == deckAnalyzer.DP_JJQQKK){
                List<string> topCards = new List<string>{"3", "3", "4", "4", "5", "5"};
                CardsType2 func = funcMap2[i.patternName()];
                tipsResults.AddRange(func(topCards, myDeckCards, singles, pairs));
            }
            else if(i == deckAnalyzer.DP_QQQKKK45){
                List<string> topCards = new List<string>{"3", "3", "3", "4", "4", "4", "6", "7"};
                Dictionary<string, object> topScore = i.patternScore(topCards);
                CardsType3 func = funcMap3[i.patternName()];
                List<List<string>> tempResults = func(topScore, myDeckCards, singles, pairs); 
                tipsResults.AddRange(tempResults);
            }
            else if(i == deckAnalyzer.DP_QQQKKK6677){
                List<string> topCards = new List<string>{"3", "3", "3", "4", "4", "4", "6", "6", "7", "7"};
                Dictionary<string, object> topScore = i.patternScore(topCards);
                CardsType3 func = funcMap3[i.patternName()];
                tipsResults.AddRange(func(topScore, myDeckCards, singles, pairs));
            }
            else if(i == deckAnalyzer.DP_KKKJJ){
                List<string> topCards = new List<string>{"3", "3", "3", "4", "4"};
                Dictionary<string, object> topScore = i.patternScore(topCards);
                CardsType3 func = funcMap3[i.patternName()];
                tipsResults.AddRange(func(topScore, myDeckCards, singles, pairs));
            }
            else if(i == deckAnalyzer.DP_KKKQ){
                List<string> topCards = new List<string>{"3", "3", "3", "4"};
                Dictionary<string, object> topScore = i.patternScore(topCards);
                CardsType3 func = funcMap3[i.patternName()];
                tipsResults.AddRange(func(topScore, myDeckCards, singles, pairs));
            }
        }
        return tipsResults;
    }

    public List<List<string>> _resortOnTipsEntroyAndValue(Dictionary<string, Dictionary<string, object>> tipsValueMap, List<List<string>> tips){
        // 根据先熵、后牌值给提示牌筛选排序
        // :param tipsValueMap: {key:index of tips, value:{'entropy': int, 'tipsValue': int}}
        // :param tips: []
        // :return: []
        List<PopUpTipsItem> allTips = new List<PopUpTipsItem>();
        foreach(string key in tipsValueMap.Keys){
            allTips.Add(new PopUpTipsItem(key, tipsValueMap[key]));
        }
        if(_VERBOSE1){
        }
        int abLen = tips.Count;
        int? minEntropy = null;
        foreach(var tip in allTips){
            int temp = (int)tip.itemValue["entropy"];
            if(null == minEntropy){
                minEntropy = temp;
                continue;
            }
            else if(minEntropy > temp){
                minEntropy = temp;
            }
        }
        List<PopUpTipsItem> pop_up_tips = new List<PopUpTipsItem>();
        foreach(var item in allTips){
            if((int)item.itemValue["entropy"] == minEntropy){
                pop_up_tips.Add(item);
            }
        }
        int addUpIdx = 0;
        foreach(var i in tipsValueMap.Keys){
            foreach(var div in (List<Division>)tipsValueMap[i]["divisions"]){
                if(div.roundsCount - 2 * div.bombsCount == minEntropy){
                    foreach(var pt in div.patterns.Keys){
                        foreach (var _tuple_ in (List<object>)(div.patterns[pt]["payloads"])){
                            List<object> tuple_ = (List<object>)_tuple_;
                            tips.Add((List<string>)tuple_[1]);
                            pop_up_tips.Add(new PopUpTipsItem((abLen + addUpIdx).ToString(), null));
                            addUpIdx += 1;
                        }
                    }
                }
            }
        }
        List<List<string>> results = new List<List<string>>();
        foreach(var i in pop_up_tips){
            results.Add(tips[int.Parse(i.itemKey)]);
        }
        if(_VERBOSE1){
        }
        return results;
    }

    public Dictionary<string, Dictionary<string, object>> _getTipsData(List<List<string>> results, List<string> myDeckCards, int ret, List<List<string>> shunzi){
        // 计算每个提示牌的熵、牌值
        // :param results: [[]] 提示牌
        // :param myDeckCards: [] 手牌
        // :param ret: int 上一把的最小熵
        // :param shunzi: [[]] 顺子们
        Dictionary<string, Dictionary<string, object>> tipsValueMap = new Dictionary<string, Dictionary<string, object>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        for(int tipsIdx = 0; tipsIdx < results.Count; ++tipsIdx){
            List<string> _myDeckCards = CopyTool.CopyStrList(myDeckCards);
            // remainCards没问题
            List<string> remainCards = _deckSubtract(_myDeckCards, results[tipsIdx]);
            List<List<List<string>>> streak = _updateShunzi(results[tipsIdx], shunzi, groups);
            List<Division> divisionList;
            if(0 == streak.Count){
                divisionList = deckAnalyzer.exhaustionDivision_prompt(remainCards);
            }
            else{
                divisionList = deckAnalyzer.exhaustionDivision_removeShunzi(remainCards, streak);
            }
            int minEntropy = _getMinEntroy(divisionList);
            if(minEntropy <= ret){
                tipsValueMap[tipsIdx.ToString()] = new Dictionary<string, object>();
                tipsValueMap[tipsIdx.ToString()]["tipsValue"] = _getTipsScore(results[tipsIdx]);
                tipsValueMap[tipsIdx.ToString()]["entropy"] = minEntropy;
                tipsValueMap[tipsIdx.ToString()]["divisions"] = divisionList;
            }
        }
        return tipsValueMap;
    }

    public List<string> _deckSubtract(List<string> deckCards, List<string> toremove){
        // :param deckCards: 原牌list
        // :param toremove: 要减的牌list
        // :return: 原牌-要减的牌 list
        foreach(var c in toremove){
            deckCards.Remove(c);
        }
        return deckCards;
    }

    public List<List<List<string>>> _updateShunzi(List<string> tips, List<List<string>> shunzi, Dictionary<string, int> groups){
        // 用上一把计算过的顺子 计算出了这个提示 还能用的顺子们
        // 然后排列组合
        // :param tips: [[]]
        // :param shunzi: [[]]
        // :param groups: {key=card,value=int}
        // :return: [[]]
        Dictionary<string, int> tipsGroup = BaseDeckPattern.groupby(tips);
        Dictionary<string, int> groups_ = new Dictionary<string, int>(groups);
        foreach(var item in tipsGroup.Keys){
            if(0 != groups_[item]){
                groups_[item] -= tipsGroup[item];
                if(groups_[item] <= 0){
                    groups_.Remove(item);
                }
            }
        }
        List<List<List<string>>> clear_streak = new List<List<List<string>>>();
        List<List<string>> streaks = new List<List<string>>();
        foreach(var i in shunzi){
            bool flag = true;
            foreach(var s in i){
                if(!groups_.ContainsKey(s)){
                    flag = false;
                    break;
                }
            }
            if(!flag){
                continue;
            }
            else{
                streaks.Add(i);
            }
        }
        for(int i = 0; i < streaks.Count; ++i){
            Dictionary<string, int> _groups = new Dictionary<string, int>(groups_);
            List<List<string>> clear = new List<List<string>>(){streaks[i]};
            foreach(var item in streaks[i]){
                _groups[item] -= 1;
            }
            for(int j = 0; j < streaks.Count; ++j){
                bool flag = true;
                for(int k = 0; k < clear.Count; ++k){
                    List<string> interct = CopyTool.StrListIntersection(CopyTool.TransStrListToSet(clear[k]), CopyTool.TransStrListToSet(streaks[j]));
                    if(null != interct && 0 != interct.Count){
                        foreach(var item in interct){
                            if(_groups[item] < 1){
                                flag = false;
                                break;
                            }
                        }
                    }
                    if(!flag){
                        break;
                    }
                }
                if(!flag){
                    continue;
                }
                else{
                    clear.Add(streaks[j]);
                    foreach(var item in streaks[j]){
                        _groups[item] -= 1;
                    }
                }
            }
            clear_streak.Add(clear);
        }
        return clear_streak;
    }

    public int _getTipsScore(List<string> tips){
        // :param tips: 单个提示
        // :param pattern: 提示的牌型
        // :return: int分数
        Dictionary<string, int> groups = BaseDeckPattern.groupby(tips);
        BaseDeckPattern pattern = deckAnalyzer.getPatternByCards(tips, false);
        int ret = 160000000;
        if(pattern == deckAnalyzer.DP_SINGLE){
            ret = CardTransfer.CARD_SCORE_MAP[tips[0]] * 10000000;
        }
        else if(pattern == deckAnalyzer.DP_QQ){
            string tips0 = tips[0];
            int tempInt = CardTransfer.CARD_SCORE_MAP[tips[0]];
            ret = CardTransfer.CARD_SCORE_MAP[tips[0]] * 10000000 + 1000000;
        }
        else if (pattern == deckAnalyzer.DP_KKKQ)
        {
            string main = "";
            string tail = "";
            foreach(var item in groups.Keys){
                if(3 == groups[item]){
                    main = item;
                }
                else if(groups[item] < 3){
                    tail = item;
                }
            }
            ret = CardTransfer.CARD_SCORE_MAP[main] * 10000000 + CardTransfer.CARD_SCORE_MAP[tail] * 10000 + 2000000;
        }
        else if(pattern == deckAnalyzer.DP_KKKJJ){
            string main = "";
            string tail = "";
            foreach(var item in groups.Keys){
                if(3 == groups[item]){
                    main = item;
                }
                else if(groups[item] < 3){
                    tail = item;
                }
            }
            ret = CardTransfer.CARD_SCORE_MAP[main] * 10000000 + CardTransfer.CARD_SCORE_MAP[tail] * 10000 + CardTransfer.CARD_SCORE_MAP[
                tail] * 100 + 3000000;
        }
        else if(pattern == deckAnalyzer.DP_KKK){
            ret = CardTransfer.CARD_SCORE_MAP[tips[0]] * 10000000 + 4000000;
        }
        else if(pattern == deckAnalyzer.DP_34567){
            string minMain = tips[0];
            int minTip = CardTransfer.CARD_SCORE_MAP[tips[0]];
            for(int i = 0; i < tips.Count; ++i){
                if(CardTransfer.CARD_SCORE_MAP[tips[i]] < minTip){
                    minMain = tips[i];
                    minTip = CardTransfer.CARD_SCORE_MAP[tips[i]];
                }
            }
            ret = CardTransfer.CARD_SCORE_MAP[minMain] * 10000000 + tips.Count * 10000 + 5000000;
        }
        else if(pattern == deckAnalyzer.DP_JJQQKK){
            string minMain = tips[0];
            int minTip = CardTransfer.CARD_SCORE_MAP[tips[0]];
            for(int i = 0; i < tips.Count; ++i){
                if(CardTransfer.CARD_SCORE_MAP[tips[i]] < minTip){
                    minMain = tips[i];
                    minTip = CardTransfer.CARD_SCORE_MAP[tips[i]];
                }
            }
            ret = CardTransfer.CARD_SCORE_MAP[minMain] * 10000000 + (tips.Count / 2) * 10000 + 6000000;
        }
        else if(pattern == deckAnalyzer.DP_QQQKKK45){
            List<string> main = new List<string>();
            List<string> tails = new List<string>();
            int tailNo = 0;
            foreach(var item in groups.Keys){
                if(3 == groups[item]){
                    main.Add(item);
                }
                else if(4 == groups[item]){
                    main.Add(item);
                    tails.Add(item);
                    tailNo += 1;
                }
                else if(groups[item] < 3){
                    tails.Add(item);
                    tailNo += 1;
                }
            }
            tails.Sort(CardSortFunc);
            string minMain = main[0];
            int minValue = CardTransfer.CARD_SCORE_MAP[main[0]];
            for(int i = 0; i < main.Count; ++i){
                if(CardTransfer.CARD_SCORE_MAP[main[i]] < minValue){
                    minMain = main[i];
                    minValue = CardTransfer.CARD_SCORE_MAP[main[i]];
                }
            }
            ret = CardTransfer.CARD_SCORE_MAP[minMain] * 10000000 + CardTransfer.CARD_SCORE_MAP[tails[0]] * 10000 + CardTransfer.CARD_SCORE_MAP[tails[1]] * 100 + tailNo + 7000000;
        }
        else if(pattern == deckAnalyzer.DP_QQQKKK6677){
            List<string> main = new List<string>();
            List<string> tails = new List<string>();
            int tailNo = 0;
            foreach(var item in groups.Keys){
                if(3 == groups[item]){
                    main.Add(item);
                }
                else if(groups[item] < 3){
                    tails.Add(item);
                    tailNo += 1;
                }
            }
            tails.Sort(CardSortFunc);
            string minMain = main[0];
            int minTip = CardTransfer.CARD_SCORE_MAP[main[0]];
            for(int i = 0; i < main.Count; ++i){
                if(CardTransfer.CARD_SCORE_MAP[main[i]] < minTip){
                    minMain = main[i];
                    minTip = CardTransfer.CARD_SCORE_MAP[main[i]];
                }
            }
            ret = CardTransfer.CARD_SCORE_MAP[minMain] * 10000000 + CardTransfer.CARD_SCORE_MAP[tails[0]] * 10000
                   + CardTransfer.CARD_SCORE_MAP[tails[1]] * 100 + tailNo + 7000000;
        }
        else if(pattern == deckAnalyzer.DP_QQQKKK){
            string minMain = tips[0];
            int minTip = CardTransfer.CARD_SCORE_MAP[tips[0]];
            for(int i = 0; i < tips.Count; ++i){
                if(CardTransfer.CARD_SCORE_MAP[tips[i]] < minTip){
                    minMain = tips[i];
                    minTip = CardTransfer.CARD_SCORE_MAP[tips[i]];
                }
            }
            ret = CardTransfer.CARD_SCORE_MAP[minMain] * 10000000 + tips.Count / 3 + 8000000;
        }
        else if(pattern == deckAnalyzer.DP_AAAA){
            ret = CardTransfer.CARD_SCORE_MAP[tips[0]] + 150000000;
        }
        else if(pattern == deckAnalyzer.DP_joker_JOKER){
            ret = CardTransfer.CARD_SCORE_MAP[tips[0]] + 150000000;
        }
        else{
            ret = 160000000;
        }
        return ret;
    }

    public int _getEntropy(Division div){
        // 计算牌组的熵 = 手数 - 炸弹数 * 2
        // :param div: Division 分牌牌组
        // :return: int
        return div.roundsCount - 2 * div.bombsCount;
    }

    public int _getMinEntroy(List<Division> divisionList){
        // :param divisionList: 手里的牌全部的拆解方法
        // :return: 返回拆解方法中手数最小的组 中 最小混乱度
        int ret = 9999;
        foreach(var div in divisionList){
            int entroy = div.roundsCount - 2 * div.bombsCount;
            if(entroy < ret){
                ret = entroy;
            }
        }
        return ret;
    }

    public List<string> _transToReadableCards(List<int> intCards){
        return HumanReadableCardHelper.readableList(intCards);
    }

    public List<int> _transToIntCards(List<int> intCards, List<string> readableCards)
    {
        Dictionary<string, List<int>> intMap = new Dictionary<string, List<int>>();
        foreach(var ic in intCards){
            string k = HumanReadableCardHelper.readableOne(ic);
            if(intMap.ContainsKey(k)){
                intMap[k].Add(ic);
            }
            else{
                intMap[k] = new List<int>{ic};
            }
        }

        List<int> ret = new List<int>();
        foreach(var rc in readableCards){
            Debug.Assert(intMap.ContainsKey(rc));
            Debug.Assert((intMap[rc]).Count > 0);
            ret.Add(intMap[rc][0]);
            intMap[rc].RemoveAt(0);
        }
        return ret;
    }

    public List<List<string>> _unique(List<List<string>> prompts){
        List<string> tokens = new List<string>();
        foreach(var strL in prompts){
            strL.Sort(CardSortFunc);
            tokens.Add(string.Join("-", strL));
        }
        List<string> unique = CopyTool.TransStrListToSet(tokens);
        List<List<string>> ret = new List<List<string>>();
        foreach(string str in unique){
            ret.Add(new List<string>(str.Split('-')));
        }
        return ret;
    }

    // checked
    public List<List<string>> _drudgery_DP_34567(List<string> topCards, List<string> myDeckCards)
    {
        List<List<string>> ret = new List<List<string>>();
        int streaklen = topCards.Count;
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        int minIdx = GetStreakableIndex(topCards[0]);
        Debug.Assert(minIdx >= 0);
        int maxIdx = this.streakable.Count - streaklen;
        if(maxIdx < minIdx){
            return ret;
        }
        for(int idx = minIdx; idx < maxIdx + 1; ++idx){
            bool followable = true;
            for(int i = 0; i < streaklen; ++i){
                if(!groups.ContainsKey(this.streakable[idx + i])){
                    followable = false;
                    break;
                }
                else if(1 != groups[this.streakable[idx + i]]){
                    followable = false;
                    break;
                }
            }
            if(followable){
                ret.Add(new List<string>());
                for(int i = 0; i < streaklen; ++i){
                    for(int j = 0; j < 1; ++j){
                        ret[ret.Count - 1].Add(this.streakable[idx + i]);
                    }
                }
            }
        }
        return ret;
    }

    // checked
    public List<List<string>> _drudgery_DP_JJQQKK(List<string> topCards, List<string> myDeckCards, List<List<string>> singles, List<List<string>> pairs){
        List<List<string>> ret = new List<List<string>>();
        List<int> streaklen = new List<int>{3, 4, 5, 6, 7, 8};
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        int minIdx = GetStreakableIndex(topCards[0]);
        Debug.Assert(minIdx >= 0);
        foreach(int l in streaklen){
            int maxIdx = this.streakable.Count - l;
            if (maxIdx < minIdx)
            {
                return ret;
            }
            for(int idx = minIdx; idx < maxIdx + 1; ++idx){
                bool followable = true;
                for(int i = 0; i < l; ++i){
                    if(!groups.ContainsKey(this.streakable[idx + i])){
                        followable = false;
                        break;
                    }
                    else if(groups[this.streakable[idx + i]] < 2){
                        followable = false;
                        break;
                    }
                }
                if(followable){
                    ret.Add(new List<string>());
                    for(int i = 0; i < l; ++i){
                        for(int j = 0; j < 2; ++j){
                            ret[ret.Count - 1].Add(this.streakable[idx + i]);
                        }
                    }
                }
            }
        }
        return ret;
    }

    // checked
    public List<List<string>> _drudgery_DP_QQQKKK(List<string> topCards, List<string> myDeckCards, List<List<string>> singles, List<List<string>> pairs){
        List<List<string>> ret = new List<List<string>>();
        List<int> streaklen = new List<int>{2, 3, 4, 5 ,6};
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        int minIdx = GetStreakableIndex(topCards[0]);
        Debug.Assert(minIdx >= 0);
        foreach(int l in streaklen){
            int maxIdx = this.streakable.Count - l;
            if (maxIdx < minIdx)
            {
                return ret;
            }
            for(int idx = minIdx; idx < maxIdx + 1; ++idx){
                bool followable = true;
                for(int i = 0; i < l; ++i){
                    if(!groups.ContainsKey(this.streakable[idx + i])){
                        followable = false;
                        break;
                    }
                    else if(groups[this.streakable[idx + i]] != 3){
                        followable = false;
                        break;
                    }
                }
                if(followable){
                    ret.Add(new List<string>());
                    for(int i = 0; i < l; ++i){
                        for(int j = 0; j < 3; ++j){
                            ret[ret.Count - 1].Add(this.streakable[idx + i]);
                        }
                    }
                }
            }
        }
        return ret;
    }

    public List<List<string>> _drudgery_DP_KKKQ(Dictionary<string, object> topScore, List<string> myDeckCards, List<List<string>> singles, List<List<string>> pairs){
        List<List<string>> ret = new List<List<string>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<string> candidates = new List<string>();
        foreach(var item in groups){
            string card = item.Key;
            int count = item.Value;
            if(3 == count && CardTransfer.CARD_SCORE_MAP[card] >= (int)topScore["value"]){
                candidates.Add(card);
            }
        }
        foreach(string k in candidates){
            foreach(List<string> card in singles){
                if(card[0] == k){
                    continue;
                }
                ret.Add(new List<string>{k, k, k, card[0]});
            }
        }
        return ret;
    }

    public List<List<string>> _drudgery_DP_KKKJJ(Dictionary<string, object> topScore, List<string> myDeckCards, List<List<string>> singles, List<List<string>> pairs){
        List<List<string>> ret = new List<List<string>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<string> candidates = new List<string>();
        foreach(var item in groups){
            string card = item.Key;
            int count = item.Value;
            if(3 == count && CardTransfer.CARD_SCORE_MAP[card] >= (int)topScore["value"]){
                candidates.Add(card);
            }
        }
        foreach(string k in candidates){
            foreach(List<string> card in pairs){
                if(card[0] == k){
                    continue;
                }
                ret.Add(new List<string>{k, k, k, card[0], card[0]});
            }
        }
        return ret;
    }

    public List<List<string>> _drudgery_DP_QQQKKK45(Dictionary<string, object> topScore, List<string> myDeckCards, List<List<string>> singles, List<List<string>> pairs){
        // 由于使用的排列组合方法是不考虑顺序(如：[1, 2] 和 [2, 1]是相同的，因此该方法结果元素数量比python脚本少一半)
        List<List<string>> ret = new List<List<string>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<List<string>> candidates = new List<List<string>>();
        List<int> streaklen = new List<int>{2, 3, 4, 5, 6};
        foreach(var item in groups){
            string card = item.Key;
            int count = item.Value;
            if(3 != count){
                continue;
            }
            if(!CardTransfer.STREAKABLE.Contains(card)){
                continue;
            }
            if(CardTransfer.CARD_SCORE_MAP[card] < (int)topScore["value"]){
                continue;
            }

            foreach(int l in streaklen){
                bool ok = true;
                List<string> tmp = new List<string>{card};
                for(int idx = 0; idx < l - 1; ++idx){
                    int nextIdx = GetStreakableIndex(card) + idx + 1;
                    // 找到可以成为飞机
                    if(nextIdx < this.streakable.Count){
                        string nextCard = this.streakable[nextIdx];
                        if(groups.ContainsKey(nextCard) && groups[nextCard] >= 3){
                            tmp.Add(nextCard);
                            continue;
                        }
                    }
                    ok = false;
                    break;
                }
                if(ok){
                    candidates.Add(tmp);
                }
            }
        }
        List<string> availables = new List<string>();
        foreach(List<string> item in singles){
            availables.Add(item[0]); 
        }
        foreach(List<string> streak in candidates){
            if(availables.Count < streak.Count){
                continue;
            }
            List<string[]> _pmlist = PermutationAndCombination<string>.GetCombination(availables.ToArray(), streak.Count);
            List<List<string>> pmlist = new List<List<string>>();
            foreach(var item in _pmlist){
                pmlist.Add(new List<string>(item));
            }
            foreach(List<string> pm in pmlist){
                List<string> cards = new List<string>();
                foreach(string mainCard in streak){
                    cards.Add(mainCard);
                    cards.Add(mainCard);
                    cards.Add(mainCard);
                }
                List<string> subs = CopyTool.CopyStrList(pm);
                List<string> cardsSet = CopyTool.TransStrListToSet(cards);
                List<string> intersct = CopyTool.StrListIntersection(CopyTool.TransStrListToSet(subs), cardsSet);
                if(null != intersct && 0 != intersct.Count){
                    continue;
                }
                else{
                    subs.Sort(CardSortFunc);
                    cards.AddRange(subs);
                    ret.Add(cards);
                }
            }
        }
        return ret;
    }

    public List<List<string>> _drudgery_DP_QQQKKK6677(Dictionary<string, object> topScore, List<string> myDeckCards, List<List<string>> singles, List<List<string>> pairs){
        List<List<string>> ret = new List<List<string>>();
        List<int> streaklen = new List<int>{2, 3, 4, 5, 6};
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<List<string>> candidates = new List<List<string>>();

        foreach(var item in groups){
            string card = item.Key;
            int count = item.Value;
            if(3 != count){
                continue;
            }
            if(!CardTransfer.STREAKABLE.Contains(card)){
                continue;
            }
            if(CardTransfer.CARD_SCORE_MAP[card] < (int)topScore["value"]){
                continue;
            }

            foreach(int l in streaklen){
                bool ok = true;
                List<string> tmp = new List<string>{card};
                for(int idx = 0; idx < (int)topScore["condition"] - 1; ++idx){
                    int nextIdx = GetStreakableIndex(card) + idx + 1;
                    // 找到可以成为飞机
                    if(nextIdx < this.streakable.Count){
                        string nextCard = this.streakable[nextIdx];
                        if(groups.ContainsKey(nextCard) && groups[nextCard] >= 3){
                            tmp.Add(nextCard);
                            continue;
                        }
                    }
                    ok = false;
                    break;
                }
                if(ok){
                    candidates.Add(tmp);
                }
            }
        }
        List<string> availables = new List<string>();
        foreach(List<string> item in pairs){
            availables.Add(item[0]); 
        }
        foreach(List<string> streak in candidates){
            if(availables.Count < streak.Count){
                continue;
            }
            List<string[]> _pmlist = PermutationAndCombination<string>.GetCombination(availables.ToArray(), streak.Count);
            List<List<string>> pmlist = new List<List<string>>();
            foreach(var item in _pmlist){
                pmlist.Add(new List<string>(item));
            }
            foreach(List<string> pm in pmlist){
                List<string> cards = new List<string>();
                foreach(string mainCard in streak){
                    cards.Add(mainCard);
                    cards.Add(mainCard);
                    cards.Add(mainCard);
                }
                List<string> subs = CopyTool.CopyStrList(pm);
                List<string> cardsSet = CopyTool.TransStrListToSet(cards);
                List<string> intersct = CopyTool.StrListIntersection(CopyTool.TransStrListToSet(subs), cardsSet);
                if(null != intersct && 0 != intersct.Count){
                    continue;
                }
                else{
                    subs.AddRange(CopyTool.CopyStrList(pm));
                    subs.Sort(CardSortFunc);
                    cards.AddRange(subs);
                    ret.Add(cards);
                }
            }
        }
        return ret;
    }

    // public bool _isReserveCard()
    // public int _getMinEntroyDivision
    // public int _minRoundsCount

    # region
    private int GetStreakableIndex(string str){
        for(int i = 0; i < this.streakable.Count; ++i){
            if(str == this.streakable[i]){
                return i;
            }
        }
        return -1;
    }

    private int CardSortFunc(string left, string right){
        return CardTransfer.CARD_SCORE_MAP[left] - CardTransfer.CARD_SCORE_MAP[right];
    }
    # endregion
}
