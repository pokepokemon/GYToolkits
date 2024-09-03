using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace GYLib.GYFrame
{
    /// <summary>
    /// 为了可视化Moudle而添加的基于UnityEngine
    /// </summary>
    public class ModuleInUnity
    {
        public static readonly ModuleInUnity Instance = new ModuleInUnity();

        private GameObject _gameObject;
        private Dictionary<System.Type, Module> _moduleMap = new Dictionary<Type, Module>();

        public ModuleInUnity()
        {
            GameObject go = new GameObject("ModuleInUnity");
            GameObject.DontDestroyOnLoad(go);
            _gameObject = go;
        }

        /// <summary>
        /// 添加模块
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddModule<T>() where T : Module
        {
            Type tmpType = typeof(T);
            GameObject module = new GameObject(tmpType.ToString());
            T m = module.AddComponent<T>();
            module.gameObject.transform.SetParent(_gameObject.transform);
            _moduleMap[tmpType] = m;
        }

        public bool TryGetModule<T>(out Module module) where T : Module
        {
            Type tmpType = typeof(T);
            return _moduleMap.TryGetValue(tmpType, out module);
        }
    }
}
