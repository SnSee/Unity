using System.Collections.Generic;

namespace Tuyoo
{
    public class CardCommon
    {
        private const int LAIZI_COLOR = 9;

        //List<int>类型转换为List<CardInfo>类型，即获取无序牌列
        public static List<CardInfo> ConvertCardData2CardInfo(List<int> cardList)
        {
            List<CardInfo> ci = new List<CardInfo>();
            int total = cardList.Count;
            for (int i = 0; i < total; i++)
            {
                ci.Add(GenCardInfo(cardList[i]));
            }
            return ci;
        }

        //获取有序牌列
        public static List<CardInfo> GetCardListSorted(List<CardInfo> cardList, bool myHand = false)
        {
            if (null == cardList)
            {
                return null;
            }
            int total = cardList.Count;
            if (myHand)
            {
                // 手牌癞子最大
                for (int j = 0; j < total; j++)
                {
                    for (int k = 0; k < total - j - 1; k++)
                    {
                        if (!CompareMyHandCards(cardList[k], cardList[k + 1]))
                        {
                            CardInfo tmp = cardList[k + 1];
                            cardList[k + 1] = cardList[k];
                            cardList[k] = tmp;
                        }
                    }
                }
                return cardList;
            }
            //纸牌大到小
            for (int j = 0; j < total; j++)
            {
                for (int k = 0; k < total - j - 1; k++)
                {
                    if (!CompareCards(cardList[k], cardList[k + 1]))
                    {
                        CardInfo tmp = cardList[k + 1];
                        cardList[k + 1] = cardList[k];
                        cardList[k] = tmp;
                    }
                }
            }
            return cardList;
        }

        //获取有序牌列
        public static List<CardInfo> GetCardListSorted(List<int> cardList, bool myHand = false)
        {
            if (null == cardList)
            {
                return null;
            }
            List<CardInfo> ci = new List<CardInfo>();
            int total = cardList.Count;
            for (int i = 0; i < total; i++)
            {
                ci.Add(GenCardInfo(cardList[i]));
            }

            return GetCardListSorted(ci, myHand);
        }

        // cardInfo转severNum的List
        private List<int> CardListChangeToSeverNumList(List<CardInfo> cardInfos)
        {
            List<int> listData = new List<int>();
            for (int i = 0; i < cardInfos.Count; i++)
            {
                listData.Add(cardInfos[i].serverNum);
            }
            return listData;
        }

        //校正牌列组合
        public static List<CardInfo> FixedCardListSorted(List<CardInfo> srcCardInfo)
        {
            //3带1
            if (srcCardInfo.Count == 4)
            {
                if (srcCardInfo[0].num != srcCardInfo[1].num && srcCardInfo[2].num == srcCardInfo[3].num)
                {
                    CardInfo tmp = srcCardInfo[0];
                    srcCardInfo.RemoveAt(0);
                    srcCardInfo.Add(tmp);
                }
            }
            //3带对
            if (srcCardInfo.Count == 5)
            {
                if (srcCardInfo[0].num == srcCardInfo[1].num && srcCardInfo[2].num == srcCardInfo[3].num && srcCardInfo[0].num != srcCardInfo[2].num)
                {
                    CardInfo tmp1 = srcCardInfo[0];
                    CardInfo tmp2 = srcCardInfo[1];
                    srcCardInfo.RemoveRange(0, 2);
                    srcCardInfo.Add(tmp1);
                    srcCardInfo.Add(tmp2);
                }
            }
            //4带2
            if (srcCardInfo.Count == 6)
            {
                if (srcCardInfo[1].num == srcCardInfo[4].num && srcCardInfo[2].num == srcCardInfo[3].num && srcCardInfo[1].num == srcCardInfo[3].num && srcCardInfo[0].num != srcCardInfo[1].num)
                {
                    CardInfo tmp = srcCardInfo[0];
                    srcCardInfo.RemoveAt(0);
                    srcCardInfo.Insert(4, tmp);
                }
                if (srcCardInfo[2].num == srcCardInfo[5].num && srcCardInfo[3].num == srcCardInfo[4].num && srcCardInfo[2].num == srcCardInfo[3].num && srcCardInfo[0].num != srcCardInfo[2].num)
                {
                    CardInfo tmp1 = srcCardInfo[0];
                    CardInfo tmp2 = srcCardInfo[1];
                    srcCardInfo.RemoveRange(0, 2);
                    srcCardInfo.Add(tmp1);
                    srcCardInfo.Add(tmp2);
                }
            }
            //4带2对(不处理带炸弹类飞机）->输出aaaabbcc
            if (srcCardInfo.Count == 8)
            {
                //直接返回aaaabbcc
                if (srcCardInfo[0].num == srcCardInfo[3].num && srcCardInfo[1].num == srcCardInfo[2].num && srcCardInfo[0].num == srcCardInfo[1].num && srcCardInfo[0].num != srcCardInfo[4].num
                    && srcCardInfo[0].num != srcCardInfo[6].num && srcCardInfo[4].num != srcCardInfo[6].num && srcCardInfo[4].num == srcCardInfo[5].num && srcCardInfo[6].num == srcCardInfo[7].num)
                {
                    return srcCardInfo;
                }
                //直接返回aaaabbbb a!=b+1
                if (srcCardInfo[0].num == srcCardInfo[3].num && srcCardInfo[1].num == srcCardInfo[2].num && srcCardInfo[0].num == srcCardInfo[1].num && srcCardInfo[0].num != srcCardInfo[4].num + 1
                     && srcCardInfo[4].num == srcCardInfo[6].num && srcCardInfo[4].num == srcCardInfo[5].num && srcCardInfo[6].num == srcCardInfo[7].num)
                {
                    return srcCardInfo;
                }
                //aabbbbcc
                if (srcCardInfo[2].num == srcCardInfo[5].num && srcCardInfo[3].num == srcCardInfo[4].num && srcCardInfo[2].num == srcCardInfo[3].num && srcCardInfo[0].num != srcCardInfo[2].num
                    && srcCardInfo[0].num != srcCardInfo[6].num && srcCardInfo[2].num != srcCardInfo[6].num && srcCardInfo[0].num == srcCardInfo[1].num && srcCardInfo[6].num == srcCardInfo[7].num)
                {
                    CardInfo tmp1 = srcCardInfo[0];
                    CardInfo tmp2 = srcCardInfo[1];
                    srcCardInfo.RemoveRange(0, 2);
                    srcCardInfo.Insert(4, tmp1);
                    srcCardInfo.Insert(5, tmp2);
                    return srcCardInfo;
                }
                //aabbcccc
                if (srcCardInfo[4].num == srcCardInfo[7].num && srcCardInfo[5].num == srcCardInfo[6].num && srcCardInfo[4].num == srcCardInfo[5].num && srcCardInfo[0].num != srcCardInfo[4].num
                    && srcCardInfo[0].num != srcCardInfo[2].num && srcCardInfo[2].num != srcCardInfo[4].num && srcCardInfo[0].num == srcCardInfo[1].num && srcCardInfo[2].num == srcCardInfo[3].num)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        CardInfo tmp = srcCardInfo[i];
                        srcCardInfo[i] = srcCardInfo[i + 4];
                        srcCardInfo[i + 4] = tmp;
                    }
                    return srcCardInfo;
                }
            }
            //飞机(3顺默认正序)
            /*传入[J9998885]
             * onetime[J985]
             * twotime[98]
             * threetime[98]
             * 输出[999888J5]
             */
            if (srcCardInfo.Count > 7)
            {
                List<CardInfo> onetime = new List<CardInfo>(); //出现1次
                List<CardInfo> twotime = new List<CardInfo>(); //出现2次
                List<CardInfo> threetime = new List<CardInfo>();//出现3次

                CardInfo tmp1 = null;
                CardInfo tmp2 = null;
                foreach (var item in srcCardInfo)
                {
                    if (null == tmp1)
                    {
                        tmp1 = item;
                        onetime.Add(item);
                        continue;
                    }
                    if (item.num == tmp1.num)
                    {
                        if (null == tmp2)
                        {
                            tmp2 = item;
                            twotime.Add(item);
                            continue;
                        }
                        if (item.num == tmp2.num)
                        {
                            threetime.Add(item);
                            continue;
                        }
                    }
                    else
                    {
                        onetime.Add(item);
                        tmp1 = item;
                        tmp2 = null;
                    }
                }

                //正序牌
                List<CardInfo> fix = new List<CardInfo>();
                //先装三张
                for (int idx = 0; idx < threetime.Count; idx++)
                {
                    bool found = false;
                    int val = threetime[idx].num;
                    foreach (var item in onetime)
                    {
                        if (item.num == val)
                        {
                            found = true;
                            fix.Add(item);
                            onetime.Remove(item);
                            break;
                        }
                    }
                    foreach (var item in twotime)
                    {
                        if (item.num == val)
                        {
                            found = true;
                            fix.Add(item);
                            twotime.Remove(item);
                            break;
                        }
                    }
                    //特殊处理 AAAABBBB之类的牌型 这样的threetime会流入2次
                    if (found)
                        fix.Add(threetime[idx]);
                    else
                        onetime.Add(threetime[idx]);
                }
                //再装对子
                for (int idx = 0; idx < twotime.Count; idx++)
                {
                    int val = twotime[idx].num;
                    foreach (var item in onetime)
                    {
                        if (item.num == val)
                        {
                            fix.Add(item);
                            onetime.Remove(item);
                            break;
                        }
                    }
                    fix.Add(twotime[idx]);
                }

                //重新排序单牌后合并单牌
                for (int j = 0; j < onetime.Count; j++)
                {
                    for (int k = 0; k < onetime.Count - j - 1; k++)
                    {
                        if (!CompareCards(onetime[k], onetime[k + 1]))
                        {
                            CardInfo tmp = onetime[k + 1];
                            onetime[k + 1] = onetime[k];
                            onetime[k] = tmp;
                        }
                    }
                }
                fix.AddRange(onetime);

                return fix;
            }
            return srcCardInfo;
        }

        //服务器牌转客户端
        private static CardInfo GenCardInfo(int serverNum)
        {
            CardInfo ci = new CardInfo();
            ci.serverNum = serverNum;
            //先看看是不是癞子转换
            if (ci.serverNum > 53)
            {
                ci.laizi = true;
                //换成实质3 - A，2
                ci.num = serverNum - 55 > 0 ? serverNum - 55 : serverNum - 55 + 13;
                ci.color = LAIZI_COLOR;
                ci.showCard = LaiziNumberToChar(ci.serverNum - 53);
                //todo
                ci.showDiCard = "";
                return ci;
            }
            //是不是我本局手牌的癞子
            // if (TableDataMgr.Instance.TableData.MyLaiziCards.ContainsKey(ci.serverNum))
            // {
            //     ci.laizi = true;
            //     ci.num = serverNum > 51 ? serverNum : serverNum % 13 - 1;
            //     if (ci.num <= 0)
            //         ci.num += 13;
            //     ci.color = LAIZI_COLOR;
            //     int tmp = ci.serverNum > 25 ? ci.serverNum - 25 : ci.serverNum + 1;
            //     tmp = tmp > 13 ? tmp - 13 : tmp;
            //     ci.showCard = LaiziNumberToChar(tmp);//变成a-m
            // }
            ci.laizi = false;
            ci.num = serverNum > 51 ? serverNum : serverNum % 13 - 1;
            if (ci.num <= 0)
                ci.num += 13;
            ci.color = serverNum / 13;
            int transNum = ci.serverNum > 25 ? ci.serverNum - 25 : ci.serverNum + 1;
            ci.showCard = NumberToChar(transNum);
            //大小王位图
            if (ci.serverNum > 51)
            {
                ci.showCard = (ci.serverNum - 48).ToString();
                ci.showDiCard = (ci.serverNum - 46).ToString();
            }
            return ci;
        }

        //确定癞子
        public static CardInfo ChangeToLaiZi(CardInfo ci)
        {
            ci.laizi = true;
            ci.color = LAIZI_COLOR;
            int transNum = ci.serverNum > 25 ? ci.serverNum - 25 : ci.serverNum + 1;
            transNum = transNum > 13 ? transNum - 13 : transNum;
            ci.showCard = LaiziNumberToChar(transNum);//变成a-m
            return ci;
        }

        private static string LaiziNumberToChar(int number)
        {
            if (1 <= number && 26 >= number)
            {
                int num = number + 96;
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] btNumber = { (byte)num };
                return asciiEncoding.GetString(btNumber);
            }
            return "数字不在转换范围内";
        }

        private static string NumberToChar(int number)
        {
            if (1 <= number && 26 >= number)
            {
                int num = number + 64;
                System.Text.ASCIIEncoding asciiEncoding = new System.Text.ASCIIEncoding();
                byte[] btNumber = { (byte)num };
                return asciiEncoding.GetString(btNumber);
            }
            return "数字不在转换范围内";
        }

        //比较两张卡牌大小 癞子的默认小
        private static bool CompareCards(CardInfo ci1, CardInfo ci2)
        {
            if (ci1.num != ci2.num)
                return ci1.num > ci2.num;
            if (ci1.color == LAIZI_COLOR)
                return false;
            if (ci2.color == LAIZI_COLOR)
                return true;
            return ci1.color > ci2.color;
        }

        private static bool CompareMyHandCards(CardInfo ci1, CardInfo ci2)
        {
            if (ci1.color == LAIZI_COLOR)
                return true;
            if (ci2.color == LAIZI_COLOR)
                return false;
            if (ci1.num != ci2.num)
                return ci1.num > ci2.num;
            return ci1.color > ci2.color;
        }

        //几炸计算
        public static int CountZhaNum(List<CardInfo> cardInfos)
        {
            int zhaNum = 0;
            bool haveBlackJoker = false;
            bool haveRedJoker = false;
            for (int i = 1; i < 14; i++)
            {
                int pokerKindNum = 0;
                for (int j = 0; j < cardInfos.Count; j++)
                {
                    int number = cardInfos[j].num;
                    if (number == i)
                    {
                        pokerKindNum += 1;
                        if (pokerKindNum == 4)
                        {
                            zhaNum++;
                        }
                    }
                    else if (GameCommon.IsBlackJoker(number))
                    {
                        haveBlackJoker = true;
                    }
                    else if (GameCommon.IsRedJoker(number))
                    {
                        haveRedJoker = true;
                    }
                }
            }
            if (haveBlackJoker && haveRedJoker)
            {
                zhaNum++;
            }

            return zhaNum;
        }

    }
}
