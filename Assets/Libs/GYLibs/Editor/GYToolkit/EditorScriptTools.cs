using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// 编辑器下的脚本工具
/// </summary>
public class EditorScriptTools
{
    public static void SaveString(string filePath, string content)
    {
        try
        {
            CheckAndCreateDoc(filePath);
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.Write(content);
            }
        }
        catch (System.Exception e)
        {
            return;
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
        catch (System.Exception ex)
        {
            UnityEngine.Debug.Log("ReadFile error : " + ex.ToString());
            return "";
        }
    }
}
