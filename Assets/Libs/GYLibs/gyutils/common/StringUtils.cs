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
    public static int[] GetIntArguments(string effect, char[] splitKey)
    {
        if (!string.IsNullOrEmpty(effect))
        {
            string[] strArr = effect.Split(splitKey, StringSplitOptions.RemoveEmptyEntries);
            if (strArr.Length > 1)
            {
                int[] intArr = new int[strArr.Length - 1];
                for (int i = 1; i < strArr.Length; i++)
                {
                    intArr[i - 1] = Convert.ToInt32(strArr[i]);
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
    public static float[] GetFloatArguments(string effect, char[] splitKey)
    {
        if (!string.IsNullOrEmpty(effect))
        {
            string[] strArr = effect.Split(splitKey, StringSplitOptions.RemoveEmptyEntries);
            if (strArr.Length > 1)
            {
                float[] arr = new float[strArr.Length - 1];
                for (int i = 1; i < strArr.Length; i++)
                {
                    arr[i - 1] = Convert.ToSingle(strArr[i]);
                }
                return arr;
            }
        }
        return null;
    }
}
