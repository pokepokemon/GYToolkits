using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 坐标转换相关类
/// </summary>
public class MazeCoordinate
{
    public const float cellSizeX = 1;
    public const float cellSizeY = 1;

    public const int roomGridWidth = 50;
    public const int roomGridHeight = 50;

    public static int roomRealGridX = Mathf.FloorToInt((roomGridWidth - 1) / 2) * 2 + 1;
    public static int roomRealGridY = Mathf.FloorToInt((roomGridHeight - 1) / 2) * 2 + 1;

    /// <summary>
    /// 转换格子坐标到世界坐标
    /// 原点 左下
    /// </summary>
    /// <param name="localPos"></param>
    /// <returns></returns>
    public static Vector3 TileToWorld(Vector2Int localPos)
    {
        return new Vector3(
            localPos.x * cellSizeX,
            localPos.y * cellSizeY,
            0);
    }

    /// <summary>
    /// 转换世界坐标到格子坐标
    /// 原点 左下
    /// </summary>
    /// <param name="localPos"></param>
    /// <returns></returns>
    public static Vector3Int WorldToTile(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.FloorToInt(worldPos.x / cellSizeX),
            Mathf.FloorToInt(worldPos.y / cellSizeY),
            0);
    }

    /// <summary>
    /// 通过房间内的相对Tile坐标获得房间内的块的全局Tile坐标
    /// </summary>
    /// <param name="roomX"></param>
    /// <param name="roomY"></param>
    /// <param name="relativeX"></param>
    /// <param name="relativeY"></param>
    /// <returns></returns>
    public static Vector2Int GetGlobalTilePos(int roomX, int roomY, int relativeX, int relativeY)
    {
        return new Vector2Int(roomX * roomRealGridX + relativeX, roomY * roomRealGridY + relativeY);
    }

    /// <summary>
    /// 通过全局Tile坐标获取房间内的相对Tile坐标
    /// </summary>
    /// <param name="roomX"></param>
    /// <param name="roomY"></param>
    /// <param name="globalX"></param>
    /// <param name="globalY"></param>
    /// <returns></returns>
    public static Vector2Int GetLocalTilePos(int roomX, int roomY, int globalX, int globalY)
    {
        return new Vector2Int(globalX - roomX * roomRealGridX, globalY - roomY * roomRealGridY);
    }

    /// <summary>
    /// 转换世界坐标到房间坐标
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector2Int WorldToRoom(float x, float y)
    {
        return new Vector2Int(
            Mathf.FloorToInt(x / (cellSizeX * roomRealGridX)),
            Mathf.FloorToInt(y / (cellSizeY * roomRealGridY))
            );
    }

    /// <summary>
    /// 转换格子坐标到房间坐标
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public static Vector2Int TileToRoom(int x, int y)
    {
        return new Vector2Int(
            Mathf.FloorToInt(x / roomRealGridX),
            Mathf.FloorToInt(y / roomRealGridY)
            );
    }

    /// <summary>
    /// 获取房间位置
    /// </summary>
    /// <param name="roomX"></param>
    /// <param name="roomY"></param>
    /// <returns></returns>
    public static Vector3 GetRoomPosition(int roomX, int roomY)
    {
        return new Vector3(
            roomX * cellSizeX * roomRealGridX,
            roomY * cellSizeY * roomRealGridY,
            0);
    }

    private static List<Vector2Int> _showBufferList = new List<Vector2Int>();
    /// <summary>
    /// 获取应当展示的范围地块
    /// </summary>
    /// <param name="cam"></param>
    /// <returns></returns>
    public static Vector2Int[] GetCamRangeArea(Camera cam)
    {
        var transform = cam.transform;
        var orthoSize = cam.orthographicSize;
        var width = orthoSize * cam.aspect;
        float lb_x = transform.position.x - width;
        float lb_y = transform.position.y - orthoSize;

        float rt_x = transform.position.x + width;
        float rt_y = transform.position.y + orthoSize;

        Vector2Int lbPos = WorldToRoom(lb_x, lb_y);
        Vector2Int rtPos = WorldToRoom(rt_x, rt_y);

        _showBufferList.Clear();
        int i = 0;
        for (int x = lbPos.x - 1; x <= rtPos.x + 1; x++)
        {
            for (int y = lbPos.y - 1; y <= rtPos.y + 1; y++)
            {
                if (x >= 0 && y >= 0)
                {
                    _showBufferList.Add(new Vector2Int(x, y));
                }
            }
        }
        return _showBufferList.ToArray();
    }
}

