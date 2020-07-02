using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class FileReadTask
{
    public delegate void onReadCompleted(FileReadTask task);
    public onReadCompleted onCompleted;

    public virtual void Read()
    {
    }

    public virtual void Finish()
    {
        if (onCompleted != null)
            onCompleted.Invoke(this);
    }

    public virtual bool IsCompleted()
    {
        return false;
    }
}
