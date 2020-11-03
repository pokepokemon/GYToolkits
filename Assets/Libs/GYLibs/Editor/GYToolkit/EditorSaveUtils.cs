using CodeStage.AntiCheat.Storage;
using GYLib.Utils;
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
        FileManager.Instance.Delete(SaveProcessor.SAVE_PATH, true);
        PlayerPrefs.DeleteAll();
        ObscuredPrefs.DeleteAll();
        Debug.Log("删除完成");
    }

    [MenuItem("Assets/存档/解密D盘存档")]
    public static void DecodeDDisk()
    {
        if (File.Exists("D:/" + SaveProcessor.SAVE_PATH))
        {
            GYEncryptCenter encrypt = new GYEncryptCenter();
            string rs = File.ReadAllText("D:/" + SaveProcessor.SAVE_PATH);
            rs = encrypt.Decode(rs);
            File.WriteAllText("D:/" + SaveProcessor.SAVE_PATH + ".txt", rs);
            Debug.Log("解密完成");
        }
    }


    [MenuItem("Assets/存档/加密D盘存档")]
    public static void EncodeDDisk()
    {
        if (File.Exists("D:/" + SaveProcessor.SAVE_PATH + ".txt"))
        {
            GYEncryptCenter encrypt = new GYEncryptCenter();
            string rs = File.ReadAllText("D:/" + SaveProcessor.SAVE_PATH + ".txt");
            rs = encrypt.Encode(rs);
            File.WriteAllText("D:/" + SaveProcessor.SAVE_PATH, rs);
            Debug.Log("加密完成");
        }
    }
}
