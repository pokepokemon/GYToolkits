using UnityEngine;
using System.Collections;

public class FileSaveObjectQuest : CoreTimeQuest
{

    public delegate void fileSaveCompleted(FileSaveObjectQuest quest);
    //执行完成后的回调
    public fileSaveCompleted onCompleted;

    private bool _isCompleted;
    public string path { private set; get; }
    public object obj { private set; get; }

    public FileSaveObjectQuest(string pathArg, object objArg)
    {
        obj = objArg;
        path = pathArg;
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
        FileManager.Instance.SaveObject(path, obj);
        if (onCompleted != null)
            onCompleted.Invoke(this);
        _isCompleted = true;
    }
}
