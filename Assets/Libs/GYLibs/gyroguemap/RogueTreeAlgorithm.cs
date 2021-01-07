using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 简单的随机迷宫生成算法
/// </summary>
public class RogueTreeAlgorithm
{
    public MazeRoomRawData maze;

    int _digBlock = 0;

    Vector2Int _currentDigPos;
    HashSet<string> _openSet = new HashSet<string>();

    /// <summary>
    /// 挖了即会产生环路的边集合
    /// </summary>
    List<Vector2Int> _crossEdgeList = new List<Vector2Int>();
    
    /// <summary>
    /// 可挖掘的障碍点开集
    /// </summary>
    List<Vector2Int> _edges = new List<Vector2Int>();

    private const string POS_KEY_TEMPLATE = "{0}_{1}";

    public bool IsDigOnePathFinish = false;

    /// <summary>
    /// 初始化起始点
    /// </summary>
    public void SetStartPos()
    {
        IsDigOnePathFinish = false;
        _openSet.Clear();
        _edges.Clear();
        _crossEdgeList.Clear();

        //当前step的目标选点
        _currentDigPos = new Vector2Int() { x = 1, y = 1 };
        _digBlock = 0;
    }

    /// <summary>
    /// 初始化可挖的边集合
    /// </summary>
    public void InitCrossEdge()
    {
        for (int y = 0; y < maze.height; y++)
        {
            for (int x = 0; x < maze.width; x++)
            {
                if (((x % 2 == 0 && y % 2 == 1) || (x % 2 == 1 && y % 2 == 0)) && maze.tileDatas[x, y] == MazeTiledConst.BLOCK && !maze.IsBound(x, y))
                {
                    _crossEdgeList.Add(new Vector2Int { x = x, y = y });
                }
            }
        }
    }

    /// <summary>
    /// 从可挖掘列表中移除障碍
    /// </summary>
    /// <param name="list"></param>
    /// <param name="pos"></param>
    private void RemoveEdge(ref List<Vector2Int> list, Vector2Int pos)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].x == pos.x && list[i].y == pos.y)
            {
                list.RemoveAt(i);
            }
        }
    }

    private void AddEdge(ref List<Vector2Int> list, HashSet<string> openSet, Vector2Int pos)
    {
        //在地图边缘不参与挖
        //横穿已有空间,不挖
        if (!maze.IsBound(pos.x, pos.y) && !IsCrossMySpace(openSet, pos))
        {
            list.Add(pos);
        }
    }

    /// <summary>
    /// 是否会产生环路
    /// </summary>
    /// <param name="openSet"></param>
    /// <param name="pos"></param>
    private bool IsCrossMySpace(HashSet<string> openSet, Vector2Int pos)
    {
        //向该边缘点试探 下一格是否是
        if (pos.x % 2 == 0 && pos.y % 2 == 1)
        {
            //纵边
            var leftKey = string.Format(POS_KEY_TEMPLATE, pos.x - 1, pos.y);
            var rightKey = string.Format(POS_KEY_TEMPLATE, pos.x + 1,pos.y);
            if (openSet.Contains(leftKey) && openSet.Contains(rightKey))
            {
                return true;
            }
        }
        else
        {
            //横边
            var upKey = string.Format(POS_KEY_TEMPLATE, pos.x, pos.y + 1);
            var downKey = string.Format(POS_KEY_TEMPLATE, pos.x, pos.y - 1);
            if (openSet.Contains(upKey) && openSet.Contains(downKey))
            {
                return true;
            }
        }
        return false;
    }

    private Vector2Int GetNextTargetPos(HashSet<string> openSet, Vector2Int digPos)
    {
        if (digPos.x % 2 == 0 && digPos.y % 2 == 1)
        {
            //纵边
            var leftKey = string.Format(POS_KEY_TEMPLATE, digPos.x - 1, digPos.y);
            if (openSet.Contains(leftKey))
            {
                return new Vector2Int { x = digPos.x + 1, y = digPos.y };
            }
            else
            {
                return new Vector2Int { x = digPos.x - 1, y = digPos.y };
            }
        }
        else
        {
            //横边
            var upKey = string.Format(POS_KEY_TEMPLATE, digPos.x, digPos.y + 1);
            if (openSet.Contains(upKey))
            {
                return new Vector2Int { x = digPos.x, y = digPos.y - 1 };
            }
            else
            {
                return new Vector2Int { x = digPos.x, y = digPos.y + 1 };
            }
        }
    }

    /// <summary>
    /// 整理并丢弃可能自循环的边缘
    /// </summary>
    /// <param name="openSet"></param>
    /// <param name="list"></param>
    private List<Vector2Int> removeListBuffer = new List<Vector2Int>();
    private void RemoveAllCrossEdge(HashSet<string> openSet, ref List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (IsCrossMySpace(openSet, list[i]))
            {
                removeListBuffer.Add(list[i]);
            }
        }
        for (int i = 0; i < removeListBuffer.Count; i++)
        {
            RemoveEdge(ref list, removeListBuffer[i]);
        }
        if (removeListBuffer.Count > 0)
        {
            removeListBuffer.Clear();
        }
    }

    /// <summary>
    /// 开始挖掘单向路
    /// </summary>
    public void StartDigStep()
    {
        if (IsDigOnePathFinish)
            return;

        _openSet.Add(string.Format(POS_KEY_TEMPLATE, _currentDigPos.x, _currentDigPos.y));

        RemoveAllCrossEdge(_openSet, ref _edges);
        AddEdge(ref _edges, _openSet, new Vector2Int { x = _currentDigPos.x + 1, y = _currentDigPos.y });
        AddEdge(ref _edges, _openSet, new Vector2Int { x = _currentDigPos.x - 1, y = _currentDigPos.y });
        AddEdge(ref _edges, _openSet, new Vector2Int { x = _currentDigPos.x, y = _currentDigPos.y + 1 });
        AddEdge(ref _edges, _openSet, new Vector2Int { x = _currentDigPos.x, y = _currentDigPos.y - 1 });

        if (!IsDigOnePathFinish && _edges.Count == 0)
        {
            IsDigOnePathFinish = true;
            return;
        }

        //随机从可挖掘障碍抽取
        int digIndex = ShareRogueDatas.random.RandomIndex(_edges.Count);
        Vector2Int digPos = _edges[digIndex];
        maze.tileDatas[digPos.x, digPos.y] = MazeTiledConst.NONE;

        //移除挖掉的障碍块
        RemoveEdge(ref _edges, digPos);

        //获取下一个开集中的点
        _currentDigPos = GetNextTargetPos(_openSet, digPos);

        //步数统计
        _digBlock++;
    }

    /// <summary>
    /// 追加挖掘障碍制造环路
    /// </summary>
    public void StartDigLoopEdge()
    {
        if (_crossEdgeList.Count > 0)
        {
            int digIndex = ShareRogueDatas.random.RandomIndex(_crossEdgeList.Count);
            Vector2Int digPos = _crossEdgeList[digIndex];
            maze.tileDatas[digPos.x, digPos.y] = MazeTiledConst.NONE;
            _crossEdgeList.RemoveAt(digIndex);
        }
    }
}
