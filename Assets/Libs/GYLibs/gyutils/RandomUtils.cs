using GYLibs.gyutils.random;
using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomUtils
{
    static Dictionary<int, int> _seedDict = new Dictionary<int, int>();
    static List<int> _list = new List<int>();
    static int counter = 0;
    static MersenneTwister _twister;

    public static void SetSeed(int seed, int pool = 0)
    {
        counter = 0;
        _list.Clear();
        _seedDict[pool] = seed;
        _twister = _twister ?? new MersenneTwister((uint)seed);
        if (pool == 0)
        {
            UnityEngine.Random.InitState(seed);
        }
    }

    public static int GetSeed(int pool = 0)
    {
        return _seedDict[pool];
    }

    public static void RandomSeed(int pool)
    {
        int tmpSeed = _seedDict[pool];
        _seedDict[pool] = _twister.Next(0, 100000);// (((tmpSeed * 214013 + 2531011) >> 16) & 0x7fff) % 100000;
        counter++;
    }

    /// <summary>
    /// 从0到range,包括0
    /// </summary>
    /// <param name="range"></param>
    /// <param name="pool"></param>
    /// <returns></returns>
    public static int RandomUint(int range, int pool = 0)
    {
        int rate = 1;
        if (range < 0)
        {
            rate = -1;
        }
        else if (range == 0)
            return 0;
        int tmpSeed = _seedDict[pool];
        UnityEngine.Random.InitState(tmpSeed);
        RandomSeed(pool);
        return Convert.ToInt32(UnityEngine.Random.Range(0, range)) * rate;
        /*
        int mod = tmpSeed % 20;
        int count = 1;
        for (int i = 0; i < mod; i++)
        {
            count += count * tmpSeed * i;
            if (count > range)
            {
                count = count % range;
            }
        }
        
        return count;*/
    }

    public static float Range(float min, float max, int pool = 0)
    {
        int tmpSeed = _seedDict[pool];
        UnityEngine.Random.InitState(tmpSeed);
        RandomSeed(pool);
        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    ///  Return a random int within [minInclusive..maxExclusive)
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="pool"></param>
    /// <returns></returns>
    public static int RangeInt(int min, int max, int pool = 0)
    {
        int tmpSeed = _seedDict[pool];
        UnityEngine.Random.InitState(tmpSeed);
        RandomSeed(pool);
        return UnityEngine.Random.Range(min, max);
    }

    public static Vector2 RangePointInCircle(int pool = 0)
    {
        int tmpSeed = _seedDict[pool];
        UnityEngine.Random.InitState(tmpSeed);
        RandomSeed(pool);
        return UnityEngine.Random.insideUnitCircle;
    }

    /// <summary>
    /// Random array index
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public static int RandomIndex(int count)
    {
        if (count <= 1)
        {
            return 0;
        }
        int index = Mathf.FloorToInt(UnityEngine.Random.value * count);
        if (index >= count || index < 0)
        {
            index = count - 1;
        }
        return index;
    }

    /// <summary>
    /// 根据ID组的权重随机
    /// </summary>
    /// <param name="randomItemArr"></param>
    /// <param name="randomProbArr"></param>
    /// <returns></returns>
    public static int? GetRandomItemInArray(int[] randomItemArr, int[] randomProbArr)
    {
        int? rsIndex = GetRandomIndexInArray(randomItemArr, randomProbArr);
        if (rsIndex != null)
        {
            return randomItemArr[rsIndex.Value];
        }
        else
        {
            return null;
        }
    }


    /// <summary>
    /// 根据ID组的权重随机索引
    /// </summary>
    /// <param name="randomItemArr"></param>
    /// <param name="randomProbArr"></param>
    /// <returns></returns>
    public static int? GetRandomIndexInArray(int[] randomItemArr, int[] randomProbArr)
    {
        if (randomItemArr == null || randomProbArr == null ||
            randomItemArr.Length == 0 ||
            randomItemArr.Length > randomProbArr.Length)
        {
            return null;
        }
        long totalWeight = 0;
        for (int i = 0; i < randomItemArr.Length; i++)
        {
            totalWeight += randomProbArr[i];
        }

        float resultWeight = totalWeight * RandomUtils.Range(0, 1);
        float curWeight = 0;
        int rndWordIndex = randomItemArr.Length - 1;
        for (int i = 0; i < randomItemArr.Length; i++)
        {
            curWeight += randomProbArr[i];
            if (resultWeight <= curWeight)
            {
                rndWordIndex = i;
                break;
            }
        }
        return rndWordIndex;
    }
}