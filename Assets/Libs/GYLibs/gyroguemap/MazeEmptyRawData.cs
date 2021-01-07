using UnityEngine;
using System.Collections;
using System.IO;

public class MazeEmptyRawData
{
    public int rawWidth;

    public int rawHeight;

    public int width { private set; get; }

    public int height { private set; get; }

    private int boundXIndex;
    private int boundYIndex;

    public byte[,] tileDatas;

    public void InitTiles()
    {
        CalcSize();
        GenerateDefaultData();
    }

    /// <summary>
    /// 根据RawSize设置宽高边界
    /// </summary>
    private void CalcSize()
    {
        width = rawWidth * 2 + 1;
        height = rawHeight * 2 + 1;
        boundXIndex = width - 1;
        boundYIndex = height - 1;
        tileDatas = new byte[width, height];
    }

    /// <summary>
    /// 生成默认块数据(网格状)
    /// </summary>
    private void GenerateDefaultData()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                tileDatas[x, y] = MazeTiledConst.VOID;
            }
        }
    }

    /// <summary>
    /// 是否为边界点
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool IsBound(int x, int y)
    {
        return x == 0 || x == boundXIndex || y == 0 || y == boundYIndex;
    }

    /// <summary>
    /// 出界
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public bool OutOfBound(int x, int y)
    {
        return x < 0 || x > boundXIndex || y < 0 || y > boundYIndex;
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <returns></returns>
    public byte[] ToBytes()
    {
        MemoryStream ms = new MemoryStream();
        using (BinaryWriter writer = new BinaryWriter(ms))
        {
            writer.Write(rawWidth);
            writer.Write(rawHeight);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    writer.Write(tileDatas[x, y]);
                }
            }
        }
        byte[] bytes = ms.ToArray();
        ms.Close();
        return bytes;
    }

    /// <summary>
    /// 反序列化
    /// </summary>
    /// <param name="bytes"></param>
    public void InitByBytes(byte[] bytes)
    {
        MemoryStream ms = new MemoryStream(bytes);
        using (BinaryReader reader = new BinaryReader(ms))
        {
            rawWidth = reader.ReadInt32();
            rawHeight = reader.ReadInt32();
            CalcSize();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    tileDatas[x, y] = reader.ReadByte();
                }
            }
        }
        ms.Close();
    }
}
