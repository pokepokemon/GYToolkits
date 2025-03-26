using CodeStage.AntiCheat.Storage;
using GYLib.Utils;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class EditorSaveUtils : Editor
{
    [MenuItem("Assets/存档/删除存档", false, 100) ]
    public static void DeleteSave()
    {
        FileManager.Instance.Delete(SaveProcessor.AUTO_SAVE_PATH, true);
        FileManager.Instance.Delete(BattleReplayUtils.GetReplayFolder(), false);
        string folderPath = FileManager.Instance.GetPath("");
        if (Directory.Exists(folderPath))
        {
            string[] datFiles = Directory.GetFiles(folderPath, "*.dat");
            foreach (string datFile in datFiles)
            {
                File.Delete(datFile);
            }
        }
        PlayerPrefs.DeleteAll();
        ObscuredPrefs.DeleteAll();
        Debug.Log("删除完成");
    }

    [MenuItem("Assets/存档/打开存档目录", false, 102)]
    public static void OpenSaveFolder()
    {
        var itemPath = FileManager.Instance.GetPath(SaveProcessor.AUTO_SAVE_PATH);
        if (!File.Exists(itemPath))
        {
            itemPath = FileManager.Instance.GetPath("");
        }
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    }

    [MenuItem("Assets/存档/解密D盘存档", false, 103)]
    public static void DecodeDDisk()
    {
        if (File.Exists("D:/" + SaveProcessor.AUTO_SAVE_PATH))
        {
            byte[] bytes = File.ReadAllBytes("D:/" + SaveProcessor.AUTO_SAVE_PATH);
            byte[] buffer = new byte[SaveDetailData.FixedLength];
            GYEncryptCenter encrypt = new GYEncryptCenter();
            string rs = ReadStringFromSaveBytes(bytes, buffer);
            rs = encrypt.Decode(rs);
            File.WriteAllText("D:/" + SaveProcessor.AUTO_SAVE_PATH + ".txt", rs);
            File.WriteAllBytes("D:/" + SaveProcessor.AUTO_SAVE_PATH + ".append", buffer);
            Debug.Log("解密完成");
        }
    }

    private static string ReadStringFromSaveBytes(byte[] bytes, byte[] appendBuffer)
    {
        long fileLength = bytes.LongLength;
        // 计算文件的主内容长度（总长度 - 附加缓冲区长度）
        long mainContentLength = fileLength - appendBuffer.Length;

        using (MemoryStream fs = new MemoryStream(bytes))
        {
            // 读取文件的主内容部分为字符串
            using (StreamReader sr = new StreamReader(fs, System.Text.Encoding.UTF8, true, 4096))
            {
                char[] mainContentChars = new char[mainContentLength];
                sr.ReadBlock(mainContentChars, 0, (int)mainContentLength);
                string mainContent = new string(mainContentChars);

                // 读取附加缓冲区部分
                fs.Seek(mainContentLength, SeekOrigin.Begin);
                fs.Read(appendBuffer, 0, appendBuffer.Length);

                return mainContent;
            }
        }
    }

    [MenuItem("Assets/存档/加密D盘存档", false, 104)]
    public static void EncodeDDisk()
    {
        string fullPath = "D:/" + SaveProcessor.AUTO_SAVE_PATH;
        if (File.Exists(fullPath + ".txt") && File.Exists(fullPath + ".append"))
        {
            GYEncryptCenter encrypt = new GYEncryptCenter();
            string rs = File.ReadAllText(fullPath + ".txt");
            rs = encrypt.Encode(rs);
            byte[] appendBytes = File.ReadAllBytes(fullPath + ".append");
            WriteSaveData(fullPath, rs, appendBytes);
            File.WriteAllText(fullPath, rs);
            Debug.Log("加密完成");
        }
    }

    private static void WriteSaveData(string fullPath, string content, byte[] appendBytes)
    {
        byte[] preBytes = UTF8Encoding.UTF8.GetBytes(content);
        // 打开文件句柄（覆盖写入模式）
        using (FileStream fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            fs.Write(preBytes);
            // 如果有附加字节数据，追加到文件末尾
            if (appendBytes != null && appendBytes.Length > 0)
            {
                fs.Write(appendBytes);
            }
        }
    }
}
