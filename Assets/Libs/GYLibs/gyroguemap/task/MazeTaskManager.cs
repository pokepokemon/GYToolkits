using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;

/// <summary>
/// 生成任务管理器(Mono)
/// </summary>
public class MazeTaskManager
{
    public static readonly MazeTaskManager Instance = new MazeTaskManager();
    private List<TaskBase> _taskList = new List<TaskBase>();

    private List<TaskBase> _completedBuffer = new List<TaskBase>();

    private const long _FRAME_BUDGET_MS = 7;

    private Stopwatch _watch = new Stopwatch();

    public void Add(TaskBase task)
    {
        if (!_taskList.Contains(task))
        {
            _taskList.Add(task);
        }
    }

    // Update is called once per frame
    public void Update()
    {
        UpdateForRunTask();
        UpdateForCompletedCall();
    }

    /// <summary>
    /// 执行
    /// </summary>
    private void UpdateForRunTask()
    {
        if (_taskList.Count != 0)
        {
            _watch.Reset();
            _watch.Restart();
            foreach (var task in _taskList)
            {
                while (!task.isCompleted)
                {
                    if (_watch.ElapsedMilliseconds > _FRAME_BUDGET_MS)
                    {
                        _watch.Stop();
                        return;
                    }
                    task.Update();
                }

                if (task.isCompleted)
                {
                    _completedBuffer.Add(task);
                }
            }
        }
    }

    /// <summary>
    /// 完成回调
    /// </summary>
    private void UpdateForCompletedCall()
    {
        if (_completedBuffer.Count != 0)
        {
            foreach (var task in _completedBuffer)
            {
                bool remove = _taskList.Remove(task);
                if (remove)
                {
                    task.End();
                    if (task.OnCompleted != null)
                    {
                        task.OnCompleted(task.shareData);
                    }
                }
            }
            _completedBuffer.Clear();
        }
    }

    public void CancelTask(Predicate<TaskBase> predicate)
    {
        List<TaskBase> tasks = _taskList.FindAll(predicate);
        foreach (var task in tasks)
        {
            _taskList.Remove(task);
        }
    }
}

