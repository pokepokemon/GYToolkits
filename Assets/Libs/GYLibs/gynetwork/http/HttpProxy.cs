using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System;
using LitJson;
using GYLib;
using UnityEngine.Events;
using GYLib.Utils;

namespace GYHttp
{
    /// <summary>
    /// HTTP代理类
    /// </summary>
    public class HttpProxy : MonoSingleton<HttpProxy>
    {
        private void Start()
        {
            HttpManager.Instance.Start();
        }

        private void Update()
        {
            HttpManager.Instance.Update();
        }

        /// <summary>
        /// Get方法接口
        /// </summary>
        /// <param name="url"></param>
        /// <param name="func"></param>
        /// <param name="fail_func"></param>
        public void SendGet(string url, UnityAction<string> func, UnityAction<System.Net.HttpStatusCode, string> fail_func = null, bool needEncrypt = true)
        {
            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                HttpManager.Instance.SendRequest(url,
                    (MemoryStream memoryStream) =>
                    {
                        if (memoryStream == null)
                        {
                            return;
                        }

                        string data_str = TranslateMemoryStream(memoryStream, needEncrypt);
                        if (func != null)
                        {
                            func.Invoke(data_str);
                        }
                    }
                    , (System.Net.HttpStatusCode errcode, string error) =>
                    {
                        if (fail_func != null)
                        {
                            fail_func.Invoke(errcode, error);
                        }
                    }, HttpManager.DEFAULT_TIME_OUT, "GET", "", headers);
            }
            catch (Exception ex)
            {
                Debug.Log(string.Format("Http Get, url :{0}, Exception :{1}", url, ex.ToString()));
            }
        }

        public float sendTime = 0;
        public string _sendData;
        public string _receiveData;
        /// <summary>
        /// Post方法接口
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="func"></param>
        /// <param name="fail_func"></param>
        /// <param name="needEncrypt"></param>
        public void SendPost(string url, string data, UnityAction<string> func, UnityAction<System.Net.HttpStatusCode, string> fail_func = null, bool needEncrypt = true)
        {
            try
            {
                sendTime = Time.realtimeSinceStartup;
                _sendData = data;
                _receiveData = "";

                Dictionary<string, string> headers = new Dictionary<string, string>();
                string requestData = data;
                HttpManager.Instance.SendRequest(url,
                        (MemoryStream memoryStream) =>
                        {
                            if (memoryStream == null)
                            {
                                if (fail_func != null)
                                    fail_func.Invoke(System.Net.HttpStatusCode.NoContent, "memoryStream is null");
                                return;
                            }

                            string data_str = TranslateMemoryStream(memoryStream, needEncrypt);
                            _receiveData = data_str;
                            if (func != null)
                            {
                                func.Invoke(data_str);
                            }
                        }
                    , (System.Net.HttpStatusCode errcode, string error) =>
                    {
                        if (fail_func != null)
                        {
                            fail_func.Invoke(errcode, error);
                        }
                    }, 10000, "POST", requestData, headers);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Http Post, url :{0}, Exception : {1}", url, ex.ToString());
            }
        }

        public void SendHttpMessage(string url, string requestType, UnityAction<string> func, UnityAction<System.Net.HttpStatusCode, string> fail_func = null)
        {
            try
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                DateTime send_time = DateTime.Now;
                HttpManager.Instance.SendRequest(url,
                        (MemoryStream memoryStream) =>
                        {
                            if (memoryStream == null)
                            {
                                if (fail_func != null)
                                    fail_func.Invoke(System.Net.HttpStatusCode.NoContent, "memoryStream is null");
                                return;
                            }

                            string data_str = TranslateMemoryStream(memoryStream, false);
                            if (func != null)
                                func.Invoke(data_str);
                        }
                    , (System.Net.HttpStatusCode errcode, string error) =>
                    {
                        if (fail_func != null)
                            fail_func.Invoke(errcode, error);
                    }, 10000, requestType, "", headers);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Http Send, url :{0}, Exception :{1}", url, ex.ToString());
            }
        }

        public string TranslateMemoryStream(MemoryStream memoryStream, bool needDecrypt = true)
        {
            string data = (System.Text.Encoding.UTF8.GetString(memoryStream.ToArray()));

            return data;
        }

        private void OnApplicationQuit()
        {
            HttpManager.Instance.Stop();
        }
    }
}