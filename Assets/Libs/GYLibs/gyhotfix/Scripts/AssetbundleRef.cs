using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GYHotfix.Load
{
    public class AssetbundleRef
    {
        public string key;
        public AssetBundle assetbundle;

        /// <summary>
        /// 被引用的列表
        /// </summary>
        private HashSet<string> _depSet;

        /// <summary>
        /// 引用计数
        /// </summary>
        private int reference;

        private Dictionary<string, int> _assetRefMap;

        /// <summary>
        /// 改变asset的引用计数(直接引用)
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="isIncrease"></param>
        public void ChangeAssetRef(string assetName, bool isIncrease)
        {
            int changeCount = isIncrease ? 1 : -1;
            reference += changeCount;
            if (assetName != null)
            {
                int refCount;
                if (_assetRefMap.TryGetValue(assetName, out refCount))
                {
                    _assetRefMap[assetName] = refCount + changeCount;
                }
                else
                {
                    _assetRefMap[assetName] = changeCount;
                }
                if (_assetRefMap[assetName] < 0)
                {
                    //ERROR
                }
            }
        }

        /// <summary>
        /// 修改被依赖引用
        /// </summary>
        /// <param name="key"></param>
        /// <param name="isIncrease"></param>
        public void ChangeDependencyRef(string key, bool isIncrease)
        {
            bool containsKey = _depSet.Contains(key);
            if (isIncrease)
            {
                if (!containsKey)
                {
                    _depSet.Add(key);
                }
            }
            else
            {
                if (containsKey)
                {
                    _depSet.Remove(key);
                }
            }
        }
    }
}
