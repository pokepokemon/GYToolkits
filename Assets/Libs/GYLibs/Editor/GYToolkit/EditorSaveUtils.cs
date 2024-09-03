using CodeStage.AntiCheat.Storage;
using GYLib.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorSaveUtils : Editor
{
    [MenuItem("Assets/存档/删除存档")]
    public static void DeleteSave()
    {
        FileManager.Instance.Delete(SaveProcessor.AUTO_SAVE_PATH, true);
        PlayerPrefs.DeleteAll();
        ObscuredPrefs.DeleteAll();
        Debug.Log("删除完成");
    }

    [MenuItem("Assets/存档/打开存档目录")]
    public static void OpenSaveFolder()
    {
        var itemPath = FileManager.Instance.GetPath(SaveProcessor.AUTO_SAVE_PATH);
        itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
        System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    }

    [MenuItem("Assets/存档/解密D盘存档")]
    public static void DecodeDDisk()
    {
        if (File.Exists("D:/" + SaveProcessor.AUTO_SAVE_PATH))
        {
            GYEncryptCenter encrypt = new GYEncryptCenter();
            string rs = File.ReadAllText("D:/" + SaveProcessor.AUTO_SAVE_PATH);
            rs = encrypt.Decode(rs);
            File.WriteAllText("D:/" + SaveProcessor.AUTO_SAVE_PATH + ".txt", rs);
            Debug.Log("解密完成");
        }
    }

    [MenuItem("Assets/存档/解密D盘存档Json")]
    public static void DecodeDDiskJson()
    {
        if (File.Exists("D:/" + SaveProcessor.AUTO_SAVE_PATH))
        {
            GYEncryptCenter encrypt = new GYEncryptCenter();
            string rs = File.ReadAllText("D:/" + SaveProcessor.AUTO_SAVE_PATH);
            rs = encrypt.Decode(rs);
            rs = rs.Replace("\\\"", "\"");
            rs = rs.Replace(":\"{", "{");
            rs = rs.Replace("}\"", "}");

            File.WriteAllText("D:/" + SaveProcessor.AUTO_SAVE_PATH + "_json.txt", rs);
            Debug.Log("解密完成");
        }
    }


    [MenuItem("Assets/存档/加密D盘存档")]
    public static void EncodeDDisk()
    {
        if (File.Exists("D:/" + SaveProcessor.AUTO_SAVE_PATH + ".txt"))
        {
            GYEncryptCenter encrypt = new GYEncryptCenter();
            string rs = File.ReadAllText("D:/" + SaveProcessor.AUTO_SAVE_PATH + ".txt");
            rs = encrypt.Encode(rs);
            File.WriteAllText("D:/" + SaveProcessor.AUTO_SAVE_PATH, rs);
            Debug.Log("加密完成");
        }
    }
}
