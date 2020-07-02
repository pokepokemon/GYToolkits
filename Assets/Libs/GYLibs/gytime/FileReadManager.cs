using UnityEngine;
using System.Collections;
using GYLib;
using System.Diagnostics;
using System.Collections.Generic;

public class FileReadManager : MonoSingleton<FileReadManager>
{
    //当前帧执行的任务超过毫秒数，则停止下一个任务
    private const int _TIME_LIMIT_PER_FRAME = 2;

    private bool _isStart;
    private Stopwatch sw = new Stopwatch();
    //阻塞任务
    private List<FileReadTask> _list = new List<FileReadTask>();

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="quest"></param>
    public void Add(FileReadTask quest)
    {
        if (quest != null && !_list.Contains(quest))
        {
            _list.Add(quest);
        }
    }

    public void Remove(FileReadTask quest)
    {
        if (_list.Contains(quest))
        {
            _list.Remove(quest);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_list.Count > 0)
        {
            sw.Start();
            while (_list.Count > 0 && sw.ElapsedMilliseconds < _TIME_LIMIT_PER_FRAME)
            {
                _list[0].Read();
                if (_list[0].IsCompleted())
                {
                    _list[0].Finish();
                    _list.RemoveAt(0);
                }
            }

            sw.Stop();
            sw.Reset();
        }
        else
        {
            //抛出事件
        }
    }

    private void removeFromList(List<FileReadTask> removeList)
    {
        if (removeList != null)
        {
            for (int i = 0; i < removeList.Count; i++)
                _list.Remove(removeList[i]);
        }
    }
}
