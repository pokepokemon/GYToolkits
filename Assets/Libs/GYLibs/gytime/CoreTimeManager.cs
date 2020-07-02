using UnityEngine;
using System.Collections;
using GYLib;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// 用于分片时间 核心计算任务的管理器
/// </summary>
public class CoreTimeManager : MonoSingleton<CoreTimeManager>
{
    private bool _isStart;
    //阻塞任务
    private List<CoreTimeQuest> _list = new List<CoreTimeQuest>();
    //并行任务
    private List<CoreTimeQuest> _loopList = new List<CoreTimeQuest>();

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
        //Stopwatch sw = new Stopwatch();
        //sw.Start();
        for (int i = 0; i < _loopList.Count; i++)
            _loopList[i].Update();
        if (_list.Count > 0)
        {
            _list[0].Update();
            if (_list[0].IsCompleted())
            {
                _list.RemoveAt(0);
            }
        }
        else
        {
            //抛出事件
        }
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
