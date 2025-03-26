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
    public delegate void HandleErrorCall(string reason);

    public static readonly MazeThreadTaskManager Instance = new MazeThreadTaskManager();
    private Thread _thread = null;

    private List<TaskBase> _taskList = new List<TaskBase>();

    private List<TaskBase> _completedBuffer = new List<TaskBase>();

    public bool unsupport = false;
    private string _exceptionStr = "";

    public HandleErrorCall OnErrorCall;

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
        if (!unsupport)
        {
            UpdateForCompletedCall();
            UpdateForTraceError();
        }
        else
        {
            //只调用一次
            if (OnErrorCall != null)
            {
                OnErrorCall.Invoke(_exceptionStr);
                OnErrorCall = null;
                UnityEngine.Debug.Log("exception : " + _exceptionStr);
            }
        }
    }

    /// <summary>
    /// 调加错误调用回调
    /// </summary>
    /// <param name="callback"></param>
    public void AddErrorCallback(HandleErrorCall callback)
    {
        if (OnErrorCall == null)
        {
            OnErrorCall = callback;
        }
        else
        {
            OnErrorCall += callback;
        }
    }
    
    public void CheckStartThread()
    {
        if (_thread == null && _taskList.Count != 0)
        {
            _thread = new Thread(new ThreadStart(LoopDoTask));
            _thread.IsBackground = true;
            _thread.Start();
        }
    }

    public void CheckStopThread()
    {
        if (_thread != null && _taskList.Count == 0)
        {
            Stop();
        }
    }

    private void LoopDoTask()
    {
        while (true)
        {
            bool needStop = false;
            try
            {
                UpdateForRunTask();
                Thread.Sleep(400);
            }
            catch (Exception e)
            {
                if (!(e is System.Threading.ThreadAbortException))
                {
                    _exceptionStr = e.ToString();
                    unsupport = true;
                }
                needStop = true;
            }
            if (needStop)
            {
                Stop();
                return;
            }
        }
    }

    public void Stop(bool immediatly = false)
    {
        if (_thread != null)
        {
            if (!_thread.Join(500))//等待0.5秒
            {
                try
                {
#if UNITY_IPHONE
				    _thread.Abort();
#else
                    _thread.Abort();
#endif
                }
                catch (Exception e)
                {
                }
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

