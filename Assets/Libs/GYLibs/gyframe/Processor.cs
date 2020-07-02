using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.GYFrame
{
    public class Processor : MonoBehaviour
    {
        protected Module _module;

        public List<string> eventList = new List<string>();

        /// <summary>
        /// 将在Start时调用
        /// </summary>
        public virtual void Init()
        {
        }

        void Start()
        {
            Init();
        }

        public void RegistModule(Module module)
        {
            this._module = module;
            Type[] arr = listenModuleEvents();
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                ModuleEventManager.instance.addEvent(arr[i], this.receivedModuleEvent);
                eventList.Add(arr[i].ToString());
            }
        }

        /// <summary>
        /// 添加模块事件监听列表
        /// </summary>
        /// <returns></returns>
        protected virtual Type[] listenModuleEvents()
        {
            return new Type[0]{};
        }

        /// <summary>
        /// 接到模块事件进行处理的重载函数
        /// </summary>
        /// <param name="evt"></param>
        protected virtual void receivedModuleEvent(ModuleEvent evt)
        {

        }
    }
}