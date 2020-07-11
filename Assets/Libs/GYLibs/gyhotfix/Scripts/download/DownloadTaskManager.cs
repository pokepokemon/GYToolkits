using CurlUnity;
using GYLib;
using GYLib.Hotfix;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GYLib.Hotfix
{
    /// <summary>
    /// 下载任务管理器
    /// Base on curl-unity
    /// </summary>
    [RequireComponent(typeof(CurlMultiUpdater))]
    public class DownloadTaskManager : MonoSingleton<DownloadTaskManager>
    {
        private const string _TASK_POOL_KEY = "DownloadTask";

        /// <summary>
        /// 等待中任务列表
        /// </summary>
        private List<DownloadTask> _waitingTaskList = new List<DownloadTask>();
        /// <summary>
        /// 总任务集合
        /// </summary>
        private Dictionary<string, DownloadTask> _taskMap = new Dictionary<string, DownloadTask>();
        /// <summary>
        /// 下载中任务列表
        /// </summary>
        private List<DownloadTask> _runningList = new List<DownloadTask>();
        /// <summary>
        /// 下载完成任务列表
        /// </summary>
        private Queue<DownloadTask> _completedList = new Queue<DownloadTask>();

        /// <summary>
        /// 缓存删除列表,作用域仅单个函数
        /// </summary>
        private List<DownloadTask> _deleteBuffer = new List<DownloadTask>();

        public CurlMulti multi;
        public int MAX_TASK_COUNT = 3;
        
        private bool _isRunning = false;

        public void Init()
        {
            multi = new CurlMulti();
        }

        /// <summary>
        /// 添加下载任务
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        public void AddTask(string url, string savePath, UnityAction<string> callback, UnityAction<string, string> errorCallback, int priority = -1)
        {
            bool isChangePriority = false;
            DownloadTask task;
            if (!_taskMap.TryGetValue(url, out task))
            {
                task = ObjectPool.Instance.Get(_TASK_POOL_KEY) as DownloadTask ?? new DownloadTask();
                task.url = url;
                task.callback = callback;
                task.savePath = savePath;
                task.errorCallback = errorCallback;
                isChangePriority = task.SetPriority(priority);

                _waitingTaskList.Add(task);
                _taskMap.Add(url, task);
            }
            else
            {
                task.callback += callback;
                if (errorCallback != null)
                {
                    task.errorCallback += errorCallback;
                }
                isChangePriority = task.SetPriority(priority);
            }
            if (!_isRunning)
            {
                _isRunning = true;
            }
        }

        /// <summary>
        /// 相关任务是否存在
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool HasTask(string url)
        {
            return _taskMap.ContainsKey(url);
        }

        // Update is called once per frame
        private void Update()
        {
            if (_isRunning)
            {
                UpdateForDownloaded();
                UpdateForStartNew();
                UpdateForCompleted();
                CheckFinish();
            }
        }

        /// <summary>
        /// 处理下载完成的任务
        /// </summary>
        private void UpdateForDownloaded()
        {
            foreach (var task in _runningList)
            {
                if (task.isCompleted)
                {
                    _completedList.Enqueue(task);
                    _deleteBuffer.Add(task);
                }
            }
            foreach (var task in _deleteBuffer)
            {
                _runningList.Remove(task);
            }
            _deleteBuffer.Clear();
        }

        /// <summary>
        /// 开始添加新下载任务
        /// </summary>
        private void UpdateForStartNew()
        {
            if (_runningList.Count < MAX_TASK_COUNT && _waitingTaskList.Count != 0)
            {
                int deltaCount = MAX_TASK_COUNT - _runningList.Count;

                for (int i = 0; i < deltaCount; i++)
                {
                    if (_waitingTaskList.Count != 0)
                    {
                        DownloadTask task = _waitingTaskList[0];
                        _waitingTaskList.RemoveAt(0);
                        _runningList.Add(task);
                        task.Start();
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 完成回调
        /// </summary>
        private void UpdateForCompleted()
        {
            while (_completedList.Count > 0)
            {
                DownloadTask task = _completedList.Dequeue();
                _taskMap.Remove(task.url);
                if (task.isError)
                {
                    task.CallErrorCallback();
                }
                else
                {
                    task.CallCompletedCallback();
                }
                ObjectPool.Instance.Push(_TASK_POOL_KEY, task);
            }
        }

        /// <summary>
        /// 检查是否可以停止下载器运作
        /// </summary>
        private void CheckFinish()
        {
            if (_isRunning && _taskMap.Count == 0)
            {
                _isRunning = false;
            }
        }

        public bool SetChangePriority(int priority)
        {

        }

        public void Dispose()
        {
            foreach (var task in _runningList)
            {
                task.Reset();
            }
            _waitingTaskList.Clear();
            _taskMap.Clear();
            _runningList.Clear();
            _completedList.Clear();
            _isRunning = false;
        }

        protected override void OnDestroy()
        {
            Dispose();
            base.OnDestroy();
        }
    }
}