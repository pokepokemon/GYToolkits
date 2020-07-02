using System;
using System.Collections.Generic;

namespace GYLib.Utils.ASEvents
{
    public delegate void EventListener(ASEvent e);

    public class EventDispatcher : IEventDispatcher
    {
        protected EventDispatcherImpl impl = null;

        public EventDispatcher(object sender = null)
        {
            impl = new EventDispatcherImpl(sender ?? this);
        }

        /// <summary>
        /// Sort by priority in called
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="listener"></param>
        /// <param name="priority"></param>
        public void AddEventListener(string eventType, EventListener listener, int priority = 0)
        {
            impl.AddEventListener(eventType, listener, priority);
        }

        public void RemoveEventListener(string eventType, EventListener listener)
        {
            impl.RemoveEventListener(eventType, listener);
        }

        public void DispatchEvent(ASEvent e)
        {
            impl.DispatchEvent(e);
        }

        public bool HasEventListener(string eventType)
        {
            return impl.HasEventListener(eventType);
        }
    }
}
