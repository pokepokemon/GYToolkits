using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// 规定加载函数并逐步完成的任务
/// </summary>
namespace Game.interact
{
    /// <summary>
    /// 单步逐函数执行基类
    /// </summary>
    public class CoreTimeEventsQuest : CoreTimeQuest
    {
        protected bool _isCompleted;
        protected List<UnityAction> _stepList;

        public CoreTimeEventsQuest()
        {
            _isCompleted = false;
            _stepList = new List<UnityAction>();
        }

        public bool IsCompleted()
        {
            return _isCompleted;
        }

        public bool IsNeedSave()
        {
            return false;
        }

        private int _index = 0;
        public virtual void Update()
        {
            if (_isCompleted || _index >= _stepList.Count)
            {
                _isCompleted = true;
                return;
            }
            _stepList[_index++].Invoke();
        }
    }
}