using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 随机数获取管道
/// </summary>
public class RandomPipeline
{
    private int _seedRange = 100000;
    private int _RANDOM_COUNT = 100;
    public int nextSeed { private set; get; }
    private Queue<double> _queue;
    private System.Random _random;

    private int _curCount;
    
    public RandomPipeline(int seed)
    {
        _queue = new Queue<double>();
        nextSeed = seed;
    }

    public void ResetSeed(int seed)
    {
        nextSeed = seed;
        _queue.Clear();
    }


    public double Next()
    {
        if (_queue.Count <= 0)
        {
            _random = new System.Random(nextSeed);
            for (int i = 0; i < _RANDOM_COUNT; i++)
            {
                _queue.Enqueue(_random.NextDouble());
            }
        }

        var result = _queue.Dequeue();
        nextSeed = Convert.ToInt32(Math.Ceiling(result * _seedRange));
        return result;
    }

    /// <summary>
    /// 替代RandomUtils,获取索引
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public int RandomIndex(int count)
    {
        if (count <= 1)
        {
            return 0;
        }
        int index = Convert.ToInt32(System.Math.Floor(Next() * count));
        if (index >= count)
        {
            index = count - 1;
        }
        return index;
    }
}
