using GYLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorEncryptConfigSwitch : Editor
{
    private const string _ENCRYPT_FILE_PATH = "Assets/3rdLibs/GYLibs/gyframe/FrameSettings.cs";
    private const string _CONFIG_PATH = "Assets/Resources/config/";

    [MenuItem("Assets/GYTools/Enable or Disable Encrypt")]
    /// <summary>
    /// 获取全路径
    /// </summary>
    public static void SwitchConfig()
    {
        int status = GetEncryptStatus();
        if (status == -1)
        {
            Debug.Log("Encrypt status Error! Check File!");
        }
        else if (status == 0)
        {
            DoEncrypt();
        }
        else if (status == 1)
        {
            DoDecrypt();
        }
    }

    /// <summary>
    /// 加密
    /// </summary>
    public static void DoEncrypt()
    {
        string scriptPath = Application.dataPath + _ENCRYPT_FILE_PATH.Substring("Assets".Length);
        string dirPath = Application.dataPath + FrameSettings.ReplaceEncodePath(_CONFIG_PATH).Substring("Assets".Length);
        string dirOriginPath = Application.dataPath + _CONFIG_PATH.Substring("Assets".Length);

        if (Directory.Exists(dirPath))
        {
            Directory.Delete(dirPath, true);
        }

        string[] strArr = Directory.GetFiles(dirOriginPath, "*.json.txt", SearchOption.AllDirectories);
        GYEncryptCenter center = new GYEncryptCenter();
        for (int i = 0; i < strArr.Length; i++)
        {
            strArr[i] = strArr[i].Replace("\\", "/");
            string resultCode = EditorScriptTools.ReadFile(strArr[i]);
            string encodeCode = center.Encode(resultCode);
            EditorScriptTools.SaveString(FrameSettings.ReplaceEncodePath(strArr[i]), encodeCode);
        }

        //symbol
        string frameFileStr = EditorScriptTools.ReadFile(scriptPath);
        string newFileStr = frameFileStr.Replace("public const bool enable = false", "public const bool enable = true");
        EditorScriptTools.SaveString(scriptPath, newFileStr);

        //删掉多余文件
        if (Directory.Exists(dirOriginPath))
        {
            Directory.Delete(dirOriginPath, true);
        }
        Debug.Log("Encrypt Completed!");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 解密
    /// </summary>
    public static void DoDecrypt()
    {
        string scriptPath = Application.dataPath + _ENCRYPT_FILE_PATH.Substring("Assets".Length);
        string dirPath = Application.dataPath + FrameSettings.ReplaceEncodePath(_CONFIG_PATH).Substring("Assets".Length);
        string dirOriginPath = Application.dataPath + _CONFIG_PATH.Substring("Assets".Length);

        if (Directory.Exists(dirOriginPath))
        {
            Directory.Delete(dirOriginPath, true);
        }

        string[] strArr = Directory.GetFiles(dirPath, "*.json.txt", SearchOption.AllDirectories);
        GYEncryptCenter center = new GYEncryptCenter();
        for (int i = 0; i < strArr.Length; i++)
        {
            strArr[i] = strArr[i].Replace("\\", "/");
            string resultCode = EditorScriptTools.ReadFile(strArr[i]);
            string decodeCode = center.Decode(resultCode);

            EditorScriptTools.SaveString(FrameSettings.ReplaceDecodePath(strArr[i]), decodeCode);
        }

        //symbol
        string frameFileStr = EditorScriptTools.ReadFile(scriptPath);
        string newFileStr = frameFileStr.Replace("public const bool enable = true", "public const bool enable = false");
        EditorScriptTools.SaveString(scriptPath, newFileStr);

        //删掉多余文件
        if (Directory.Exists(dirPath))
        {
            Directory.Delete(dirPath, true);
        }
        Debug.Log("Decrypt Completed!");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取当前加密状态
    /// </summary>
    /// <returns>-1 : 异常状态 0 : 未加密 1 : 加密</returns>
    private static int GetEncryptStatus()
    {
        string filePath = Application.dataPath + _ENCRYPT_FILE_PATH.Substring("Assets".Length);
        string dirPath = Application.dataPath + FrameSettings.ReplaceEncodePath(_CONFIG_PATH).Substring("Assets".Length);
        string dirOriginPath = Application.dataPath + _CONFIG_PATH.Substring("Assets".Length);

        if (File.Exists(filePath) && (Directory.Exists(dirPath) || Directory.Exists(dirOriginPath)))
        {
            string frameFileStr = EditorScriptTools.ReadFile(filePath);
            bool fileEnable = frameFileStr.IndexOf(@"public const bool enable = false") == -1 ? true : false;
            bool dirEnable = Directory.Exists(dirPath);

            if (fileEnable != dirEnable)
            {
                return -1;
            }
            else
            {
                return fileEnable ? 1 : 0;
            }
        }
        else
        {
            Debug.Log("Config File Not Exist!");
            return -1;
        }
    }
}
