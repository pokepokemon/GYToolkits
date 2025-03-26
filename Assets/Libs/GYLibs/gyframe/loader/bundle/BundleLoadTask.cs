using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BundleLoadTask
{
    public string path;

    public List<UnityAction<Object, string>> callbackList = new List<UnityAction<Object, string>>();

    public ResourceRequest req;

    private int _callbackCount = 0;

    public System.Type type;

    public void AddCallback(UnityAction<Object, string> callback)
    {
        callbackList.Add(callback);
        _callbackCount++;
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
            if (req == null || req.asset == null)
            {
                Debug.LogError("asset is null : " + path);
            }
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
