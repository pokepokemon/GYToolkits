using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// 保存文件的任务
/// </summary>
public class FileSaveQuest : CoreTimeQuest
{
    public delegate void fileSaveCompleted(FileSaveQuest quest);
    //执行完成后的回调
    public fileSaveCompleted onCompleted;

    private bool _isCompleted;
    public string path { private set; get; }
    public string fileContent { private set; get; }

    private bool _isBytes = false;
    public byte[] bytes { private set; get; }

    public FileSaveQuest(string pathArg, string text)
    {
        fileContent = text;
        path = pathArg;
        _isBytes = false;
    }

    public FileSaveQuest(string pathArg, byte[] bytesArg)
    {
        bytes = bytesArg;
        path = pathArg;
        _isBytes = true;
    }

    public bool IsCompleted()
    {
        return _isCompleted;
    }

    public bool IsNeedSave()
    {
        return false;
    }

    public void Update()
    {
        if (_isBytes)
            FileManager.Instance.Save(path, bytes);
        else
            FileManager.Instance.Save(path, System.Text.Encoding.UTF8.GetBytes(fileContent));
        if (onCompleted != null)
            onCompleted.Invoke(this);
        _isCompleted = true;
    }
}
