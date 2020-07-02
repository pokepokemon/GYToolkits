using UnityEngine;
using System.Collections;
using GYLib;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// 专门处理文件存储任务的管理器
/// </summary>
public class FileSaveManager : MonoSingleton<FileSaveManager>
{
    //当前帧执行的任务超过毫秒数，则停止下一个任务
    private const int _TIME_LIMIT_PER_FRAME = 2;

    private bool _isStart;
    private Stopwatch sw = new Stopwatch();
    //阻塞任务
    private List<CoreTimeQuest> _list = new List<CoreTimeQuest>();

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="quest"></param>
    public void Add(CoreTimeQuest quest)
    {
        if (quest != null && !_list.Contains(quest))
        {
            _list.Add(quest);
        }
    }

    public void Remove(CoreTimeQuest quest)
    {
        if (_list.Contains(quest))
        {
            _list.Remove(quest);
        }
    }

    // Update is called once per frame
    void Update()
    {
        sw.Start();
        if (_list.Count > 0)
        {
            while (_list.Count > 0 && sw.ElapsedMilliseconds < _TIME_LIMIT_PER_FRAME)
            {
                _list[0].Update();
                if (_list[0].IsCompleted())
                {
                    _list.RemoveAt(0);
                }
            }
        }
        else
        {
            //抛出事件
        }
        sw.Stop();
        sw.Reset();
    }

    private void removeFromList(List<CoreTimeQuest> removeList)
    {
        if (removeList != null)
        {
            for (int i = 0; i < removeList.Count; i++)
                _list.Remove(removeList[i]);
        }
    }
}