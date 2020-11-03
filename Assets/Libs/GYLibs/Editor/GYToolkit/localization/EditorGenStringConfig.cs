using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class EditorGenStringConfig : Editor {
    private const string _STRING_CONFIG_PATH = "Assets/Resources/config/localization.json.txt";
    private static Dictionary<string, string> _local2EnMap;

    [MenuItem("Assets/GYTools/打印选中中文字符串")]
    public static void GenerateChineseConfig()
    {
        HashSet<string> mainMap = new HashSet<string>();
        ParseCSharpEntry(mainMap);
        ParsePrefabEntry(mainMap);
        ParseJsonEntry(mainMap);
        LoadLocalConfig();
        PrintStringConfig(mainMap);
    }

    /// <summary>
    /// 检测选中的文件夹里的所有C#文件 
    /// </summary>
    public static void ParseCSharpEntry(HashSet<string> mainMap)
    {
        string selectPath = GetSelectionPath();
        
        if (!string.IsNullOrEmpty(selectPath))
        {

            string fullPath = selectPath;
            if (selectPath.StartsWith("Assets"))
            {
                fullPath = Application.dataPath + fullPath.Substring("Assets".Length);
            }
            string[] strArr = Directory.GetFiles(fullPath, "*.cs", SearchOption.AllDirectories);
            for (int i = 0; i < strArr.Length; i++)
            {
                strArr[i] = strArr[i].Replace("\\", "/");
                if (strArr[i].IndexOf("/Editor/") != -1 || strArr[i].IndexOf("Assets/Library") != -1)
                {
                    continue;
                }
                string resultCode = ReadFile(strArr[i]);
                SimpleParseCSharp cs = new SimpleParseCSharp(resultCode, mainMap);
                cs.ParseCode();
                if (i % 10 == 0)
                {
                    System.GC.Collect();
                }
            }
        }
    }

    /// <summary>
    /// 扫描选中的文件夹里的所有Json文件路径
    /// </summary>
    public static void ParseJsonEntry(HashSet<string> mainMap)
    {
        string selectPath = GetSelectionPath();

        if (!string.IsNullOrEmpty(selectPath))
        {

            string fullPath = selectPath;
            if (selectPath.StartsWith("Assets"))
            {
                fullPath = Application.dataPath + fullPath.Substring("Assets".Length);
            }
            string[] strArr = Directory.GetFiles(fullPath, "*.json.txt", SearchOption.AllDirectories);
            for (int i = 0; i < strArr.Length; i++)
            {
                if (strArr[i].Contains("localization.json") || strArr[i].Contains("global.json"))
                {
                    continue;
                }
                strArr[i] = strArr[i].Replace("\\", "/");
                string resultCode = ReadFile(strArr[i]);
                SimpleParseCSharp cs = new SimpleParseCSharp(resultCode, mainMap);
                cs.ParseCode();
                if (i % 10 == 0)
                {
                    System.GC.Collect();
                }
            }
        }
    }

    /// <summary>
    /// 扫描选中的文件夹里的所有Prefab文件路径
    /// </summary>
    public static void ParsePrefabEntry(HashSet<string> mainMap)
    {
        string selectPath = GetSelectionPath();

        if (!string.IsNullOrEmpty(selectPath))
        {

            string fullPath = selectPath;
            if (selectPath.StartsWith("Assets"))
            {
                fullPath = Application.dataPath + fullPath.Substring("Assets".Length);
            }
            string[] strArr = Directory.GetFiles(fullPath, "*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < strArr.Length; i++)
            {
                strArr[i] = strArr[i].Replace("\\", "/");
                if (strArr[i].IndexOf("/Editor/") != -1 || strArr[i].IndexOf("Assets/Library") != -1)
                {
                    continue;
                }
                string tmpStr = strArr[i].Replace(Application.dataPath, "Assets");
                GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(tmpStr);
                if (go != null)
                {
                    Text[] textArr = go.GetComponentsInChildren<Text>(true);
                    if (textArr != null)
                    {
                        for (int j = 0; j < textArr.Length; j++)
                        {
                            string tmpKey = textArr[j].text;
                            if (!string.IsNullOrEmpty(tmpKey) && !mainMap.Contains(tmpKey))
                            {
                                mainMap.Add(tmpKey);
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 加载本地已有配置
    /// </summary>
    public static void LoadLocalConfig()
    {
        _local2EnMap = new Dictionary<string, string>();
        List<LocalizationConfigData> localConfigs;
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(_STRING_CONFIG_PATH);
        if (textAsset != null)
        {
            localConfigs = JsonMapper.ToObject<List<LocalizationConfigData>>(textAsset.text);
        }
        else
        {
            localConfigs = new List<LocalizationConfigData>();
        }
        foreach (var cfg in localConfigs)
        {
            if (!string.IsNullOrEmpty(cfg.text))
            {
                _local2EnMap[cfg.src_string] = cfg.text;
            }
        }
    }

    public static void PrintStringConfig(HashSet<string> resultSet)
    {
        StringBuilder sb = new StringBuilder();
        StringBuilder sbNoTranslate = new StringBuilder();
        StringBuilder sbTotal = new StringBuilder();
        string tmpStr;
        foreach (var key in resultSet)
        {
            tmpStr = key.Replace("\n", "\\n");
            tmpStr = key.Replace("\r\n", "\\r\\n");
            bool containChinese = false; ;
            for (int i = 0; i < tmpStr.Length; i++)
            {
                if (char.GetUnicodeCategory(tmpStr[i]) == UnicodeCategory.OtherLetter)
                {
                    containChinese = true;
                    break;
                }
            }
            if (containChinese)
            {
                sb.AppendLine(tmpStr);
                if (!_local2EnMap.ContainsKey(tmpStr))
                {
                    sbNoTranslate.AppendLine(tmpStr);
                    sbTotal.AppendLine(tmpStr);
                }
                else
                {
                    sbTotal.AppendLine(tmpStr + "\t" + _local2EnMap[tmpStr]);
                }
            }
        }
        SaveString(sb.ToString(), Application.dataPath + "/Resources/config/localize_word.txt");
        SaveString(sbNoTranslate.ToString(), Application.dataPath + "/Resources/config/localize_no_translate.txt");
        SaveString(sbTotal.ToString(), Application.dataPath + "/Resources/config/localize_all.txt");
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 获取鼠标选中的文件夹路径
    /// </summary>
    /// <returns></returns>
    public static string GetSelectionPath()
    {
        string path = "Assets";

        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
                break;
            }
        }
        return path;
    }


    /// <summary>
    /// 保存文本
    /// </summary>
    /// <param name="target"></param>
    /// <param name="savePath"></param>
    public static void SaveString(string target, string savePath)
    {
        FileStream fs = null;
        try
        {
            CheckAndCreateDoc(savePath);
            string path = savePath;
            path = path.Replace("\\", "/");
            fs = File.Create(path);
            var Tbytes = System.Text.Encoding.UTF8.GetBytes(target);
            fs.Write(Tbytes, 0, Tbytes.Length);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }
        catch (Exception e)
        {
            if (fs != null)
                fs.Close();
            Debug.Log("build file list error : " + e.ToString());
        }
    }


    /// <summary>
    /// 检查文件夹是否存在并创建
    /// </summary>
    /// <param name="sPath"></param>
    public static void CheckAndCreateDoc(string sPath)
    {
        string doc_path = Path.GetDirectoryName(sPath);
        if (!Directory.Exists(doc_path))
        {
            Directory.CreateDirectory(doc_path);
        }
    }

    /// <summary>
    /// 读取文件
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ReadFile(string path)
    {
        try
        {
            using (StreamReader sr = new StreamReader(path))
            {
                return sr.ReadToEnd();
            }
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log("ReadFile error : " + ex.ToString());
            return "";
        }
    }
}
