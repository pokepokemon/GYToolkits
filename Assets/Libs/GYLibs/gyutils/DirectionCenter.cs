using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DirectionCenter
{
    public static readonly DirectionCenter instance = new DirectionCenter();

    private float[] _angleRangeMin = { -22.5f, 22.5f, 67.5f, 112.5f, 157.5f, -157.5f, -112.5f, -67.5f, };
    private float[] _angleRangeMax = { 22.5f, 67.5f, 112.5f, 157.5f, -157.5f, -112.5f, -67.5f, -22.5f, };
    

    private int[] _angleRangeDir = { 6, 9, 8, 7, 4, 1, 2, 3 };

    private float[] _angleRangeMin4 = { -45f, 45f, 135f, -135f };
    private float[] _angleRangeMax4 = { 45f, 135f, -135f, -45f };

    private int[] _angleRangeDir4 = { 6, 8, 4, 2 };

    private Dictionary<int, float> _dirOffsetAngle;
    private Dictionary<int, int> _dirSmallIndex;
    private Dictionary<int, int> _dirLargeIndex;
    private HashSet<int> _crossDirSet;

    public DirectionCenter()
    {
        _dirOffsetAngle = new Dictionary<int, float>();
        _dirSmallIndex = new Dictionary<int, int>();
        _dirLargeIndex = new Dictionary<int, int>();
        _crossDirSet = new HashSet<int>();

        for (int i = 0; i < _angleRangeDir.Length; i++)
        {
            _dirOffsetAngle.Add(_angleRangeDir[i], _angleRangeMin[i]);
        }
        _crossDirSet.Add(1);
        _crossDirSet.Add(3);
        _crossDirSet.Add(9);
        _crossDirSet.Add(7);

        _dirSmallIndex.Add(9, 6);
        _dirSmallIndex.Add(7, 8);
        _dirSmallIndex.Add(1, 4);
        _dirSmallIndex.Add(3, 2);

        _dirLargeIndex.Add(9, 8);
        _dirLargeIndex.Add(7, 4);
        _dirLargeIndex.Add(1, 2);
        _dirLargeIndex.Add(3, 6);
    }

    /// <summary>
    /// 获取方向(4方向)
    /// </summary>
    /// <param name="angle"></param>
    /// <returns></returns>
    public int GetDir4(float angle)
    {
        for (int i = 0; i < 4; i++)
        {
            if (i == 2 && (angle >= _angleRangeMin4[i] || angle <= _angleRangeMax4[i]))
            {
                return _angleRangeDir4[i];
            }
            else if (angle >= _angleRangeMin4[i] && angle <= _angleRangeMax4[i])
            {
                return _angleRangeDir4[i];
            }
        }
        return -1;

    }

    /// <summary>
    /// 获取方向(8方向)
    /// </summary>
    /// <param name="angle"> -180 ~ 180 </param>
    /// <returns></returns>
    public int GetDir(float angle)
    {
        for (int i = 0; i < 8; i++)
        {
            if (i == 4 && (angle >= _angleRangeMin[i] || angle <= _angleRangeMax[i]))
            {
                return _angleRangeDir[i];
            }
            else if (angle >= _angleRangeMin[i] && angle <= _angleRangeMax[i])
            {
                return _angleRangeDir[i];
            }
        }
        return -1;
    }
    
    /// <summary>
    /// 获取正方向的方向
    /// </summary>
    /// <param name="isNearly"></param>
    /// <returns></returns>
    public int GetVHDir(int currentDir, float currentAngle, bool isNearly)
    {
        if (_dirSmallIndex.ContainsKey(currentDir))
        {
            if (currentDir != -1)
            {
                float deltaAngle = currentAngle - _dirOffsetAngle[currentDir];
                if (deltaAngle <= 22.5)
                {
                    return isNearly ? _dirSmallIndex[currentDir] : _dirLargeIndex[currentDir];
                }
                else
                {
                    return isNearly ? _dirLargeIndex[currentDir] : _dirSmallIndex[currentDir];
                }
            }
        }
        return currentDir;
    }

    /// <summary>
    /// 是否为斜线
    /// </summary>
    /// <param name="dir"></param>
    /// <returns></returns>
    public bool IsCrossDir(int dir)
    {
        return _crossDirSet.Contains(dir);
    }

    /// <summary>
    /// 获取随机圆环上的点
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public Vector2 GetRandomCircleRange(float min, float max)
    {
        Vector2 rndCircle = Random.insideUnitCircle;
        return rndCircle.normalized * (Random.value * (max - min) + min);
    }
}
