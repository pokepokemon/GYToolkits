using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Threading;

/// <summary>
/// 生成任务管理器(Thread)
/// </summary>
public class MazeThreadTaskManager
{
    public static readonly MazeThreadTaskManager Instance = new MazeThreadTaskManager();
    private Thread _thread = null;

    private List<TaskBase> _taskList = new List<TaskBase>();

    private List<TaskBase> _completedBuffer = new List<TaskBase>();

    private bool unsupport = false;
    private string _exceptionStr = "";

    public void Add(TaskBase task)
    {
        if (!_taskList.Contains(task))
        {
            _taskList.Add(task);
            CheckStartThread();
        }
    }

    // Update is called once per frame
    public void Update()
    {
        UpdateForCompletedCall();
        UpdateForTraceError();
    }
    
    public void CheckStartThread()
    {
        if (_thread == null && _taskList.Count != 0)
        {
            _thread = new Thread(new ThreadStart(LoopDoTask));
            _thread.IsBackground = true;
            _thread.Start();

            UnityEngine.Debug.Log("new thread = " + _taskList.Count);
        }
    }

    public void CheckStopThread()
    {
        if (_thread != null && _taskList.Count == 0)
        {
            Stop();
            UnityEngine.Debug.Log("stop = " + _taskList.Count);
        }
    }

    private void LoopDoTask()
    {
        while (true)
        {
            try
            {
                UpdateForRunTask();
            }
            catch (Exception e)
            {
                _exceptionStr = e.ToString();
                unsupport = true;
                Stop();
                return;
            }
            Thread.Sleep(400);
        }
    }

    public void Stop(bool immediatly = false)
    {
        if (_thread != null)
        {
            if (!_thread.Join(500))//等待0.5秒
            {
#if UNITY_IPHONE
				_thread.Abort();
#else
                _thread.Abort();
#endif
                _thread = null;
            }
        }
    }

    /// <summary>
    /// 执行
    /// </summary>
    private void UpdateForRunTask()
    {
        if (_taskList.Count != 0)
        {
            foreach (var task in _taskList)
            {
                while (!task.isCompleted)
                {
                    task.Update();
                }

                if (task.isCompleted)
                {
                    lock (_completedBuffer)
                    {
                        _completedBuffer.Add(task);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 完成回调
    /// </summary>
    private void UpdateForCompletedCall()
    {
        lock (_completedBuffer)
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
                CheckStopThread();
            }
        }
    }

    private void UpdateForTraceError()
    {
        if (!string.IsNullOrEmpty(_exceptionStr))
        {
            UnityEngine.Debug.LogError("Thread error : " + _exceptionStr);
            _exceptionStr = null;
        }
    }
}

