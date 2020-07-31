using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.Utils
{
    public class EnterFrame
    {
        public static readonly EnterFrame instance = new EnterFrame();
        private static List<IFrame> _list = new List<IFrame>();

        public bool isSafe = true;

        public EnterFrame()
        {
        }

        List<IFrame> _frameListBuffer = new List<IFrame>();
        // Update is called once per frame
        public void Update()
        {
            if (isSafe)
            {
                _frameListBuffer.Clear();
                _frameListBuffer.AddRange(_list);
                for (int i = 0; i < _frameListBuffer.Count; i++)
                {
                    if (_list.IndexOf(_frameListBuffer[i]) != -1)
                    {
                        _frameListBuffer[i].Update();
                    }
                }
            }
            else
            {
                for (int i = 0; i < _list.Count; i++)
                {
                    _list[i].Update();
                }
            }
        }

        public void add(IFrame frame)
        {
            if (frame != null && !_list.Contains(frame))
            {
                _list.Add(frame);
            }
        }

        public void remove(IFrame frame)
        {
            if (_list.Contains(frame))
            {
                _list.Remove(frame);
            }
        }

        public void removeAll()
        {
            _list.Clear();
            _frameListBuffer.Clear();
        }
    }
}