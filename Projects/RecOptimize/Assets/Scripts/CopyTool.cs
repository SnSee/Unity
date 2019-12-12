using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class CopyTool
{
    public static Dictionary<string, int> CopyStrIntDic(Dictionary<string, int> oriDic)
    {
        Dictionary<string, int> newDic = new Dictionary<string, int>();
        foreach (string key in oriDic.Keys)
        {
            newDic[key] = oriDic[key];
        }
        return newDic;
    }

    public static List<string> CopyStrList(List<string> strList)
    {
        List<string> newStrList = new List<string>();
        foreach(string str in strList)
        {
            newStrList.Add(str);
        }
        return newStrList;
    }

    public static List<List<string>> CopyStrListList(List<List<string>> strLL){
        List<List<string>> newStrLL = new List<List<string>>();
        foreach(List<string> strL in strLL){
            List<string> inList = new List<string>();
            foreach(string str in strL){
                inList.Add(str);
            }
            newStrLL.Add(inList);
        }
        return newStrLL;
    }

    public static List<string> TransStrListToSet(List<string> strList)
    {
        List<string> strSet = new List<string>();
        foreach(string str in strList){
            if(!strSet.Contains(str)){
                strSet.Add(str);
            }
        }
        return strSet;
    }

    // 求交集
    public static List<string> StrListIntersection(List<string> sL1, List<string> sL2){
        List<string> commonList = new List<string>();
        foreach(string s1 in sL1){
            if(sL2.Contains(s1)){
                commonList.Add(s1);
            }
        }
        return commonList;
    }
}

public static class StaticObj
{
    public static StringBuilder sb = new StringBuilder();
}

public class StringTool
{
    public static string TransStrListToStrWithoutSymbol(List<string> strL){
        StringBuilder sb = StaticObj.sb;
        sb.Clear();
        foreach(string str in strL){
            sb.Append(str);
        }
        return sb.ToString();
    }
    public static string TransStrIntDicToStr(Dictionary<string, int> dic)
    {
        StringBuilder sb = StaticObj.sb;
        sb.Clear();
        foreach(string key in dic.Keys){
            for(int i = 0; i < dic[key]; ++i){
                sb.Append(key);
            }
        }
        return sb.ToString();
    }

    public static string TransStrListToStr(List<string> strList)
    {
        StringBuilder sb = new StringBuilder("[");
        for(int i = 0; i < strList.Count; ++i){
            sb.Append(strList[i]);
            if(i != strList.Count - 1){
                sb.Append(", ");
            }
        }
        sb.Append("]");
        return sb.ToString();
    }

    public static string JsonDumpsStrStrDcit(Dictionary<string, string> dict)
    {
        StringBuilder sb = new StringBuilder("{");
        int count = 0;
        foreach(var item in dict){
            string key = item.Key;
            string value = item.Value;
            sb.Append("\"");
            sb.Append(key);
            sb.Append("\"");
            sb.Append(": ");
            sb.Append("\"");
            sb.Append(value);
            sb.Append("\"");
            if(count != dict.Count - 1){
                sb.Append(", ");
            }
            ++count;
        }
        sb.Append("}");
        return sb.ToString();
    }

    public static void SortStrLLL(List<List<List<string>>> strLLL){
        foreach(List<List<string>> strLL in strLLL){
            foreach(List<string> strL in strLL){
                strL.Sort(delegate(string left, string right){
                    return string.Compare(left, right);
                });
            }
            strLL.Sort(delegate(List<string> left, List<string> right){
                return string.Compare(string.Join("-", left), string.Join("-", right));
            });
        }
        strLLL.Sort(delegate(List<List<string>> left, List<List<string>> right){
            string strL = "";
            string strR = "";
            foreach(List<string> l in left){
                strL += string.Join("-", l);
            }
            foreach(List<string> r in right){
                strL += string.Join("-", r);
            }
            return string.Compare(strL, strR);
        });
    }
}

public class PermutationAndCombination<T>
{
    // 调用方法如下：
    //
    // 1.GetPermutation(T[], startIndex, endIndex)
    // 对startIndex到endIndex进行排列，其余元素不变
    //
    // 2.GetPermutation(T[])
    // 返回数组所有元素的全排列
    //
    // 3.GetPermutation(T[], n)
    // 返回数组中n个元素的排列
    //
    // 4.GetCombination(T[], n)
    // 返回数组中n个元素的组合

    /// <summary>
    /// 交换两个变量
    /// </summary>
    /// <param name="a">变量1</param>
    /// <param name="b">变量2</param>
    public static void Swap(ref T a, ref T b)
    {
        T temp = a;
        a = b;
        b = temp;
    }

    /// <summary>
    /// 递归算法求数组的组合(私有成员)
    /// </summary>
    /// <param name="list">返回的范型</param>
    /// <param name="t">所求数组</param>
    /// <param name="n">t数组长度</param>
    /// <param name="m">目标数组元素个数</param>
    /// <param name="b">辅助数组</param>
    /// <param name="M">辅助数组长度</param>
    private static void GetCombination(ref List<T[]> list, T[] t, int n, int m, int[] b, int M)
    {
        for (int i = n; i >= m; i--)
        {
            b[m - 1] = i - 1;
            if (m > 1)
            {
                GetCombination(ref list, t, i - 1, m - 1, b, M);
            }
            else
            {
                if (list == null)
                {
                    list = new List<T[]>();
                }
                T[] temp = new T[M];
                for (int j = 0; j < b.Length; j++)
                {
                    temp[j] = t[b[j]];
                }
                list.Add(temp);
            }
        }
    }

    /// <summary>
    /// 递归算法求排列(私有成员)
    /// </summary>
    /// <param name="list">返回的列表</param>
    /// <param name="t">所求数组</param>
    /// <param name="startIndex">起始标号</param>
    /// <param name="endIndex">结束标号</param>
    private static void GetPermutation(ref List<T[]> list, T[] t, int startIndex, int endIndex)
    {
        if (startIndex == endIndex)
        {
            if (list == null)
            {
                list = new List<T[]>();
            }
            T[] temp = new T[t.Length];
            t.CopyTo(temp, 0);
            list.Add(temp);
        }
        else
        {
            for (int i = startIndex; i <= endIndex; i++)
            {
                Swap(ref t[startIndex], ref t[i]);
                GetPermutation(ref list, t, startIndex + 1, endIndex);
                Swap(ref t[startIndex], ref t[i]);
            }
        }
    }

    /// <summary>
    /// 求从起始标号到结束标号的排列，其余元素不变
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <param name="startIndex">起始标号</param>
    /// <param name="endIndex">结束标号</param>
    /// <returns>从起始标号到结束标号排列的范型</returns>
    public static List<T[]> GetPermutation(T[] t, int startIndex, int endIndex)
    {
        if (startIndex < 0 || endIndex > t.Length - 1)
        {
            return null;
        }
        List<T[]> list = new List<T[]>();
        GetPermutation(ref list, t, startIndex, endIndex);
        return list;
    }

    /// <summary>
    /// 求数组中n个元素的排列
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <param name="n">元素个数</param>
    /// <returns>数组中n个元素的排列</returns>
    public static List<T[]> GetPermutation(T[] t, int n)
    {
        if (n > t.Length)
        {
            return null;
        }
        List<T[]> list = new List<T[]>();
        List<T[]> c = GetCombination(t, n);
        for (int i = 0; i < c.Count; i++)
        {
            List<T[]> l = new List<T[]>();
            GetPermutation(ref l, c[i], 0, n - 1);
            list.AddRange(l);
        }
        return list;
    }


    /// <summary>
    /// 求数组中n个元素的组合
    /// </summary>
    /// <param name="t">所求数组</param>
    /// <param name="n">元素个数</param>
    /// <returns>数组中n个元素的组合的范型</returns>
    public static List<T[]> GetCombination(T[] t, int n)
    {
        if (t.Length < n)
        {
            return null;
        }
        int[] temp = new int[n];
        List<T[]> list = new List<T[]>();
        GetCombination(ref list, t, t.Length, n, temp, n);
        return list;
    }

    public static List<T[]> GetCombinationWithOrder(T[] oriList, int retLength){
        return null;
    }
}

public static class CardRecomDeepCopy
{
    public static object DeepCopy(this object obj)
    {
        Debug.Log("enter");
        System.Object targetDeepCopyObj;
        if (null == obj)
        {
            return null;
        }
        Type targetType = obj.GetType();
        //值类型
        if (targetType.IsValueType == true)
        {
            targetDeepCopyObj = obj;
        }
        //引用类型 
        else
        {
            targetDeepCopyObj = System.Activator.CreateInstance(targetType);   //创建引用对象 
            System.Reflection.MemberInfo[] memberCollection = obj.GetType().GetMembers();

            foreach (System.Reflection.MemberInfo member in memberCollection)
            {
                if (member.MemberType == System.Reflection.MemberTypes.Field)
                {
                    System.Reflection.FieldInfo field = (System.Reflection.FieldInfo)member;
                    System.Object fieldValue = field.GetValue(obj);
                    if (fieldValue is ICloneable)
                    {
                        field.SetValue(targetDeepCopyObj, (fieldValue as ICloneable).Clone());
                    }
                    else
                    {
                        field.SetValue(targetDeepCopyObj, DeepCopy(fieldValue));
                    }

                }
                else if (member.MemberType == System.Reflection.MemberTypes.Property)
                {
                    System.Reflection.PropertyInfo myProperty = (System.Reflection.PropertyInfo)member;
                    MethodInfo info = myProperty.GetSetMethod(false);
                    if (info != null)
                    {
                        object propertyValue = myProperty.GetValue(obj, null);
                        if (propertyValue is ICloneable)
                        {
                            myProperty.SetValue(targetDeepCopyObj, (propertyValue as ICloneable).Clone(), null);
                        }
                        else
                        {
                            myProperty.SetValue(targetDeepCopyObj, DeepCopy(propertyValue), null);
                        }
                    }

                }
            }
        }
        return targetDeepCopyObj;
    }
}

