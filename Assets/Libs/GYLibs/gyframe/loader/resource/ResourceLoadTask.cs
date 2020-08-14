using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ResourceLoadTask
{
    public string path;
 
    public List<UnityAction<Object, string>> callbackList = new List<UnityAction<Object, string>>();

    public ResourceRequest req;

    public System.Type type;

    public void AddCallback(UnityAction<Object, string> callback)
    {
        callbackList.Add(callback);
    }

    public void RemoveCallback(UnityAction<Object, string> callback)
    {
        callbackList.Remove(callback);
    }

    public void DoCallback()
    {
        if (callbackList.Count > 0)
        {
            int lastIndex = callbackList.Count - 1;
            UnityAction<Object, string> callback = callbackList[lastIndex];
            callbackList.RemoveAt(lastIndex);
            callback(req.asset, path);
        }
    }

    public bool GetIsCompleted()
    {
        return req.isDone && callbackList.Count == 0;
    }

    public bool IsCallbackEmpty()
    {
        return callbackList.Count == 0; 
    }
}
