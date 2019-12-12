using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Tuyoo
{

    public class CardInfo
    {
        public int serverNum;//服务器牌值 >53是癞子 54 - 66：A,2,3,4,5,6,7,8,9,10,J,Q,K
        public int color;//花色：43210 对应 joker黑红梅方 * 癞子的星星花色
        public int num;// 1-13 对应 3-K A 2 joker52 53
        /* UI位图展现规则:
         *543210 对应大王小王黑红梅方
         *A-Z 对应红色A-K黑色A-K
         *a-m 癞子A-K
         */
        public string showCard;
        public string showDiCard;// 大小王底牌特殊位图 6 7
        public bool laizi;// 癞子

        //癞子确认value
        public void LaiZiValueChanged(int _num)
        {
            if (_num < 12)
                serverNum = 53 + _num + 2;
            else
                serverNum = 53 + _num - 11;
        }
    }

    public enum CardType : ushort
    {
        ZERO                    = 0,//不出
        SINGLE_CARD             = 1,//单牌
        DOUBLE_CARD             = 2,//对子
        THREE_CARD              = 3,//三张
        BOMB_CARD               = 4,//炸弹
        THREE_WITH_ONE_SINGLE   = 5,//三带一单张
        THREE_WITH_ONE_PAIR     = 6,//三带一对
        FOUR_WITH_TWO_SINGLE    = 7,//四带二单张
        FOUR_WITH_TWO_PAIR      = 8,//四带两对
        SHUNZI_CARD             = 9,//顺子
        LIANDUI_CARD            = 10,//连对（双顺）
        AIRCRAFT_CARD           = 11,//飞机（三顺）
        AIRCRAFT_SINGLE_CARD    = 12,//飞机带单牌
        AIRCRAFT_DOUBLE_CARD    = 13,//飞机带对子
        ROCKET                  = 14,//火箭
        NULL                    = 100,//未定
    }

    /// <summary>
    /// 拆牌
    /// </summary>
    public class DividedCard
    {
        public int count;
        public int num;
        public List<CardInfo> cards;

        public DividedCard(int _num, CardInfo _card)
        {
            count = 1;
            num = _num;
            cards = new List<CardInfo> { _card };
        }

        public DividedCard(int _num)
        {
            count = 0;
            num = _num;
            cards = new List<CardInfo>();
        }

        public void Add(CardInfo _card)
        {
            count += 1;
            cards.Add(_card);
        }

        public DividedCard()
        { 
        }
    }

    public static class CardUtil
    {
        //简单类型对象深拷贝
        public static T DeepCopy<T>(T obj)
        {
            object result = Activator.CreateInstance(obj.GetType());
            foreach (FieldInfo field in obj.GetType().GetFields())
            {
                if (field.FieldType.GetInterface("IList", false) == null)
                {
                    field.SetValue(result, field.GetValue(obj));
                }
                else
                {
                    IList listObject = (IList)field.GetValue(result);
                    if (listObject != null)
                    {
                        foreach (object item in ((IList)field.GetValue(obj)))
                        {
                            listObject.Add(DeepCopy(item));
                        }
                    }
                }
            }
            return (T)result;
        }

        #region card消息回调后的牌型判断
        //包含转换后癞子牌型判断
        public static CardType CheckCardType(List<CardInfo> li, out int lv)
        {
            CardType ct = CardType.NULL;
            lv = 0;
            switch (li.Count)
            {
                case 0:
                    ct = CardType.ZERO;
                    break;
                case 1:
                    ct = CardType.SINGLE_CARD;
                    lv = li[0].num < 51 ? li[0].num - 1 : li[0].num - 39;
                    break;
                case 2:
                    Is2(li, ref ct, ref lv);
                    IsRocket(li, ref ct, ref lv);
                    break;
                case 3:
                    Is3(li, ref ct, ref lv);
                    break;
                case 4:
                    Is31(li, ref ct, ref lv);
                    IsBomb(li, ref ct, ref lv);
                    break;
                case 5:
                    Is32(li, ref ct, ref lv);
                    break;
                case 6:
                    Is411(li, ref ct, ref lv);
                    break;
                case 8:
                    Is422(li, ref ct, ref lv);
                    break;
                default:
                    ct = CardType.NULL;
                    break;
            }

            if(ct == CardType.NULL && li.Count > 4)
            {
                //如果上面都不是 判断顺子连对最后判断飞机及翅膀
                IsShunZi(li, ref ct, ref lv);
                IsLianDui(li, ref ct, ref lv);
                IsAircraft(li, ref ct, ref lv);
                IsAircraftWithSingle(li, ref ct, ref lv);
                IsAircraftWithPair(li, ref ct, ref lv);
            }
            return ct;
        }

        static void Is2(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            if(li[0].num == li[1].num)
            {
                ct = CardType.DOUBLE_CARD;
                lv = li[0].num - 1;
            }
        }

        static void IsRocket(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            if (li[0].num == 53 && li[1].num == 52)
            {
                ct = CardType.ROCKET;
                lv = 999;
            }
        }

        static void Is3(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            if (li[0].num == li[1].num && li[1].num == li[2].num)
            {
                ct = CardType.THREE_CARD;
                lv = li[0].num - 1;
            }
        }

        static void Is31(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            if (li[0].num == li[1].num && li[1].num == li[2].num && li[0].num != li[3].num)
            {
                ct = CardType.THREE_WITH_ONE_SINGLE;
                lv = li[0].num - 1;
            }
        }

        static void IsBomb(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            if (li[0].num == li[3].num && li[1].num == li[2].num && li[2].num == li[3].num)
            {
                ct = CardType.BOMB_CARD;
                lv = 100 + li[0].num - 1;
            }
        }

        static void IsShunZi(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            bool success = true;
            int init = li[0].num;
            for (int i = 0; i < li.Count; i++)
            {
                if(li[i].num != init - i)
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                ct = CardType.SHUNZI_CARD;
                lv = li[0].num - 1;
            }
        }

        static void IsLianDui(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            bool success = true;
            int init = li[0].num;
            if (li.Count % 2 != 0) success = false;
            //奇偶相等
            for (int j = 0; j < li.Count-1; j+=2)
            {
                if(li[j].num != li[j+1].num)
                {
                    success = false;
                    break;
                }
            }
            //步减
            for (int i = 0; i < li.Count-1; i+=2)
            {
                if (li[i].num != init - i/2)
                {
                    success = false;
                    break;
                }
            }

            if (success)
            {
                ct = CardType.LIANDUI_CARD;
                lv = li[0].num - 1;
            }
        }

        static void Is32(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            if (li[0].num == li[1].num && li[1].num == li[2].num && li[3].num == li[4].num && li[0].num != li[3].num)
            {
                ct = CardType.THREE_WITH_ONE_PAIR;
                lv = li[0].num - 1;
            }
        }

        static void Is411(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            //牌型校正后只需要简略判断了前四位是不是炸弹
            if (li[0].num == li[3].num && li[1].num == li[2].num && li[2].num == li[3].num)
            {
                ct = CardType.FOUR_WITH_TWO_SINGLE;
                lv = li[0].num - 1;
            }
        }

        static void Is422(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            //牌型校正后只需要简略判断了前四位是不是炸弹
            if (li[0].num == li[3].num && li[1].num == li[2].num && li[2].num == li[3].num)
            {
                ct = CardType.FOUR_WITH_TWO_PAIR;
                lv = li[0].num - 1;
            }
        }

        static void IsAircraft(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            //简便判断了
            bool success = li.Count % 3 == 0;
            if (li[li.Count - 1].num != li[li.Count - 2].num || li[li.Count - 1].num != li[li.Count - 3].num)
            {
                success = false;
            }

            if (success)
            {
                ct = CardType.AIRCRAFT_CARD;
                lv = li[0].num - 1;
            }
        }

        static void IsAircraftWithSingle(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            //简便判断了
            bool success = true && li.Count % 4 == 0;
            if (li[0].num != li[1].num || li[0].num != li[2].num)
            {
                success = false;
            }

            if (success)
            {
                ct = CardType.AIRCRAFT_SINGLE_CARD;
                lv = li[0].num - 1;
            }
        }

        static void IsAircraftWithPair(List<CardInfo> li, ref CardType ct, ref int lv)
        {
            //简便判断了
            bool success = true && li.Count % 5 == 0;
            if (li[0].num != li[1].num || li[0].num != li[2].num || li[li.Count - 1].num != li[li.Count - 2].num)
            {
                success = false;
            }

            if (success)
            {
                ct = CardType.AIRCRAFT_DOUBLE_CARD;
                lv = li[0].num - 1;
            }
        }
        #endregion

        #region 主动出牌合法检查及癞子转换
        /// <summary>
        /// 主动出牌是否合法
        /// </summary>
        /// <returns>The valid card type.</returns>
        /// <param name="selectCards">选中的牌</param>
        /// <param name="isLzQuick">If set to <c>true</c> is lz quick.</param>
        /// <param name="result">If set to <c>true</c> result.</param>
        public static List<CardInfo> IsValidCardType(List<CardInfo> selectCards, bool isLzQuick, out bool result)
        {
            //暂时就是癞子快速和经典
            List<CardInfo> final_list = new List<CardInfo>();
            result = false;

            List<CardInfo> noLaiziArray = new List<CardInfo>();
            noLaiziArray.AddRange(selectCards);

            int lz_count = 0;
            //先拿出癞子
            List<CardInfo> laiziArray = new List<CardInfo>();
            for (int i = 0; i < selectCards.Count; i++)
            {
                if (selectCards[i].laizi)
                {
                    laiziArray.Add(selectCards[i]);
                    lz_count++;
                }
                else
                    break;
            }
            noLaiziArray.RemoveRange(0, lz_count);

            //拆牌
            /*cardArray
             * [{牌权值13,数量4,具体牌对象List<cardInfo>}, {9,4,List<cardInfo>}... ]           
             */
            List<DividedCard> cardsArray = new List<DividedCard>();
            DividedCard dc = null;
            for (int i = 0; i < noLaiziArray.Count; i++)
            {
                if (dc == null)
                {
                    dc = new DividedCard(noLaiziArray[i].num, noLaiziArray[i]);
                }
                else
                {
                    if (dc.num == noLaiziArray[i].num)
                    {
                        dc.Add(noLaiziArray[i]);
                    }
                    else
                    {
                        cardsArray.Add(dc);
                        dc = new DividedCard(noLaiziArray[i].num, noLaiziArray[i]);
                    }
                }
                //最后一张牌
                if (i == noLaiziArray.Count - 1)
                    cardsArray.Add(dc);
            }

            List<DividedCard> dicArray = FillDicarrWithAll(cardsArray);

            switch (selectCards.Count)
            {
                case 0:
                    final_list = selectCards;
                    result = false;
                    break;
                case 1:
                    IsVilid1(ref final_list, ref result, selectCards);
                    break;
                case 2:
                    IsVilidRocket(ref final_list, ref result, selectCards);
                    if(!result) 
                        IsVilid2(ref final_list, ref result, laiziArray, cardsArray);
                    break;
                case 3:
                    IsVilid3(ref final_list, ref result, laiziArray, cardsArray);
                    break;
                case 4:
                    IsVilidBomb(ref final_list, ref result, laiziArray, cardsArray);
                    if (!result) 
                        IsVilid31(ref final_list, ref result, laiziArray, cardsArray);
                    break;
                case 5:
                    IsVilid32(ref final_list, ref result, laiziArray, cardsArray);
                    break;
                case 6:
                    if (!isLzQuick) 
                        IsVilid411(ref final_list, ref result, laiziArray, cardsArray);
                    break;
                case 8:
                    if (!isLzQuick)
                        IsVilid422(ref final_list, ref result, laiziArray, cardsArray);
                    if (!result)
                        IsVilid3311(ref final_list, ref result, laiziArray, dicArray);
                    break;
                case 10:
                    IsVilid3322(ref final_list, ref result, laiziArray, dicArray);
                    break;
                case 12:
                    IsVilid333111(ref final_list, ref result, laiziArray, dicArray);
                    break;
                case 15:
                    IsVilid333222(ref final_list, ref result, laiziArray, dicArray);
                    break;
                case 16:
                    IsVilid33331111(ref final_list, ref result, laiziArray, dicArray);
                    break;
                case 20:
                    IsVilid33332222(ref final_list, ref result, laiziArray, dicArray);
                    if(!result)
                        IsVilid3333311111(ref final_list, ref result, laiziArray, dicArray);
                    break;
            }

            if(selectCards.Count > 4 && !result)
            {
                //快速癞子最大长度为9
                if ((isLzQuick && selectCards.Count <= 9) || !isLzQuick)
                    IsVilidContinuous(ref final_list, ref result, laiziArray, dicArray, 1, selectCards.Count);//顺子
                if (!result)
                    IsVilidContinuous(ref final_list, ref result, laiziArray, dicArray, 2, selectCards.Count);//连对
                if (!result)
                    IsVilidContinuous(ref final_list, ref result, laiziArray, dicArray, 3, selectCards.Count);//飞机
            }

            if (!result)
                final_list.AddRange(selectCards);
            return final_list;
        }

        static void IsVilid1(ref List<CardInfo> final_list, ref bool result, List<CardInfo> selectCards)
        {
            if (selectCards[0].laizi)
                selectCards[0].LaiZiValueChanged(selectCards[0].num);
            final_list.AddRange(selectCards);
            result = true;
        }

        static void IsVilidRocket(ref List<CardInfo> final_list, ref bool result, List<CardInfo> selectCards)
        {
            if (selectCards[0].num == 53 && selectCards[1].num == 52)
            {
                result = true;
                final_list.AddRange(selectCards);
            }
        }

        static void IsVilid2(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> cardsArray)
        {
            //3种有效情况
            if (laiziArray.Count == 2)
            {
                laiziArray[0].LaiZiValueChanged(laiziArray[0].num);
                laiziArray[1].LaiZiValueChanged(laiziArray[0].num);
                result = true;
                final_list.AddRange(laiziArray);
            }
            else if (laiziArray.Count == 1 && cardsArray.Count == 1)
            {
                result = true;
                final_list.AddRange(cardsArray[0].cards);
                laiziArray[0].LaiZiValueChanged(cardsArray[0].num);
                final_list.Add(laiziArray[0]);
            }
            else if (laiziArray.Count == 0 && cardsArray.Count == 1)
            {
                result = true;
                final_list.AddRange(cardsArray[0].cards);
            }
            else
            {
                result = false;
            }
        }

        static void IsVilid3(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> cardsArray)
        {
            //4种有效情况
            if (laiziArray.Count == 3)
            {
                laiziArray[0].LaiZiValueChanged(laiziArray[0].num);
                laiziArray[1].LaiZiValueChanged(laiziArray[0].num);
                laiziArray[2].LaiZiValueChanged(laiziArray[0].num);
                result = true;
                final_list.AddRange(laiziArray);
            }
            else if (laiziArray.Count == 0 && cardsArray.Count == 1)
            {
                result = true;
                final_list.AddRange(cardsArray[0].cards);
            }
            else if (laiziArray.Count == 1 && cardsArray.Count == 1)
            {
                result = true;
                final_list.AddRange(cardsArray[0].cards);
                laiziArray[0].LaiZiValueChanged(cardsArray[0].num);
                final_list.Add(laiziArray[0]);
            }
            else if (laiziArray.Count == 2 && cardsArray.Count == 1)
            {
                result = true;
                final_list.AddRange(cardsArray[0].cards);
                laiziArray[0].LaiZiValueChanged(cardsArray[0].num);
                laiziArray[1].LaiZiValueChanged(cardsArray[0].num);
                final_list.AddRange(laiziArray);
            }
            else
            {
                result = false;
            }
        }

        static void IsVilidBomb(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> cardsArray)
        {
            if (laiziArray.Count == 4)//chun癞子炸弹
            {
                for (int i = 0; i < laiziArray.Count; i++)
                {
                    laiziArray[i].LaiZiValueChanged(laiziArray[i].num);
                }
                result = true;
                final_list.AddRange(laiziArray);
            }
            else if(cardsArray.Count == 1 && laiziArray.Count + cardsArray[0].count == 4) //软炸弹或实炸弹
            {
                for (int i = 0; i < laiziArray.Count; i++)
                {
                    laiziArray[i].LaiZiValueChanged(cardsArray[0].num);
                }
                result = true;
                final_list.AddRange(cardsArray[0].cards);
                final_list.AddRange(laiziArray);
            }
            else
            {
                result = false;
            }
        }

        static void IsVilid31(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> cardsArray)
        {
            //3癞默认软炸弹 3334 / 33L4 / 4LL3 三种形式
            if (laiziArray.Count == 0)
            {
                if (cardsArray[0].count == 3)
                {
                    result = true;
                    final_list.AddRange(cardsArray[0].cards);
                    final_list.AddRange(cardsArray[1].cards);
                }
                else
                {
                    result = true;
                    final_list.AddRange(cardsArray[1].cards);
                    final_list.AddRange(cardsArray[0].cards);
                }
            }
            else if (laiziArray.Count == 1)
            {
                if (cardsArray[0].count == 2)
                {
                    result = true;
                    final_list.AddRange(cardsArray[0].cards);
                    laiziArray[0].LaiZiValueChanged(cardsArray[0].num);
                    final_list.AddRange(laiziArray);
                    final_list.AddRange(cardsArray[1].cards);
                }
                else
                {
                    result = true;
                    final_list.AddRange(cardsArray[1].cards);
                    laiziArray[0].LaiZiValueChanged(cardsArray[1].num);
                    final_list.AddRange(laiziArray);
                    final_list.AddRange(cardsArray[0].cards);
                }
            }
            else if (laiziArray.Count == 2)
            {
                result = true;
                final_list.AddRange(cardsArray[0].cards);
                laiziArray[0].LaiZiValueChanged(cardsArray[0].num);
                laiziArray[1].LaiZiValueChanged(cardsArray[0].num);
                final_list.AddRange(laiziArray);
                final_list.AddRange(cardsArray[1].cards);
            }
            else
            {
                result = false;
            }
        }

        static List<DividedCard> GetSubArrayExceptIndex(List<DividedCard> arr, List<int> idxs)
        {
            List<DividedCard> newarr = new List<DividedCard>();
            newarr.AddRange(arr);
            //idxs.Sort();//idx从小到大 然后从大的删除不影响角标
            for (int i = idxs.Count - 1; i >= 0; i--)
            {
                newarr.RemoveAt(idxs[i]);
            }
            return newarr;
        }

        //检查能否凑出指定数量对子
        static List<CardInfo> CheckDoubles(List<DividedCard> arr, int count, List<CardInfo> lz)
        {
            //上来就审查数量对不对
            int c = 0;
            for (int i = 0; i < arr.Count; i++)
            {
                c += arr[i].count;
            }
            if (count * 2 != c + lz.Count)
            { return new List<CardInfo>(); }
            int lz_need = 0;
            List<CardInfo> cards = new List<CardInfo>();
            for (int index = 0; index < arr.Count; index++)
            {
                if (arr[index].count == 0)
                { continue; }

                if (arr[index].num > 13 && arr[index].count > 0)
                { return new List<CardInfo>(); }//无法用王组对子了

                int need = 2 - arr[index].count;
                if (need < 0)
                {
                    need = need == -2 ? 0 : 1;
                }
                lz_need += need;

                if (lz_need <= lz.Count)
                {
                    cards.AddRange(arr[index].cards);
                    if(need > 0)
                    {
                        lz[lz.Count - 1].LaiZiValueChanged(arr[index].num);
                        cards.Add(lz[lz.Count - 1]);
                        lz.RemoveAt(lz.Count - 1);
                    }
                }
                else //癞子不够了 返回
                {
                    return new List<CardInfo>();
                }
            }
            //剩余癞子组对子
            if(lz.Count >= 0 && lz.Count % 2 == 0)
            {
                for (int i = 0; i < lz.Count; i++)
                {
                    lz[i].LaiZiValueChanged(lz[i].num);
                }

                cards.AddRange(lz);
                return cards;
            }

            return new List<CardInfo>();
        }

        static void IsVilid32(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> cardsArray)
        {
            //先判断3癞子的特殊情况 LLL33 不能 变成 333LL
            if(laiziArray.Count == 3 && cardsArray.Count == 1)
            {
                if(laiziArray[0].num > cardsArray[0].num)
                {
                    for (int i = 0; i < laiziArray.Count; i++)
                    {
                        laiziArray[i].LaiZiValueChanged(laiziArray[i].num);
                    }
                    final_list.AddRange(laiziArray);
                    final_list.AddRange(cardsArray[0].cards);
                    result = true;
                    return;
                }
            }
            //通用判断
            for (int index = 0; index < cardsArray.Count; index++)
            {
                DividedCard dc = cardsArray[index];
                if(dc.num > 13) {//有王不可能3带2
                    result = false;
                    return;
                }

                int need = 3 - dc.count;
                if(need < 0) {//4444L 非法牌型
                    result = false;
                    return;
                }

                if(need <= laiziArray.Count)
                {
                    List<CardInfo> tmp = new List<CardInfo>();
                    tmp.AddRange(cardsArray[index].cards);
                    List<CardInfo> tmpLz = new List<CardInfo>();
                    tmpLz.AddRange(laiziArray);
                    if (need > 0) {
                        for (int i = 0; i < need; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(cardsArray[index].num);
                            tmp.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need, need);
                    }
                    //将剩下的牌凑成一对
                    List<CardInfo> duizi = CheckDoubles(GetSubArrayExceptIndex(cardsArray, new List<int> { index }), 1, tmpLz);
                    if(duizi.Count == 2)
                    {
                        tmp.AddRange(duizi);
                        final_list.AddRange(tmp);
                        result = true;
                        return;
                    }
                }
            }
            result = false;
        }

        static void IsVilid411(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> cardsArray)
        {
            //先判断4癞子的特殊情况 LLLL43 不能 变成 4444L3
            if (laiziArray.Count == 4 && cardsArray.Count <= 2)
            {
                if (laiziArray[0].num > cardsArray[0].num)
                {
                    for (int i = 0; i < laiziArray.Count; i++)
                    {
                        laiziArray[i].LaiZiValueChanged(laiziArray[i].num);
                    }
                    final_list.AddRange(laiziArray);
                    final_list.AddRange(cardsArray[0].cards);
                    if (cardsArray.Count == 2)
                        final_list.AddRange(cardsArray[1].cards);
                    result = true;
                    return;
                }
            }

            //通用判断
            for (int index = 0; index < cardsArray.Count; index++)
            {
                DividedCard dc = cardsArray[index];
                if (dc.num > 13) //王不能凑四张
                {
                    continue;
                }
                int need = 4 - dc.count;
                if(need <= laiziArray.Count)
                {
                    //找到四张
                    List<CardInfo> tmp = new List<CardInfo>();
                    tmp.AddRange(cardsArray[index].cards);
                    List<CardInfo> tmpLz = new List<CardInfo>();
                    tmpLz.AddRange(laiziArray);
                    if (need > 0)
                    {
                        for (int i = 0; i < need; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(cardsArray[index].num);
                            tmp.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need, need);
                    }

                    //剩下癞子直接变原值
                    for (int i = 0; i < tmpLz.Count; i++)
                    {
                        tmpLz[i].LaiZiValueChanged(tmpLz[i].num);
                    }
                    tmp.AddRange(tmpLz);
                    //剩下的直接装回去
                    for (int j = 0; j < cardsArray.Count; j++)
                    {
                        if(j != index)
                        {
                            tmp.AddRange(cardsArray[j].cards);
                        }
                    }
                    final_list.AddRange(tmp);
                    result = true;
                    return;
                }
            }

            result = false;
        }

        static void IsVilid422(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> cardsArray)
        {
            //先判断4癞子的特殊情况 LLLL4433 不能 变成 4444,33,33； LLLL3333不能变成 3333,LL,LL
            if (laiziArray.Count == 4 && cardsArray.Count == 1)
            {
                if (laiziArray[0].num > cardsArray[0].num)
                {
                    for (int i = 0; i < laiziArray.Count; i++)
                    {
                        laiziArray[i].LaiZiValueChanged(laiziArray[i].num);
                    }
                    final_list.AddRange(laiziArray);
                    final_list.AddRange(cardsArray[0].cards);
                    result = true;
                    return;
                }
            }
            if(laiziArray.Count == 4 && cardsArray.Count == 2 && cardsArray[0].count == 2)
            {
                if (laiziArray[0].num > cardsArray[0].num)
                {
                    for (int i = 0; i < laiziArray.Count; i++)
                    {
                        laiziArray[i].LaiZiValueChanged(laiziArray[i].num);
                    }
                    final_list.AddRange(laiziArray);
                    final_list.AddRange(cardsArray[0].cards);
                    final_list.AddRange(cardsArray[1].cards);
                    result = true;
                    return;
                }
            }
            //通用判断
            for (int index = 0; index < cardsArray.Count; index++)
            {
                DividedCard dc = cardsArray[index];
                if (dc.num > 13)
                {//有王不可能是带对子
                    result = false;
                    return;
                }

                int need = 4 - dc.count;

                if (need <= laiziArray.Count)
                {
                    List<CardInfo> tmp = new List<CardInfo>();
                    tmp.AddRange(cardsArray[index].cards);
                    List<CardInfo> tmpLz = new List<CardInfo>();
                    tmpLz.AddRange(laiziArray);
                    if (need > 0)
                    {
                        for (int i = 0; i < need; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(cardsArray[index].num);
                            tmp.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need, need);
                    }
                    //将剩下的牌凑成2对
                    List<CardInfo> duizi = CheckDoubles(GetSubArrayExceptIndex(cardsArray, new List<int> { index }), 2, tmpLz);
                    if (duizi.Count == 4)
                    {
                        tmp.AddRange(duizi);
                        final_list.AddRange(tmp);
                        result = true;
                        return;
                    }
                }
            }
            result = false;
        }

        static void IsVilid3311(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> dicArray)
        {
            //从AK开始找 丢弃大小王2
            for (int index = 3; index < dicArray.Count - 1; index++)
            {
                DividedCard tmp1 = dicArray[index];
                DividedCard tmp2 = dicArray[index + 1];
                int need1 = 3 - tmp1.count;
                int need2 = 3 - tmp2.count;
                if (need1 < 0)
                {
                    need1 = 0;
                }
                if (need2 < 0)
                {
                    need2 = 0;
                }

                int left = laiziArray.Count - need1 - need2;
                //找到
                if(left >= 0)
                {
                    List<CardInfo> cards = new List<CardInfo>();
                    cards.AddRange(dicArray[index].cards);
                    if (need1 > 0)
                    {
                        for (int i = 0; i < need1; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need1, need1);
                    }

                    cards.AddRange(dicArray[index + 1].cards);
                    if (need2 > 0)
                    {
                        for (int i = 0; i < need2; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 1].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need2, need2);
                    }
                    //把剩下的癞子转换原值
                    for (int i = 0; i < laiziArray.Count; i++)
                    {
                        laiziArray[i].LaiZiValueChanged(laiziArray[i].num);
                    }
                    cards.AddRange(laiziArray);
                    //剩下的直接装回去
                    for (int j = 0; j < dicArray.Count; j++)
                    {
                        if (j != index && j != index + 1)
                        {
                            cards.AddRange(dicArray[j].cards);
                        }
                    }
                    final_list.AddRange(cards);
                    result = true;
                    return;
                }
            }

            result = false;
        }

        static void IsVilid3322(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> dicArray)
        {
            //从AK开始找
            for (int index = 3; index < dicArray.Count - 1; index++)
            {
                DividedCard tmp1 = dicArray[index];
                DividedCard tmp2 = dicArray[index + 1];
                int need1 = 3 - tmp1.count;
                int need2 = 3 - tmp2.count;
                if (need1 < 0)
                {
                    need1 = 0;
                }
                if (need2 < 0)
                {
                    need2 = 0;
                }

                int left = laiziArray.Count - need1 - need2;
                //找到
                if (left >= 0)
                {
                    List<CardInfo> tmpLz = new List<CardInfo>();
                    tmpLz.AddRange(laiziArray);
                    List<CardInfo> cards = new List<CardInfo>();
                    cards.AddRange(dicArray[index].cards);
                    if (need1 > 0)
                    {
                        for (int i = 0; i < need1; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(dicArray[index].num);
                            cards.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need1, need1);
                    }

                    cards.AddRange(dicArray[index + 1].cards);
                    if (need2 > 0)
                    {
                        for (int i = 0; i < need2; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(dicArray[index + 1].num);
                            cards.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need2, need2);
                    }
                    //将剩下的牌凑成2对
                    List<CardInfo> duizi = CheckDoubles(GetSubArrayExceptIndex(dicArray, new List<int> { index, index + 1 }), 2, tmpLz);
                    if (duizi.Count == 4)
                    {
                        cards.AddRange(duizi);
                        final_list.AddRange(cards);
                        result = true;
                        return;
                    }
                }
            }

            result = false;
        }

        static void IsVilid333111(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> dicArray)
        {
            //从AK开始找
            for (int index = 3; index < dicArray.Count - 2; index++)
            {
                DividedCard tmp1 = dicArray[index];
                DividedCard tmp2 = dicArray[index + 1];
                DividedCard tmp3 = dicArray[index + 2];
                int need1 = 3 - tmp1.count;
                int need2 = 3 - tmp2.count;
                int need3 = 3 - tmp3.count;
                if (need1 < 0)
                {
                    need1 = 0;
                }
                if (need2 < 0)
                {
                    need2 = 0;
                }
                if (need3 < 0)
                {
                    need3 = 0;
                }

                int left = laiziArray.Count - need1 - need2 - need3;
                //找到
                if (left >= 0)
                {
                    List<CardInfo> cards = new List<CardInfo>();
                    cards.AddRange(dicArray[index].cards);
                    if (need1 > 0)
                    {
                        for (int i = 0; i < need1; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need1, need1);
                    }

                    cards.AddRange(dicArray[index + 1].cards);
                    if (need2 > 0)
                    {
                        for (int i = 0; i < need2; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 1].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need2, need2);
                    }

                    cards.AddRange(dicArray[index + 2].cards);
                    if (need3 > 0)
                    {
                        for (int i = 0; i < need3; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 2].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need3, need3);
                    }
                    //把剩下的癞子转换原值
                    for (int i = 0; i < laiziArray.Count; i++)
                    {
                        laiziArray[i].LaiZiValueChanged(laiziArray[i].num);
                    }
                    cards.AddRange(laiziArray);
                    //剩下的直接装回去
                    for (int j = 0; j < dicArray.Count; j++)
                    {
                        if (j != index && j != index + 1 && j != index + 2)
                        {
                            cards.AddRange(dicArray[j].cards);
                        }
                    }
                    final_list.AddRange(cards);
                    result = true;
                    return;
                }
            }

            result = false;
        }

        static void IsVilid333222(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> dicArray)
        {
            //从AK开始找
            for (int index = 3; index < dicArray.Count - 2; index++)
            {
                DividedCard tmp1 = dicArray[index];
                DividedCard tmp2 = dicArray[index + 1];
                DividedCard tmp3 = dicArray[index + 2];
                int need1 = 3 - tmp1.count;
                int need2 = 3 - tmp2.count;
                int need3 = 3 - tmp3.count;
                if (need1 < 0)
                {
                    need1 = 0;
                }
                if (need2 < 0)
                {
                    need2 = 0;
                }
                if (need3 < 0)
                {
                    need3 = 0;
                }

                int left = laiziArray.Count - need1 - need2 - need3;
                //找到
                if (left >= 0)
                {
                    List<CardInfo> tmpLz = new List<CardInfo>();
                    tmpLz.AddRange(laiziArray);
                    List<CardInfo> cards = new List<CardInfo>();
                    cards.AddRange(dicArray[index].cards);
                    if (need1 > 0)
                    {
                        for (int i = 0; i < need1; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(dicArray[index].num);
                            cards.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need1, need1);
                    }

                    cards.AddRange(dicArray[index + 1].cards);
                    if (need2 > 0)
                    {
                        for (int i = 0; i < need2; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(dicArray[index + 1].num);
                            cards.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need2, need2);
                    }
                    cards.AddRange(dicArray[index + 2].cards);
                    if (need3 > 0)
                    {
                        for (int i = 0; i < need3; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(dicArray[index + 2].num);
                            cards.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need3, need3);
                    }
                    //将剩下的牌凑成3对
                    List<CardInfo> duizi = CheckDoubles(GetSubArrayExceptIndex(dicArray, new List<int> { index, index + 1, index + 2 }), 3, tmpLz);
                    if (duizi.Count == 6)
                    {
                        cards.AddRange(duizi);
                        final_list.AddRange(cards);
                        result = true;
                        return;
                    }
                }
            }

            result = false;
        }

        static void IsVilid33331111(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> dicArray)
        {
            //从AK开始找
            for (int index = 3; index < dicArray.Count - 3; index++)
            {
                DividedCard tmp1 = dicArray[index];
                DividedCard tmp2 = dicArray[index + 1];
                DividedCard tmp3 = dicArray[index + 2];
                DividedCard tmp4 = dicArray[index + 3];
                int need1 = 3 - tmp1.count;
                int need2 = 3 - tmp2.count;
                int need3 = 3 - tmp3.count;
                int need4 = 3 - tmp4.count;
                if (need1 < 0)
                {
                    need1 = 0;
                }
                if (need2 < 0)
                {
                    need2 = 0;
                }
                if (need3 < 0)
                {
                    need3 = 0;
                }
                if (need4 < 0)
                {
                    need4 = 0;
                }

                int left = laiziArray.Count - need1 - need2 - need3 - need4;
                //找到
                if (left >= 0)
                {
                    List<CardInfo> cards = new List<CardInfo>();
                    cards.AddRange(dicArray[index].cards);
                    if (need1 > 0)
                    {
                        for (int i = 0; i < need1; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need1, need1);
                    }

                    cards.AddRange(dicArray[index + 1].cards);
                    if (need2 > 0)
                    {
                        for (int i = 0; i < need2; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 1].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need2, need2);
                    }

                    cards.AddRange(dicArray[index + 2].cards);
                    if (need3 > 0)
                    {
                        for (int i = 0; i < need3; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 2].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need3, need3);
                    }
                    cards.AddRange(dicArray[index + 3].cards);
                    if (need4 > 0)
                    {
                        for (int i = 0; i < need4; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 3].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need4, need4);
                    }
                    //把剩下的癞子转换原值
                    for (int i = 0; i < laiziArray.Count; i++)
                    {
                        laiziArray[i].LaiZiValueChanged(laiziArray[i].num);
                    }
                    cards.AddRange(laiziArray);
                    //剩下的直接装回去
                    for (int j = 0; j < dicArray.Count; j++)
                    {
                        if (j != index && j != index + 1 && j != index + 2 && j != index + 3 && dicArray[j].count != 0)
                        {
                            cards.AddRange(dicArray[j].cards);
                        }
                    }
                    final_list.AddRange(cards);
                    result = true;
                    return;
                }
            }

            result = false;
        }

        static void IsVilid33332222(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> dicArray)
        {
            //从AK开始找
            for (int index = 3; index < dicArray.Count - 3; index++)
            {
                DividedCard tmp1 = dicArray[index];
                DividedCard tmp2 = dicArray[index + 1];
                DividedCard tmp3 = dicArray[index + 2];
                DividedCard tmp4 = dicArray[index + 3];
                int need1 = 3 - tmp1.count;
                int need2 = 3 - tmp2.count;
                int need3 = 3 - tmp3.count;
                int need4 = 3 - tmp4.count;
                if (need1 < 0)
                {
                    need1 = 0;
                }
                if (need2 < 0)
                {
                    need2 = 0;
                }
                if (need3 < 0)
                {
                    need3 = 0;
                }
                if (need4 < 0)
                {
                    need4 = 0;
                }

                int left = laiziArray.Count - need1 - need2 - need3 - need4;
                //找到
                if (left >= 0)
                {
                    List<CardInfo> tmpLz = new List<CardInfo>();
                    tmpLz.AddRange(laiziArray);
                    List<CardInfo> cards = new List<CardInfo>();
                    cards.AddRange(dicArray[index].cards);
                    if (need1 > 0)
                    {
                        for (int i = 0; i < need1; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(dicArray[index].num);
                            cards.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need1, need1);
                    }

                    cards.AddRange(dicArray[index + 1].cards);
                    if (need2 > 0)
                    {
                        for (int i = 0; i < need2; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(dicArray[index + 1].num);
                            cards.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need2, need2);
                    }
                    cards.AddRange(dicArray[index + 2].cards);
                    if (need3 > 0)
                    {
                        for (int i = 0; i < need3; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(dicArray[index + 2].num);
                            cards.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need3, need3);
                    }
                    cards.AddRange(dicArray[index + 3].cards);
                    if (need4 > 0)
                    {
                        for (int i = 0; i < need4; i++)
                        {
                            tmpLz[tmpLz.Count - i - 1].LaiZiValueChanged(dicArray[index + 3].num);
                            cards.Add(tmpLz[tmpLz.Count - i - 1]);
                        }
                        tmpLz.RemoveRange(tmpLz.Count - need4, need4);
                    }
                    //将剩下的牌凑成3对
                    List<CardInfo> duizi = CheckDoubles(GetSubArrayExceptIndex(dicArray, new List<int> { index, index + 1, index + 2, index + 3 }), 4, tmpLz);
                    if (duizi.Count == 8)
                    {
                        cards.AddRange(duizi);
                        final_list.AddRange(cards);
                        result = true;
                        return;
                    }
                }
            }

            result = false;
        }

        static void IsVilid3333311111(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> dicArray)
        {
            //从AK开始找
            for (int index = 3; index < dicArray.Count - 4; index++)
            {
                DividedCard tmp1 = dicArray[index];
                DividedCard tmp2 = dicArray[index + 1];
                DividedCard tmp3 = dicArray[index + 2];
                DividedCard tmp4 = dicArray[index + 3];
                DividedCard tmp5 = dicArray[index + 4];
                int need1 = 3 - tmp1.count;
                int need2 = 3 - tmp2.count;
                int need3 = 3 - tmp3.count;
                int need4 = 3 - tmp4.count;
                int need5 = 3 - tmp5.count;
                if (need1 < 0)
                {
                    need1 = 0;
                }
                if (need2 < 0)
                {
                    need2 = 0;
                }
                if (need3 < 0)
                {
                    need3 = 0;
                }
                if (need4 < 0)
                {
                    need4 = 0;
                }
                if (need5 < 0)
                {
                    need5 = 0;
                }

                int left = laiziArray.Count - need1 - need2 - need3 - need4 - need5;
                //找到
                if (left >= 0)
                {
                    List<CardInfo> cards = new List<CardInfo>();
                    cards.AddRange(dicArray[index].cards);
                    if (need1 > 0)
                    {
                        for (int i = 0; i < need1; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need1, need1);
                    }

                    cards.AddRange(dicArray[index + 1].cards);
                    if (need2 > 0)
                    {
                        for (int i = 0; i < need2; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 1].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need2, need2);
                    }

                    cards.AddRange(dicArray[index + 2].cards);
                    if (need3 > 0)
                    {
                        for (int i = 0; i < need3; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 2].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need3, need3);
                    }
                    cards.AddRange(dicArray[index + 3].cards);
                    if (need4 > 0)
                    {
                        for (int i = 0; i < need4; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 3].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need4, need4);
                    }
                    cards.AddRange(dicArray[index + 4].cards);
                    if (need5 > 0)
                    {
                        for (int i = 0; i < need4; i++)
                        {
                            laiziArray[laiziArray.Count - i - 1].LaiZiValueChanged(dicArray[index + 4].num);
                            cards.Add(laiziArray[laiziArray.Count - i - 1]);
                        }
                        laiziArray.RemoveRange(laiziArray.Count - need5, need5);
                    }
                    //把剩下的癞子转换原值
                    for (int i = 0; i < laiziArray.Count; i++)
                    {
                        laiziArray[i].LaiZiValueChanged(laiziArray[i].num);
                    }
                    cards.AddRange(laiziArray);
                    //剩下的直接装回去
                    for (int j = 0; j < dicArray.Count; j++)
                    {
                        if (j != index && j != index + 1 && j != index + 2 && j != index + 3 && j != index + 4)
                        {
                            cards.AddRange(dicArray[j].cards);
                        }
                    }
                    final_list.AddRange(cards);
                    result = true;
                    return;
                }
            }

            result = false;
        }

        static void IsVilidContinuous(ref List<CardInfo> final_list, ref bool result, List<CardInfo> laiziArray, List<DividedCard> dicArray, int cell_count, int total_count)
        {
            if(dicArray[0].count > 0 || dicArray[1].count > 0 || dicArray[2].count > 0)
            {
                result = false;
                return; 
            }

            for (int index = 3; index < dicArray.Count; index++)
            {
                int lz_use = 0;
                int i = 0;
                List<CardInfo> cards = new List<CardInfo>();
                List<CardInfo> tmpLz = new List<CardInfo>();
                tmpLz.AddRange(laiziArray);
                int card_use = 0;
                while (i < dicArray.Count - index)
                {
                    int sub_index = index + i;
                    DividedCard dc = dicArray[sub_index];
                    int need = cell_count - dc.count;
                    if (need < 0)
                    {
                        result = false;
                        return;
                    }
                    if (lz_use > laiziArray.Count - need)
                    {
                        break;
                    }

                    lz_use += need;
                    cards.AddRange(dc.cards);
                    if (need > 0)
                    {
                        for (int j = 0; j < need; j++)
                        {
                            tmpLz[j].LaiZiValueChanged(dc.num);
                            cards.Add(tmpLz[j]);
                        }
                        tmpLz.RemoveRange(0, need);
                    }
                    card_use += dc.count;
                    i++;
                }

                int clen = cards.Count;

                if (clen >= 5 && laiziArray.Count == lz_use && card_use + lz_use == total_count)
                {
                    result = true;
                    final_list.AddRange(cards);
                    return;
                }
            }

            result = false;
        }
        #endregion

        #region 被动出牌推荐
        /// <summary>
        /// 提示出牌
        /// </summary>
        /// <param name="myCards">手牌</param>
        /// <param name="compareCards">比较的牌</param>
        /// <param name="ct">比较的牌型</param>
        public static List<List<CardInfo>> AIFindWinCards(List<CardInfo> handCards, List<CardInfo> compareCards, CardType ct)
        {
            List<CardInfo> myCards = new List<CardInfo>();
            myCards.AddRange(handCards);
            //筛选后的排组库
            List<List<CardInfo>> return_cards = new List<List<CardInfo>>();
            //先拿出癞子
            List<CardInfo> laiziArray = new List<CardInfo>();
            for (int i = 0; i < 4; i++)
            {
                if (myCards[i].laizi)
                    laiziArray.Add(myCards[i]);
                else
                    break;
            }
            myCards.RemoveRange(0, laiziArray.Count);
            //是否有火箭
            bool rocket = myCards.Count >= 2 && (GameCommon.IsRedJoker(myCards[0].num) && GameCommon.IsBlackJoker(myCards[1].num));
            List<CardInfo> rocketCards = rocket ? new List<CardInfo> { myCards[0], myCards[1] } : null;
            //拆牌
            /*cardArray
             * [{牌权值13,数量4,具体牌对象List<cardInfo>}, {9,4,List<cardInfo>}... ]           
             */
            List<DividedCard> cardsArray = new List<DividedCard>();
            DividedCard dc = null;
            for (int i = 0; i < myCards.Count; i++)
            {
                if (dc == null)
                {
                    dc = new DividedCard(myCards[i].num, myCards[i]);
                    if(1 == myCards.Count)
                    {
                        cardsArray.Add(dc);
                    }
                }
                else
                {
                    if (dc.num == myCards[i].num)
                    {
                        dc.Add(myCards[i]);
                    }
                    else
                    {
                        cardsArray.Add(dc);
                        dc = new DividedCard(myCards[i].num, myCards[i]);
                    }
                    //最后一种牌
                    if (i == myCards.Count - 1)
                        cardsArray.Add(dc);
                }
            }

            //根据牌型确定出牌选项
            switch (ct)
            {
                case CardType.ZERO:
                    ////
                    //if(myCards.Count >= 2 && myCards[myCards.Count - 1].num == myCards[myCards.Count - 2].num)
                    //    return_cards.Add(new List<CardInfo> { myCards[myCards.Count - 2], myCards[myCards.Count - 1] });
                    //else
                        //return_cards.Add(new List<CardInfo> { myCards[myCards.Count - 1] });
                    break;
                case CardType.ROCKET:
                    //直接过 火箭
                    break;
                case CardType.SINGLE_CARD:
                    return_cards = FindWinSingle(cardsArray, laiziArray, compareCards, rocketCards);
                    break;
                case CardType.DOUBLE_CARD:
                    return_cards = FindWinDouble(cardsArray, laiziArray, compareCards, rocketCards);
                    break;
                case CardType.THREE_CARD:
                case CardType.THREE_WITH_ONE_SINGLE:
                case CardType.THREE_WITH_ONE_PAIR:
                    return_cards = FindWinThree(cardsArray, laiziArray, compareCards, ct, rocketCards);
                    break;
                case CardType.BOMB_CARD:
                case CardType.FOUR_WITH_TWO_SINGLE:
                case CardType.FOUR_WITH_TWO_PAIR:
                    return_cards = FindWinBomb(cardsArray, laiziArray, compareCards, ct, rocketCards);
                    break;
                case CardType.LIANDUI_CARD:
                case CardType.SHUNZI_CARD:
                case CardType.AIRCRAFT_CARD:
                case CardType.AIRCRAFT_SINGLE_CARD:
                case CardType.AIRCRAFT_DOUBLE_CARD:
                    return_cards = FindWinContinous(cardsArray, laiziArray, compareCards, ct, rocketCards);
                    break;
            }

            return return_cards;
        }

        static List<List<CardInfo>> FindWinSingle(List<DividedCard> all, List<CardInfo> lz, List<CardInfo> top, List<CardInfo> rocket=null)
        {
            Debug.Log("OutCardTest all.count=" + all.Count);
            List<List<CardInfo>> return_ci = new List<List<CardInfo>>();
            all.Reverse();
            //需要大过这么多点
            int need = top[0].num;
            //单牌大过的
            for (int i = 0; i < all.Count; i++)
            {
                Debug.Log("OutCardTest all[i].num=" + all[i].num + ", need=" + need + ", count=" + all[i].count);
                if (all[i].num > need && all[i].count == 1)
                {
                    return_ci.Add(new List<CardInfo> { all[i].cards[0] });
                }
            }
            //拆对子 三张
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].num > need && (all[i].count == 2 || all[i].count == 3))
                {
                    return_ci.Add(new List<CardInfo> { all[i].cards[all[i].count - 1] });
                }
            }

            //癞子
            if(lz.Count > 0 && lz[0].num > need)
            {
                lz[lz.Count - 1].LaiZiValueChanged(lz[0].num);
                return_ci.Add(new List<CardInfo> { lz[lz.Count - 1] });
            }

            //炸弹
            return_ci.AddRange(FindBomb(all, lz));
            //rocket
            if (rocket != null)
                return_ci.Add(rocket);

            //完成了
            return return_ci;
        }

        static List<List<CardInfo>> FindWinDouble(List<DividedCard> all, List<CardInfo> lz, List<CardInfo> top, List<CardInfo> rocket = null)
        {
            List<List<CardInfo>> return_ci = new List<List<CardInfo>>();
            all.Reverse();
            //需要大过这么多点
            int need = top[0].num;
            //对子大过的
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].num > need && all[i].count == 2)
                {
                    return_ci.Add(all[i].cards);
                }
            }
            //拿癞子凑
            if(lz.Count > 0)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    //排除大小王
                    if (all[i].num > need && all[i].count == 1 && all[i].num < 14)
                    {
                        lz[lz.Count - 1].LaiZiValueChanged(all[i].num);
                        return_ci.Add(new List<CardInfo> { all[i].cards[0], lz[lz.Count - 1] });
                    }
                }
            }
            //对癞子
            if (lz.Count >= 2 && lz[0].num > need)
            {
                lz[lz.Count - 1].LaiZiValueChanged(lz[0].num);
                lz[lz.Count - 2].LaiZiValueChanged(lz[0].num);
                return_ci.Add(new List<CardInfo> { lz[lz.Count - 2], lz[lz.Count - 1] });
            }
            //拆三张
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].num > need && all[i].count == 3)
                {
                    return_ci.Add(new List<CardInfo> { all[i].cards[1], all[i].cards[2] });
                }
            }

            //炸弹
            return_ci.AddRange(FindBomb(all, lz));
            //rocket
            if (rocket != null)
                return_ci.Add(rocket);

            //完成了
            return return_ci;
        }

        //大过所有三张 三带一 三带二
        static List<List<CardInfo>> FindWinThree(List<DividedCard> all, List<CardInfo> lz, List<CardInfo> top, CardType ct, List<CardInfo> rocket = null)
        {
            List<List<CardInfo>> return_ci = new List<List<CardInfo>>();
            all.Reverse();
            //需要大过这么多点
            int need = top[0].num;
            /////////////////不使用癞子////////////////////
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].num > need && all[i].count == 3)
                {
                    List<CardInfo> tmp = new List<CardInfo>();
                    tmp.AddRange(all[i].cards);

                    if(ct == CardType.THREE_WITH_ONE_SINGLE)
                    {
                        List<CardInfo> extra = FindMinSingle(lz, all, 1, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                        if (extra.Count > 0)
                        {
                            tmp.Add(extra[0]);
                        }else
                        {
                            tmp.Clear();
                        }
                    }

                    if (ct == CardType.THREE_WITH_ONE_PAIR)
                    {
                        List<List<CardInfo>> extra = FindMinDouble(lz, all, 1, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                        if (extra.Count > 0)
                        {
                            tmp.Add(extra[0][0]);
                            tmp.Add(extra[0][1]);
                        }else
                        {
                            tmp.Clear();
                        }
                    }

                    if (tmp.Count > 0)
                    {
                        return_ci.Add(tmp);
                    }
                }
            }
            ///////////////////用癞子///////////////////
            if(lz.Count > 0)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    //2+1
                    if (all[i].count == 2 && lz.Count > 0 && all[i].num > need)
                    {
                        List<CardInfo> tmp = new List<CardInfo>();
                        tmp.AddRange(all[i].cards);
                        List<CardInfo> lztmp = new List<CardInfo>();
                        lztmp.AddRange(lz);
                        lztmp[lztmp.Count - 1].LaiZiValueChanged(all[i].num);
                        tmp.Add(lztmp[lztmp.Count - 1]);
                        lztmp.RemoveAt(lztmp.Count - 1);
                        //带1带2

                        if (ct == CardType.THREE_WITH_ONE_SINGLE)
                        {
                            List<CardInfo> extra = FindMinSingle(lztmp, all, 1, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.Add(extra[0]);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (ct == CardType.THREE_WITH_ONE_PAIR)
                        {
                            List<List<CardInfo>> extra = FindMinDouble(lztmp, all, 1, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.Add(extra[0][0]);
                                tmp.Add(extra[0][1]);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (tmp.Count > 0)
                        {
                            return_ci.Add(tmp);
                        }
                    }

                    //1+2
                    if (all[i].count == 1 && lz.Count > 1 && all[i].num > need && all[i].num < 14)
                    {
                        List<CardInfo> tmp = new List<CardInfo>();
                        tmp.AddRange(all[i].cards);
                        List<CardInfo> lztmp = new List<CardInfo>();
                        lztmp.AddRange(lz);
                        lztmp[lztmp.Count - 1].LaiZiValueChanged(all[i].num);
                        lztmp[lztmp.Count - 2].LaiZiValueChanged(all[i].num);
                        tmp.Add(lztmp[lztmp.Count - 1]);
                        tmp.Add(lztmp[lztmp.Count - 2]);
                        lztmp.RemoveRange(lztmp.Count - 2, 2);
                        //带1带2

                        if (ct == CardType.THREE_WITH_ONE_SINGLE)
                        {
                            List<CardInfo> extra = FindMinSingle(lztmp, all, 1, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.Add(extra[0]);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (ct == CardType.THREE_WITH_ONE_PAIR)
                        {
                            List<List<CardInfo>> extra = FindMinDouble(lztmp, all, 1, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.Add(extra[0][0]);
                                tmp.Add(extra[0][1]);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (tmp.Count > 0)
                        {
                            return_ci.Add(tmp);
                        }
                    }
                }

                //癞子3带1 不变成炸弹 目前没有这种需求
            }

            //炸弹
            return_ci.AddRange(FindBomb(all, lz));
            //rocket
            if (rocket != null)
                return_ci.Add(rocket);

            //完成了
            return return_ci;
        }

        //判断炸弹类型
        // 'pureLzBomb',    //纯癞子炸弹，大于普通炸弹小于王炸
        // 'normalBomb',    //普通炸弹，大于癞子炸弹
        // 'softBomb',      //软炸弹，小于普通炸弹，大于其他牌
        // 'notBomb'        //不是炸弹
        static string GetBombType(List<CardInfo> ci)
        {
            string type = "notBomb";
            if (CheckCardType(ci, out int lv) == CardType.BOMB_CARD)
            {
                int lzCount = 0;
                for (int i = 0; i < ci.Count; i++)
                {
                    if(ci[i].laizi)
                    {
                        lzCount += 1; 
                    }
                }
                switch (lzCount)
                {
                    case 0:
                        type = "normalBomb";
                        break;
                    case 1:
                    case 2:
                    case 3:
                        type = "softBomb";
                        break;
                    case 4:
                        type = "pureLzBomb";
                        break;
                }
            }
            return type;
        }
        //大过所有炸弹 四带二 四带两对
        static List<List<CardInfo>> FindWinBomb(List<DividedCard> all, List<CardInfo> lz, List<CardInfo> top, CardType ct, List<CardInfo> rocket = null)
        {
            List<List<CardInfo>> return_ci = new List<List<CardInfo>>();
            all.Reverse();//从小到大找
            //需要大过这么多点
            int need = top[0].num;
            /////////////////不使用癞子////////////////////
            for (int i = 0; i < all.Count; i++)
            {
                if (all[i].num > need && all[i].count == 4)
                {
                    List<CardInfo> tmp = new List<CardInfo>();
                    tmp.AddRange(all[i].cards);

                    if (ct == CardType.BOMB_CARD)
                    {
                        if (GetBombType(top) == "pureLzBomb")
                        { tmp.Clear(); }
                    }

                    if (ct == CardType.FOUR_WITH_TWO_SINGLE)
                    {
                        List<CardInfo> extra = FindMinSingle(lz, all, 2, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                        if (extra.Count > 0)
                        {
                            tmp.AddRange(extra);
                        }
                        else
                        {
                            tmp.Clear();
                        }
                    }

                    if (ct == CardType.FOUR_WITH_TWO_PAIR)
                    {
                        List<List<CardInfo>> extra = FindMinDouble(lz, all, 2, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                        if (extra.Count > 0)
                        {
                            tmp.AddRange(extra[0]);
                            tmp.AddRange(extra[1]);
                        }
                        else
                        {
                            tmp.Clear();
                        }
                    }

                    if (tmp.Count > 0)
                    {
                        return_ci.Add(tmp);
                    }
                }
            }
            /////////////////考虑癞子////////////////////
            if (lz.Count > 0)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    //3+1
                    if (all[i].count == 3 && lz.Count > 0 && all[i].num > need)
                    {
                        List<CardInfo> tmp = new List<CardInfo>();
                        tmp.AddRange(all[i].cards);
                        List<CardInfo> lztmp = new List<CardInfo>();
                        lztmp.AddRange(lz);
                        lztmp[lztmp.Count - 1].LaiZiValueChanged(all[i].num);
                        tmp.Add(lztmp[lztmp.Count - 1]);
                        lztmp.RemoveAt(lztmp.Count - 1);

                        if (ct == CardType.FOUR_WITH_TWO_SINGLE)
                        {
                            List<CardInfo> extra = FindMinSingle(lztmp, all, 2, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.AddRange(extra);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (ct == CardType.FOUR_WITH_TWO_PAIR)
                        {
                            List<List<CardInfo>> extra = FindMinDouble(lztmp, all, 2, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.AddRange(extra[0]);
                                tmp.AddRange(extra[1]);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (tmp.Count > 0)
                        {
                            return_ci.Add(tmp);
                        }
                    }

                    //2+2
                    if (all[i].count == 2 && lz.Count > 1 && all[i].num > need)
                    {
                        List<CardInfo> tmp = new List<CardInfo>();
                        tmp.AddRange(all[i].cards);
                        List<CardInfo> lztmp = new List<CardInfo>();
                        lztmp.AddRange(lz);
                        lztmp[lztmp.Count - 1].LaiZiValueChanged(all[i].num);
                        lztmp[lztmp.Count - 2].LaiZiValueChanged(all[i].num);
                        tmp.Add(lztmp[lztmp.Count - 1]);
                        tmp.Add(lztmp[lztmp.Count - 2]);
                        lztmp.RemoveRange(lztmp.Count - 2, 2);

                        if (ct == CardType.FOUR_WITH_TWO_SINGLE)
                        {
                            List<CardInfo> extra = FindMinSingle(lztmp, all, 2, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.AddRange(extra);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (ct == CardType.FOUR_WITH_TWO_PAIR)
                        {
                            List<List<CardInfo>> extra = FindMinDouble(lztmp, all, 2, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.AddRange(extra[0]);
                                tmp.AddRange(extra[1]);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (tmp.Count > 0)
                        {
                            return_ci.Add(tmp);
                        }
                    }

                    //1+3
                    if (all[i].count == 1 && lz.Count > 2 && all[i].num > need && all[i].num < 14)
                    {
                        List<CardInfo> tmp = new List<CardInfo>();
                        tmp.AddRange(all[i].cards);
                        List<CardInfo> lztmp = new List<CardInfo>();
                        lztmp.AddRange(lz);
                        lztmp[lztmp.Count - 1].LaiZiValueChanged(all[i].num);
                        lztmp[lztmp.Count - 2].LaiZiValueChanged(all[i].num);
                        lztmp[lztmp.Count - 3].LaiZiValueChanged(all[i].num);
                        tmp.Add(lztmp[lztmp.Count - 1]);
                        tmp.Add(lztmp[lztmp.Count - 2]);
                        tmp.Add(lztmp[lztmp.Count - 3]);
                        lztmp.RemoveRange(lztmp.Count - 3, 3);

                        if (ct == CardType.FOUR_WITH_TWO_SINGLE)
                        {
                            List<CardInfo> extra = FindMinSingle(lztmp, all, 2, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.AddRange(extra);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (ct == CardType.FOUR_WITH_TWO_PAIR)
                        {
                            List<List<CardInfo>> extra = FindMinDouble(lztmp, all, 2, new Dictionary<int, int> { { all[i].num, all[i].num } }, true);
                            if (extra.Count > 0)
                            {
                                tmp.AddRange(extra[0]);
                                tmp.AddRange(extra[1]);
                            }
                            else
                            {
                                tmp.Clear();
                            }
                        }

                        if (tmp.Count > 0)
                        {
                            return_ci.Add(tmp);
                        }
                    }
                }
            }
            //纯癞子
            if (lz.Count == 4)
            {
                List<CardInfo> tmp = new List<CardInfo>();
                tmp.AddRange(lz);

                if (ct == CardType.FOUR_WITH_TWO_SINGLE)
                {
                    List<CardInfo> extra = FindMinSingle(new List<CardInfo>(), all, 2, new Dictionary<int, int>(), true);
                    if (extra.Count > 0)
                    {
                        tmp.AddRange(extra);
                    }
                    else
                    {
                        tmp.Clear();
                    }
                }

                if (ct == CardType.FOUR_WITH_TWO_PAIR)
                {
                    List<List<CardInfo>> extra = FindMinDouble(new List<CardInfo>(), all, 2, new Dictionary<int, int>(), true);
                    if (extra.Count > 0)
                    {
                        tmp.AddRange(extra[0]);
                        tmp.AddRange(extra[1]);
                    }
                    else
                    {
                        tmp.Clear();
                    }
                }

                if (tmp.Count > 0)
                {
                    return_ci.Add(tmp);
                }
            }
            //炸弹
            if (ct == CardType.FOUR_WITH_TWO_SINGLE || ct == CardType.FOUR_WITH_TWO_PAIR)
                return_ci.AddRange(FindBomb(all, lz));
            //rocket
            if (rocket != null)
                return_ci.Add(rocket);

            //完成了
            return return_ci;
        }

        //找到大过的顺子, 连对，飞机
        static List<List<CardInfo>> FindWinContinous(List<DividedCard> all, List<CardInfo> lz, List<CardInfo> top, CardType ct, List<CardInfo> rocket = null)
        {
            List<List<CardInfo>> return_ci = new List<List<CardInfo>>();
            int continue_count = 0;
            int cell_count = 0;
            int need = 0;
            switch (ct)
            {
                case CardType.SHUNZI_CARD:
                    continue_count = top.Count;
                    cell_count = 1;
                    need = top[top.Count - 1].num;
                    break;
                case CardType.LIANDUI_CARD:
                    continue_count = top.Count / 2;
                    cell_count = 2;
                    need = top[top.Count - 1].num;
                    break;
                case CardType.AIRCRAFT_CARD:
                    continue_count = top.Count / 3;
                    cell_count = 3;
                    need = top[top.Count - 1].num;
                    break;
                case CardType.AIRCRAFT_SINGLE_CARD:
                    continue_count = top.Count / 4;
                    cell_count = 3;
                    need = top[top.Count - continue_count - 1].num;
                    break;
                case CardType.AIRCRAFT_DOUBLE_CARD:
                    continue_count = top.Count / 5;
                    cell_count = 3;
                    need = top[top.Count - continue_count * 2 - 1].num;
                    break;
            }
            List<DividedCard> dic_cards = FillDicarr(all);
            dic_cards.Reverse();
            all.Reverse();
            for (int index = 1; index + continue_count < dic_cards.Count; index++)
            {
                if (dic_cards[index].num <= need)
                    continue;

                for (int laiziMax = 0; laiziMax < lz.Count; laiziMax++)
                {
                    List<CardInfo> cards = new List<CardInfo>();
                    Dictionary<int, int> ex = new Dictionary<int, int>();
                    int lz_used = 0;
                    int sub_index = 0;

                    while(sub_index < continue_count)
                    {
                        if(dic_cards[index + sub_index].count >= cell_count)
                        {
                            for (int i = 0; i < cell_count; i++)
                            {
                                cards.Add(dic_cards[index + sub_index].cards[i]);
                            }
                        }
                        else {
                            int use = cell_count - dic_cards[index + sub_index].count;
                            //癞子够用
                            if((use == laiziMax)&&(lz.Count -lz_used>= use))
                            {
                                cards.AddRange(dic_cards[index + sub_index].cards);
                                for (int i = 0; i < use; i++)
                                {
                                    lz[lz.Count - 1].LaiZiValueChanged(dic_cards[index + sub_index].num);
                                    cards.Add(lz[lz.Count - 1]);
                                }
                                lz_used += use;
                            }
                            else
                            {
                                cards.Clear();
                                break;
                            }
                        }

                        ex.Add(dic_cards[index + sub_index].num, dic_cards[index + sub_index].num);
                        sub_index++;
                    }
                    //飞机找翅膀
                    if (cards.Count > 0)
                    {
                        List<CardInfo> newLz = lz;
                        newLz.RemoveRange(lz.Count - lz_used, lz_used);
                        if (ct == CardType.AIRCRAFT_SINGLE_CARD)
                        {
                            List<CardInfo> ci = FindMinSingle(newLz, all, continue_count, ex, true);
                            if (ci.Count > 0)
                                cards.AddRange(ci);
                            else
                                cards.Clear();
                        }
                        else if(ct == CardType.AIRCRAFT_DOUBLE_CARD)
                        {
                            List<List<CardInfo>> ci = FindMinDouble(newLz, all, continue_count, ex, true);
                            if (ci.Count > 0)
                            {
                                for (int i = 0; i < ci.Count; i++)
                                {
                                    cards.AddRange(ci[i]);
                                }
                            }
                            else
                                cards.Clear();
                        }
                    }

                    if (cards.Count > 0)
                    {
                        return_ci.Add(cards); 
                    }
                }
            }
            //炸弹
            return_ci.AddRange(FindBomb(all, lz));
            //rocket
            if (rocket != null)
                return_ci.Add(rocket);

            //完成了
            return return_ci;
        }

        //不带王 2
        static List<DividedCard> FillDicarr(List<DividedCard> all)
        {
            List<DividedCard> return_arr = new List<DividedCard>();
            //AKQ-43 没有的用count=0填充
            for (int i = 12; i > 0; i--)
            {
                bool found = false;
                for (int j = 0; j < all.Count; j++)
                {
                    if(all[j].num == i)
                    {
                        found = true;
                        return_arr.Add(all[j]);
                        break; 
                    }
                }
                if(!found)
                {
                    return_arr.Add(new DividedCard(i));
                }

            }
            return return_arr;
        }
        //带王 和 2
        static List<DividedCard> FillDicarrWithAll(List<DividedCard> all)
        {
            List<DividedCard> return_arr = new List<DividedCard>();
            //joker
            if (all.Count >0 && all[0].num == 53)
            {
                return_arr.Add(all[0]);
            }
            else
            {
                return_arr.Add(new DividedCard(53));
            }
            if (all.Count > 1 && (all[0].num == 52 || all[1].num == 52))
            {
                return_arr.Add(all[0].num == 52 ? all[0] : all[1]);
            }
            else
            {
                return_arr.Add(new DividedCard(52));
            }
            //2AKQ-43 没有的用count=0填充
            for (int i = 13; i > 0; i--)
            {
                bool found = false;
                for (int j = 0; j < all.Count; j++)
                {
                    if (all[j].num == i)
                    {
                        found = true;
                        return_arr.Add(all[j]);
                        break;
                    }
                }
                if (!found)
                {
                    return_arr.Add(new DividedCard(i));
                }

            }
            return return_arr;
        }

        static List<List<CardInfo>> FindBomb(List<DividedCard> all, List<CardInfo> lz)
        {
            List<List<CardInfo>> result = new List<List<CardInfo>>();
            for (int laiziMax = 0; laiziMax <= lz.Count; laiziMax++)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    int need = 4 - all[i].count;
                    if ((need == laiziMax) && (need <= lz.Count) && (all[i].num < 14))
                    {
                        List<CardInfo> ci = new List<CardInfo>();
                        for (int j = 0; j < laiziMax; j++)
                        {
                            lz[j].LaiZiValueChanged(all[i].num);
                        }
                        ci.AddRange(lz.GetRange(0, laiziMax));
                        ci.AddRange(all[i].cards);
                        result.Add(ci);
                    }
                }
            }

            //纯癞子炸弹
            if (lz.Count == 4)
            {
                for (int i = 0; i < lz.Count; i++)
                {
                    lz[i].LaiZiValueChanged(lz[i].num);
                }
                result.Add(lz);
            }
            return result;
        }
        /// <summary>
        /// 找单牌 配合三带 四带 飞机等~
        /// </summary>
        /// <returns>The minimum single.</returns>
        /// <param name="lz">Lz.</param>
        /// <param name="all">All.</param>
        /// <param name="count">Count.</param>
        /// <param name="ex_point">Ex point.</param>
        /// <param name="bDivide">If set to <c>true</c> b divide.</param>
        static List<CardInfo> FindMinSingle(List<CardInfo> lz, List<DividedCard> all, int count, Dictionary<int, int> ex_point, bool bDivide = true)
        {
            List<CardInfo> result = new List<CardInfo>();
            for (int i = 0; i < all.Count; i++)
            {
                if(all[i].count != 1 || ex_point.ContainsKey(all[i].num))
                { continue; }
                result.Add(all[i].cards[0]);
                if (result.Count >= count)
                    return result;
            }

            if(bDivide) //单牌不足时拆对子等
            {
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].count > 1 && !ex_point.ContainsKey(all[i].num))
                        for (int j = 0; j < all[i].count; j++)
                        {
                            result.Add(all[i].cards[j]);
                            if (result.Count >= count)
                                return result;
                        }
                }
            }
            return new List<CardInfo>();
        }
        /// <summary>
        /// 找对子匹配
        /// </summary>
        /// <returns>The minimum double.</returns>
        /// <param name="lz">Lz.</param>
        /// <param name="all">All.</param>
        /// <param name="count">Count.</param>
        /// <param name="ex_point">Ex point.</param>
        /// <param name="bDivide">If set to <c>true</c> b divide.</param>
        static List<List<CardInfo>> FindMinDouble(List<CardInfo> lz, List<DividedCard> all, int count, Dictionary<int, int> ex_point, bool bDivide = true)
        {
            List<List<CardInfo>> result = new List<List<CardInfo>>();
            for (int i = 0; i < all.Count; i++)
            {
                if(all[i].count != 2 || ex_point.ContainsKey(all[i].num))
                { continue; }
                result.Add(all[i].cards);
                if (result.Count >= count)
                    return result;
            }
            //找单牌癞子拼对子
            if(lz.Count > 0)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].count == 1 && !ex_point.ContainsKey(all[i].num) && all[i].num < 14)//不能和王凑对子
                    {
                        CardInfo ci = lz[lz.Count - 1];
                        ci.LaiZiValueChanged(all[i].num);
                        lz.RemoveAt(lz.Count - 1);
                        result.Add(new List<CardInfo> { all[i].cards[0], ci });
                        if (result.Count >= count)
                            return result;
                        if (lz.Count == 0)
                            break;
                    }

                }
            }
            //拆三张
            if (bDivide)
            {
                for (int i = 0; i < all.Count; i++)
                {
                    if (all[i].count == 3 && !ex_point.ContainsKey(all[i].num))
                    {
                        result.Add(new List<CardInfo> { all[i].cards[1], all[i].cards[2] });
                        if (result.Count >= count)
                            return result;
                    }
                }
            }
            //对癞子 四个赖子拆成2对的情况不合常理不做推荐
            if(lz.Count >= 2)
            {
                lz[lz.Count - 1].LaiZiValueChanged(lz[0].num);
                lz[lz.Count - 2].LaiZiValueChanged(lz[0].num);

                result.Add(lz.GetRange(lz.Count - 2, 2));
                if (result.Count >= count)
                    return result;
            }
            return new List<List<CardInfo>>();
        }
        #endregion

        #region 主动出牌推荐
        public static List<List<CardInfo>> AIFindActiveCards(List<CardInfo> handCards)
        {
            List<CardInfo> myCards = new List<CardInfo>();
            myCards.AddRange(handCards);
            //筛选后的排组库
            List<List<CardInfo>> return_cards = new List<List<CardInfo>>();
            //先拿出癞子
            List<CardInfo> laiziArray = new List<CardInfo>();
            for (int i = 0; i < 4; i++)
            {
                if (myCards[i].laizi)
                    laiziArray.Add(myCards[i]);
                else
                    break;
            }
            myCards.RemoveRange(0, laiziArray.Count);
            //是否有火箭
            bool rocket = false || (myCards[0].num == 53 && myCards[1].num == 52);
            List<CardInfo> rocketCards = rocket ? new List<CardInfo> { myCards[0], myCards[1] } : null;
            //手里只剩下火箭
            if(myCards.Count == 2 && rocket)
            {
                return new List<List<CardInfo>> { myCards };
            }
            //拆牌
            /*cardArray
             * [{牌权值13,数量4,具体牌对象List<cardInfo>}, {9,4,List<cardInfo>}... ]
             */
            List<DividedCard> cardsArray = new List<DividedCard>();
            DividedCard dc = null;
            for (int i = 0; i < myCards.Count; i++)
            {
                if (dc == null)
                {
                    dc = new DividedCard(myCards[i].num, myCards[i]);
                }
                else
                {
                    if (dc.num == myCards[i].num)
                    {
                        dc.Add(myCards[i]);
                    }
                    else
                    {
                        cardsArray.Add(dc);
                        dc = new DividedCard(myCards[i].num, myCards[i]);
                    }
                    //最后一种牌
                    if (i == myCards.Count - 1)
                        cardsArray.Add(dc);
                }
            }
            //从小到大找 单 对子 三张 炸弹(先不考虑癞子凑)
            //找单牌
            for (int i = cardsArray.Count - 1; i >= 0; i--)
            {
                if(cardsArray[i].count == 1)
                {
                    return_cards.Add(cardsArray[i].cards); 
                }
            }
            if (return_cards.Count > 0)
            {
                return return_cards;
            }
            //找对子
            for (int i = cardsArray.Count - 1; i >= 0; i--)
            {
                if (cardsArray[i].count == 2)
                {
                    return_cards.Add(cardsArray[i].cards);
                }
            }
            if (return_cards.Count > 0)
            {
                return return_cards;
            }
            //找三张
            for (int i = cardsArray.Count - 1; i >= 0; i--)
            {
                if (cardsArray[i].count == 3)
                {
                    return_cards.Add(cardsArray[i].cards);
                }
            }
            if (return_cards.Count > 0)
            {
                return return_cards;
            }
            //找炸弹
            for (int i = cardsArray.Count - 1; i >= 0; i--)
            {
                if (cardsArray[i].count == 4)
                {
                    return_cards.Add(cardsArray[i].cards);
                }
            }
            if (return_cards.Count > 0)
            {
                return return_cards;
            }
            //癞子
            if(laiziArray.Count > 0)
            {
                return_cards.Add(laiziArray);
            }
            return return_cards;
        }
        #endregion

        static List<List<CardInfo>> FindContinous(List<DividedCard> arr, int cell_count)
        {
            List<List<CardInfo>> return_arr = new List<List<CardInfo>>();
            for (int index = 0; index < 11; index++)
            {
                int curIdx = 0;
                List<CardInfo> cards = new List<CardInfo>();

                while (curIdx <= 11 - index)
                {
                    int sub_index = index + curIdx;
                    DividedCard tmp = arr[sub_index];
                    int need = cell_count - tmp.count;

                    if (need > 0)
                    {
                        break;
                    }

                    for (int i = 0; i < cell_count; i++)
                    {
                        cards.Add(tmp.cards[i]);
                    }
                    
                    curIdx++;
                }

                int clen = cards.Count;
                if ((cell_count == 1 && clen >= 5) || (cell_count == 2 && clen >= 6))
                {
                    return_arr.Add(cards);
                }
            }
            if(return_arr.Count > 0)
            {
                return_arr.Sort((a, b) => { return b.Count - a.Count; });
            }
            return return_arr;
        }

        #region 换三张提示的三张牌
        public static List<List<CardInfo>> Change3CardsTips(List<CardInfo> all)
        {
            List<List<CardInfo>> return_arr = new List<List<CardInfo>>();
            //拆牌
            /*cardArray
             * [{牌权值13,数量4,具体牌对象List<cardInfo>}, {9,4,List<cardInfo>}... ]           
             */
            List<DividedCard> cardsArray = new List<DividedCard>();
            DividedCard dc = null;
            for (int i = 0; i < all.Count; i++)
            {
                if (dc == null)
                {
                    dc = new DividedCard(all[i].num, all[i]);
                }
                else
                {
                    if (dc.num == all[i].num)
                    {
                        dc.Add(all[i]);
                    }
                    else
                    {
                        cardsArray.Add(dc);
                        dc = new DividedCard(all[i].num, all[i]);
                    }
                }
                //最后一张牌
                if (i == all.Count - 1)
                    cardsArray.Add(dc);
            }

            List<DividedCard> dicArray = FillDicarrWithAll(cardsArray);
            //升序排列 345678910JQKA2wW
            cardsArray.Reverse();
            dicArray.Reverse();

            List<List<CardInfo>> shunzis = FindContinous(dicArray, 1);
            List<List<CardInfo>> lianduis = FindContinous(dicArray, 2);
            //不破坏连对
            for (int i = 0; i < lianduis.Count; i++)
            {
                List<DividedCard> tmp = new List<DividedCard>();
                //tmp.AddRange(cardsArray);
                for (int x = 0; x < cardsArray.Count; x++)
                {
                    DividedCard tmpdc = new DividedCard
                    {
                        count = cardsArray[x].count,
                        num = cardsArray[x].num,
                        cards = new List<CardInfo>()
                    };
                    for (int idx = 0; idx < cardsArray[x].cards.Count; idx++)
                    {
                        tmpdc.cards.Add(DeepCopy(cardsArray[x].cards[idx]));
                    }
                    tmp.Add(tmpdc);
                }
                //remove连对中的牌
                for (int j = 0; j < lianduis[i].Count; j++)
                {
                    for (int index = 0; index < tmp.Count; index++)
                    {
                        if(tmp[index].num == lianduis[i][j].num)
                        {
                            for (int k = 0; k < tmp[index].cards.Count; k++)
                            {
                                if(tmp[index].cards[k].serverNum == lianduis[i][j].serverNum)
                                {
                                    tmp[index].cards.Remove(tmp[index].cards[k]);
                                    tmp[index].count--;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }

                var tmp1 = Dan3Cards(tmp);
                if (tmp1 != null)
                    AddNoRepeatArray(return_arr, tmp1);

                var tmp2 = Dui3Cards(tmp);
                if (tmp2 != null)
                    AddNoRepeatArray(return_arr, tmp2);
            }
            //不破坏顺子
            for (int i = 0; i < shunzis.Count; i++)
            {
                List<DividedCard> tmp = new List<DividedCard>();
                //tmp.AddRange(cardsArray);
                for (int x = 0; x < cardsArray.Count; x++)
                {
                    DividedCard tmpdc = new DividedCard
                    {
                        count = cardsArray[x].count,
                        num = cardsArray[x].num,
                        cards = new List<CardInfo>()
                    };
                    for (int idx = 0; idx < cardsArray[x].cards.Count; idx++)
                    {
                        tmpdc.cards.Add(DeepCopy(cardsArray[x].cards[idx]));
                    }
                    tmp.Add(tmpdc);
                }
                //remove顺子中的牌
                for (int j = 0; j < shunzis[i].Count; j++)
                {
                    for (int index = 0; index < tmp.Count; index++)
                    {
                        if (tmp[index].num == shunzis[i][j].num)
                        {
                            for (int k = 0; k < tmp[index].cards.Count; k++)
                            {
                                if (tmp[index].cards[k].serverNum == shunzis[i][j].serverNum)
                                {
                                    tmp[index].cards.Remove(tmp[index].cards[k]);
                                    tmp[index].count--;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }

                var tmp1 = Dan3Cards(tmp);
                if (tmp1 != null)
                    AddNoRepeatArray(return_arr, tmp1);

                var tmp2 = Dui3Cards(tmp);
                if (tmp2 != null)
                    AddNoRepeatArray(return_arr, tmp2);
            }

            //找不到优解 直接装入最小单牌/单对组合
            if (return_arr.Count == 0)
            {
                var tmp1 = Dan3Cards(cardsArray);
                if (tmp1 != null)
                    return_arr.Add(tmp1);

                var tmp2 = Dui3Cards(cardsArray);
                if (tmp2 != null)
                    return_arr.Add(tmp2);
            }
            return return_arr;
        }

        static void AddNoRepeatArray(List<List<CardInfo>> result, List<CardInfo> oneArr)
        {
            bool found = false;
            for (int i = 0; i < result.Count; i++)
            {
                if(result[i][0].num == oneArr[0].num && result[i][1].num == oneArr[1].num && result[i][2].num == oneArr[2].num)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                result.Add(oneArr);
        }

        static List<CardInfo> Dan3Cards(List<DividedCard> cardsArray)
        {
            List<CardInfo> result = new List<CardInfo>();
            for (int i = 0; i < cardsArray.Count; i++)
            {
                if (cardsArray[i].count == 1 && cardsArray[i].num < 13) //排除2wW
                {
                    result.AddRange(cardsArray[i].cards);
                }
                if (result.Count >= 3) break;
            }
            if (result.Count == 3)
                return result;
            return null;
        }

        static List<CardInfo> Dui3Cards(List<DividedCard> cardsArray)
        {
            List<CardInfo> result = new List<CardInfo>();
            for (int i = 0; i < cardsArray.Count; i++)
            {
                if (cardsArray[i].count == 2 && cardsArray[i].num < 13 && result.Count <= 1) //排除2
                {
                    result.AddRange(cardsArray[i].cards);
                }
                if (cardsArray[i].count == 1 && cardsArray[i].num < 13 && (result.Count % 2 == 0)) //排除2wW
                {
                    result.AddRange(cardsArray[i].cards);
                }

                if (result.Count >= 3) break;
            }
            if (result.Count == 3)
                return result;
            return null;
        }
        #endregion
    }
}

