using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.Utils
{
    public class AStar
    {
        public const byte FREE = 1;
        public const byte BLOCK = 2;

        public const byte REASON_NONE = 0;
        public const byte REASON_OUTSIDE = 2;
        public const byte REASON_BLOCK = 1;
        public const byte REASON_DELAY = 3;
        public const byte REASON_CLOSESET = 4;
        public const byte REASON_EXIST = 5;

        public bool isDir8 = false;

        private int _width;
        private int _height;
        private byte[,] _map;
        private AStarNode[,] _openMap;
        private bool[,] _closeMap;

        private MinHeap<AStarNode> _openTree = new MinHeap<AStarNode>();
        private Stack _resultStack = new Stack();
        private List<AStarNode> _closeSet = new List<AStarNode>();
        private Queue<AStarNode> _freePool = new Queue<AStarNode>();

        private int _sourceX;
        private int _sourceY;
        private int _targetX;
        private int _targetY;
        private AStarNode _targetNode;

        public void SetMap(int width, int height, byte[,] map)
        {
            _width = width;
            _height = height;
            _map = map;

            _openMap = new AStarNode[width, height];
            _closeMap = new bool[width, height];
        }

        /// <summary>
        /// 清理上次寻路留下的数据
        /// </summary>
        private void ClearResult()
        {
            _targetNode = null;
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _openMap[x, y] = null;
                    _closeMap[x, y] = false;
                }
            }
            foreach (var node in _openTree)
            {
                if (node != null)
                {
                    _freePool.Enqueue(node);
                }
            }
            foreach (var node in _closeSet)
            {
                _freePool.Enqueue(node);
            }
            _openTree.Clear();
            _resultStack.Clear();
            _closeSet.Clear();
        }

        /// <summary>
        /// 设置某块地的阻挡状态
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="status">AStar.FREE/BLOCK</param>
        public void SetMapStatus(int x, int y, byte status)
        {
            _map[x, y] = status;
        }

        private List<Vector2Int> _resultBuffer = new List<Vector2Int>();
        public Vector2Int[] FindPath(int sourceX, int sourceY, int targetX, int targetY)
        {
            ClearResult();
            _sourceX = sourceX;
            _sourceY = sourceY;
            _targetX = targetX;
            _targetY = targetY;
            byte reason = REASON_NONE;
            AStarNode node = AddNode(sourceX, sourceY, null, out reason);
            if (node != null)
            {
                _openTree.Add(node);
                _openMap[node.x, node.y] = node;

                while (_openTree.Count > 0)
                {
                    node = _openTree.Extract();
                    _openMap[node.x, node.y] = null;
                    _closeMap[node.x, node.y] = true;
                    _closeSet.Add(node);

                    int x = node.x;
                    int y = node.y;
                    if (AddNodeToOpenSet(x + 1, y, node)
                        || AddNodeToOpenSet(x - 1, y, node)
                        || (isDir8 && AddNodeToOpenSet(x + 1, y + 1, node))
                        || (isDir8 && AddNodeToOpenSet(x + 1, y - 1, node))
                        || AddNodeToOpenSet(x, y + 1, node)
                        || AddNodeToOpenSet(x, y - 1, node)
                        || (isDir8 && AddNodeToOpenSet(x - 1, y + 1, node))
                        || (isDir8 && AddNodeToOpenSet(x - 1, y - 1, node)))
                    {
                        _resultBuffer.Clear();
                        AStarNode tmpNode = _targetNode;
                        while (tmpNode != null)
                        {
                            _resultBuffer.Add(new Vector2Int(tmpNode.x, tmpNode.y));
                            tmpNode = tmpNode.parent;
                        }
                        _resultBuffer.Reverse();
                        return _resultBuffer.ToArray();
                    }
                }

            }

            return null;
        }

        private static bool _isFirst = true;
        /// <summary>
        /// 失败后获取最短的路径
        /// </summary>
        /// <returns></returns>
        public Vector2Int[] GetCloserPath()
        {
            if (_closeSet != null && _closeSet.Count > 0)
            {
                _closeSet.Sort((x, y) =>
                ((x.x - _targetX) * (x.x - _targetX) + (x.y - _targetY) * (x.y - _targetY)) -
                ((y.x - _targetX) * (y.x - _targetX) + (y.y - _targetY) * (y.y - _targetY)));
                AStarNode tmpNode = null;
                for (int i = 0; i < _closeSet.Count; i++)
                {
                    tmpNode = _closeSet[i];
                    if (tmpNode.F == 0)
                    {
                        tmpNode = null;
                    }
                    else
                    {
                        break;
                    }
                }
                while (tmpNode != null)
                {
                    _resultBuffer.Add(new Vector2Int(tmpNode.x, tmpNode.y));
                    tmpNode = tmpNode.parent;
                }
                _resultBuffer.Reverse();
                if (_resultBuffer.Count > 0 && (_resultBuffer[0].x != _sourceX || _resultBuffer[0].y != _sourceY))
                {
                    return null;
                }
                else
                {
                    return _resultBuffer.ToArray();
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="srcNode"></param>
        /// <returns>TargetPoint or null</returns>
        private bool AddNodeToOpenSet(int x, int y, AStarNode srcNode)
        {
            byte reason = REASON_NONE;
            AStarNode node = AddNode(x, y, srcNode, out reason);
            if (node != null)
            {
                if (node.x == _targetX && node.y == _targetY)
                {
                    _targetNode = node;
                    return true;
                }

                _openTree.Add(node);
                _openMap[node.x, node.y] = node;
            }
            return false;
        }

        private AStarNode AddNode(int x, int y, AStarNode srcNode, out byte reason)
        {
            if (x >= 0 && x < _width && y >= 0 && y < _height)
            {
                if (_closeMap[x, y])
                {
                    reason = REASON_CLOSESET;
                    return null;
                }
                if (_map[x, y] == AStar.BLOCK && !((x == _sourceX && y == _sourceY) || (x == _targetX && y == _targetY)))
                {
                    reason = REASON_BLOCK;
                    return null;
                }
                AStarNode tmpNode = _openMap[x, y];
                if (tmpNode != null)
                {
                    if (srcNode != null && tmpNode.parent != null)
                    {
                        if (srcNode.G < tmpNode.parent.G)
                        {
                            //Recalc
                            bool hMove = x != srcNode.x;
                            bool vMove = y != srcNode.y;
                            tmpNode.G = srcNode.G + ((hMove && vMove) ? 14 : 10);
                            tmpNode.Calc();
                            tmpNode.parent = srcNode;

                            _openTree.Remove(tmpNode);
                            _openTree.Add(tmpNode);
                        }
                    }

                    reason = REASON_EXIST;
                    return null;
                }
                if (isDir8 && srcNode != null)
                {
                    //绊脚
                    if (((srcNode.x == x - 1 || srcNode.x == x + 1) && _map[x, srcNode.y] == AStar.BLOCK) ||
                        ((srcNode.y == y - 1 || srcNode.x == y + 1) && _map[srcNode.x, y] == AStar.BLOCK))
                    {
                        reason = REASON_DELAY;
                        return null;
                    }
                }

                AStarNode newNode = null;
                if (_freePool.Count > 0)
                {
                    newNode = _freePool.Dequeue();
                }
                else
                {
                    newNode = new AStarNode();
                }
                newNode.x = x;
                newNode.y = y;
                newNode.status = _map[x, y];
                newNode.parent = srcNode;

                if (srcNode == null)
                {
                    newNode.G = 0;
                    newNode.H = 0;
                }
                else
                {
                    int deltaX = x - _targetX;
                    int deltaY = y - _targetY;
                    //Manhattan
                    newNode.H = ((deltaX > 0 ? deltaX : -deltaX) + (deltaY > 0 ? deltaY : -deltaY)) * 10;

                    bool hMove = x != srcNode.x;
                    bool vMove = y != srcNode.y;
                    newNode.G = srcNode.G + ((hMove && vMove) ? 14 : 10);
                }
                newNode.Calc();

                reason = REASON_NONE;
                return newNode;
            }
            reason = REASON_OUTSIDE;
            return null;
        }
    }
}