using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GYLib;
using System.IO;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// 文件操作管理
/// 文件路径开头不需要有/
/// </summary>
public class FileManager {
    public static readonly FileManager Instance = new FileManager();

    private string _prefix = "";
    /// <summary>
    /// 末尾没有/
    /// </summary>
    private string _filePath = Application.persistentDataPath + "/";

    /// <summary>
    /// 设置前缀目录（可用于存档）
    /// </summary>
    /// <param name="prefix">结尾都要有/</param>
    public void SetPrefix(string prefix)
    {
        _prefix = prefix;
    }

    /// <summary>
    /// 删除某 文件/文件夹
    /// </summary>
    /// <param name="path"></param>
    /// <param name="needPrefix"></param>
    /// <param name="isFile"></param>
    public void Delete(string path, bool isFile = false)
    {
        bool isExist = false;
        string fullPath = GetPath(path);
        if (isFile)
            isExist = File.Exists(fullPath);
        else
            isExist = Directory.Exists(fullPath);

        if (isExist)
        {
            if (isFile)
                File.Delete(fullPath);
            else
                Directory.Delete(fullPath, true);
        }
    }

    /// <summary>
    /// 判断是否存在目录或文件（可用于存档）
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool Exist(string path, bool isFile = false)
    {
        string fullPath = GetPath(path);
        if (isFile)
            return File.Exists(fullPath);
        else
            return Directory.Exists(fullPath);
    }

    /// <summary>
    /// 创建目录,以/开头
    /// </summary>
    /// <param name="path"></param>
    public void CreateFolder(string path)
    {
        string fullPath = GetPath(path);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    /// <summary>
    /// 覆盖保存
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="bytes"></param>
    public void Save(string fileName, byte[] bytes)
    {
        try
        {
            string targetName = fileName + ".tmp";
            string fullPath = GetPath(fileName);
            string fullTargetPath = GetPath(targetName);
            FileStream fs = GetFileCover(targetName);
            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
            File.Copy(fullPath, fullTargetPath, true);
        }catch (Exception e)
        {
            Debug.Log("save error: " + e.ToString());
        }
    }

    /// <summary>
    /// 检查存在文件夹
    /// </summary>
    /// <param name="fileName"></param>
    private void checkFolder(string fileName)
    {
        string folder = fileName.Substring(0, fileName.LastIndexOf("/") + 1);
        CreateFolder(folder);
    }

    /// <summary>
    /// 获取FileStream(并不会删除原文件)
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public FileStream GetFile(string fileName)
    {
        string fullPath = _filePath + _prefix + fileName;
        checkFolder(fileName);
        return File.Open(fullPath, FileMode.OpenOrCreate);
    }
    

    /// <summary>
    /// 覆盖获取FileStream(会删除原文件)
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public FileStream GetFileCover(string fileName)
    {
        string fullPath = GetPath(fileName);
        checkFolder(fileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
        return File.Open(fullPath, FileMode.OpenOrCreate);
    }

    /// <summary>
    /// 获取文件文本
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public string GetFileString(string fileName)
    {
        try
        {
            string fullPath = GetPath(fileName);
            if (File.Exists(fullPath))
            {
                using (StreamReader sr = new StreamReader(fullPath))
                {
                    return sr.ReadToEnd();
                }
            }
        }
        catch (Exception e)
        {
            return string.Empty;
        }
        return string.Empty;
    }

    public void SaveString(string filePath, string content)
    {
        try
        {
            string fullPath = GetPath(filePath);
            using (StreamWriter sw = new StreamWriter(fullPath))
            {
                sw.Write(content);
            }
        }
        catch (Exception e)
        {
            return;
        }
    }

    /// <summary>
    /// 反序列后储存
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public T GetSaveObject<T>(string fileName) where T : class
    {
        IFormatter formatter = new BinaryFormatter();
        FileStream stream = GetFile(fileName);
        T obj = formatter.Deserialize(stream) as T;
        stream.Close();
        return obj;
    }

    /// <summary>
    /// 序列化后储存
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="obj"></param>
    public void SaveObject(string fileName, object obj)
    {
        try
        {
            string targetName = fileName + ".tmp";
            string fullPath = _filePath + _prefix + fileName;
            string fullTargetPath = _filePath + _prefix + fileName;
            IFormatter formatter = new BinaryFormatter();
            FileStream stream = GetFileCover(targetName);
            formatter.Serialize(stream, obj);
            stream.Close();
            File.Copy(fullPath, fullTargetPath, true);
        }
        catch (Exception e)
        {
            Debug.Log("save object error: " + e.ToString());
        }
    }

    /// <summary>
    /// 获得FileManager的路径
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public string GetPath(string fileName)
    {
        string fullPath = _filePath + _prefix + fileName;
        return fullPath;
    }

    public void SetFilePath(string path)
    {
        _filePath = path;
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
}
