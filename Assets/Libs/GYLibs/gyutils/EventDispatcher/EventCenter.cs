using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.Utils.EventDispatcher
{
    public class EventCenter
    {
        private static EventCenter instance = null;

        public static EventCenter GetInstance()
        {
            if (instance == null)
            {
                instance = new EventCenter();
            }

            return instance;
        }

        private Dictionary<string, List<Action<BaseEvent>>> eventHandlers;

        public EventCenter()
        {
            eventHandlers = new Dictionary<string, List<Action<BaseEvent>>>();
        }

        public void AddEventListener(string eventType, Action<BaseEvent> listener)
        {
            if (eventHandlers.ContainsKey(eventType) == false)
            {
                eventHandlers.Add(eventType, new List<Action<BaseEvent>>());
            }

            List<Action<BaseEvent>> listeners = eventHandlers[eventType];

            if (listeners.IndexOf(listener) == -1)
            {
                listeners.Add(listener);
            }
        }

        public void RemoveEventListener(string eventType, Action<BaseEvent> listener)
        {
            if (eventHandlers.ContainsKey(eventType))
            {
                List<Action<BaseEvent>> listeners = eventHandlers[eventType];
                listeners.Remove(listener);
            }
        }

        public void DispatchEvent(BaseEvent evt)
        {
            if (eventHandlers.ContainsKey(evt.Type))
            {
                List<Action<BaseEvent>> listeners = eventHandlers[evt.Type];

                for (int i = 0; i < listeners.Count; ++i)
                {
                    listeners[i](evt);
                }
            }
        }

        public void Clear()
        {
            eventHandlers.Clear();
        }
    }
}