using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GYLib
{
    /// <summary>
    /// 自主管理资源表
    /// </summary>
    public class ResourceCache : MonoSingleton<ResourceCache>
    {
        private const int _MAX_COUNT = 50;
        
        private Dictionary<string, Object> _resDict = new Dictionary<string, Object>();
        [SerializeField]
        private LinkedList<string> _resLink = new LinkedList<string>();

        /// <summary>
        /// 查找资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Object Load(string path)
        {
            Object obj;
            if (_resDict.TryGetValue(path, out obj))
            {
                _resLink.Remove(path);
                _resLink.AddFirst(path);
                return obj;
            }
            else
            {
                obj = Resources.Load(path);
                this.Add(path, obj);
            }
            return obj;
        }

        /// <summary>
        /// 查找资源
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public T Load<T>(string path) where T : Object
        {
            Object obj;
            if (_resDict.TryGetValue(path, out obj))
            {
                _resLink.Remove(path);
                _resLink.AddFirst(path);
                return obj as T;
            }
            else
            {
                T tObj = Resources.Load<T>(path);
                return tObj;
            }
        }

        /// <summary>
        /// 添加资源到列表中
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public void Add(string path, Object obj)
        {
            if (_resDict.TryGetValue(path, out obj))
            {
                _resLink.Remove(path);
                _resLink.AddFirst(path);
                _resDict[path] = obj;
            }
            else
            {
                _resLink.AddFirst(path);
                _resDict.Add(path, obj);
            }
            CheckLimit();
        }

        /// <summary>
        /// 检查是否超出限度
        /// </summary>
        public void CheckLimit()
        {
            int delta = _resLink.Count - _MAX_COUNT;
            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                {
                    string key = _resLink.Last.Value;
                    _resDict.Remove(key);
                    _resLink.RemoveLast();
                }
            }
        }

        /// <summary>
        /// 移除所有资源
        /// </summary>
        public void RemoveAll()
        {
            Resources.UnloadUnusedAssets();
            _resDict.Clear();
            _resDict.Clear();
        }
    }
}