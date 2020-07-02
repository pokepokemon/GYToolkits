
using System;

namespace GYLib.Utils.ASEvents
{
    public class EventListenerInfo : IComparable
    {
        public EventListener listener;
        public int priority;

        public int CompareTo(object obj)
        {
            try
            {
                EventListenerInfo info = obj as EventListenerInfo;
                if (info != null)
                {
                    return info.priority - this.priority;
                }
            }
            catch (Exception)
            {

                return 0;
            }

            return 0;

        }
    }
}
