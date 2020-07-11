using GYLib;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GYHotfix.Load
{
    internal class LoadAssetbundleManager : MonoSingleton<LoadAssetbundleManager>
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        /// <summary>
        /// 本地文件系统有没有该ab
        /// </summary>
        /// <param name="assetbundleKey"></param>
        /// <returns></returns>
        public bool HasAssetbundleFile(string assetbundleKey)
        {
            string abPath = LoadCenter.Instance.config.GetAssetbundlePath(assetbundleKey);
            return File.Exists(abPath);
        }
    }
}