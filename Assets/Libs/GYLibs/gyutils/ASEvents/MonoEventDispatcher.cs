using System;
using System.Collections.Generic;
using UnityEngine;

namespace GYLib.Utils.ASEvents
{
    public class MonoEventDispatcher : MonoBehaviour, IEventDispatcher
    {
        
        protected bool bInit =false;

        protected IEventDispatcher impl;

        void Awake()
        {
            InitMonoEventDispatcher();
        }

        protected void InitMonoEventDispatcher()
        {
            if (!bInit)
            {
                impl = new EventDispatcherImpl(this);

                bInit = true;
            }
        }

        /// <summary>
        /// Sort by priority in called
        /// </summary>
        public void AddEventListener(string eventType, EventListener listener, int priority = 0)
        {
            if (impl != null)
            {
                impl.AddEventListener(eventType, listener, priority);
            }
            else
            {
                Debug.LogError("Impl is null, Function:InitMonoEventDispatcher() was not be called.");
            }
        }

        public void RemoveEventListener(string eventType, EventListener listener)
        {
            if (impl != null)
            {
                impl.RemoveEventListener(eventType, listener);
            }
            else
            {
                Debug.LogError("Impl is null, Function:InitMonoEventDispatcher() was not be called.");
            }
        }

        public void DispatchEvent(ASEvent e)
        {
            if (impl != null)
            {
                impl.DispatchEvent(e);

                if (e.bubbles && !e.needStopPropagation)
                {
                    MonoEventDispatcher com = e.currSender as MonoEventDispatcher;
                    if (com != null)
                    {
                        MonoEventDispatcher parentMonoEventDispatcher = GetParentMonoEventDispatcher(com.transform);
                        if (parentMonoEventDispatcher != null)
                        {
                            parentMonoEventDispatcher.DispatchEvent(e);
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Impl is null, Function:InitMonoEventDispatcher() was not be called.");
            }
        }

        private MonoEventDispatcher GetParentMonoEventDispatcher(Transform transform)
        {
            MonoEventDispatcher result = null;

            Transform currTransform = transform.parent;
            while (currTransform != null)
            {
                result = currTransform.GetComponent<MonoEventDispatcher>();
                if (result != null)
                {
                    return result;
                }
                else
                {
                    currTransform = currTransform.parent;
                }
            }

            return result;
        }

        public bool HasEventListener(string eventType)
        {
            if (impl != null)
            {
                return impl.HasEventListener(eventType);
            }
            else
            {
                Debug.LogError("Impl is null, Function:InitMonoEventDispatcher() was not be called.");
                return false;
            }
        }

        public static MonoEventDispatcher Get(GameObject go)
        {
            MonoEventDispatcher component = go.GetComponent<MonoEventDispatcher>();
            if (component == null)
            {
                return go.AddComponent<MonoEventDispatcher>();
            }
            else
            {
                return component;
            }
        }
    }
}
