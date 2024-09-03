using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GYLib.GYFrame
{
    public class WaitingModuleEvent
    {
        public List<string> eventList = new List<string>();

        private List<Type> typeList = new List<Type>();

        private ModuleEvent _completedEvent;

        private GameObject _go;

        public void StartWaiting(ModuleEvent completedEventArg = null, string name = "WaitingForModuleEvent")
        {
            GameObject go = new GameObject(name);
            _go = go;
            MonoBehaviour.DontDestroyOnLoad(_go);
            WaitingModuleMonobehaviour behaviour = go.AddComponent<WaitingModuleMonobehaviour>();
            behaviour.allList = new List<string>(eventList.ToArray());
            behaviour.waitingList = eventList;

            _completedEvent = completedEventArg;

            ModuleEventManager.instance.ReceiveHandler += ReceiveEvent;
        }

        public void AddWaitingEvent<T>() where T : ModuleEvent
        {
            Type type = typeof(T);
            if (!typeList.Contains(type))
            {
                typeList.Add(type);
                eventList.Add(type.ToString());
            }
        }

        /// <summary>
        /// 接收到事件时检测
        /// </summary>
        /// <param name="evt"></param>
        public void ReceiveEvent(ModuleEvent evt)
        {
            Type type = evt.GetType();
            if (typeList.Contains(type))
            {
                typeList.Remove(type);
                eventList.Remove(type.ToString());
                if (typeList.Count <= 0)
                {
                    if (_completedEvent != null)
                    {
                        ModuleEventManager.instance.dispatchEvent(_completedEvent);
                    }
                    ModuleEventManager.instance.ReceiveHandler -= ReceiveEvent;
                    GameObject.Destroy(_go);
                }
            }
        }

        public void ForceDispose()
        {
            ModuleEventManager.instance.ReceiveHandler -= ReceiveEvent;
            GameObject.Destroy(_go);
        }
    }
}