using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GYLib;

namespace GYTime
{
    /// <summary>
    /// 用于可控制中断的定时执行管理器
    /// </summary>
    public class WorldTimeManager : MonoSingleton<WorldTimeManager>
    {
        public delegate void timeStatusChange(bool isPlaying);
        public timeStatusChange onTimeStatusChange;

        private bool _isPlaying;
        private static List<ITimeFrame> _list = new List<ITimeFrame>();

        // Update is called once per frame
        public void Update()
        {
            for (int i = 0; i < _list.Count; i++)
            {
                if (_isPlaying || (!_isPlaying && _list[i].keepPlaying()))
                    _list[i].Update();
            }
        }

        public void Add(ITimeFrame frame)
        {
            if (frame != null && !_list.Contains(frame))
            {
                _list.Add(frame);
            }
        }

        public void Remove(ITimeFrame frame)
        {
            if (_list.Contains(frame))
            {
                _list.Remove(frame);
            }
        }

        public void removeAll()
        {
            _list.Clear();
        }

        public bool isPlaying
        {
            set
            {
                if (_isPlaying != value)
                {
                    _isPlaying = value;
                    if (onTimeStatusChange != null)
                        onTimeStatusChange.Invoke(_isPlaying);
                }
            }
            get
            {
                return _isPlaying;
            }
        }
    }
}
