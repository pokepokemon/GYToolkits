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

        // Update is called once per frame
        public void Update()
        {
            if (isSafe)
            {
                List<IFrame> copyList = new List<IFrame>();
                copyList.AddRange(_list);
                for (int i = 0; i < copyList.Count; i++)
                {
                    if (_list.IndexOf(copyList[i]) != -1)
                    {
                        copyList[i].Update();
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
        }
    }
}