using GYLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BundleLoader : MonoSingleton<BundleLoader>
{

    private List<BundleLoadTask> _reqList = new List<BundleLoadTask>();

    private List<BundleLoadTask> _reqBuffer = new List<BundleLoadTask>();

    private const int MAX_BUFFER_COUNT = 3;

    private const float LOAD_FRAME_TIME = 0.005f;

    public UnityAction onTaskClear;

    private bool _isRunning = false;

    public int taskCount = 0;

    public void AddTask(string path, UnityAction<UnityEngine.Object, string> loadCallback)
    {
        int reqIndex = _reqList.FindIndex((x) => x.path == path && x.type == null);
        if (reqIndex != -1)
        {
            _reqList[reqIndex].AddCallback(loadCallback);
        }
        else
        {
            BundleLoadTask task = new BundleLoadTask();
            task.path = path;
            task.AddCallback(loadCallback);
            _reqList.Add(task);
        }
        if (!_isRunning)
        {
            _isRunning = true;
        }
    }

    public void AddTaskSetType(string path, UnityAction<UnityEngine.Object, string> loadCallback, Type type)
    {
        int reqIndex = _reqList.FindIndex((x) => x.path == path && x.type == type);
        if (reqIndex != -1)
        {
            _reqList[reqIndex].AddCallback(loadCallback);
        }
        else
        {
            BundleLoadTask task = new BundleLoadTask();
            task.path = path;
            task.AddCallback(loadCallback);
            task.type = type;
            _reqList.Add(task);
        }
        if (!_isRunning)
        {
            _isRunning = true;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (_isRunning)
        {
            UpdateForBuffer();
            UpdateForLoad();
            UpdateForCallback();
            UpdateForTaskRemove();
            UpdateForCheckRunning();
        }
        taskCount = _reqList.Count;
    }

    /// <summary>
    /// 检查Buffer是否需要补充
    /// </summary>
    void UpdateForBuffer()
    {
        if (_reqBuffer.Count < MAX_BUFFER_COUNT)
        {
            if (_reqList.Count > _reqBuffer.Count)
            {
                int deltaCount = MAX_BUFFER_COUNT - _reqBuffer.Count;
                int oldIndex = _reqBuffer.Count;
                for (int i = 0; i < deltaCount; i++)
                {
                    if (oldIndex + i < _reqList.Count)
                    {
                        _reqBuffer.Add(_reqList[oldIndex + i]);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查申请加载
    /// </summary>
    void UpdateForLoad()
    {
        for (int i = 0; i < _reqBuffer.Count; i++)
        {
            BundleLoadTask task = _reqBuffer[i];
            
            if (task.req == null)
            {
                if (BundleResHub.Instance.TryGetBundle(task.path, out AssetBundle assetbundle, out string resFullName))
                {
                    if (task.type == null)
                    {
                        Debug.Log("assetbundle : " + string.Join("|", assetbundle.GetAllAssetNames()));
                        Debug.Log("load bundle : " + resFullName);
                        task.req = assetbundle.LoadAssetAsync<UnityEngine.Object>(resFullName);
                    }
                    else
                    {
                        task.req = assetbundle.LoadAssetAsync(resFullName, task.type);
                    }
                }
                else
                {
                    if (task.type == null)
                    {
                        task.req = Resources.LoadAsync<UnityEngine.Object>(task.path);
                    }
                    else
                    {
                        task.req = Resources.LoadAsync(task.path, task.type);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 检查执行回调
    /// </summary>
    void UpdateForCallback()
    {
        float startTime = Time.realtimeSinceStartup;
        for (int i = 0; i < _reqBuffer.Count; i++)
        {
            BundleLoadTask task = _reqBuffer[i];
            if (task.req.isDone)
            {
                while (Time.realtimeSinceStartup - startTime < LOAD_FRAME_TIME && !task.GetIsCompleted())
                {
                    task.DoCallback();
                }
            }
            if (Time.realtimeSinceStartup - startTime >= LOAD_FRAME_TIME)
            {
                break;
            }
        }
    }

    private List<BundleLoadTask> _removeList = new List<BundleLoadTask>();
    /// <summary>
    /// 检查加载任务完成
    /// </summary>
    void UpdateForTaskRemove()
    {
        foreach (BundleLoadTask task in _reqBuffer)
        {
            if (task.GetIsCompleted())
            {
                _removeList.Add(task);
            }
        }
        for (int i = 0; i < _removeList.Count; i++)
        {
            _reqBuffer.Remove(_removeList[i]);
            _reqList.Remove(_removeList[i]);
        }
        _removeList.Clear();
    }

    /// <summary>
    /// 检查管理器是否在运行
    /// </summary>
    void UpdateForCheckRunning()
    {
        if (_isRunning && _reqList.Count == 0)
        {
            _isRunning = false;
        }
    }

    /// <summary>
    /// 是否有以参数开始的路径 正在加载中
    /// </summary>
    /// <param name="subStr"></param>
    /// <returns></returns>
    public bool HasStartWithLoadingPath(string subStr, string ignoreStr = "")
    {
        int count = _reqList.Count;
        for (int i = 0; i < count; i++)
        {
            if (_reqList[i].path.StartsWith(subStr) && !_reqList[i].path.Contains(ignoreStr))
            {
                return true;
            }
        }
        return false;
    }

    public void CancelCallback(string path, Type type, UnityAction<UnityEngine.Object, string> loadCallback)
    {
        int reqIndex = _reqList.FindIndex((x) => x.path == path && x.type == null);
        if (reqIndex != -1)
        {
            BundleLoadTask task = _reqList[reqIndex];
            task.RemoveCallback(loadCallback);
            if (task.IsCallbackEmpty() && !_reqBuffer.Contains(task))
            {
                _reqList.RemoveAt(reqIndex);
            }
        }
    }

    public UnityEngine.Object LoadObjectSync(string path)
    {
        if (BundleResHub.Instance.TryGetBundle(path, out AssetBundle assetbundle, out string resFullName))
        {
            return assetbundle.LoadAsset<UnityEngine.Object>(resFullName);
        }
        else
        {
            return Resources.Load<UnityEngine.Object>(path);
        }
    }

    public UnityEngine.Sprite LoadSpriteSync(string path)
    {
        if (BundleResHub.Instance.TryGetBundle(path, out AssetBundle assetbundle, out string resFullName))
        {
            return assetbundle.LoadAsset<UnityEngine.Sprite>(resFullName);
        }
        else
        {
            return Resources.Load<UnityEngine.Sprite>(path);
        }
    }
}