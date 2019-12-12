using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class CardTransfer
{
    public static bool _DEBUG = true;
    public static Dictionary<string, int> CARD_SCORE_MAP {get;}= new Dictionary<string, int>
    {
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
    };
    public static List<string> STREAKABLE = new List<string>{"3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"};
}

public class PatternDickAndRounds
{
    public Dictionary<string, List<List<string>>> division;
    public List<List<string>> rounds;
    public PatternDickAndRounds(Dictionary<string, List<List<string>>> division, List<List<string>> rounds)
    {
        this.division = division;
        this.rounds = rounds;
    }
}

public class Division
{
    public Dictionary<string, Dictionary<string, object>> patterns;
    public Dictionary<string, List<string>> readable;
    public List<string> cards;
    public List<object> rounds = new List<object>();
    public List<object> bombs = new List<object>();
    public string identity;
    public int roundsCount;
    public int bombsCount;
    public int irregularity;           // 混乱度，值越高评分越低
    public Division(DizhuDeckAnalyzer analyzer, Dictionary<string, List<List<string>>> patternDict, List<string> deckCards)
    {
        patterns = new Dictionary<string, Dictionary<string, object>>();
        if(CardTransfer._DEBUG){
            this.cards = deckCards;
            // this.readable = DeepCopyPatternDict(patterns);
        }
        this.rounds = new List<object>();
        this.bombs = new List<object>();
        List<string> _identity = new List<string>();
        foreach(string pn in patternDict.Keys){
            this.patterns[pn] = new Dictionary<string, object>{
                {"minScore", null},
                {"maxScore", null},
                {"payloads", new List<object>()}        // [pattern, cards, score]
            };
            foreach(List<string> pc in patternDict[pn]){
                BaseDeckPattern pt = analyzer.getPatternByName(pn);
                Dictionary<string, object> score = pt.patternScore(pc);
                if(null == this.patterns[pn]["minScore"]){
                    this.patterns[pn]["minScore"] = score["value"];
                    this.patterns[pn]["maxScore"] = score["value"];
                }
                else{
                    this.patterns[pn]["minScore"] = Mathf.Min((int)score["value"], (int)this.patterns[pn]["minScore"]);
                    this.patterns[pn]["maxScore"] = Mathf.Max((int)score["value"], (int)this.patterns[pn]["maxScore"]);
                }
                List<object> payload = new List<object>{pt, pc, score};

                ((List<object>)this.patterns[pn]["payloads"]).Add(payload);
                this.rounds.Add(payload);
                if(pt == analyzer.DP_joker_JOKER || pt == analyzer.DP_AAAA){
                    this.bombs.Add(payload);
                }
                _identity.Add(string.Join("", pc));
            }
            this.roundsCount = this.rounds.Count;
            this.bombsCount = this.bombs.Count;
            this.irregularity = this.roundsCount - this.bombsCount * 2;
            _identity.Sort();
            this.identity = StringTool.TransStrListToStr(_identity);
        }
    }
    // 是不是一个无效的拆解方案（从顺子里拆出一张单牌
    public bool _isInferior(DizhuDeckAnalyzer analyzer)
    {
        if(patterns.ContainsKey(analyzer.DP_34567.patternName())){
            if(patterns.ContainsKey(analyzer.DP_SINGLE.patternName())){
                List<object> streaks = (List<object>)patterns[analyzer.DP_34567.patternName()]["payloads"];
                List<object> singles = (List<object>)patterns[analyzer.DP_SINGLE.patternName()]["payloads"];
                foreach(var single in singles){
                    List<string> pc = (List<string>)((List<object>)single)[1];
                    if(CardTransfer.STREAKABLE.Contains(pc[0])){
                        continue;
                    }
                    int idx = GetIndex(pc[0]);
                    foreach(var s in streaks){
                        List<string> pcS = (List<string>)((List<object>)s)[1];
                        if(idx == GetIndex(pcS[0]) - 1){
                            return true;
                        }
                        if(idx == GetIndex(pcS[pcS.Count - 1]) + 1){
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
    private int GetIndex(string v)
    {
        for (int i = 0; i < CardTransfer.STREAKABLE.Count; ++i)
        {
            if (v == CardTransfer.STREAKABLE[i])
            {
                return i;
            }
        }
        return -1;
    }

    public static int comp(Division lhr, Division rhr)
    {
        if(lhr.irregularity != rhr.irregularity){
            return lhr.irregularity - rhr.irregularity;
        }
        else{
            return rhr.bombsCount - lhr.bombsCount;
        }
    }
}

public class DizhuDeckAnalyzer
{
    # region
    public Dictionary<string, BaseDeckPattern> patterns = null;
    public Dictionary<string, BaseDeckPattern> cards2PatternMap = null;
    public BaseDeckPattern DP_joker_JOKER;
    public BaseDeckPattern DP_AAAA;
    public BaseDeckPattern DP_34567;
    public BaseDeckPattern DP_QQQKKK;
    public BaseDeckPattern DP_JJQQKK;
    public BaseDeckPattern DP_KKK;
    public BaseDeckPattern DP_QQ;
    public BaseDeckPattern DP_SINGLE;
    public List<BaseDeckPattern> defaultPatternSeq;

    public BaseDeckPattern DP_QQQKKK45;
    public BaseDeckPattern DP_QQQKKK6677;
    public BaseDeckPattern DP_KKKQ;
    public BaseDeckPattern DP_KKKJJ;
    public BaseDeckPattern DP_AAAA45;
    public BaseDeckPattern DP_AAAA4455;
    # endregion
    public DizhuDeckAnalyzer()
    {
        patterns = new Dictionary<string, BaseDeckPattern>();
        cards2PatternMap = new Dictionary<string, BaseDeckPattern>();

        DP_joker_JOKER = registerPattern(new DeckPattern_joker_JOKER());
        DP_AAAA = registerPattern(new DeckPattern_AAAA());
        DP_34567 = registerPattern(new DeckPattern_34567());
        DP_QQQKKK = registerPattern(new DeckPattern_QQQKKK());
        DP_JJQQKK = registerPattern(new DeckPattern_JJQQKK());
        DP_KKK = registerPattern(new DeckPattern_KKK());
        DP_QQ = registerPattern(new DeckPattern_QQ());
        DP_SINGLE = registerPattern(new DeckPattern_SINGLE());
        defaultPatternSeq = new List<BaseDeckPattern>{
            DP_joker_JOKER,
            DP_AAAA,
            DP_QQQKKK,
            DP_34567,
            DP_KKK,
            DP_JJQQKK,
            DP_QQ,
            DP_SINGLE
        };

        DP_QQQKKK45 = registerPattern(new DeckPattern_QQQKKK45());
        DP_QQQKKK6677 = registerPattern(new DeckPattern_QQQKKK6677());
        DP_KKKQ = registerPattern(new DeckPattern_KKKQ());
        DP_KKKJJ = registerPattern(new DeckPattern_KKKJJ());
        DP_AAAA45 = registerPattern(new DeckPattern_AAAA45());
        DP_AAAA4455 = registerPattern(new DeckPattern_AAAA4455());
    }
    private BaseDeckPattern registerPattern(BaseDeckPattern patternCls)
    {
        patterns[patternCls.patternName()] = patternCls;
        return patternCls;
    }

    public BaseDeckPattern getPatternByName(string name)
    {
        return patterns[name];
    }

    public BaseDeckPattern getPatternByCards(List<string> cards, bool assertMatch=true)
    {
        BaseDeckPattern.sortscore(cards);
        string key = StringTool.TransStrListToStr(cards);
        if(cards2PatternMap.ContainsKey(key)){
            return cards2PatternMap[key];
        }
        else{
            BaseDeckPattern result = _cards2Pattern(cards);
            if(assertMatch && null == result){
                Debug.LogError(StringTool.TransStrListToStr(cards));
            }
            cards2PatternMap[key] = result;
            return result;
        }
    }

    public bool isBomb(BaseDeckPattern pattern)
    {
        return DP_AAAA == pattern || DP_joker_JOKER == pattern;
    } 

    public bool isGreaterPatternScore(Dictionary<string, object> smaller, Dictionary<string, object> greater, bool containEqual=false)
    {
        if(!smaller.ContainsKey("condition")){
            return (int)greater["value"] > (int)smaller["value"];
        }
        else{
            if((int)greater["condition"] == (int)smaller["condition"]){
                if(containEqual){
                    if((int)greater["value"] >= (int)smaller["value"]){
                        return true;
                    }
                }
                else{
                    if((int)greater["value"] > (int)smaller["value"]){
                        return true;
                    }
                }
            }
            return false;
        }
    }

    public int _getEntropy(Division div)
    {
        return div.roundsCount - 2 * div.bombsCount;
    }

    // 该方法验证完毕(由于python的list转set会打乱原有list的顺序，所以输出结果各元素的顺序可能不同)
    public List<List<List<string>>> _uniqueShunzi(List<List<List<string>>> exhaustion)
    {
        // exhuastion: 一手牌可用顺子的全排列组合
        // return: 排列组合去重
        List<List<List<string>>> tokens = new List<List<List<string>>>();
        List<string> strTokens = new List<string>();
        foreach(List<List<string>> ex in exhaustion){
            ex.Sort(delegate(List<string> left, List<string> right){
                if(left[0] != right[0]){
                    // 先按顺子第一张牌的大小排序
                    return CardTransfer.CARD_SCORE_MAP[left[0]] - CardTransfer.CARD_SCORE_MAP[right[0]];
                }
                else{
                    // 如果第一张牌一样，按长度排序
                    return left.Count - right.Count;
                }
            });
            List<string> tokenStrL = new List<string>();
            foreach(List<string> strL in ex){
                tokenStrL.Add(string.Join("-", strL));
            }
            strTokens.Add(string.Join("#", tokenStrL));
        }
        List<string> setTokens = CopyTool.TransStrListToSet(strTokens);
        List<List<string>> tokerLL = new List<List<string>>();
        foreach(string tk in setTokens){
            tokerLL.Add(new List<string>(tk.Split('#')));
        }
        foreach(List<string> i in tokerLL){
            List<List<string>> tempTLL = new List<List<string>>();
            foreach(string j in i){
                if(null != j && "" != j){
                    List<string> tokenL = new List<string>(j.Split('-'));
                    tempTLL.Add(tokenL);
                }
            }
            tokens.Add(tempTLL);
        }
        return tokens;
    }

    public List<Division> exhaustionDivision(List<string> deckCards)
    {
        List<BaseDeckPattern> patternSeq = new List<BaseDeckPattern>
        {
            this.DP_joker_JOKER,
            this.DP_AAAA,
            this.DP_QQQKKK,
            this.DP_KKK,
            this.DP_JJQQKK,
            this.DP_QQQKKK,
            this.DP_SINGLE
        };
        Dictionary<string, int> groups = BaseDeckPattern.groupby(deckCards);
        List<List<List<string>>> exhuastion = _uniqueShunzi(_streakExhaustion(groups));
        List<Division> allDivisions = new List<Division>();
        foreach(List<List<string>> ex in exhuastion){
            List<string> cards = new List<string>(deckCards);
            BaseDeckPattern.sortscore(cards);
            if(null != ex && 0 != ex.Count){
                foreach(List<string> streak in ex){
                    foreach(string c in streak){
                        cards.Remove(c);
                    }
                }
            }
            Dictionary<string, List<List<string>>> elementary = elementaryDivision(cards, patternSeq);
            if (null != ex && 0 != ex.Count){
                foreach(List<string> streak in ex){
                    if(!elementary.ContainsKey(this.DP_34567.patternName())){
                        elementary[this.DP_34567.patternName()] = new List<List<string>>();
                    }
                    elementary[this.DP_34567.patternName()].Add(new List<string>(streak));
                }
            }
            _greedyMerge(elementary);
            foreach(bool keepBombs in new List<bool>{true, false}){
                PatternDickAndRounds pdar = aggressiveDivision(elementary, keepBombs);
                Dictionary<string, List<List<string>>> patternDict = pdar.division;
                List<List<string>> rounds = pdar.rounds;
                Division division = new Division(this, patternDict, deckCards);
                bool redundant = false;
                foreach(var d in allDivisions){
                    if(division.identity == d.identity){
                        redundant = true;
                        break;
                    }
                }
                if(!redundant){
                    allDivisions.Add(division);
                }
            }
        }
        allDivisions.Sort(Division.comp);
        return allDivisions;
    }

    private List<List<BaseDeckPattern>> GetDPComb(List<BaseDeckPattern> dpL){
        // 三选二
        List<List<BaseDeckPattern>> ret = new List<List<BaseDeckPattern>>();
        for(int i = 0; i < dpL.Count; ++i){
            List<BaseDeckPattern> inRet = new List<BaseDeckPattern>{dpL[i]};
            for(int j = 0; j < dpL.Count; ++j){
                if(i == j){
                    continue;
                }
                inRet.Add(dpL[j]);
                ret.Add(inRet);
            }
        }
        return ret;
    }

    // 由于字典的无序性导致和python的结果allDivisions.Count可能不一样，出现差别的地方在minEntroy和entropy比较的地方,
    // 如果较早将minEntroy所有entropy的最小值，则count也会较小
    public List<Division> exhaustionDivision_prompt(List<string> deckCards){
        // :param deckCards: list[str,str...] 手牌
        // :return: list of Division class  牌组
        List<List<BaseDeckPattern>> tryList = new List<List<BaseDeckPattern>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(deckCards);
        List<List<List<string>>> exhaustion = _uniqueShunzi(_streakExhaustion(groups));       // 找到所有顺子，然后去重
        if(exhaustion.Count > 10 || deckCards.Count > 17){
            // 如果能找到10组以上的顺子，那么这个牌基本全是顺子，不需要那么多尝试
            // 或者手牌过多 计算量过大 我们也缩减尝试 做次优解
            List<BaseDeckPattern> seq = new List<BaseDeckPattern>{
                this.DP_joker_JOKER, this.DP_AAAA, this.DP_QQQKKK, this.DP_KKK, this.DP_JJQQKK, this.DP_QQ, this.DP_SINGLE
            };
            tryList.Add(seq);
        }
        else{
            List<List<BaseDeckPattern>> _pmlist = GetDPComb(new List<BaseDeckPattern>{DP_QQQKKK, DP_KKK, DP_JJQQKK});
            foreach(var p in _pmlist){
                List<BaseDeckPattern> seq = new List<BaseDeckPattern>{DP_joker_JOKER, DP_AAAA};
                seq.AddRange(new List<BaseDeckPattern>(p));
                seq.AddRange(new List<BaseDeckPattern>{DP_QQ, DP_SINGLE});
                tryList.Add(seq);
            }
        }
        List<Division> allDivisions = new List<Division>();
        int minEntropy = 9999;      // 记录牌型组的最小熵
        foreach(var ex in exhaustion){
            List<string> cards = CopyTool.CopyStrList(deckCards);
            cards = BaseDeckPattern.sortscore(cards);
            if(null != ex && 0 != ex.Count){
                foreach(var streak in ex){
                    foreach(var c in streak){
                        cards.Remove(c);
                    }
                }
            }
            List<Dictionary<string, List<List<string>>>> elementaryList = new List<Dictionary<string, List<List<string>>>>();
            foreach(var patternSeq in tryList){
                Dictionary<string, List<List<string>>> eleDiv = elementaryDivision(cards, patternSeq);
                elementaryList.Add(eleDiv);
            }
            elementaryList = _uniqueElementaryDivision(elementaryList);
            foreach(var elementary in elementaryList){
                if(null != ex && 0 != ex.Count){
                    foreach(var streak in ex){
                        if(elementary.ContainsKey(DP_34567.patternName())){
                            elementary[DP_34567.patternName()].Add(CopyTool.CopyStrList(streak));
                        }
                        else{
                            elementary[DP_34567.patternName()] = new List<List<string>>{CopyTool.CopyStrList(streak)};
                        }
                    }
                }
                _greedyMerge(elementary);
                foreach(var keepBombs in new List<bool>{true, false}){
                    PatternDickAndRounds pdar = aggressiveDivision(elementary, keepBombs);
                    Dictionary<string, List<List<string>>> patternDict = pdar.division;
                    List<List<string>> rounds = pdar.rounds;
                    Division division = new Division(this, patternDict, deckCards);
                    int entropy = _getEntropy(division);
                    if(entropy > minEntropy){
                        // 对于比当前大的熵 不再记录 尽量缩减计算
                        continue;
                    }
                    else{
                        minEntropy = entropy;
                    }
                    bool redundant = false;
                    foreach(var d in allDivisions){
                        if(division.identity == d.identity){
                            redundant = true;
                            break;
                        }
                    }
                    if(!redundant){
                        allDivisions.Add(division);
                    }
                }
            }
        }
        allDivisions.Sort(Division.comp);
        // 由于字典的无序性导致和python的结果allDivisions.Count可能不一样，出现差别的地方在minEntroy和entropy比较的地方,
        // 如果较早将minEntroy所有entropy的最小值，则count也会较小
        return allDivisions;
    }

    private List<List<BaseDeckPattern>> GetCombine3to3Bdk(List<BaseDeckPattern> pattenList){
        List<List<BaseDeckPattern>> bdkLL = new List<List<BaseDeckPattern>>();
        foreach(BaseDeckPattern p in pattenList){
            foreach(BaseDeckPattern ap in pattenList){
                foreach(BaseDeckPattern aap in pattenList){
                    if (p != ap && p != aap && ap != aap) {
                        List<BaseDeckPattern> inbdkL = new List<BaseDeckPattern> { p, ap, aap};
                        bdkLL.Add(inbdkL);
                    }
                }
            }
        }
        return bdkLL;
    }

    public List<Division> exhaustionDivision_removeShunzi(List<string> deckCards, List<List<List<string>>> shunzi){
        // :param deckCards:  list[str,str...] 手牌
        // :param shunzi: list 顺子排列组合
        // :return: list of Division class  牌组
        deckCards.Sort();
        List<List<string>> cardsList = new List<List<string>>();
        List<List<BaseDeckPattern>> tryList = new List<List<BaseDeckPattern>>();
        List<BaseDeckPattern> baseList = new List<BaseDeckPattern>{DP_QQQKKK, DP_KKK, DP_JJQQKK};
        // List<BaseDeckPattern[]> _pmlist = PermutationAndCombination<BaseDeckPattern>.GetCombination(
            // new List<BaseDeckPattern>{DP_QQQKKK, DP_KKK, DP_JJQQKK}.ToArray(), 3
        // );
        List<List<BaseDeckPattern>> _pmlist = GetCombine3to3Bdk(baseList);
        foreach(var p in _pmlist){
            List<BaseDeckPattern> seq = new List<BaseDeckPattern>{DP_joker_JOKER, DP_AAAA};
            seq.AddRange(new List<BaseDeckPattern>(p));
            seq.Add(DP_QQ);
            seq.Add(DP_SINGLE);
            tryList.Add(seq);
        }
        shunzi.AddRange(new List<List<List<string>>>{new List<List<string>>()});
        List<List<List<string>>> exhaustion = _uniqueShunzi(shunzi);
        List<Division> allDivisions = new List<Division>();
        int minEntropy = 9999;      // 记录牌型组的最小熵
        foreach(var ex in exhaustion){
            List<string> cards = CopyTool.CopyStrList(deckCards);
            BaseDeckPattern.sortscore(cards);
            if(null != ex && 0 != ex.Count){
                foreach(var streak in ex){
                    foreach(var c in streak){
                        cards.Remove(c);
                    }
                }
            }
            List<Dictionary<string, List<List<string>>>> elementaryList = new List<Dictionary<string, List<List<string>>>>();
            foreach(var patternSeq in tryList){
                elementaryList.Add(elementaryDivision(cards, patternSeq));
            }
            elementaryList = _uniqueElementaryDivision(elementaryList);
            foreach(var elementary in elementaryList){
                if(null != ex && 0 != ex.Count){
                    foreach(var streak in ex){
                        if(elementary.ContainsKey(DP_34567.patternName())){
                            elementary[DP_34567.patternName()].Add(CopyTool.CopyStrList(streak));
                        }
                        else{
                            elementary[DP_34567.patternName()] = new List<List<string>>{streak};
                        }
                    }
                }
                _greedyMerge(elementary);
                foreach(var keepBombs in new List<bool>{true, false}){
                    PatternDickAndRounds pdar = aggressiveDivision(elementary, keepBombs);
                    Dictionary<string, List<List<string>>> patternDict = pdar.division;
                    List<List<string>> rounds = pdar.rounds;
                    Division division = new Division(this, patternDict, deckCards);
                    int entropy = _getEntropy(division);
                    if(entropy > minEntropy){
                        // 对于比当前大的熵 不再记录 尽量缩减计算
                        continue;
                    }
                    else{
                        minEntropy = entropy;
                    }
                    bool redundant = false;
                    foreach(var d in allDivisions){
                        if(division.identity == d.identity){
                            redundant = true;
                            break;
                        }
                    }
                    if(!redundant){
                        allDivisions.Add(division);
                    }
                }
            }
        } 
        allDivisions.Sort(Division.comp);
        int minEntropy0 = new DizhuPromptAI_Freeout_V1()._getMinEntroy(allDivisions);
        return allDivisions;
    }

    public List<Dictionary<string, List<List<string>>>> _uniqueElementaryDivision(List<Dictionary<string, List<List<string>>>> elementaryList){
        Dictionary<string, Dictionary<string, List<List<string>>>> unique = new Dictionary<string, Dictionary<string, List<List<string>>>>();
        foreach(var ele in elementaryList){
            Dictionary<string, string> token = new Dictionary<string, string>();
            List<string> keys = new List<string>(ele.Keys);
            keys.Sort();
            foreach(var k in keys){
                List<string> v = new List<string>();
                foreach(var clist in ele[k]){
                    v.Add(string.Join("-", clist));
                }
                v.Sort();
                token[k] = string.Join("#", v);
            }
            string tokenStr = StringTool.JsonDumpsStrStrDcit(token);
            if(!unique.ContainsKey(tokenStr)){
                unique[tokenStr] = ele;
            }
        }
        List<Dictionary<string, List<List<string>>>> ret = new List<Dictionary<string, List<List<string>>>>(); 
        foreach(var value in unique.Values){
            ret.Add(value);
        }
        return ret;
    }

    // 该方法验证完毕
    public List<List<List<string>>> _streakExhaustion(Dictionary<string, int> groups)
    {
        List<bool> streak = new List<bool>();
        // foreach(var x in CardTransfer.STREAKABLE){
        //     streak.Add(groups.ContainsKey(x));
        // }
        for(int i = 0; i < CardTransfer.STREAKABLE.Count; ++i){
            streak.Add(groups.ContainsKey(CardTransfer.STREAKABLE[i]));
        }
        int start = -1, end = -1;
        for(int idx = 0; idx < streak.Count; ++idx){
            if(streak[idx]){
                if(-1 == start){
                    start = idx;
                }
                end = idx;
            }
            else{
                if(-1 != start && -1 != end && end - start >= 4){
                    break;
                }
                start = -1;
                end = -1;
            }
        }
        List<List<List<string>>> results = new List<List<List<string>>>{
            new List<List<string>>()
        };
        if(-1 == start || -1 == end || end - start < 4){
            return results;
        }
        List<List<string>> substreaks = new List<List<string>>();
        int minLen = 5, maxLen = end - start + 1;
        for(int i = minLen; i < maxLen + 1; ++i){
            for(int j = 0; j < maxLen - i + 1; ++j){
                int substart = start + j;
                int subend = start + j + i;
                List<string> inList = new List<string>();
                for(int k = substart; k < subend; ++k){
                    inList.Add(CardTransfer.STREAKABLE[k]);
                }
                substreaks.Add(inList);
            }
        }
        foreach(List<string> sub in substreaks){
            Dictionary<string, int> copiedGroups = new Dictionary<string, int>(groups);
            foreach(string c in sub){
                copiedGroups[c] -= 1;
                if(0 == copiedGroups[c]){
                    copiedGroups.Remove(c);
                }
            }
            List<List<List<string>>> subResults = _streakExhaustion(copiedGroups);
            foreach(List<List<string>> sr in subResults){
                sr.Add(sub);
                results.Add(sr);
            }
        }
        return results;
    }

    // 拆分成基本牌型，由于和python字典的实现方式不同，最终结果中元素的顺序可能有所不同
    public Dictionary<string, List<List<string>>> elementaryDivision(List<string> _deckCards, List<BaseDeckPattern> patternSeq=null)
    {
        if(null == patternSeq){
            patternSeq = this.defaultPatternSeq;
        }
        List<string> deckCards = new List<string>(_deckCards);
        Dictionary<string, int> deckGroups = BaseDeckPattern.groupby(deckCards);

        Dictionary<string, List<List<string>>> ret = new Dictionary<string, List<List<string>>>();
        foreach(BaseDeckPattern pattern in patternSeq){
            List<List<string>> results = pattern.greedyMatch(deckCards, deckGroups);
            if(null != results && 0 != results.Count){
                ret[pattern.patternName()] = results;
                foreach(List<string> r in results){
                    foreach(string card in r){
                        deckCards.Remove(card);
                        deckGroups[card] -= 1;
                        if(0 == deckGroups[card]){
                            deckGroups.Remove(card);
                        }
                    }
                }
            }
        }
        _greedyMerge(ret);
        return ret;
    }

    private void RemoveStrLFromStrLL(List<List<string>> strLL, List<string> strL){
        string strLstr = string.Join("-", strL);
        int removeIndex = -1;
        for(int i = 0; i < strLL.Count; ++i){
            string strL2str = string.Join("-", strLL[i]);
            if(strLstr == strL2str){
                removeIndex = i;
                break;
            }
        }
        if(-1 != removeIndex){
            strLL.RemoveAt(removeIndex);
        }
    }

    // 贪婪匹配组合牌型，使手数最小，同时权值最大
    // 按照飞机带单、飞机带对，三带单、三带对、四代二单，四代二对的顺序
    public PatternDickAndRounds aggressiveDivision(Dictionary<string, List<List<string>>> _division, bool keepBombs=false)
    {
        Dictionary<string, List<List<string>>> division = CopyDivision(_division);
        # region
        List<List<string>> singleList = new List<List<string>>();
        List<List<string>> pairList = new List<List<string>>();
        List<List<string>> chain = new List<List<string>>();
        if(division.ContainsKey(this.DP_SINGLE.patternName())){
            singleList = division[this.DP_SINGLE.patternName()];
        } 
        if(division.ContainsKey(this.DP_QQ.patternName())){
            pairList = division[this.DP_QQ.patternName()];
        } 
        if(division.ContainsKey(this.DP_34567.patternName())){
            chain = division[this.DP_34567.patternName()];
        }
        # endregion

        if(division.ContainsKey(this.DP_34567.patternName())){
            // 单顺和可以连起来三张以上的单牌 合并
            List<List<int>> streakable_single = _checkPotentialChain(singleList);
            for(int s = 0; s < streakable_single.Count; ++s){
                for(int c = 0; c < chain.Count; ++c){
                    List<string> link = chain[chain.Count - c - 1];
                    List<int> streak = streakable_single[streakable_single.Count - s - 1];
                    List<string> r_streak = HumanReadableCardHelper.readableList(streak);
                    List<string> common = StrListIntersection(CopyTool.TransStrListToSet(link), r_streak);
                    if(0 == common.Count || link.Count - common.Count > 2){
                        // 如果顺子和要合并的短顺长度差>2，就没必要合了，因为手数不会减少
                        continue;
                    }
                    else if(link.Count - common.Count <= 2){
                        // 如果顺子和要合并的短顺 长度差值<2 合并起来才能减少手数
                        List<List<string>> inStrLL = new List<List<string>>();
                        foreach(string item in link){
                            if(!common.Contains(item)){
                                inStrLL.Add(new List<string>(){item});
                            }
                        }
                        singleList.AddRange(inStrLL);
                        foreach(string item in common){
                            RemoveStrLFromStrLL(singleList, new List<string>{item});
                        }
                        RemoveStrLFromStrLL(chain, link);
                        List<string> inList = new List<string>();

                        common.AddRange(common);
                        if(division.ContainsKey(this.DP_JJQQKK.patternName())){
                            division[this.DP_JJQQKK.patternName()].Add(common);
                        }
                        else{
                            division[this.DP_JJQQKK.patternName()] = new List<List<string>>{common};
                        }
                        break;
                    }
                }
            }
        }
        if(division.ContainsKey(this.DP_QQQKKK.patternName())){
            List<List<string>> QQQKKK = division[this.DP_QQQKKK.patternName()];
            while(null != QQQKKK && 0 != QQQKKK.Count){
                List<string> head = QQQKKK[0];
                Debug.Assert(0 == head.Count % 3);
                int count = head.Count / 3;
                if(singleList.Count >= count){
                    for(int i = 0; i < count; ++i){
                        head.AddRange(singleList[i]);
                    }
                    if(division.ContainsKey(this.DP_QQQKKK45.patternName())){
                        division[this.DP_QQQKKK45.patternName()].Add(head);
                    }
                    else{
                        division[this.DP_QQQKKK45.patternName()] = new List<List<string>>(){head};
                    }
                    QQQKKK.RemoveAt(0);
                    List<List<string>> newSingleList = new List<List<string>>();
                    for(int i = count; i < singleList.Count; ++i){
                        newSingleList.Add(singleList[i]);
                    }
                    singleList = newSingleList;
                }
                else if(pairList.Count >= count){
                    for(int i = 0; i < count; ++i){
                        head.AddRange(pairList[i]);
                    }
                    if(division.ContainsKey(this.DP_QQQKKK6677.patternName())){
                        division[this.DP_QQQKKK6677.patternName()].Add(head);
                    }
                    else{
                        division[this.DP_QQQKKK6677.patternName()] = new List<List<string>>(){head};
                    }
                    QQQKKK.RemoveAt(0);
                    List<List<string>> newPairList = new List<List<string>>();
                    for(int i = count; i < pairList.Count; ++i){
                        newPairList.Add(pairList[i]);
                    }
                    pairList = newPairList;
                }
                else{
                    break;
                }
            }
        }
        if(division.ContainsKey(this.DP_KKK.patternName())){
            List<List<string>> KKK = division[this.DP_KKK.patternName()];
            while(null != KKK && 0 != KKK.Count){
                List<string> head = KKK[0];
                if (null != singleList && 0 != singleList.Count && null != pairList && 0 != pairList.Count)
                {
                    // 就是让三带一/二 先带 单或者对 里面，数量少的那一组，这样给炸弹带牌的机会
                    if (singleList.Count <= pairList.Count)
                    {
                        head.AddRange(singleList[0]);
                        if (division.ContainsKey(this.DP_KKKQ.patternName()))
                        {
                            division[this.DP_KKKQ.patternName()].Add(head);
                        }
                        else
                        {
                            division[this.DP_KKKQ.patternName()] = new List<List<string>>() { head };
                        }
                        KKK.RemoveAt(0);
                        List<List<string>> newSingleList = new List<List<string>>();
                        for (int i = 1; i < singleList.Count; ++i)
                        {
                            newSingleList.Add(singleList[i]);
                        }
                        singleList = newSingleList;
                    }
                    else{
                        head.AddRange(pairList[0]);
                        if (division.ContainsKey(this.DP_KKKJJ.patternName()))
                        {
                            division[this.DP_KKKJJ.patternName()].Add(head);
                        }
                        else
                        {
                            division[this.DP_KKKJJ.patternName()] = new List<List<string>>() { head };
                        }
                        KKK.RemoveAt(0);
                        List<List<string>> newPairList = new List<List<string>>();
                        for (int i = 1; i < pairList.Count; ++i)
                        {
                            newPairList.Add(pairList[i]);
                        }
                        pairList = newPairList;
                    }
                }
                else if(null != singleList && 0 != singleList.Count){
                    head.AddRange(singleList[0]);
                    if (division.ContainsKey(this.DP_KKKQ.patternName()))
                    {
                        division[this.DP_KKKQ.patternName()].Add(head);
                    }
                    else
                    {
                        division[this.DP_KKKQ.patternName()] = new List<List<string>>() { head };
                    }
                    KKK.RemoveAt(0);
                    List<List<string>> newSingleList = new List<List<string>>();
                    for (int i = 1; i < singleList.Count; ++i)
                    {
                        newSingleList.Add(singleList[i]);
                    }
                    singleList = newSingleList;
                }
                else if(null != pairList && 0 != pairList.Count){
                    head.AddRange(pairList[0]);
                    if (division.ContainsKey(this.DP_KKKJJ.patternName()))
                    {
                        division[this.DP_KKKJJ.patternName()].Add(head);
                    }
                    else
                    {
                        division[this.DP_KKKJJ.patternName()] = new List<List<string>>() { head };
                    }
                    KKK.RemoveAt(0);
                    List<List<string>> newPairList = new List<List<string>>();
                    for (int i = 1; i < pairList.Count; ++i)
                    {
                        newPairList.Add(pairList[i]);
                    }
                    pairList = newPairList;
                }
                else{
                    break;
                }
            }
        }
        if (!keepBombs)
        {
            if (division.ContainsKey(this.DP_AAAA.patternName()))
            {
                List<List<string>> AAAA = division[this.DP_AAAA.patternName()];
                while (null != AAAA && 0 != AAAA.Count)
                {
                    List<string> head = AAAA[0];
                    if (singleList.Count >= 2)
                    {
                        head.AddRange(singleList[0]);
                        head.AddRange(singleList[1]);
                        if (division.ContainsKey(this.DP_AAAA45.patternName()))
                        {
                            division[this.DP_AAAA45.patternName()].Add(head);
                        }
                        else
                        {
                            division[this.DP_AAAA45.patternName()] = new List<List<string>>() { head };
                        }
                        AAAA.RemoveAt(0);
                        List<List<string>> newSingleList = new List<List<string>>();
                        for (int i = 2; i < singleList.Count; ++i)
                        {
                            newSingleList.Add(singleList[i]);
                        }
                        singleList = newSingleList;
                    }
                    else if (pairList.Count >= 2)
                    {
                        head.AddRange(pairList[0]);
                        head.AddRange(pairList[1]);
                        if (division.ContainsKey(this.DP_AAAA4455.patternName()))
                        {
                            division[this.DP_AAAA4455.patternName()].Add(head);
                        }
                        else
                        {
                            division[this.DP_AAAA4455.patternName()] = new List<List<string>>() { head };
                        }
                        AAAA.RemoveAt(0);
                        List<List<string>> newPairList = new List<List<string>>();
                        for (int i = 2; i < pairList.Count; ++i)
                        {
                            newPairList.Add(pairList[i]);
                        }
                        pairList = newPairList;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        division[this.DP_SINGLE.patternName()] = singleList;
        division[this.DP_QQ.patternName()] = pairList;
        division[this.DP_34567.patternName()] = chain;

        division = _clearEmptyPattern(division);
        List<List<string>> rounds = new List<List<string>>();     // 手数
        foreach(string k in division.Keys){
            rounds.AddRange(division[k]);
        }
        return new PatternDickAndRounds(division, rounds);
    }

    private List<string> StrListIntersection(List<string> sL1, List<string> sL2){
        List<string> commonList = new List<string>();
        foreach(string s1 in sL1){
            if(sL2.Contains(s1)){
                commonList.Add(s1);
            }
        }
        return commonList;
    }

    private Dictionary<string, List<List<string>>> CopyDivision(Dictionary<string, List<List<string>>> division)
    {
        Dictionary<string, List<List<string>>> newDivision = new Dictionary<string, List<List<string>>>();
        foreach(string key in division.Keys){
            newDivision[key] = new List<List<string>>();
            foreach(List<string> strL in division[key]){
                newDivision[key].Add(new List<string>(strL));
            }
        }
        return newDivision;
    }

    public List<List<int>> _checkPotentialChain(List<List<string>> singleList){
        // return: 可以形成短顺(长度<5)的短顺链表
        List<string> flat_list = new List<string>();
        foreach(var subList in singleList){
            foreach(var item in subList){
                flat_list.Add(item);
            }
        }
        Dictionary<string, int> intMap = new Dictionary<string, int>
        {
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
            {"A", 13}
        };
        List<int> int_list = new List<int>();
        foreach(var single in flat_list){
            if(intMap.ContainsKey(single)){
                int_list.Add(intMap[single]);
            }
        }
        List<List<int>> result = new List<List<int>>();
        List<int> int_list_copy = new List<int>(int_list);
        foreach(int i in int_list){
            List<int> chain = new List<int>();
            int count = 1;
            bool flag = true;
            chain.Add(i);
            while(flag){
                if(int_list_copy.Contains(i + count)){
                    chain.Add(i + count);
                    int_list_copy.Remove(i + count);
                    count += 1;
                }
                else{
                    if(chain.Count >= 3){
                        result.Add(chain);
                    }
                    flag = false;
                    continue;
                }
            }
        }
        return result;
    }

    private bool InIntList(List<int> intList, int v, int startIndex){
        for(int i = startIndex; i < intList.Count; ++i){
            if(v == intList[i]){
                return true;
            }
        }
        return false;
    }

    private Dictionary<string, List<List<string>>> _clearEmptyPattern(Dictionary<string, List<List<string>>> division)
    {
        // C#不允许删除正在遍历中的字典项
        // foreach(var item in division){
        //     string k = item.Key;
        //     List<List<string>> v = item.Value;
        //     if(null == v || 0 == v.Count){
        //         division.Remove(k);
        //     }
        // }
        Dictionary<string, List<List<string>>> newDivision = new Dictionary<string, List<List<string>>>();
        foreach(var item in division){
            string k = item.Key;
            List<List<string>> v = item.Value;
            if(null != v && 0 != v.Count){
                newDivision[k] = v;
            }
        }
        return newDivision;
    }

    public void _greedyMerge(Dictionary<string, List<List<string>>> division)
    {
        if(!division.ContainsKey(this.DP_34567.patternName())){
            return;
        }
        List<List<string>> checklist = division[this.DP_34567.patternName()];
        checklist.Sort(delegate(List<string> left, List<string> right){
            return left.Count - right.Count;
        });
        List<List<string>> combination = new List<List<string>>();
        if(checklist.Count >= 2){
            division.Remove(this.DP_34567.patternName());   // 全部从队列中移出
            while(0 != checklist.Count){
                bool flag = false;  // 第一个顺子是否已被合并
                for(int idx = 1; idx < checklist.Count; ++idx){
                    if(checklist[0].Count == checklist[idx].Count){ // 顺子长度相同
                        if(checklist[0][0] == checklist[idx][0]){   // 顺手相同
                            // 合并成双顺，存储到combination
                            List<string> merged = checklist[idx];
                            checklist.RemoveAt(idx);
                            merged.AddRange(checklist[0]);
                            checklist.RemoveAt(0);
                            BaseDeckPattern.sortscore(merged);
                            combination.Add(merged);
                            flag = true;
                            break;
                        }
                    }
                }
                if(!flag){      // 没有被合并，则pop出头部并添加会单顺队列，继续检查下一个单顺
                    List<string> inList = checklist[0];
                    checklist.RemoveAt(0);
                    if(division.ContainsKey(this.DP_34567.patternName())){
                        division[this.DP_34567.patternName()].Add(inList);
                    }
                    else{
                        division[this.DP_34567.patternName()] = new List<List<string>>{inList};
                    }
                }
            }
        }

        if(null != combination && 0 != combination.Count){  // 添加到双顺队列
            if(division.ContainsKey(this.DP_JJQQKK.patternName())){
                division[this.DP_JJQQKK.patternName()].AddRange(combination);
            }
            else{
                division[this.DP_JJQQKK.patternName()] = new List<List<string>>(combination);
            }
        }
        
        if(!division.ContainsKey(this.DP_34567.patternName())){
            return;
        }
        if(!division.ContainsKey(this.DP_JJQQKK.patternName())){
            return;
        }

        combination = new List<List<string>>();
        checklist = division[this.DP_34567.patternName()];
        checklist.Sort(delegate(List<string> left, List<string> right){
            return left.Count - right.Count;
        });
        division.Remove(this.DP_34567.patternName());       // 全部从单顺队列中移出

        List<List<string>> checklist2 = division[this.DP_JJQQKK.patternName()];
        checklist2.Sort(delegate(List<string> left, List<string> right){
            return left.Count - right.Count;
        });
        division.Remove(this.DP_JJQQKK.patternName());       // 全部从双顺队列中移出

        while(0 != checklist2.Count){
            bool flag = false;  // 是否发生合并
            for(int idx = 0; idx < checklist.Count; ++idx){
                if(checklist2[0].Count == 2 * checklist[idx].Count){    // 长度匹配
                    if(checklist2[0][0] == checklist[idx][0]){          // 顺首相同
                        // 合并成三顺，存储到combination
                        List<string> merged = checklist2[0];
                        checklist2.RemoveAt(0);
                        List<string> inList = checklist[idx];
                        checklist.RemoveAt(idx);
                        merged.AddRange(inList);
                        BaseDeckPattern.sortscore(merged);
                        combination.Add(merged);
                        flag = true;
                        break;
                    }
                }
            }
            if(!flag){
                List<string> inList = checklist2[0];
                checklist2.RemoveAt(0);
                if(division.ContainsKey(this.DP_JJQQKK.patternName())){
                    division[this.DP_JJQQKK.patternName()].Add(inList);
                }
                else
                {
                    division[this.DP_JJQQKK.patternName()] = new List<List<string>>{inList};
                }
            }
        }
        if(null != checklist && 0 != checklist.Count){      // 剩余未被合并的单顺
            division[this.DP_34567.patternName()] = checklist;
        }
        if(null != combination && 0 != combination.Count){
            if(division.ContainsKey(this.DP_QQQKKK.patternName())){
                division[this.DP_QQQKKK.patternName()].AddRange(combination);
            }
            else{
                division[this.DP_QQQKKK.patternName()] = new List<List<string>>(combination);
            }
        }
    }

    public BaseDeckPattern _cards2Pattern(List<string> cards){
        Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);
        foreach(var item in patterns){
            BaseDeckPattern pattern = item.Value;
            if(pattern.preciseMatch(cards, groups)){
                return pattern;
            }
        }
        return null;
    }
}

public class BaseDeckPattern
{
    public static Dictionary<string, int> groupby(List<string> deckCards)
    {
        Dictionary<string, int> ret = new Dictionary<string, int>();
        foreach(var card in deckCards)
        {
            if(ret.ContainsKey(card))
            {
                ret[card] = ret[card] + 1;
            }
            else
            {
                ret[card] = 1;
            }
        }
        return ret;
    }

    public static List<string> sortscore(List<string> deckCards)
    {
        deckCards.Sort(delegate(string x, string y){
            return CardTransfer.CARD_SCORE_MAP[x] - CardTransfer.CARD_SCORE_MAP[y];
        });
        return deckCards;
    }

    // 牌型名称
    public virtual string patternName(){return "";}

    public bool __eq__(BaseDeckPattern other)
    {
        return this.patternName() == other.patternName();
    }

    public bool __ne__(BaseDeckPattern other)
    {
        return this.patternName() != other.patternName();
    }

    public virtual Dictionary<string, object> patternScore(List<string> cards){return null;}

    // 贪婪匹配手牌中所有可匹配的牌
    public virtual List<List<string>> greedyMatch(List<string> deckCards, Dictionary<string, int> deckGroups){return null;}
    // 精确匹配一手牌
    public virtual bool preciseMatch(List<string> deckCards, Dictionary<string, int> deckGroups){return false;}
    // 列举出所有可能的跟牌
    public virtual List<List<string>> allFollowable(List<string> patternCards, List<string> deckCards){return null;}
}

// 基本牌型：对子、顺子、炸弹等
public class DeckPatternBasic: BaseDeckPattern
{

}

// 组合牌型：三带一、四代二、飞机等
public class DeckPatternComposite: BaseDeckPattern
{
    // 组合牌型对应的基本牌型
    public virtual string basicPatternName(){return "";}
    // 拆分出基本牌型对应的牌
    public virtual List<string> basicCards(List<string> cards){return null;}
}

// 三带一、三代二、四代二单、四代二对
public class DeckPattern_XXXYZ: DeckPatternComposite
{
    List<int> countList;
    int keyIdx;
    public DeckPattern_XXXYZ(List<int> countList, int keyIdx)
    {
        this.countList = countList;
        this.keyIdx = keyIdx;
    }

    public override Dictionary<string, object> patternScore(List<string> cards)
    {
        Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);
        foreach(string card in groups.Keys)
        {
            if(groups[card] >= countList[keyIdx])
            {
                return new Dictionary<string, object>(){{"value", CardTransfer.CARD_SCORE_MAP[card]}};
            }
        }
        return null;
    }

    private int SumList(List<int> list)
    {
        int sum = 0;
        for(int i = 0; i < list.Count; ++i)
        {
            sum += list[i];
        }
        return sum;
    }

    public override bool preciseMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        if(deckCards.Count != SumList(countList))
        {
            return false;
        }
        string major = null;
        List<int> tails = new List<int>();
        foreach(var item in deckGroups)
        {
            string card = item.Key;
            int count = item.Value;
            if(count == countList[keyIdx])
            {
                major = card;
            }
            else
            {
                tails.Add(count);
            }
        }
        if(null == major)
        {
            return false;
        }
        tails.Sort();
        tails.Reverse();
        for(int idx = 0; idx < countList.Count; ++idx)
        {
            if(idx == this.keyIdx){continue;}
            if(0 == tails.Count){return false;}
            tails[0] -= this.countList[idx];
            if(0 == tails[0]){
                tails.RemoveAt(0);
            }
            else if(tails[0] > 0){
            }
            else{
                return false;
            }
        }
        return true;
    }

    public override List<string> basicCards(List<string> cards)
    {
        Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);
        int stack = countList[keyIdx];
        foreach(string k in groups.Keys)
        {
            if(groups[k] >= stack)
            {
                List<string> keyList = new List<string>();
                for(int i = 0; i < stack; ++i)
                {
                    keyList.Add(k);
                }
                return keyList;
            }
        }
        return null;
    }
}

// 特殊牌型：王炸
public class DeckPatternSpecific: DeckPatternBasic
{
    List<string> specificCards;
    public DeckPatternSpecific(List<string> specificCards)
    {
        this.specificCards = specificCards;
    }
    public override List<List<string>> greedyMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        Dictionary<string, int> copied = CopyTool.CopyStrIntDic(deckGroups);
        foreach(string card in this.specificCards)
        {
            if(!deckGroups.ContainsKey(card)){
                return new List<List<string>>();
            }
            else{
                copied[card] -= 1;
            }
        }
        return new List<List<string>>(){this.specificCards};
    }

    private string TransListToString(List<string> list)
    {
        StringBuilder sb = new StringBuilder("[");
        for(int i = 0; i < list.Count; ++i){
            sb.Append(list[i]);
            if(i < list.Count - 1){
                sb.Append(", ");
            }
        }
        return sb.ToString();
    }
    public override bool preciseMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        BaseDeckPattern.sortscore(deckCards);
        return TransListToString(this.specificCards) == TransListToString(deckCards);
    }
}

// 单牌
public class DeckPattern_SINGLE: DeckPatternBasic
{
    public override string patternName()
    {
        return "3";
    }
    public override bool preciseMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        return 1 == deckCards.Count;
    }
    public override Dictionary<string, object> patternScore(List<string> cards)
    {
        return new Dictionary<string, object>{{"value", CardTransfer.CARD_SCORE_MAP[cards[0]]}};
    }
    public override List<List<string>> greedyMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        List<List<string>> ret = new List<List<string>>();
        foreach(string x in deckCards){
            ret.Add(new List<string>{x});
        }
        ret.Sort(delegate(List<string> left, List<string> right){
            // 左 - 右 是从小到大
            return CardTransfer.CARD_SCORE_MAP[left[0]] - CardTransfer.CARD_SCORE_MAP[right[0]];
        });
        return ret;
    }

    public override List<List<string>> allFollowable(List<string> patternCards, List<string> deckCards)
    {
        Debug.Assert(1 == patternCards.Count);
        List<List<string>> ret = new List<List<string>>();
        foreach(string c in deckCards){
            if(CardTransfer.CARD_SCORE_MAP[c] > CardTransfer.CARD_SCORE_MAP[patternCards[0]]){
                ret.Add(new List<string>{c});
            }
        }
        return ret;
    }
}

// 聚合牌型：对子、三张、炸弹
public class DeckPatternAggregate: DeckPatternBasic
{
    public int stackCount;
    public DeckPatternAggregate(int stackCount){
        this.stackCount = stackCount;
    }
    public override List<List<string>> greedyMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        List<List<string>> ret = new List<List<string>>();
        foreach(var item in deckGroups)
        {
            string card = item.Key;
            int count = item.Value;
            if(count >= stackCount){
                List<string> inList = new List<string>();
                for(int i = 0; i < stackCount; ++i){
                    inList.Add(card);
                }
                ret.Add(inList);
            }
        } 
        ret.Sort(delegate(List<string> left, List<string> right){
            // 左 - 右 是从小到大
            return CardTransfer.CARD_SCORE_MAP[left[0]] - CardTransfer.CARD_SCORE_MAP[right[0]];
        });
        return ret;
    }
    public override bool preciseMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        if(1 != deckGroups.Keys.Count){
            return false;
        }
        else{
            int dgValue = 0;
            foreach(var v in deckGroups.Values){
                dgValue = v;
                break;
            }
            if(dgValue != stackCount)
                return false;
            else
                return true;
        }
    }
    public override Dictionary<string, object> patternScore(List<string> cards)
    {
        return new Dictionary<string, object>{{"value", CardTransfer.CARD_SCORE_MAP[cards[0]]}};
    }
    public override List<List<string>> allFollowable(List<string> patternCards, List<string> deckCards)
    {
        List<List<string>> ret = new List<List<string>>();
        Dictionary<string, int> groups = BaseDeckPattern.groupby(deckCards);
        foreach(var item in groups){
            string k = item.Key;
            int count = item.Value;
            if(count >= stackCount && CardTransfer.CARD_SCORE_MAP[k] > CardTransfer.CARD_SCORE_MAP[patternCards[0]]){
                List<string> strList = new List<string>();
                for(int i = 0; i < stackCount; ++i){
                    strList.Add(k);
                }
                ret.Add(strList);
            }
        }
        return ret;
    }
}

// 顺子牌型：单顺、双顺、三顺
public class DeckPatternStreak: DeckPatternBasic
{
    List<string> streakable;
    int stackCount;
    int minStreak;
    public DeckPatternStreak(List<string> streakable, int stackCount, int minStreak)
    {
        this.streakable = CopyTool.CopyStrList(streakable);
        this.streakable.Add("#sentinel#");
        this.stackCount = stackCount;
        this.minStreak = minStreak;
    }
    public override List<List<string>> greedyMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        Dictionary<string, List<List<string>>> buffer = new Dictionary<string, List<List<string>>>();
        _recursiveMatch(deckCards, deckGroups, buffer);
        return buffer[StringTool.TransStrIntDicToStr(deckGroups)];
        // string key = StringTool.TransStrListToStrWithoutSymbol(deckCards);
        // return buffer[key];
    }
    private bool StrListHasElem(List<string> strList, string str){
        foreach(string s in strList){
            if(str == s){
                return true;
            }
        }
        return false;
    }
    private int GetIndex(List<string> strList, string value){
        for(int i = 0; i < strList.Count; ++i){
            if(strList[i] == value){
                return i;
            }
        }
        return -1;
    }
    public override bool preciseMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        if(deckGroups.Keys.Count < minStreak){
            return false;
        }
        List<string> streakSet = CopyTool.TransStrListToSet(streakable);
        foreach(var item in deckGroups){
            string card = item.Key;
            int count = item.Value;
            if(!StrListHasElem(streakSet, card)){
                return false;
            }
            if(count != stackCount){
                return false;
            }
        } 
        List<int> idxList = new List<int>();
        foreach(string key in deckGroups.Keys){
            idxList.Add(GetIndex(streakable, key));
        }
        idxList.Sort();
        if(idxList[idxList.Count - 1] - idxList[0] != idxList.Count - 1){
            return false;
        }
        return true;
    }
    public override Dictionary<string, object> patternScore(List<string> cards)
    {
        BaseDeckPattern.sortscore(cards);
        return new Dictionary<string, object>{
            {"condition", cards.Count / stackCount},
            {"value", CardTransfer.CARD_SCORE_MAP[cards[0]]}
        };
    }
    public override List<List<string>> allFollowable(List<string> patternCards, List<string> deckCards)
    {
        List<List<string>> ret = new List<List<string>>();
        int streaklen = patternCards.Count / stackCount;
        Dictionary<string, int> groups = BaseDeckPattern.groupby(deckCards);

        int minIdx = GetIndex(streakable, patternCards[0]) + 1;
        Debug.Assert(minIdx > 0);
        int maxIdx = streakable.Count - streaklen;
        if(maxIdx < minIdx){
            return ret;
        }
        for(int idx = minIdx; idx < maxIdx + 1; ++idx){
            bool followable = true;
            for(int i = 0; i < streaklen; ++i){
                if(!groups.ContainsKey(streakable[idx + i])){
                    followable = false;
                    break;
                }
                else if(groups[streakable[idx + i]] < stackCount){
                    followable = false;
                    break;
                }
            }
            if(followable){
                ret.Add(new List<string>());
                for(int i = 0; i < streaklen; ++i){
                    for(int j = 0; j < stackCount; ++j){
                        ret[ret.Count - 1].Add(streakable[idx + i]);
                    }
                }
            }
        } 
        return ret;
    }
    public List<string> seekSmaller(Dictionary<string, int> deckGroups, Dictionary<string, int> score)
    {
        List<int> toCheck = new List<int>();
        for(int idx = 0; idx < streakable.Count - 1; ++idx){
            if(CardTransfer.CARD_SCORE_MAP[streakable[idx]] < score["value"]){
                toCheck.Add(idx);
            }
        }
        foreach(int idx in toCheck){
            if(!deckGroups.ContainsKey(streakable[idx])){
                continue;
            }
            if(deckGroups[streakable[idx]] < stackCount){
                continue;
            }
            bool found = true;
            for(int offset = 1; offset < score["condition"]; ++offset){
                if(!deckGroups.ContainsKey(streakable[idx + offset])){
                    found = false;
                    break;
                }
                if(deckGroups[streakable[idx + offset]] < stackCount){
                    found = false;
                    break;
                }
            }
            if(found){
                List<string> ret = new List<string>();
                for(int offset = 0; offset < score["condition"]; ++offset){
                    List<string> inList = new List<string>();
                    for(int i = 0; i < stackCount; ++i){
                        inList.Add(streakable[idx + offset]);
                    }
                    ret.AddRange(inList);
                }
                return ret;
            }
        }
        return null;
    }
    public static int _divisionComp(List<List<string>> d1, List<List<string>> d2)
    {
        // TODO 确认是否是这个意思
        int v1 = 0, v2 = 0;
        foreach(var d in d1){
            v1 += d.Count;
        }
        foreach(var d in d2){
            v2 += d.Count;
        }
        if(v1 == v2){
            return d1.Count - d2.Count;
        }
        else if(v1 > v2){
            return -1;
        }
        else{
            return 1;
        }
    }
    // 动态规划处最优拆分
    public void _recursiveMatch(List<string> deckCards, Dictionary<string, int> groups, Dictionary<string, List<List<string>>> buffer)
    {
        string key = StringTool.TransStrIntDicToStr(groups);
        // string key = StringTool.TransStrListToStrWithoutSymbol(deckCards);
        List<List<string>> streaks = _allStreaks(groups);
        if(0 == streaks.Count){
            buffer[key] = new List<List<string>>();
        }
        else{
            List<List<string>> possibles = new List<List<string>>();
            foreach(List<string> s in streaks){
                possibles.AddRange(_subStreaks(s));
            }
            List<List<List<string>>> divisionList = new List<List<List<string>>>();
            foreach(List<string> s in possibles){
                Dictionary<string, int> copyedGroup = CopyTool.CopyStrIntDic(groups);
                List<string> copyedDeckCards = new List<string>(deckCards);
                foreach(string card in s){
                    copyedGroup[card] -= stackCount;
                    if(0 == copyedGroup[card]){
                        copyedGroup.Remove(card);
                        copyedDeckCards.Remove(card);
                    }
                }
                string subKey = StringTool.TransStrIntDicToStr(copyedGroup);
                // string subKey = StringTool.TransStrListToStrWithoutSymbol(copyedDeckCards);
                if(!buffer.ContainsKey(subKey)){
                    _recursiveMatch(copyedDeckCards, copyedGroup, buffer);
                }
                List<List<string>> division = CopyTool.CopyStrListList(buffer[subKey]);
                division.Add(_inflateStreak(s, stackCount));
                divisionList.Add(division);
            }
            if(0 == divisionList.Count){
                buffer[key] = new List<List<string>>();
            }
            else{
                divisionList.Sort(delegate(List<List<string>> left, List<List<string>> right){
                    return DeckPatternStreak._divisionComp(left, right);
                });
                buffer[key] = divisionList[0];
            }
        }
    }

    public List<List<string>> _subStreaks(List<string> streak)
    {
        List<List<string>> ret = new List<List<string>>();
        for(int i = minStreak; i < streak.Count + 1; ++i){
            for(int j = 0; j < streak.Count - i + 1; ++j){
                List<string> inList = new List<string>();
                for(int k = j; k < j + i; ++k){
                    inList.Add(streak[k]);
                }
                ret.Add(inList);
            }
        }
        return ret;
    }
    public List<List<string>> _allStreaks(Dictionary<string, int> groups){
        List<List<string>> ret = new List<List<string>>();
        int startIdx = -1;
        int streak = 0;
        for(int idx = 0; idx < streakable.Count; ++idx){
            string card = streakable[idx];
            if(groups.ContainsKey(card) && groups[card] >= stackCount){
                if(-1 == startIdx){
                    startIdx = idx;
                }
                streak += 1;
            }
            else{
                if(streak >= minStreak){
                    List<string> inList = new List<string>();
                    for(int x = startIdx; x < idx; ++x){
                        inList.Add(streakable[x]);
                    }
                    ret.Add(inList);
                }
                startIdx = -1;
                streak = 0;
            }
        }
        return ret;
    }
    public List<string> _inflateStreak(List<string> streak, int times){
        List<string> ret = new List<string>();
        foreach(string c in streak){
            List<string> inList = new List<string>();
            for(int j = 0; j < times; ++j){
                inList.Add(c);
            }
            ret.AddRange(inList);
        }
        return ret;
    }
}

// 特殊牌型：王炸
public class DeckPattern_joker_JOKER: DeckPatternSpecific
{
    public DeckPattern_joker_JOKER(): base(new List<string>{"joker", "JOKER"})
    {

    }
    public override string patternName(){
        return "joker_JOKER";
    }
    public override Dictionary<string, object> patternScore(List<string> cards){
        return new Dictionary<string, object>{{"value", CardTransfer.CARD_SCORE_MAP["JOKER"]}};
    }
}

// 基本牌型：炸弹
public class DeckPattern_AAAA: DeckPatternAggregate
{
    public DeckPattern_AAAA(): base(4){}
    public override string patternName(){return "AAAA";}
}

// 基本牌型：三张
public class DeckPattern_KKK: DeckPatternAggregate
{
    public DeckPattern_KKK(): base(3){}
    public override string patternName(){return "KKK";}
}

// 基本牌型：对子
public class DeckPattern_QQ: DeckPatternAggregate
{
    public DeckPattern_QQ(): base(2){}
    public override string patternName(){return "QQ";}
}

// 基本牌型：单顺
public class DeckPattern_34567: DeckPatternStreak
{
    public DeckPattern_34567(): base(CardTransfer.STREAKABLE, 1, 5){}
    public override string patternName(){return "34567";}
}
// 基本牌型：双顺
public class DeckPattern_JJQQKK: DeckPatternStreak
{
    public DeckPattern_JJQQKK(): base(CardTransfer.STREAKABLE, 2, 3){}
    public override string patternName(){return "JJQQKK";}
}
// 基本牌型：三顺
public class DeckPattern_QQQKKK: DeckPatternStreak
{
    public DeckPattern_QQQKKK(): base(CardTransfer.STREAKABLE, 3, 2){}
    public override string patternName(){return "QQQKKK";}
}
// 组合牌型：三带一
public class DeckPattern_KKKQ: DeckPattern_XXXYZ
{
    public DeckPattern_KKKQ(): base(new List<int>{3, 1}, 0){}
    public override string patternName(){return "KKKQ";}
    public override string basicPatternName(){return "KKK";}
}
// 组合牌型：三带一对
public class DeckPattern_KKKJJ: DeckPattern_XXXYZ
{
    public DeckPattern_KKKJJ(): base(new List<int>{3, 2}, 0){}
    public override string patternName(){return "KKKJJ";}
    public override string basicPatternName(){return "KKK";}
}
// 组合牌型：四带二单
public class DeckPattern_AAAA45: DeckPattern_XXXYZ
{
    public DeckPattern_AAAA45(): base(new List<int>{4, 1, 1}, 0){}
    public override string patternName(){return "AAAA45";}
    public override string basicPatternName(){return "AAAA";}
}
// 组合牌型：四带二对
public class DeckPattern_AAAA4455: DeckPattern_XXXYZ
{
    public DeckPattern_AAAA4455(): base(new List<int>{4, 2, 2}, 0){}
    public override string patternName(){return "AAAA4455";}
    public override string basicPatternName(){return "AAAA";}
}
// 组合牌型：飞机带单
public class DeckPattern_QQQKKK45: DeckPatternComposite
{
    List<string> streakable = new List<string>{"3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"};
    public override string patternName(){return "QQQKKK45";}
    public override string basicPatternName(){return "QQQKKK";}
    public override Dictionary<string, object> patternScore(List<string> cards)
    {
        int length = cards.Count / 4;
        Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);
        List<string> air = new List<string>();
        foreach(var item in groups){
            string card = item.Key;
            int count = item.Value;
            if(count >= 3){
                air.Add(card);
            }
        }
        int maxScore = -1;
        List<string> streak = null;
        List<string[]> permutationList = PermutationAndCombination<string>.GetCombination(air.ToArray(), length);
        foreach(string[] _possibility in permutationList){
            List<string> possibility = new List<string>(_possibility);
            List<int> idxList = new List<int>();
            foreach(var x in possibility){
                idxList.Add(GetIndex(streakable, x));
            }
            idxList.Sort();
            if(idxList[idxList.Count - 1] - idxList[0] == idxList.Count - 1){
                BaseDeckPattern.sortscore(possibility);
                if(-1 == maxScore){
                    maxScore = CardTransfer.CARD_SCORE_MAP[possibility[0]];
                    streak = possibility;
                }
                else if(maxScore < CardTransfer.CARD_SCORE_MAP[possibility[0]]){
                    maxScore = CardTransfer.CARD_SCORE_MAP[possibility[0]];
                    streak = possibility;
                }
            }
        }
        Debug.Assert(-1 != maxScore);
        return new Dictionary<string, object>{
            {"streak", streak},
            {"condition", length},
            {"value", maxScore}
        };
    }
    private int GetIndex(List<string> strList, string value){
        for(int i = 0; i < strList.Count; ++i){
            if(strList[i] == value){
                return i;
            }
        }
        return -1;
    }
    public override List<string> basicCards(List<string> cards)
    {
        Dictionary<string, object> score = patternScore(cards);
        List<string> ret = new List<string>();
        foreach(var c in (List<string>)score["streak"]){
            ret.AddRange(new List<string>{c, c, c});
        }
        return ret;
    }
    public override bool preciseMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        if(deckCards.Count < 8){
            return false;
        }
        if(deckCards.Count % 4 != 0){
            return false;
        }
        int length = deckCards.Count / 4;
        List<string> air = new List<string>();
        foreach(var item in deckGroups){
            string card = item.Key;
            int count = item.Value;
            if(1 == count){}
            else if(2 == count){}
            else if(streakable.Contains(card)){
                air.Add(card);
            }
        }
        if(air.Count < length){
            return false;
        }
        List<string[]> permutationList = PermutationAndCombination<string>.GetCombination(air.ToArray(), length);
        foreach(var possibility in permutationList){
            List<int> idxList = new List<int>();
            foreach(var x in possibility){
                idxList.Add(GetIndex(streakable, x));
            }
            idxList.Sort();
            if(idxList[idxList.Count - 1] - idxList[0] == idxList.Count - 1){
                return true;
            }
        }
        return false;
    }
}

// 组合牌型：飞机带对
public class DeckPattern_QQQKKK6677: DeckPatternComposite
{
    List<string> streakable = new List<string>{"3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A"};
    public override string patternName(){return "QQQKKK6677";}
    public override string basicPatternName(){return "QQQKKK";}
    private int GetIndex(List<string> strList, string value){
        for(int i = 0; i < strList.Count; ++i){
            if(strList[i] == value){
                return i;
            }
        }
        return -1;
    }
    public override Dictionary<string, object> patternScore(List<string> cards)
    {
        int length = cards.Count / 5;
        Dictionary<string, int> groups = BaseDeckPattern.groupby(cards);
        List<string> air = new List<string>();
        foreach(var item in groups){
            string card = item.Key;
            int count = item.Value;
            if(count >= 3){
                air.Add(card);
            }
        }
        int maxScore = -1;
        List<string> streak = null;
        List<string[]> permutationList = PermutationAndCombination<string>.GetCombination(air.ToArray(), length);
        foreach(string[] _possibility in permutationList){
            List<string> possibility = new List<string>(_possibility);
            List<int> idxList = new List<int>();
            foreach(var x in possibility){
                idxList.Add(GetIndex(streakable, x));
            }
            idxList.Sort();
            if(idxList[idxList.Count - 1] - idxList[0] == idxList.Count - 1){
                BaseDeckPattern.sortscore(possibility);
                if(-1 == maxScore){
                    maxScore = CardTransfer.CARD_SCORE_MAP[possibility[0]];
                    streak = possibility;
                }
                else if(maxScore < CardTransfer.CARD_SCORE_MAP[possibility[0]]){
                    maxScore = CardTransfer.CARD_SCORE_MAP[possibility[0]];
                    streak = possibility;
                }
            }
        }
        Debug.Assert(-1 != maxScore);
        return new Dictionary<string, object>{
            {"streak", streak},
            {"condition", length},
            {"value", maxScore}
        };
    }
    public override List<string> basicCards(List<string> cards)
    {
        Dictionary<string, object> score = patternScore(cards);
        List<string> ret = new List<string>();
        foreach(var c in (List<string>)score["streak"]){
            ret.AddRange(new List<string>{c, c, c});
        }
        return ret;
    }
    public override bool preciseMatch(List<string> deckCards, Dictionary<string, int> deckGroups)
    {
        if(deckCards.Count < 10){
            return false;
        }
        if(deckCards.Count % 5 != 0){
            return false;
        }
        int length = deckCards.Count / 5;
        List<string> air = new List<string>();
        foreach(var item in deckGroups){
            string card = item.Key;
            int count = item.Value;
            if(1 == count){return false;}
            else if(2 == count){}
            else if(3 == count){
                if(streakable.Contains(card)){
                    air.Add(card);
                }
                else{
                    return false;
                }
            }
            else if(count % 2 == 1){
                if(streakable.Contains(card)){
                    air.Add(card);
                }
                else{
                    return false;
                }
            }
        }
        if(air.Count < length){
            return false;
        }
        List<string[]> permutationList = PermutationAndCombination<string>.GetCombination(air.ToArray(), length);
        foreach(var possibility in permutationList){
            List<int> idxList = new List<int>();
            foreach(var x in possibility){
                idxList.Add(GetIndex(streakable, x));
            }
            idxList.Sort();
            if(idxList[idxList.Count - 1] - idxList[0] == idxList.Count - 1){
                return true;
            }
        }
        return false;
    }
}