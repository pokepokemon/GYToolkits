using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GYLib.GYFrame
{
    public class Module : MonoBehaviour
    {
        private Dictionary<Type, Processor> _dict = new Dictionary<Type, Processor>();

        /// <summary>
        /// Star时调用
        /// </summary>
        public virtual void Init()
        {
            
        }

        private void Start()
        {
            listProcessors();
            Init();
        }

        protected void RegistProcessor<T>() where T : Processor
        {
            Type tmpType = typeof(T);
            GameObject go = new GameObject(tmpType.ToString());
            T tmpT = go.AddComponent<T>();
            tmpT.RegistModule(this);
            _dict.Add(tmpType, tmpT);
            go.transform.SetParent(this.transform);
        }

        protected virtual void listProcessors()
        {
        }

        public Processor GetProcessor(Type processor)
        {
            Type key = processor;
            if (_dict.ContainsKey(key))
            {
                return _dict[key];
            }
            return null;
        }
    }
}
