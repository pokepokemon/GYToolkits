﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class RandomUtils
{
    static Dictionary<int, int> _seedDict = new Dictionary<int, int>();
    static List<int> _list = new List<int>();
    static int counter = 0;

    public static void SetSeed(int seed, int pool = 0)
    {
        counter = 0;
        _list.Clear();
        _seedDict[pool] = seed;
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
        _seedDict[pool] = (((tmpSeed * 214013 + 2531011) >> 16) & 0x7fff) % 100000;
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
}