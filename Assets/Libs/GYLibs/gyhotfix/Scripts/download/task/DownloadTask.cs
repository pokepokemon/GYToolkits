using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using GYLib;
using CurlUnity;

namespace GYLib.Hotfix
{
    /// <summary>
    /// 下载任务
    /// </summary>
    public class DownloadTask : IRecycle
    {
        public string url;
        public string savePath;

        public UnityAction<string> callback;
        public UnityAction<string, string> errorCallback;
        public bool isCompleted { get; private set; }
        public bool isError { get; private set; }
        public bool isDownloading { get; private set; }
        public string errorCode { get; private set; }
        public int priority { get; private set; }

        /// <summary>
        /// 下载器
        /// </summary>
        private CurlEasy easy;

        public DownloadTask()
        {
            Reset();
        }

        /// <summary>
        /// 重置各变量状态
        /// </summary>
        public void Reset()
        {
            isError = false;
            isCompleted = false;
            callback = null;
            errorCallback = null;
            url = null;
            isDownloading = false;
            errorCode = null;

            if (easy != null)
            {
                easy.Dispose();
                easy = null;
            }
        }

        public void Start()
        {
            easy = new CurlEasy();
            easy.uri = new System.Uri(url);

            Debug.Log("start dl : " + url);
            easy.outputPath = savePath;
            easy.debug = false;
            easy.connectionTimeout = 3000;
            easy.performCallback = OnDownloadCompleted;
            easy.skipDump = true;
            isDownloading = true;

            easy.MultiPerform(DownloadTaskManager.Instance.multi);
        }

        /// <summary>
        /// 设置优先级
        /// </summary>
        /// <param name="p"></param>
        /// <returns>isChange</returns>
        public bool SetPriority(int p)
        {
            if (p > priority)
            {
                priority = p;
                return true;
            }
            return false;
        }

        private void OnDownloadProgress(long dltotal, long dlnow, long ultotal, long ulnow, CurlEasy easy)
        {
            Debug.Log($"dlnow {dlnow} total {dltotal}");
        }

        /// <summary>
        /// 下载完成
        /// </summary>
        /// <param name="result"></param>
        /// <param name="easy"></param>
        private void OnDownloadCompleted(CURLE result, CurlEasy easy)
        {
            isDownloading = false;
            isCompleted = true;
            if (result == CURLE.OK)
            {
                isError = false;
                errorCode = null;
            }
            else
            {
                isError = true;
                errorCode = result.ToString();
            }
        }

        /// <summary>
        /// 调用异常回调
        /// </summary>
        public void CallErrorCallback()
        {
            errorCallback(url, errorCode);
        }

        /// <summary>
        /// 调用正常完成回调
        /// </summary>
        public void CallCompletedCallback()
        {
            callback(url);
        }

        #region IRecycle
        public void OnRecycle()
        {
            Reset();
        }

        public void OnReUse()
        {
        }
        #endregion

    }
}