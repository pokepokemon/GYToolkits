using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GYHotfix.Load
{
    internal class AssetbundleManager
    {
        public static readonly AssetbundleManager Instance = new AssetbundleManager();

        private Dictionary<string, AssetbundleRef> _abMap;

        public AssetbundleManager()
        {
            _abMap = new Dictionary<string, AssetbundleRef>();
        }
        
        void Update()
        {

        }
        

        /// <summary>
        /// 更改依赖计数
        /// </summary>
        public void ChangeDependencyRef(string key, bool isIncrease)
        {
            string[] depList = LoadCenter.Instance.config.GetDependencyRefs(key);
            if (depList != null)
            {
                foreach (var depKey in depList)
                {
                    AssetbundleRef abRef = GetABRef(depKey);
                    if (abRef != null)
                    {
                        abRef.ChangeDependencyRef(key, isIncrease);
                    }
                }
            }
        }

        /// <summary>
        /// 创建Assetbundle引用类
        /// </summary>
        /// <param name="key"></param>
        /// <param name="ab"></param>
        /// <returns></returns>
        public AssetbundleRef AddABRef(string key, AssetBundle ab)
        {
            AssetbundleRef abRef = new AssetbundleRef();
            abRef.key = key;
            abRef.assetbundle = ab;
            _abMap.Add(key, abRef);

            return abRef;
        }

        /// <summary>
        /// 获取AB引用类
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public AssetbundleRef GetABRef(string key)
        {
            AssetbundleRef abRef;
            if (_abMap.TryGetValue(key, out abRef))
            {
                return abRef;
            }
            return null;
        }
    }
}