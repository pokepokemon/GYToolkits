using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.GYFrame
{
    public class ModuleEventManager
    {
        public static readonly ModuleEventManager instance = new ModuleEventManager();
        public delegate void ModuleDelegate(ModuleEvent arg);
        public delegate void WaitingDelegate(ModuleEvent arg);
        Dictionary<Type, ModuleDelegate> _dict = new Dictionary<Type, ModuleDelegate>();

        public ModuleEventManager()
        {
            _dict = new Dictionary<Type, ModuleDelegate>();
        }

        internal WaitingDelegate ReceiveHandler;

        /**
         * 添加事件
         */
        public void addEvent(Type argClass, ModuleDelegate func)
        {
            Type key = argClass;
            ModuleDelegate tmpFunc = null;
            if (_dict.ContainsKey(key))
            {
                tmpFunc = _dict[key] as ModuleDelegate;
            }
            else
            {
                tmpFunc = new ModuleDelegate(delegateFunc);
                _dict.Add(key, tmpFunc);
            }
            tmpFunc += func;
            _dict[key] = tmpFunc;
        }

        /**
         * 移除事件
         */
        public void removeEvent(Type argClass, ModuleDelegate func)
        {
            Type key = argClass;
            if (_dict.ContainsKey(key))
            {
                _dict.Remove(key);
            }
            return;
        }

        /**
         * 派发事件
         */
        public void dispatchEvent(ModuleEvent evt)
        {
            Type evtName = evt.GetType();
            ModuleDelegate func;
            //Debug.Log("evtName : " + evtName.ToString());
            
            if (_dict.TryGetValue(evtName, out func))
            {
                func.Invoke(evt);
            }
            else
            {
                //Debug.Log("no one care about " + evtName.ToString());
            }
            if (ReceiveHandler != null)
            {
                ReceiveHandler.Invoke(evt);
            }
        }

        private void delegateFunc(ModuleEvent evt)
        {

        }
    }
}