using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GYLib
{
    /// <summary>
    /// Singleton in unity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance = null;
      
        /// <summary>
        ///  获取单例
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null && SingletonManager.Instance!=null)
                {
                    _instance = SingletonManager.Instance.GetMonInstance<T>();
                }
                return _instance;
            }
        }



        protected virtual void Awake()
        {
            if (_instance == null)
            {
                SingletonManager.Instance.AddMonInstance<T>(this as T);
                _instance = this as T;
            }
                
        }

        protected virtual void OnDestroy()
        {
            if (_instance == this&& SingletonManager.Instance!=null)
            {
                SingletonManager.Instance.RemoveMonInstance<T>();
                _instance = null;
            }
               
        }

    
    }
}
