using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GYLib
{
    /// <summary>
    /// 针对各种对象的通用对象池
    /// </summary>
    public class ObjectPool : MonoSingleton<ObjectPool>
    {
        private const int _DEFAULT_LIMIT = 150;
        private Dictionary<string, Queue<object>> _objectDict = new Dictionary<string, Queue<object>>();
        private Dictionary<string, int> _limitDict = new Dictionary<string, int>();

        public void Start()
        {
            this.gameObject.SetActive(false);
        }

        /// <summary>
        /// 添加到对象池
        /// </summary>
        /// <param name="key"></param>
        /// <param name="obj"></param>
        public void Push(string key, object obj, bool skipSetParent = false)
        {
            if (string.IsNullOrEmpty(key) || obj == null)
            {
                Debug.LogError("Don't try to push a null to pool!");
                return;
            }
            Queue<object> list;
            if (!_objectDict.TryGetValue(key, out list))
            {
                list = new Queue<object>();
                if (!_limitDict.ContainsKey(key))
                {
                    _limitDict.Add(key, _DEFAULT_LIMIT);
                }
                _objectDict.Add(key, list);
            }
            if (list.Count > _limitDict[key])
            {
                destoryObject(obj);
                return;
            }

            list.Enqueue(obj);

            if (!skipSetParent)
            {
                if (obj is GameObject)
                {
                    Transform trans = (obj as GameObject).transform;
                    trans.SetParent(this.transform);
                    trans.localPosition = Vector3.zero;
                }
            }
            if (obj is IRecycle)
                (obj as IRecycle).OnRecycle();
            if (!skipSetParent)
            {
                if (obj is Component)
                {
                    Transform trans = (obj as Component).transform;
                    trans.SetParent(this.transform);
                    trans.localPosition = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// 设置限制数量
        /// </summary>
        /// <param name="key"></param>
        /// <param name="limit"></param>
        public void SetLimit(string key, int limit)
        {
            int originLimit = _DEFAULT_LIMIT;
            if (!_limitDict.ContainsKey(key))
            {
                _limitDict.Add(key, limit);
            }
            else
            {
                originLimit = _limitDict[key];
                _limitDict[key] = limit;
            }

            if (originLimit > limit)
            {
                Queue<object> list;
                if (_objectDict.TryGetValue(key, out list))
                {
                    int delta = _limitDict.Count - limit;
                    for (int i = 0; i < delta; i++)
                    {
                        object obj = list.Dequeue();
                        destoryObject(obj);
                    }
                }
            }
        }

        /// <summary>
        /// 是否已经被设置过上限
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool HasLimit(string key)
        {
            return _limitDict.ContainsKey(key);
        }

        /// <summary>
        /// 从对象池中获取对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns>若对象池中对象不足返回null</returns>
        public object Get(string key)
        {
            Queue<object> list;
            if (_objectDict.TryGetValue(key, out list))
            {
                if (list.Count > 0)
                {
                    object obj = list.Dequeue();
                    if (obj is IRecycle)
                        (obj as IRecycle).OnReUse();
                    return obj;
                }
            }
            return null;
        }

        /// <summary>
        /// 清除某个池
        /// </summary>
        /// <param name="key"></param>
        public void Clear(string key)
        {
            Queue<object> list;
            if (_objectDict.TryGetValue(key, out list))
            {
                int length = list.Count;
                for (int i = 0; i < length; i++)
                {
                    object obj = list.Dequeue();
                    if (obj is GameObject)
                        GameObject.Destroy(obj as GameObject);
                    if (obj is MonoBehaviour)
                        GameObject.Destroy((obj as MonoBehaviour).gameObject);
                }
                _objectDict.Remove(key);
            }
        }

        /// <summary>
        /// 全部清除
        /// </summary>
        public void ClearAll()
        {
            foreach (Queue<object> list in _objectDict.Values)
            {
                int length = list.Count;
                for (int i = 0; i < length; i++)
                {
                    object obj = list.Dequeue();
                    destoryObject(obj);
                }
            }
            _objectDict.Clear();
        }

        /// <summary>
        /// 销毁单个object
        /// </summary>
        /// <param name="obj"></param>
        private void destoryObject(object obj)
        {
            if (obj is GameObject)
                GameObject.Destroy(obj as GameObject);
            if (obj is MonoBehaviour)
                GameObject.Destroy((obj as MonoBehaviour).gameObject);
        }

#if UNITY_EDITOR

        public Dictionary<string, Queue<object>> GetObjectDict()
        {
            return _objectDict;
        }

        public Dictionary<string, int> GetObjectLimitDict()
        {
            return _limitDict;
        }
#endif
    }
}