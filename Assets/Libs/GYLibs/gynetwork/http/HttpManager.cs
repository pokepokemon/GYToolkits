using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Threading;
using System.IO;
using System.Text;
using System.Linq;
using System.Net.Sockets;
using GYLib;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

namespace GYHttp
{
    public delegate void OnHttpReqCallBack(MemoryStream memoryStream);
    public delegate void OnUrlErrorCallBack(HttpStatusCode errcode, string error);

    public class HttpManager : Singleton<HttpManager> 
    {
        public const int DEFAULT_TIME_OUT = 3000;
        public bool _isRunning = false;

        public int networkStatus = (int)UnityEngine.Application.internetReachability;
        private Thread _thread = null;
        private List<RequestInfo> _requestInfoList = new List<RequestInfo>();
        private List<RequestInfo> _requestInfoRemoveList = new List<RequestInfo>();

        private string _errReport = null;

        //多线程调用
        void LoopHttpReqForThread()
        {
            while (_isRunning)
            {
                bool success = TraceForReq();
                if (success)
                {
                    Thread.Sleep(3);
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private List<RequestInfo> _arrReqList = new List<RequestInfo>();
        /// <summary>
        /// 循环获取列表元素
        /// </summary>
        /// <returns></returns>
        bool TraceForReq()
        {
            _arrReqList.Clear();

            if (_requestInfoList.Count > 0)
            {
                lock (_requestInfoList)
                {
                    for (int i = 0; i < _requestInfoList.Count; i++)
                    {
                        if (_requestInfoList[i].GetLoadingState() != RequestLoadingState.Loading)
                        {
                            continue;
                        }
                        _arrReqList.Add(_requestInfoList[i]);
                    }
                }
            }


            for (int i = 0; i < _arrReqList.Count; i++)
            {
                var dealItem = _arrReqList[i];
                if (dealItem.SendCore(out dealItem.memoryStream))
                {
                    var successed = dealItem.memoryStream == null ? RequestLoadingState.Failed : RequestLoadingState.Successed;
                    dealItem.SetLoadingState(successed);
                }
            }

            return true;
        }

        public void Update()
        {
            var curReport = _errReport;
            if (curReport != null)
            {
                //TO DO Error curReport
                _errReport = null;
            }

            _requestInfoRemoveList.Clear();
            if (_requestInfoList.Count <= 0)
            {
                return;
            }

            lock (_requestInfoList)
            {
                var curTime = Time.realtimeSinceStartup;

                for (int i = 0; i < _requestInfoList.Count; i++)
                {
                    var request_info = _requestInfoList[i];
                    var loadingState = request_info.GetLoadingState();

                    if (loadingState == RequestLoadingState.Successed)
                    {
                        try
                        {
                            if (request_info.callBack != null)
                            {
                                request_info.callBack(request_info.memoryStream);
                            }
                        }
                        catch (Exception ex)
                        {
                            string err = ex.ToString();
                            Debug.LogErrorFormat("Http callback Error, url :{0}, ErrorCode :{1}", request_info.curUrl, err);

                            if (request_info.failCallBack != null)
                            {
                                request_info.failCallBack(request_info.errCode, request_info.errorDesc);
                            }

                            break;
                        }
                        finally
                        {
                            _requestInfoRemoveList.Add(request_info);
                        }
                    }
                    else if (loadingState == RequestLoadingState.Failed)
                    {
                        if (request_info.failCallBack != null)
                        {
                            request_info.failCallBack(request_info.errCode, request_info.errorDesc);
                        }
                        _requestInfoRemoveList.Add(request_info);
                    }
                }

                //Do Clean Here
                for (int i = 0; i < _requestInfoRemoveList.Count; i++)
                {
                    var request = _requestInfoRemoveList[i];
                    request.Dispose();
                    _requestInfoList.Remove(request);
                }
                _requestInfoRemoveList.Clear();
            }
        }

        public bool SendRequest(string url, OnHttpReqCallBack call_back, OnUrlErrorCallBack fail_call_back = null, int time_out = 0,
                string method = "GET", string data = "", Dictionary<string, string> headers = null)
        {
            networkStatus = (int)Application.internetReachability;
            RequestInfo info = new RequestInfo();
            info.timeOut = (time_out == 0) ? DEFAULT_TIME_OUT : time_out;

            if (headers == null)
            {
                headers = new Dictionary<string, string>();
            }

            info.curRawUrl = url;

            info.curUrl = url;
            info.callBack = call_back;
            info.failCallBack = fail_call_back;
            info.tryTimes = 0;
            info.SetLoadingState(RequestLoadingState.Loading);
            info.curMethod = method;
            info.postData = data;
            info.startDownloadTime = Time.realtimeSinceStartup;

            JObject headerJson = new JObject();
            headerJson["device_id"] = SystemInfo.deviceUniqueIdentifier;
            headerJson["device_name"] = SystemInfo.deviceName;
            headerJson["device_model"] = SystemInfo.deviceModel;
            headerJson["bundle_id"] = Application.identifier;
            headers.Add("CSUM", Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(headerJson.ToString())));
            info.headers = headers;
            lock (_requestInfoList)
            {
                _requestInfoList.Add(info);
            }

            return true;
        }

        public void Stop()
        {
            _isRunning = false;
            if (_thread != null)
            {
                if (!_thread.Join(1000))//等待1秒
                {
#if UNITY_IPHONE
				    _thread.Interrupt();
#else
                    _thread.Abort();
#endif
                }
                _thread = null;
            }
        }

        public void Start()
        {
            _isRunning = true;
            if (_thread != null)
            {
                _thread.Abort();
                _thread = null;
            }
            _thread = new Thread(new ThreadStart(LoopHttpReqForThread));
            _thread.IsBackground = true;
            _thread.Start();
        }

        public IEnumerator GetTextureByUrl(string url, UnityAction<Texture2D> callback)
        {
            WWW www = new WWW(url);
            yield return www;

            if (string.IsNullOrEmpty(www.error) && www.isDone)
            {
                callback(www.texture);
            }

            www.Dispose();
            yield break;
        }
    }
}