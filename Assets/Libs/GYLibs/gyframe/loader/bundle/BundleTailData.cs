using System;
using System.IO;
using System.Text;
using UnityEngine;

public class BundleTailData
{

    public const string Salt = "DoNotCrackIt";
    public const int FixedLength = 100; // 固定尾部长度
    public const int MarkerLength = 12; // 验证标记长度

    public ulong jsonLength;

    /// <summary>
    /// 创建存档详细数据
    /// </summary>
    /// <param name="tailData"></param>
    /// <param name="skipIndex">需要跳过的长度</param>
    /// <returns></returns>
    public static BundleTailData CreateFromBytes(byte[] tailData, int skipIndex)
    {
        BundleTailData detailData = null;
        using (MemoryStream stream = new MemoryStream(tailData))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                detailData = new BundleTailData();
                reader.ReadBytes(skipIndex);
                // 解析等级和时间戳
                detailData.jsonLength = reader.ReadUInt64();
            }
        }

        return detailData;
    }

    public byte[] BuildByteData()
    {
        // 创建尾部固定数据
        byte[] tailData = new byte[FixedLength];
        using (MemoryStream stream = new MemoryStream(tailData))
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                // 设置验证标记
                Encoding.UTF8.GetBytes(Salt, 0, MarkerLength, tailData, 0);

                writer.Seek(MarkerLength, SeekOrigin.Begin);
                writer.Write(BitConverter.GetBytes(jsonLength));
            }
        }
        return tailData;
    }

    /// <summary>
    /// 检查存档文件合法性
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static bool CheckFileData(string filePath, out byte[] tailData)
    {
        const int fixedLength = FixedLength; // 固定尾部长度
        const int markerLength = MarkerLength; // 验证标记长度
        const string marker = Salt; // 验证标记

        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            if (fs.Length < fixedLength)
            {
                Debug.LogError("File length not enough!");
                tailData = null;
                return false;
            }

            // 定位到尾部固定数据的位置
            fs.Seek(-fixedLength, SeekOrigin.End);

            // 读取固定数据
            tailData = new byte[fixedLength];
            fs.Read(tailData, 0, fixedLength);

            // 验证标记
            string fileMarker = Encoding.UTF8.GetString(tailData, 0, markerLength);
            if (fileMarker != marker)
            {
                Debug.LogError("Save read failed!");
                return false;
            }

            return true;
        }
    }
}
