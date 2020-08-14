using UnityEngine;
using System.Collections;
using GYLib;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

/// <summary>
/// 异步ResourceLoad加载器
/// </summary>
public class ResourcesLoader : MonoSingleton<ResourcesLoader>
{
    private List<ResourceLoadTask> _reqList = new List<ResourceLoadTask>();

    private List<ResourceLoadTask> _reqBuffer = new List<ResourceLoadTask>();

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
            ResourceLoadTask task = new ResourceLoadTask();
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
            ResourceLoadTask task = new ResourceLoadTask();
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
            ResourceLoadTask task = _reqBuffer[i];
            if (task.req == null)
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

    /// <summary>
    /// 检查执行回调
    /// </summary>
    void UpdateForCallback()
    {
        float startTime = Time.realtimeSinceStartup;
        for (int i = 0; i < _reqBuffer.Count; i++)
        {
            ResourceLoadTask task = _reqBuffer[i];
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

    private List<ResourceLoadTask> _removeList = new List<ResourceLoadTask>();
    /// <summary>
    /// 检查加载任务完成
    /// </summary>
    void UpdateForTaskRemove()
    {
        foreach (ResourceLoadTask task in _reqBuffer)
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
            ResourceLoadTask task = _reqList[reqIndex];
            task.RemoveCallback(loadCallback);
            if (task.IsCallbackEmpty() && !_reqBuffer.Contains(task))
            {
                _reqList.RemoveAt(reqIndex);
            }
        }
    }
}
