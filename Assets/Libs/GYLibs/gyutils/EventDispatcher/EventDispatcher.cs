using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.Utils.EventDispatcher
{
    public class EventDispatcher
    {
        private static EventDispatcher instance = null;

        static public EventDispatcher GetInstance()
        {
            if (instance == null)
            {
                instance = new EventDispatcher();
            }

            return instance;
        }

        private Dictionary<Transform, Dictionary<string, List<Action<BaseEvent>>>> eventListeners;

        public EventDispatcher()
        {
            eventListeners = new Dictionary<Transform, Dictionary<string, List<Action<BaseEvent>>>>();
        }

        public void AddEventListener(Transform target, string eventType, Action<BaseEvent> listener)
        {
            if (eventListeners.ContainsKey(target) == false)
            {
                eventListeners.Add(target, new Dictionary<string, List<Action<BaseEvent>>>());
            }

            Dictionary<string, List<Action<BaseEvent>>> evtListenersOfTarget = eventListeners[target];

            if (evtListenersOfTarget.ContainsKey(eventType) == false)
            {
                evtListenersOfTarget.Add(eventType, new List<Action<BaseEvent>>());
            }

            List<Action<BaseEvent>> listeners = evtListenersOfTarget[eventType];

            if (listeners.IndexOf(listener) == -1)
            {
                listeners.Add(listener);
            }
        }

        public void RemoveEventListener(Transform target, string eventType, Action<BaseEvent> listener)
        {
            List<Action<BaseEvent>> listeners = GetListenersFromTransform(target, eventType);

            if (listeners != null)
            {
                listeners.Remove(listener);

                if (listeners.Count == 0)
                {
                    eventListeners.Remove(target);
                }
            }
        }

        public void DispatchEvent(Transform target, BaseEvent evt)
        {
            List<Action<BaseEvent>> listeners = GetListenersFromTransform(target, evt.Type);

            if (listeners != null)
            {
                if (evt.Target == null)
                {
                    evt.Target = target;
                }

                evt.CurrentTarget = target;

                for (int i = 0; i < listeners.Count; ++i)
                {
                    listeners[i](evt);

                    if (evt.NeedStopImmediatePropagation == true)
                    {
                        return;
                    }
                }
            }

            if (evt.Bubbles && !evt.NeedStopPropagation && target.parent != null)
            {
                DispatchEvent(target.parent, evt);
            }
        }

        private List<Action<BaseEvent>> GetListenersFromTransform(Transform transform, string eventType)
        {
            if (eventListeners.ContainsKey(transform))
            {
                Dictionary<string, List<Action<BaseEvent>>> evtListenersOfTarget = eventListeners[transform];

                if (evtListenersOfTarget.ContainsKey(eventType))
                {
                    List<Action<BaseEvent>> listeners = evtListenersOfTarget[eventType];
                    return listeners;
                }
            }

            return null;
        }

        public void Clear()
        {
            eventListeners.Clear();
        }
    }
}