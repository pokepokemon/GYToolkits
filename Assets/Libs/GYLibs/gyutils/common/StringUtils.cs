using UnityEngine;
using System.Collections;
using System;

public class StringUtils
{
    /// <summary>
    /// |
    /// </summary>
    public static char[] splitKey = new char[] { '|' };
    /// <summary>
    /// ,
    /// </summary>
    public static char[] splitKey2 = new char[] { ',' };

    /// <summary>
    /// 用指定key分割字符串
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="splitKey"></param>
    /// <returns></returns>
    public static string[] GetArgument(string arguments, char[] splitKey)
    {
        if (!string.IsNullOrEmpty(arguments))
        {
            string[] strArr = arguments.Split(splitKey, StringSplitOptions.RemoveEmptyEntries);
            return strArr;
        }
        return null;
    }

    /// <summary>
    /// 分割字符串获取整数参数数组
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public static int[] GetIntArguments(string effect, char[] splitKey, bool discardFirst = false)
    {
        if (!string.IsNullOrEmpty(effect))
        {
            string[] strArr = effect.Split(splitKey, StringSplitOptions.RemoveEmptyEntries);
            if (strArr.Length >= 1 || (discardFirst && strArr.Length > 1))
            {
                int targetLen = discardFirst ? strArr.Length - 1 : strArr.Length;
                int[] intArr = new int[targetLen];
                for (int i = discardFirst ? 1 : 0; i < strArr.Length; i++)
                {
                    intArr[discardFirst ? i - 1 : i] = Convert.ToInt32(strArr[i]);
                }
                return intArr;
            }
        }
        return null;
    }

    /// <summary>
    /// 分割字符串获取单精度数参数数组
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    public static float[] GetFloatArguments(string effect, char[] splitKey, bool discardFirst = false)
    {
        if (!string.IsNullOrEmpty(effect))
        {
            string[] strArr = effect.Split(splitKey, StringSplitOptions.RemoveEmptyEntries);
            if (strArr.Length >= 1 || (discardFirst && strArr.Length > 1))
            {
                int targetLen = discardFirst ? strArr.Length - 1 : strArr.Length;
                float[] arr = new float[targetLen];
                for (int i = discardFirst ? 1 : 0; i < strArr.Length; i++)
                {
                    arr[discardFirst ? i - 1 : i] = Convert.ToSingle(strArr[i]);
                }
                return arr;
            }
        }
        return null;
    }
}
