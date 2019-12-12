/***
 * 类名称：GameCommon
 * 类功能：与游戏相关的通用方法
***/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Tuyoo
{
    public class GameCommon
    {
        public static bool IsJoker(int cardNum)
        {
            return IsRedJoker(cardNum) || IsBlackJoker(cardNum);
        }

        public static bool IsRedJoker(int cardNum)
        {
            return cardNum == 53;
        }

        public static bool IsBlackJoker(int cardNum)
        {
            return cardNum == 52;
        }
    }
}
