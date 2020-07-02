using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace GYLib
{
    /// <summary>
    /// 单例基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class Singleton<T> where T : Singleton<T> ,new()
    {
        private static T _instance = null;

        protected Singleton()
        {
        }

        /// <summary>
        ///  Get singleton
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = SingletonManager.Instance.GetInstance<T>();
                }
                return _instance;
            }
        }

        public static void Destory()
        {
            SingletonManager.Instance.RemoveInstance<T>();
            _instance = null;
        }
    }
}
