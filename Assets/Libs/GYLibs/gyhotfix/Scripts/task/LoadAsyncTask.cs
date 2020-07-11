using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GYHotfix.Load
{
    /// <summary>
    /// 异步加载管线任务
    /// </summary>
    public class LoadAsyncTask
    {
        public LoadStepConst step;

        public string assetName;
        public string assetbundleKey;
        public string url;

        public UnityEngine.Object asset;
        public AssetbundleRef abRef;

        public UnityAction<string, UnityEngine.Object> OnLoadAssetCompleted = null;
        
        public UnityAction<string, AssetBundle> OnLoadABCompleted = null;
        
        public UnityAction<string> OnDownloadCompleted = null;

        private int _priority;
        private string[] _relateABList;

        public void InitTaskByAssetName(string assetNameArg)
        {
            assetName = assetNameArg;
            assetbundleKey = LoadCenter.Instance.config.GetAssetbundleKey(assetName);
            if (string.IsNullOrEmpty(assetbundleKey))
            {
                Debug.LogError($"Can't find assetName [{assetName}]");
                step = LoadStepConst.Error;
            }
            string[] depList = LoadCenter.Instance.config.GetDependencyRefs(assetbundleKey);
            string[] relateABList = new string[depList.Length + 1];
            Array.Copy(depList, relateABList, depList.Length);
            relateABList[depList.Length] = assetbundleKey;

            CheckDownloadCompleted();
        }

        public void InitTaskByAssetBundle(string abNameArg)
        {

        }

        private void CheckDownloadCompleted()
        {
            foreach (string assetbundleKey in _relateABList)
            {
                if (LoadCenter.Instance.config.NeedDownload(assetbundleKey))
                {
                    //有AB没下载完
                    return;
                }
            }
            step = LoadStepConst.WaitingLoadAB;
        }

        private void CheckLoadABCompleted()
        {
            foreach (string assetbundleKey in _relateABList)
            {
                if (AssetbundleManager.Instance.GetABRef(assetbundleKey) == null)
                {
                    return;
                }
            }
            step = LoadStepConst.WaitingLoadAsset;
        }

        private void CheckLoadAsset()
        {
            if (!string.IsNullOrEmpty(assetName))
            {
                if (asset != null)
                {
                    step = LoadStepConst.Completed;
                }
            }
            else
            {
                step = LoadStepConst.Completed;
            }
        }

        private void Clear()
        {
            step = LoadStepConst.None;
            assetName = null;
            asset = null;
            assetbundleKey = null;
            abRef = null;
            url = null;

            OnLoadAssetCompleted = null;
            OnLoadABCompleted = null;
            OnDownloadCompleted = null;

            _priority = -1;
            _relateABList = null;
        }
    }
}