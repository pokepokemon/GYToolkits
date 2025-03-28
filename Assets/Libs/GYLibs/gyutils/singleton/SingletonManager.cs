﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections;

namespace GYLib
{
    /// <summary>
    /// 单例管理类, 不要直接使用，只提供给Singleton<T>和MonSingleton<T>使用
    /// author 
    /// </summary>
    public class SingletonManager : MonoBehaviour
    {
        private static SingletonManager _instance = null;

        public static bool quitting;

        /// <summary>
        ///  获取单例
        /// </summary>
        public static SingletonManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SingletonManager>();
                    if (_instance == null && !quitting)
                    {
                        var go = new GameObject("SingleManager(auto create)");
                        _instance = go.AddComponent<SingletonManager>();
                        if (Application.isPlaying)
                        {
                            DontDestroyOnLoad(_instance.gameObject);
                        }
                    }
                    //SingletonManager must be sit in Scene
                }
                return _instance;
            }
        }

        private Dictionary<string,object> _singletons =new Dictionary<string, object>();
        private Dictionary<string, MonoBehaviour> _monoSingletons = new Dictionary<string, MonoBehaviour>();
        
        public Dictionary<string,object> singletons
        {
            get { return _singletons; }
        }
        public Dictionary<string, MonoBehaviour> monoSingletons
        {
            get { return _monoSingletons; }
        }

        public T GetInstance<T>() where T: class,new()
        {
            string key = typeof(T).FullName;
            object instance;
            _singletons.TryGetValue(key, out instance);
            if (instance == null)
            {
                instance = System.Activator.CreateInstance<T>();
                _singletons.Add(key, instance);
            }
            return instance as T;

        }

        public void AddInstance<T>(T instance) where T : class, new()
        {
            string key = typeof(T).FullName;
            if (_singletons.ContainsKey(key) == false)
                _singletons.Add(key, Instance);
        }

        public void RemoveInstance<T>() where T : class, new()
        {
            string key = typeof(T).FullName;
            object instance;
            _singletons.TryGetValue(key, out instance);
            if (instance != null)
            {
                _singletons.Remove(key);
            }
        }

        public T GetMonInstance<T>() where T : MonoBehaviour
        {
            string key = typeof(T).FullName;
            MonoBehaviour instance;
            _monoSingletons.TryGetValue(key, out instance);
            if (instance == null)
            {
                instance = FindObjectOfType<T>();
                if (instance == null)
                {
                    if (transform != null && !quitting)
                    {
                        GameObject instanceGO = new GameObject(key);
                        instanceGO.transform.parent = transform;
                        instance = instanceGO.AddComponent<T>();
                    }
                    else
                    {
                        return null;
                    }
                }

                if(!_monoSingletons.ContainsKey(key))
                {
                    _monoSingletons.Add(key, instance);
                }
                else
                {
                    _monoSingletons[key] = instance;
                }

            }
            return instance as T;
        }
        public void AddMonInstance<T>(T instance) where T : MonoBehaviour
        {
            string key = typeof(T).FullName;
            if (_monoSingletons.ContainsKey(key) == false)
            {
                _monoSingletons.Add(key, instance);
            }
                
        }
        public void RemoveMonInstance<T>() where T : MonoBehaviour
        {
            string key = typeof(T).FullName;
            MonoBehaviour instance;
            _monoSingletons.TryGetValue(key, out instance);
            if (instance != null)
            {
                _monoSingletons.Remove(key);
            }
               
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if(_instance!=this)
            {
                foreach (KeyValuePair<string,object> keyValue in this._singletons)
                {
                    if (_instance._singletons.ContainsKey(keyValue.Key) == false)
                        _instance._singletons.Add(keyValue.Key, keyValue.Value);
                }
                foreach(KeyValuePair < string, MonoBehaviour> keyValue in this._monoSingletons)
                {
                    if (_instance._monoSingletons.ContainsKey(keyValue.Key) == false)
                        _instance._monoSingletons.Add(keyValue.Key, keyValue.Value);
                }
                GameObject.Destroy(this.gameObject);
            }
        }
        
        public void RemoveOthersSingleton()
        {
            SingletonManager[] singleObjs = FindObjectsOfType<SingletonManager>();
            for (int i = singleObjs.Length - 1; i >= 0; i--)
            {
                if (singleObjs[i] != _instance)
                {
                    GameObject.Destroy(singleObjs[i].gameObject);
                }
            }
        }

        void OnApplicationQuit()
        {
            quitting = true;
        }
    }   
}
