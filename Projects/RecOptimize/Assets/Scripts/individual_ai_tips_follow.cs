using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DizhuPromptAI_Follow_V1
{
    #region
    // 返回类型格式
    // [{'cards': ['10', '10'], 'main': 8, 'mismatch': 0, 'sub': 0},
    //  {'cards': ['K', 'K'], 'main': 11, 'mismatch': 0, 'sub': 0}, 
    //  {'cards': ['J', 'J'], 'main': 9, 'mismatch': 0, 'sub': 0},
    //  {'cards': ['9', '9'], 'main': 7, 'mismatch': 0, 'sub': 0}
    // ]
    public delegate List<Dictionary<string, object>> CardsTypeFollow(BaseDeckPattern topPattern, List<string> topCards, Dictionary<string, object> topScore, List<string> myDeckCards);
    DizhuDeckAnalyzer deckAnalyzer;
    Dictionary<string, CardsTypeFollow> funcMap;
    Dictionary<string, int> _cardpowers;
    # endregion
    public DizhuPromptAI_Follow_V1()
    {
        deckAnalyzer = new DizhuDeckAnalyzer();
        funcMap = new Dictionary<string, CardsTypeFollow>();
        funcMap[deckAnalyzer.DP_SINGLE.patternName()] = _drudgery_DP_SIMPLE;
        funcMap[deckAnalyzer.DP_QQ.patternName()] = _drudgery_DP_SIMPLE;
        funcMap[deckAnalyzer.DP_KKK.patternName()] = _drudgery_DP_SIMPLE;
        funcMap[deckAnalyzer.DP_34567.patternName()] = _drudgery_DP_SIMPLE;
        funcMap[deckAnalyzer.DP_JJQQKK.patternName()] = _drudgery_DP_SIMPLE;
        funcMap[deckAnalyzer.DP_QQQKKK.patternName()] = _drudgery_DP_SIMPLE;

        funcMap[deckAnalyzer.DP_KKKQ.patternName()] = _drudgery_DP_KKKQ;
        funcMap[deckAnalyzer.DP_KKKJJ.patternName()] = _drudgery_DP_KKKJJ;
        funcMap[deckAnalyzer.DP_AAAA45.patternName()] = _drudgery_DP_AAAA45;
        funcMap[deckAnalyzer.DP_AAAA4455.patternName()] = _drudgery_DP_AAAA4455;
        funcMap[deckAnalyzer.DP_QQQKKK45.patternName()] = _drudgery_DP_QQQKKK45;
        funcMap[deckAnalyzer.DP_QQQKKK6677.patternName()] = _drudgery_DP_QQQKKK6677;

        _cardpowers = new Dictionary<string, int>{
            {"3", 1},
            {"4", 2},
            {"5", 3},
            {"6", 4},
            {"7", 5},
            {"8", 6},
            {"9", 7},
            {"10", 8},
            {"J", 9},
            {"Q", 10},
            {"K", 11},
            {"A", 12},
            {"2", 13},
            {"joker", 14},
            {"JOKER", 15}
        };   // 简单的给牌赋予的值
    }

    public List<string> _transToReadableCards(List<int> intCards){
        return HumanReadableCardHelper.readableList(intCards);
    }
    public List<List<int>> AI_promptCards(List<int> topCards, List<int> myDeckCards){
        List<string> rtopCards = _transToReadableCards(topCards);
        List<string> rdeckCards = _transToReadableCards(myDeckCards);
        List<List<string>> strRet = _doGetAllPrompts(rtopCards, rdeckCards);
        if(null != strRet && 0 != strRet.Count){
            List<List<int>> ret = new List<List<int>>();
            foreach(List<string> strL in strRet){
                ret.Add(_transToIntCards(myDeckCards, strL));
            }
            return ret;
        }
        return new List<List<int>>();
    }

    public List<List<string>> _doGetAllPrompts(List<string> topCards, List<string> myDeckCards){
        // :param topCards: [] 要管的牌
        // :param myDeckCards: [] 手牌
        // :return: [[]] 可用的提示
        BaseDeckPattern topPattern = deckAnalyzer.getPatternByCards(topCards, false);
        // 不认识的牌型直接返回
        if(null == topPattern){
            return null;
        }
        // 大小王管不上 不要想了 直接返回
        if(deckAnalyzer.DP_joker_JOKER == topPattern){
            return null;
        }
        Dictionary<string, object> topScore = topPattern.patternScore(topCards);
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<List<string>> bombs = _getAvailableBombs(groups, topScore, topPattern);       // 得到所有能用的炸弹
        List<List<string>> tipsResults = new List<List<string>>();
        bombs.Sort(delegate(List<string> left, List<string> right){
            return CardTransfer.CARD_SCORE_MAP[left[0]] - CardTransfer.CARD_SCORE_MAP[right[0]];
        });
        if(deckAnalyzer.isBomb(topPattern)){
            // 如果对方是炸弹，直接返回我方可用炸弹
            return bombs;
        }
        else{
            // 枚举所有可能性
            CardsTypeFollow func = funcMap[topPattern.patternName()];
            List<Dictionary<string, object>> tipsResultsDic = func(topPattern, topCards, topScore, myDeckCards);
            foreach(var x in tipsResultsDic){
                tipsResults.Add((List<string>)x["cards"]);
            }
            tipsResults = _unique(tipsResults);
        }
        tipsResults.AddRange(bombs);
        if(tipsResults.Count > 1){
            // 如果枚举所有出牌情况大于1 则算牌值 排序
            Dictionary<string, Dictionary<string, int>> remainDataMap = _getTipsData(myDeckCards, tipsResults, topPattern, bombs);
            tipsResults = _resortOnKeys(remainDataMap, tipsResults);
        }
        return tipsResults;
    }

    private List<List<string>> _resortOnKeys(Dictionary<string, Dictionary<string, int>> remainDataMap, List<List<string>> tips){
        List<string> allTipsKey = new List<string>(remainDataMap.Keys);
        int minEntropy = 9999;
        int maxRounds = -1;
        remainDataMap[allTipsKey[0]].TryGetValue("entropy", out minEntropy);
        remainDataMap[allTipsKey[0]].TryGetValue("entropy", out maxRounds);
        for(int i = 0; i < allTipsKey.Count; ++i){
            int tempEntropy = 9999;
            int tempRounds = -1;
            remainDataMap[allTipsKey[i]].TryGetValue("entropy", out tempEntropy);
            remainDataMap[allTipsKey[i]].TryGetValue("entropy", out tempRounds);
            if(tempEntropy < minEntropy){
                minEntropy = tempEntropy;
            }
            if(tempRounds > maxRounds){
                maxRounds = tempRounds;
            }
        }
        List<string> firstKey = new List<string>();
        List<string> secondKey = new List<string>();
        for(int i = 0; i < allTipsKey.Count; ++i){
            int tempE = -1;
            remainDataMap[allTipsKey[i]].TryGetValue("entropy", out tempE);
            if(minEntropy == tempE){
                firstKey.Add(allTipsKey[i]);
            }
            if(minEntropy + 1 == tempE){
                secondKey.Add(allTipsKey[i]);
            }
        }
        SortRemainDataKey(remainDataMap, firstKey); 
        SortRemainDataKey(remainDataMap, secondKey); 
        List<string> pop_up_tipsKey = new List<string>(firstKey);      // 先手数，再牌值大小排序后合并
        pop_up_tipsKey.AddRange(secondKey);
        int still_need = 3 - pop_up_tipsKey.Count;     // 最小/最小+1的手数里拿不到3个提示就要补到3个
        if(still_need > 0){
            List<string> thirdKey = new List<string>();
            int count = 2;
            while(still_need - thirdKey.Count > 0 && minEntropy + count <= maxRounds){
                foreach(string key in allTipsKey){
                    if(remainDataMap[key]["entropy"] == minEntropy + count){
                        thirdKey.Add(key);
                    }
                }
                ++count;
            }
            SortRemainDataKey(remainDataMap, thirdKey);
            List<string> newThirdKey = new List<string>();
            for(int i = 0; i < still_need; ++i){
                if(i < thirdKey.Count){
                    newThirdKey.Add(thirdKey[i]);
                }
            }
            thirdKey = newThirdKey;
            pop_up_tipsKey.AddRange(thirdKey);
        }
        List<List<string>> results = new List<List<string>>();
        foreach(string i in pop_up_tipsKey){
            results.Add(tips[int.Parse(i)]);
        }
        return results;
    }

    private void SortRemainDataKey(Dictionary<string, Dictionary<string, int>> remainDataMap, List<string> keyList){
        // 先炸弹多到少排序，在牌值小到大排序
        keyList.Sort(delegate(string left, string right){
            if(remainDataMap[left]["maxBombs"] != remainDataMap[right]["maxBombs"]){
                return remainDataMap[right]["maxBombs"] - remainDataMap[left]["maxBombs"];
            }
            if(remainDataMap[left]["bombBreak"] != remainDataMap[right]["bombBreak"]){
                return remainDataMap[left]["bombBreak"] - remainDataMap[right]["bombBreak"];
            }
            return remainDataMap[left]["tipsValue"] - remainDataMap[right]["tipsValue"];
        });
    }

    private Dictionary<string, Dictionary<string, int>> _getTipsData(List<string> myDeckCards, List<List<string>>results, BaseDeckPattern pattern, List<List<string>> bombsList){
        // 计算出了提示牌以后的熵和剩余炸弹数
        // 计算提示牌的牌值、是否拆炸弹
        // :param myDeckCards: [] 手牌
        // :param results: {}
        // :param pattern: DeckPattern
        // :param bombsList: [[]]
        // :return: {}
        // TODO 删除排序
        Dictionary<string, Dictionary<string, int>> remainDataMap = new Dictionary<string, Dictionary<string, int>>();
        for(int tipsIdx = 0; tipsIdx < results.Count; ++tipsIdx){
            List<string> _myDeckCards = new List<string>(myDeckCards);
            List<string> remainCards = _deckSubtract(_myDeckCards, results[tipsIdx]);
            remainDataMap[tipsIdx.ToString()] = new Dictionary<string, int>();
            List<Division> divisionList = deckAnalyzer.exhaustionDivision_prompt(remainCards);

            SetDefaultDicValue(remainDataMap[tipsIdx.ToString()], "maxBombs", _bombsCountInMinRounds(divisionList));
            SetDefaultDicValue(remainDataMap[tipsIdx.ToString()], "entropy", _getMinEntroy(divisionList));
            SetDefaultDicValue(remainDataMap[tipsIdx.ToString()], "tipsValue", _getTipsScore(results[tipsIdx], pattern));
            SetDefaultDicValue(remainDataMap[tipsIdx.ToString()], "bombBreak", _bombBrokenCheck(results[tipsIdx], bombsList));
        }
        return remainDataMap;
    }

    private void SetDefaultDicValue(Dictionary<string, int> dic, string key, int val){
        if(!dic.ContainsKey(key)){
            dic[key] = val;
        }
    }

    private int _bombsCountInMinRounds(List<Division> divisionList){
        // :param divisionList: 手里的牌全部的拆解方法
        // :return: 返回拆解方法中手数最小的组 中 最大的炸弹数
        int ret = 9999;
        int bombsCount = 0;
        foreach(var div in divisionList){
            int entropy = div.roundsCount - 2 * div.bombsCount;
            if(entropy < ret){
                ret = entropy;
                bombsCount = div.bombsCount;
            }
            if(ret == entropy){
                bombsCount = Mathf.Max(bombsCount, div.bombsCount);
            }
        }
        return bombsCount;
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

    public int _getTipsScore(List<string> tips, BaseDeckPattern pattern)
    {
        # region
        // 特殊牌型：三带一/二、四带一/二、飞机
        // 特殊牌型的牌值
        // 三带一
        // 牌值由7位数组成，第一位是牌型，3带1是1，百位和千位是带牌，个位十位是三张
        // QQQK牌值为1001110
        // 三带二同理

        // 四带二
        // 牌值由7位数组成，第一位是牌型，4带2是2，百万位和万位是小的带牌，百位和千位是大的带牌，个位十位是主牌
        // QQQQJ5牌值为2030910
        // 四带二对同理

        // 飞机
        // 牌值由7位数组成，第一位是牌型，2连飞机是3，百万位和万位是小的带牌，百位和千位是大的带牌，个位十位是主牌
        // QQQKKKJ5牌值为3030910
        // 三飞机同理

        // :param tips: list提示的牌牌
        // :param pattern: 牌型类
        // :return: int牌值
        # endregion
        BaseDeckPattern tips_pattern = deckAnalyzer.getPatternByCards(tips, false);
        List<BaseDeckPattern> easyCheckSeq = new List<BaseDeckPattern>{
            deckAnalyzer.DP_QQQKKK,
            deckAnalyzer.DP_KKK,
            deckAnalyzer.DP_34567,
            deckAnalyzer.DP_JJQQKK,
            deckAnalyzer.DP_QQ,
            deckAnalyzer.DP_SINGLE
        };
        List<BaseDeckPattern> specialCheckSeq = new List<BaseDeckPattern>{
            deckAnalyzer.DP_QQQKKK6677,
            deckAnalyzer.DP_QQQKKK45,
            deckAnalyzer.DP_KKKJJ,
            deckAnalyzer.DP_KKKQ,
            deckAnalyzer.DP_AAAA4455,
            deckAnalyzer.DP_AAAA45,
        };
        List<BaseDeckPattern> bombsCheckSeq = new List<BaseDeckPattern>{
            deckAnalyzer.DP_AAAA,
            deckAnalyzer.DP_joker_JOKER
        };
        if(bombsCheckSeq.Contains(tips_pattern)){
            int score = 5000000;
            score += _cardpowers[tips[0]];
            return score;
        }
        if(easyCheckSeq.Contains(pattern) && !bombsCheckSeq.Contains(tips_pattern)){
            // 基本牌型只要看第一张的大小就行了
            return _cardpowers[tips[0]];
        }
        else if(specialCheckSeq.Contains(pattern) && !bombsCheckSeq.Contains(tips_pattern)){
            if(deckAnalyzer.DP_QQQKKK45 == pattern || deckAnalyzer.DP_QQQKKK6677 == pattern){
                // QQQKKKJ5牌值为3030910; QQQKKK33牌值为3010110
                int score = 3000000;
                Dictionary<string, int> groups = BaseDeckPattern.groupby(tips);
                List<string> main = new List<string>();
                foreach(var item in groups.Keys){
                    if(3 == groups[item]){
                        main.Add(item);
                    }
                }
                main.Sort(delegate(string left, string right){
                    return CardTransfer.CARD_SCORE_MAP[left] - CardTransfer.CARD_SCORE_MAP[right];
                });
                score += _cardpowers[main[0]];
                List<string> tail = _getTails(groups, 3);
                score += _getTailsValue(tail);
                return score;
            }
            else if(deckAnalyzer.DP_KKKJJ == pattern || deckAnalyzer.DP_KKKQ == pattern){
                // QQQK牌值为1001110
                int score = 1000000;
                Dictionary<string, int> groups = BaseDeckPattern.groupby(tips);
                string main = "";
                string tail = "";
                foreach(var item in groups.Keys){
                    if(3 == groups[item]){
                        main = item;
                    }
                    else
                        tail = item;
                }
                score += _cardpowers[main];
                score += _cardpowers[tail] * 100;
                return score;
            }
            else if(deckAnalyzer.DP_AAAA45 == pattern || deckAnalyzer.DP_AAAA4455 == pattern){
                // QQQQJ5牌值为2030910
                int score = 2000000;
                Dictionary<string, int> groups = BaseDeckPattern.groupby(tips);
                List<string> main = new List<string>();
                foreach(var item in groups.Keys){
                    if(3 == groups[item]){
                        main.Add(item);
                    }
                }
                main.Sort(delegate(string left, string right){
                    return CardTransfer.CARD_SCORE_MAP[left] - CardTransfer.CARD_SCORE_MAP[right];
                });
                score += _cardpowers[main[0]];
                List<string> tail = _getTails(groups, 4);
                score += _getTailsValue(tail);
                return score;
            }
        }
        return 0;
    }

    public int _bombBrokenCheck(List<string> tips, List<List<string>> bombsList){
        // 单纯计算提示牌有没有拆炸弹
        // :param tips: []
        // :param bombsList:[[]]
        int bomb_break = 0;
        BaseDeckPattern tips_pattern = deckAnalyzer.getPatternByCards(tips, false);
        if(deckAnalyzer.DP_AAAA == tips_pattern || deckAnalyzer.DP_joker_JOKER == tips_pattern){
            return 0;
        }
        else{
            foreach(List<string> b in bombsList){
                bool flag = false;
                foreach(string t in tips){
                    if(b.Contains(t)){
                        bomb_break = 1;
                        flag = true;
                        break;
                    }
                }
                if(flag){
                    break;
                }
            }
            return bomb_break;
        }
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

    private List<List<string>> _getAvailableBombs(Dictionary<string, int> groups, Dictionary<string, object> topScore, BaseDeckPattern topPattern){
        // :param groups: {key=card, value=int} 牌和对应的数量
        // :param topScore: int 要管的牌的分数
        // :param topPattern: DeckPattern 要管的牌的牌型
        // :return:
        List<List<string>> bombs = new List<List<string>>();
        foreach(string item in groups.Keys){
            if(4 == groups[item]){
                if(deckAnalyzer.DP_AAAA != topPattern || CardTransfer.CARD_SCORE_MAP[item] > (int)topScore["value"]){
                    bombs.Add(new List<string>{item, item, item, item});
                }
            }
        }
        if(groups.ContainsKey("joker") && groups.ContainsKey("JOKER")){
            bombs.Add(new List<string>{"joker", "JOKER"});
        }
        return bombs;
    }

    public List<Dictionary<string, object>> _drudgery_DP_SIMPLE(BaseDeckPattern topPattern, List<string> topCards, Dictionary<string, object> topScore, List<string> myDeckCards){
        // 返回类型格式
        // [{'cards': ['10', '10'], 'main': 8, 'mismatch': 0, 'sub': 0},
        //  {'cards': ['K', 'K'], 'main': 11, 'mismatch': 0, 'sub': 0}, 
        //  {'cards': ['J', 'J'], 'main': 9, 'mismatch': 0, 'sub': 0},
        //  {'cards': ['9', '9'], 'main': 7, 'mismatch': 0, 'sub': 0}
        // ]
        List<List<string>> followable = topPattern.allFollowable(topCards, myDeckCards);
        List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
        for(int i = 0; i < followable.Count; ++i){
            ret.Add(new Dictionary<string, object>{
                {"mismatch", 0},
                {"main", CardTransfer.CARD_SCORE_MAP[followable[i][0]]},
                {"sub", 0},
                {"cards", followable[i]}
            });
        }
        return ret;
    }

    public List<Dictionary<string, object>> _drudgery_DP_KKKQ(BaseDeckPattern topPattern, List<string> topCards, Dictionary<string, object> topScore, List<string> myDeckCards){
        List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);

        List<string> candidates = new List<string>();
        foreach(string card in groups.Keys){
            int count = groups[card];
            if(count >= 3 && CardTransfer.CARD_SCORE_MAP[card] > (int)topScore["value"]){
                candidates.Add(card);
            }
        }
        foreach(string k in candidates){
            foreach(string card in groups.Keys){
                int count = groups[card];
                if(card == k){
                    continue;
                }
                ret.Add(new Dictionary<string, object>{
                    {"mismatch", (count - 1) * 5},
                    {"main", CardTransfer.CARD_SCORE_MAP[k]},
                    {"sub", CardTransfer.CARD_SCORE_MAP[card]},
                    {"cards", new List<string>{k, k, k, card}}
                });
            }
        }
        return ret;
    }

    public List<Dictionary<string, object>> _drudgery_DP_KKKJJ(BaseDeckPattern topPattern, List<string> topCards, Dictionary<string, object> topScore, List<string> myDeckCards){
        List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<string> candidates = new List<string>();
        foreach(string card in groups.Keys){
            int count = groups[card];
            if(count >= 3 && CardTransfer.CARD_SCORE_MAP[card] > (int)topScore["value"]){
                candidates.Add(card);
            }
        }
        foreach(string k in candidates){
            foreach(string card in groups.Keys){
                int count = groups[card];
                if(card == k){
                    continue;
                }
                if(count < 2){
                    continue;
                }
                if(_isReserveCard(card, count, myDeckCards)){
                    continue;
                }
                ret.Add(new Dictionary<string, object>{
                    {"mismatch", count - 2},
                    {"main", CardTransfer.CARD_SCORE_MAP[k]},
                    {"sub", CardTransfer.CARD_SCORE_MAP[card]},
                    {"cards", new List<string>{k, k, k, card, card}}
                });
            }
        }
        return ret;
    }

    public List<Dictionary<string, object>> _drudgery_DP_AAAA45(BaseDeckPattern topPattern, List<string> topCards, Dictionary<string, object> topScore, List<string> myDeckCards){
        List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<string> candidates = new List<string>();
        foreach(string card in groups.Keys){
            int count = groups[card];
            if(4 == count && CardTransfer.CARD_SCORE_MAP[card] > (int)topScore["value"]){
                candidates.Add(card);
            }
        }
        foreach(string k in candidates){
            List<string> availables = new List<string>();
            foreach(string card in groups.Keys){
                int count = groups[card];
                if(card == k){
                    continue;
                }
                if(_isReserveCard(card, count, myDeckCards)){
                    continue;
                }
                for(int i = 0; i < count; ++i){
                    availables.Add(card);
                }
            }
            if(availables.Count < 2){
                continue;
            }
            List<List<string>> pmlist = Get2EleFromList(availables);
            foreach(List<string> pm in pmlist){
                int mismatch;
                if(pm[0] == pm[1]){
                    mismatch = (groups[pm[0]] - 2) * 5;
                    mismatch += 1;      // 拆对子的额外惩罚值
                }
                else{
                    mismatch = (groups[pm[0]] - 1) * 5;
                    mismatch += (groups[pm[1]] - 1) * 5;
                }
                List<string> subs = new List<string>{pm[0], pm[1]};
                subs.Sort(delegate(string left, string right){
                    return CardTransfer.CARD_SCORE_MAP[left] - CardTransfer.CARD_SCORE_MAP[right];
                });
                List<string> cards = new List<string>{k, k, k, k};
                cards.AddRange(subs);

                ret.Add(new Dictionary<string, object>{
                    {"mismatch", mismatch},
                    {"main", CardTransfer.CARD_SCORE_MAP[k]},
                    {"sub", Mathf.Max(CardTransfer.CARD_SCORE_MAP[pm[0]], CardTransfer.CARD_SCORE_MAP[pm[1]]) * 10 + CardTransfer.CARD_SCORE_MAP[pm[0]] + CardTransfer.CARD_SCORE_MAP[pm[1]]},
                    {"cards", cards}
                });
            }
        }
        return ret;
    }

    public List<Dictionary<string, object>> _drudgery_DP_AAAA4455(BaseDeckPattern topPattern, List<string> topCards, Dictionary<string, object> topScore, List<string> myDeckCards){
        List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<string> candidates = new List<string>();
        foreach(string card in groups.Keys){
            int count = groups[card];
            if(4 == count && CardTransfer.CARD_SCORE_MAP[card] > (int)topScore["value"]){
                candidates.Add(card);
            }
        }
        foreach(string k in candidates){
            List<string> availables = new List<string>();
            foreach(string card in groups.Keys){
                int count = groups[card];
                if(card == k || count < 2){
                    continue;
                }
                if(_isReserveCard(card, count, myDeckCards)){
                    continue;
                }
                availables.Add(card);
            }
            if(availables.Count < 2){
                continue;
            }
            List<List<string>> pmlist = Get2EleFromList(availables);
            foreach(List<string> pm in pmlist){
                int mismatch = (groups[pm[0]] - 2) * 5;
                mismatch += (groups[pm[1]] - 2) * 5;

                List<string> subs = new List<string>{pm[0], pm[0], pm[1], pm[1]};
                subs.Sort(delegate(string left, string right){
                    return CardTransfer.CARD_SCORE_MAP[left] - CardTransfer.CARD_SCORE_MAP[right];
                });
                List<string> cards = new List<string>{k, k, k, k};
                cards.AddRange(subs);

                ret.Add(new Dictionary<string, object>{
                    {"mismatch", mismatch},
                    {"main", CardTransfer.CARD_SCORE_MAP[k]},
                    {"sub", Mathf.Max(CardTransfer.CARD_SCORE_MAP[pm[0]], CardTransfer.CARD_SCORE_MAP[pm[1]]) * 10 + CardTransfer.CARD_SCORE_MAP[pm[0]] + CardTransfer.CARD_SCORE_MAP[pm[1]]},
                    {"cards", cards}
                });
            }
        }
        return ret;
    }

    public List<Dictionary<string, object>> _drudgery_DP_QQQKKK45(BaseDeckPattern topPattern, List<string> topCards, Dictionary<string, object> topScore, List<string> myDeckCards){
        List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<List<string>> candidates = new List<List<string>>();
        foreach(string card in groups.Keys){
            int count = groups[card];
            if(count < 3 || !CardTransfer.STREAKABLE.Contains(card)){
                continue;
            }
            if(CardTransfer.CARD_SCORE_MAP[card] <= (int)topScore["value"]){
                continue;
            }

            bool ok = true;
            List<string> tmp = new List<string>{card};
            for(int idx = 0; idx < (int)topScore["condition"] - 1; ++idx){
                int nextIdx = CardTransfer.STREAKABLE.IndexOf(card) + (idx + 1);
                // 找到可以成为飞机
                if(nextIdx < CardTransfer.STREAKABLE.Count){
                    string nextCard = CardTransfer.STREAKABLE[nextIdx];
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

        foreach(List<string> streak in candidates){
            List<string> availables = new List<string>();
            foreach(string card in groups.Keys){
                int count = groups[card];
                if(streak.Contains(card)){
                    continue;
                }
                if(_isReserveCard(card, count, myDeckCards)){
                    continue;
                }
                for(int i = 0; i < count; ++i){
                    availables.Add(card);
                }
            }
            if(availables.Count < streak.Count){
                continue;
            }
            List<string[]> pmlist = PermutationAndCombination<string>.GetCombination(availables.ToArray(), streak.Count);
            foreach(string[] pm in pmlist){
                int mismatch = 0;
                foreach(string subCard in pm){
                    int count = GetEleCountInList(pm, subCard);
                    mismatch += (groups[subCard] - count) * 5;
                    if(2 == count){
                        mismatch += 1;      // 拆对子的额外惩罚值
                    }
                    else if(3 == count){
                        mismatch += 2;      // 拆三张的额外惩罚值
                    }
                }
                List<string> subs = new List<string>(pm);
                subs.Sort(delegate(string left, string right){
                    return CardTransfer.CARD_SCORE_MAP[left] - CardTransfer.CARD_SCORE_MAP[right];
                });
                List<string> cards = new List<string>();
                foreach(string mainCard in streak){
                    cards.Add(mainCard);
                    cards.Add(mainCard);
                    cards.Add(mainCard);
                }
                cards.AddRange(subs);

                int maxSub = CardTransfer.CARD_SCORE_MAP[subs[0]];
                int sumSubs = 0;
                for(int i = 1; i < subs.Count; ++i){
                    sumSubs += CardTransfer.CARD_SCORE_MAP[subs[i]];
                    if(maxSub < CardTransfer.CARD_SCORE_MAP[subs[i]]){
                        maxSub = CardTransfer.CARD_SCORE_MAP[subs[i]];
                    }
                }

                ret.Add(new Dictionary<string, object>{
                    {"mismatch", mismatch},
                    {"main", CardTransfer.CARD_SCORE_MAP[streak[0]]},
                    {"sub", maxSub * 10 + sumSubs},
                    {"cards", cards}
                });
            }
        }
        return ret;
    }

    public List<Dictionary<string, object>> _drudgery_DP_QQQKKK6677(BaseDeckPattern topPattern, List<string> topCards, Dictionary<string, object> topScore, List<string> myDeckCards){
        List<Dictionary<string, object>> ret = new List<Dictionary<string, object>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(myDeckCards);
        List<List<string>> candidates = new List<List<string>>();
        foreach(string card in groups.Keys){
            int count = groups[card];
            if(count < 3 || !CardTransfer.STREAKABLE.Contains(card)){
                continue;
            }
            if(CardTransfer.CARD_SCORE_MAP[card] <= (int)topScore["value"]){
                continue;
            }

            bool ok = true;
            List<string> tmp = new List<string>{card};
            for(int idx = 0; idx < (int)topScore["condition"] - 1; ++idx){
                int nextIdx = CardTransfer.STREAKABLE.IndexOf(card) + (idx + 1);
                // 找到可以成为飞机
                if(nextIdx < CardTransfer.STREAKABLE.Count){
                    string nextCard = CardTransfer.STREAKABLE[nextIdx];
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

        foreach(List<string> streak in candidates){
            List<string> availables = new List<string>();
            foreach(string card in groups.Keys){
                int count = groups[card];
                if(streak.Contains(card) || count < 2){
                    continue;
                }
                if(_isReserveCard(card, count, myDeckCards)){
                    continue;
                }
                availables.Add(card); 
            }
            if(availables.Count < streak.Count){
                continue;
            }
            List<string[]> pmlist = PermutationAndCombination<string>.GetCombination(availables.ToArray(), streak.Count);
            foreach(string[] pm in pmlist){
                int mismatch = 0;
                foreach(string subCard in pm){
                    mismatch += (groups[subCard] - 2) * 5;
                }
                List<string> subs = new List<string>(pm);
                subs.AddRange(new List<string>(pm));
                subs.Sort(delegate(string left, string right){
                    return CardTransfer.CARD_SCORE_MAP[left] - CardTransfer.CARD_SCORE_MAP[right];
                });
                List<string> cards = new List<string>();
                foreach(string mainCard in streak){
                    cards.Add(mainCard);
                    cards.Add(mainCard);
                    cards.Add(mainCard);
                }
                cards.AddRange(subs);

                int maxSub = CardTransfer.CARD_SCORE_MAP[subs[0]];
                int sumSubs = 0;
                for(int i = 1; i < subs.Count; ++i){
                    sumSubs += CardTransfer.CARD_SCORE_MAP[subs[i]];
                    if(maxSub < CardTransfer.CARD_SCORE_MAP[subs[i]]){
                        maxSub = CardTransfer.CARD_SCORE_MAP[subs[i]];
                    }
                }

                ret.Add(new Dictionary<string, object>{
                    {"mismatch", mismatch},
                    {"main", CardTransfer.CARD_SCORE_MAP[streak[0]]},
                    {"sub", maxSub * 10 + sumSubs},
                    {"cards", cards}
                });
            }
        }
        return ret;
    }

    private int GetEleCountInList(string[] strList, string ele){
        int count = 0;
        for(int i = 0; i < strList.Length; ++i){
            if(ele == strList[i]){
                ++count;
            }
        }
        return count;
    }

    // TODO 测试这个方法
    private List<List<string>> Get2EleFromList(List<string> strL){
        // 返回列表中任意两个元素的组合(相同元素不同顺序都添加)
        List<List<string>> ret = new List<List<string>>();
        for(int i = 0; i < strL.Count; ++i){
            for(int j = 0; j < strL.Count; ++j){
                if(j != i){
                    ret.Add(new List<string>{strL[i], strL[j]});
                }
            }
        }
        return ret;
    }

    private bool _isReserveCard(string card, int count, List<string> myDeckCards){
        if(myDeckCards.Contains("joker") && myDeckCards.Contains("JOKER")){
            if("joker" == card || "JOKER" == card){
                return true;
            }
        }
        if(count >= 4){
            return true;
        }
        return false;
    }

    public static int _promptCmp(Dictionary<string, object> lhr, Dictionary<string, object> rhr)
    {
        if((int)lhr["mismatch"] != (int)rhr["mismatch"]){
            return (int)lhr["mismatch"] - (int)rhr["mismatch"];
        }
        else if((int)lhr["main"] != (int)rhr["main"]){
            return (int)lhr["main"] - (int)rhr["main"];
        }
        else{
            return (int)lhr["sub"] - (int)rhr["sub"];
        }
    }

    public int _getSubScore(BaseDeckPattern pattern, List<string> cards){
        if(deckAnalyzer.DP_KKKQ == pattern || deckAnalyzer.DP_KKKJJ == pattern){
            Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);
            foreach(string k in groups.Keys){
                int v = groups[k];
                if(v <= 2){
                    return CardTransfer.CARD_SCORE_MAP[k];
                }
            }
        }
        else if(deckAnalyzer.DP_AAAA45 == pattern || deckAnalyzer.DP_AAAA4455 == pattern){
            Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);
            List<string> subs = new List<string>();
            foreach(string k in groups.Keys){
                if(groups[k] <= 2){
                    subs.Add(k);
                }
            }
            if(2 == subs.Count){
                int temp0 = CardTransfer.CARD_SCORE_MAP[subs[0]];
                int temp1 = CardTransfer.CARD_SCORE_MAP[subs[1]];
                return Mathf.Max(temp0, temp1) * 10 + temp0 + temp1;
            }
            else if(1 == subs.Count){
                return CardTransfer.CARD_SCORE_MAP[subs[0]] * 12;
            }
            else{
                return 9999;
            }
        }
        else if(deckAnalyzer.DP_QQQKKK45 == pattern){
            Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);
            List<string> subs = new List<string>();
            foreach(string k in groups.Keys){
                if(groups[k] < 3){
                    subs.Add(k);
                }
            }
            if(subs.Count > 0){
                int maxSub = CardTransfer.CARD_SCORE_MAP[subs[0]];
                int sumSubs = 0;
                for(int i = 1; i < subs.Count; ++i){
                    sumSubs += CardTransfer.CARD_SCORE_MAP[subs[i]];
                    if(maxSub < CardTransfer.CARD_SCORE_MAP[subs[i]]){
                        maxSub = CardTransfer.CARD_SCORE_MAP[subs[i]];
                    }
                }
                return maxSub * 10 + sumSubs;
            }
            else{
                return 9999;
            }
        }
        else if(deckAnalyzer.DP_QQQKKK6677 == pattern){
            Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);
            List<string> subs = new List<string>();
            foreach(string k in groups.Keys){
                if(2 == groups[k]){
                    subs.Add(k);
                }
            }
            if(subs.Count > 0){
                int maxSub = CardTransfer.CARD_SCORE_MAP[subs[0]];
                int sumSubs = 0;
                for(int i = 1; i < subs.Count; ++i){
                    sumSubs += CardTransfer.CARD_SCORE_MAP[subs[i]];
                    if(maxSub < CardTransfer.CARD_SCORE_MAP[subs[i]]){
                        maxSub = CardTransfer.CARD_SCORE_MAP[subs[i]];
                    }
                }
                return maxSub * 10 + sumSubs;
            }
            else{
                return 9999;
            }
        }
        return 0;
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

    private List<List<string>> _unique(List<List<string>> prompts){
        List<string> tokens = new List<string>();
        for(int i = 0; i < prompts.Count; ++i){
            tokens.Add(string.Join("-", prompts[i]));
        }
        List<string> tokensSet = new List<string>();
        for(int i = 0; i < tokens.Count; ++i){
            if(!tokensSet.Contains(tokens[i])){
                tokensSet.Add(tokens[i]);
            }
        }
        List<List<string>> ret = new List<List<string>>();
        for(int i = 0; i < tokensSet.Count; ++i){
            ret.Add(new List<string>(tokensSet[i].Split('-')));
        }
        return ret;
    }

    private List<string> _getTails(Dictionary<string, int> groups, int mainLen){
        // :param groups: 牌型字典，key=牌，value=数量
        // :param mainLen: 主牌数量，比如三带一、飞机都是3，四带是4
        // :return: 带牌list 小大顺序
        List<string> tail = new List<string>();
        foreach(string k in groups.Keys){
            int count = groups[k];
            if(count < mainLen){
                if(count > 1){
                    for(int i = 0; i < count; ++i){
                        tail.Add(k);
                    }
                }
                else{
                    tail.Add(k);
                }
            }
        }
        tail.Sort(delegate (string left, string right) {
            return CardTransfer.CARD_SCORE_MAP[left] - CardTransfer.CARD_SCORE_MAP[right];
        });
        return tail;
    }

    private int _getTailsValue(List<string> tails){
        // 返回带牌的值
        // :param tails: 带牌list
        // :return: int值
        int score = 0;
        for(int i = 0; i < tails.Count; ++i){
            score += (int)Mathf.Pow(100, tails.Count - i) * _cardpowers[tails[i]];
        }
        return score;
    }
}
