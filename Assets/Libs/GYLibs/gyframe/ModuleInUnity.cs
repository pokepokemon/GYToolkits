using UnityEngine;
using System.Collections;
using System;

namespace GYLib.GYFrame
{
    /// <summary>
    /// 为了可视化Moudle而添加的基于UnityEngine
    /// </summary>
    public class ModuleInUnity
    {
        public static readonly ModuleInUnity Instance = new ModuleInUnity();

        private GameObject _gameObject;

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
            module.AddComponent<T>();
            module.gameObject.transform.SetParent(_gameObject.transform);
        }
    }
}
