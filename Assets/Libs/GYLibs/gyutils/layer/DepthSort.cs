using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.Utils
{
    internal class DepthSort : IFrame
    {
        private List<SpriteRenderer> _list = null;

        public DepthSort(List<SpriteRenderer> list)
        {
            _list = list;
        }

        // Use this for initialization
        public void Start()
        {
            EnterFrame.instance.add(this as IFrame);
        }

        private int sort(SpriteRenderer obj1, SpriteRenderer obj2)
        {
            Vector3 pos1 = obj1.transform.localPosition;
            Vector3 pos2 = obj2.transform.localPosition;
            if (pos1.y != pos2.y)
            {
                return pos1.y > pos2.y ? -1 : 1;
            }
            else
            {
                return pos1.x > pos2.x ? -1 : 1;
            }
        }

        // Update is called once per frame
        public void Update()
        {
            if (_list.Count > 1)
            {
                _list.Sort(sort);
                int minZ = 1;
                for (int i = 0; i < _list.Count; i++)
                {
                    Vector3 pos = _list[i].transform.localPosition;
                    _list[i].sortingOrder = minZ + i;
                }
            }
        }

        public void dispose()
        {
            EnterFrame.instance.remove(this as IFrame);
        }
    }
}