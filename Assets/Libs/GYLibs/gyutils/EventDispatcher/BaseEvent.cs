using UnityEngine;
using System.Collections;

namespace GYLib.Utils.EventDispatcher
{
    public class BaseEvent
    {
        public string Type;
        public bool Bubbles;
        public Transform CurrentTarget;
        public Transform Target;
        public bool NeedStopImmediatePropagation;
        public bool NeedStopPropagation;

        public BaseEvent(string type, bool bubbles = false)
        {
            Reset();

            Type = type;
            Bubbles = bubbles;
        }

        public void StopPropagation()
        {
            NeedStopPropagation = true;
        }

        public void StopImmediatePropagation()
        {
            NeedStopPropagation = true;
        }

        public void Reset()
        {
            Type = "";
            Bubbles = false;
            CurrentTarget = null;
            Target = null;
            NeedStopImmediatePropagation = false;
            NeedStopPropagation = false;
        }
    }
}