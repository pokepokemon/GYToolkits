using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GYLib;
using System.IO;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

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
            CheckFolder(fullPath);
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
    private void CheckFolder(string fileName)
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
        CheckFolder(fileName);
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
        CheckFolder(fileName);
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
    public string GetFileString(string fileName, byte[] appendBuffer = null)
    {
        try
        {
            string fullPath = GetPath(fileName);

            // 检查文件是否存在
            if (File.Exists(fullPath))
            {
                // 如果没有附加缓冲区需求，直接完整读取文件
                if (appendBuffer == null)
                {
                    return File.ReadAllText(fullPath);
                }

                long fileLength = new FileInfo(fullPath).Length;

                // 检查附加缓冲区大小是否有效
                if (appendBuffer.Length > fileLength)
                {
                    throw new ArgumentException("File length error!");
                }

                // 计算文件的主内容长度（总长度 - 附加缓冲区长度）
                long mainContentLength = fileLength - appendBuffer.Length;

                using (FileStream fs = new FileStream(fullPath, FileMode.Open, FileAccess.Read))
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
        }
        catch (Exception e)
        {
            Console.WriteLine($"读取文件失败: {e.Message}");
        }

        return string.Empty;
    }

    public void SaveString(string filePath, string content, byte[] appendBytes = null)
    {
        try
        {
            string fullPath = GetPath(filePath);
            // 检查是否存在目录,没有则创建
            CheckAndCreateDoc(fullPath);

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
        catch (Exception e)
        {
            return;
        }
    }

    /// <summary>
    /// 获取默认保存路径下所有的文件
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    public FileInfo[] GetAllFile(string pattern = "*")
    {
        string path = _filePath;
        DirectoryInfo directory = new DirectoryInfo(path);
        FileInfo[] infos = directory.GetFiles(pattern);
        if (infos != null)
        {
            return infos;
        }
        return null;
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
