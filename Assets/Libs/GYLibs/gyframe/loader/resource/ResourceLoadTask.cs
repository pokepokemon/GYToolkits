using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ResourceLoadTask
{
    public string path;
 
    public Stack<UnityAction<Object, string>> callbackList = new Stack<UnityAction<Object, string>>();

    public ResourceRequest req;

    public System.Type type;

    public void AddCallback(UnityAction<Object, string> callback)
    {
        callbackList.Push(callback);
    }

    public void DoCallback()
    {
        if (callbackList.Count > 0)
        {
            UnityAction<Object, string> callback = callbackList.Pop();
            callback(req.asset, path);
        }
    }

    public bool GetIsCompleted()
    {
        return req.isDone && callbackList.Count == 0;
    }
}
