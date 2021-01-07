using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// 随机数获取管道
/// </summary>
public class RandomPipeline
{
    private int _RANDOM_COUNT = 100;
    private int _nextSeed;
    private Queue<double> _queue;
    private System.Random _random;
    
    public RandomPipeline(int seed)
    {
        _queue = new Queue<double>();
        _nextSeed = seed;
        _random = new System.Random(seed);
    }

    public void ResetSeed(int seed)
    {
        _nextSeed = seed;
        _random = new System.Random(_nextSeed);
        _queue.Clear();
    }

    public double Next()
    {
        if (_queue.Count <= 0)
        {
            for (int i = 0; i < _RANDOM_COUNT; i++)
            {
                _queue.Enqueue(_random.NextDouble());
            }
        }
        return _queue.Dequeue();
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
