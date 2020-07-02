using System.Collections.Generic;
using UnityEngine;

namespace GYLib.Utils.ASEvents
{
    public class EventDispatcherImpl : IEventDispatcher
    {
        /// <summary>
        /// 事件发送者
        /// </summary>
        private object sender;
        
        /// <summary>
        /// 事件名，对应监听函数
        /// </summary>
        private Dictionary<string, List<EventListenerInfo>> m_mapEvents = new Dictionary<string, List<EventListenerInfo>>();

        public EventDispatcherImpl(object sender)
        {
            this.sender = sender;
        }

        public void AddEventListener(string eventType, EventListener listener, int priority = 0)
        {
            List<EventListenerInfo> lstEventListenerInfo = null;
            m_mapEvents.TryGetValue(eventType, out lstEventListenerInfo);

            if (null == lstEventListenerInfo)
            {
                lstEventListenerInfo = new List<EventListenerInfo>();
                m_mapEvents[eventType] = lstEventListenerInfo;
            }

            // 同一个监听函数,只存在一个对象
            bool hasEventListener = false;
            foreach (EventListenerInfo eventListenerInfo in lstEventListenerInfo)
            {
                if (eventListenerInfo.listener == listener)
                {
                    eventListenerInfo.priority = priority;
                    hasEventListener = true;
                    break;
                }
            }

            if (false == hasEventListener)
            {
                EventListenerInfo info = new EventListenerInfo()
                {
                    listener = listener,
                    priority = priority
                };

                lstEventListenerInfo.Add(info);
            }


            lstEventListenerInfo.Sort();
        }

        public void RemoveEventListener(string eventType, EventListener listener)
        {
            List<EventListenerInfo> lstEventListenerInfo = null;
            m_mapEvents.TryGetValue(eventType, out lstEventListenerInfo);

            if (null != lstEventListenerInfo)
            {
                // Only one
                foreach (EventListenerInfo eventListenerInfo in lstEventListenerInfo)
                {
                    if (eventListenerInfo.listener == listener)
                    {
                        lstEventListenerInfo.Remove(eventListenerInfo);
                        break;
                    }
                }

                if (0 == lstEventListenerInfo.Count)
                {
                    m_mapEvents.Remove(eventType);
                }
            }
        }

        public void DispatchEvent(ASEvent e)
        {
            if (e.sender == null)
            {
                e.__SetSender(this.sender);
            }

            e.__SetCurrSender(this.sender);

            List<EventListenerInfo> lstEventListenerInfo = null;

            m_mapEvents.TryGetValue(e.type, out lstEventListenerInfo);
            if (null != lstEventListenerInfo && lstEventListenerInfo.Count > 0)
            {
                List<EventListenerInfo> tmp = new List<EventListenerInfo>(lstEventListenerInfo);
                
                foreach (EventListenerInfo eventListenerInfo in tmp)
                {
                    if (eventListenerInfo.listener != null)
                    {
                        eventListenerInfo.listener(e);
                        if (e.needStopImmediatePropagation)
                        {
                            return;
                        }
                    }
                }
            }
        }

        public bool HasEventListener(string eventType)
        {
            List<EventListenerInfo> lstEventListenerInfo = null;
            m_mapEvents.TryGetValue(eventType, out lstEventListenerInfo);
            if (lstEventListenerInfo != null && lstEventListenerInfo.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}