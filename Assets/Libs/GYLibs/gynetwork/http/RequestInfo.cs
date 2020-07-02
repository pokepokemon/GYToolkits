using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using System.Text;

namespace GYHttp
{
    public class RequestInfo
    {
        public int timeOut = 10000;
        public string curRawUrl = string.Empty;
        public string curUrl = string.Empty;
        public MemoryStream memoryStream = null;
        public OnHttpReqCallBack callBack = null;
        public OnUrlErrorCallBack failCallBack = null;

        public int tryTimes = 0;
        private int _tryLimit = 0;
        public string curMethod = string.Empty;
        public HttpStatusCode errCode = HttpStatusCode.OK;
        public string errorDesc = string.Empty;
        public string postData = string.Empty;
        public Dictionary<string, string> headers = null;
        private RequestLoadingState _loadingState = RequestLoadingState.None;

        public float startDownloadTime = 0;
        private static IPAddress ipLocal = null;

        public void Dispose()
        {
            if (memoryStream != null)
            {
                memoryStream.Close();
                memoryStream.Dispose();
                memoryStream = null;
            }
        }

        public RequestLoadingState GetLoadingState()
        {
            return _loadingState;
        }

        public void SetLoadingState(RequestLoadingState nt)
        {
            lock (this)
            {
                _loadingState = nt;
            }
        }

        private void CheckNetError()
        {
            if ((int)errCode >= 500)
            {
                if (tryTimes >= 2)
                {
                    //策略跑完一个循环还是不能连接，只能是终止连接了
                    _tryLimit = -1;
                    return;
                }

                _tryLimit = tryTimes;
            }
        }

        private void UploadError()
        {
            //上报错误码
            //先拿IP
        }

        public bool SendCore(out MemoryStream memoryStream)
        {
            memoryStream = null;
            HttpWebRequest request = null;

            if (tryTimes > _tryLimit)
            {
                return true;
            }

            //发送
            try
            {

#if NETWORK_LOG
				Debug.LogWarning(string.Format("Sending Http Request :{0}", mCurUrl));
#endif
                //开始创建WebRequest
                request = WebRequest.Create(curUrl) as HttpWebRequest;
                request.Timeout = timeOut;
                request.MaximumAutomaticRedirections = 4;
                request.MaximumResponseHeadersLength = 4;
                request.Credentials = CredentialCache.DefaultCredentials;


                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> entry in headers)
                    {
                        request.Headers[entry.Key] = entry.Value;
                    }
                }

                //发送准备
                if (curMethod == "GET")
                {
                    request.Method = "GET";
                }
                else if (curMethod == "POST")
                {
                    request.Method = "POST";
                    var requestStream = request.GetRequestStream();
                    byte[] data_by = UTF8Encoding.UTF8.GetBytes(postData);
                    requestStream.Write(data_by, 0, data_by.Length);
                    requestStream.Close();
                }

                if (request == null)
                {
                    errCode = HttpStatusCode.OK;
                    errorDesc = "request == null";
                    SetLoadingState(RequestLoadingState.Failed);
#if NETWORK_LOG
                    Debug.LogError("Create HttpWebRequest Failed" + mCurUrl);
#endif
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                errorDesc = "catch request:" + ex.Message;

                tryTimes++;
                if (request != null)
                {
                    request.Abort();
                }
#if NETWORK_LOG
                Debug.LogError("Create HttpWebRequest Exception" + mCurUrl + "\n" + ex);
#endif
                return false;
            }

            //接收
            HttpWebResponse response = null;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
                if (response == null)
                {
                    errorDesc = "response == null";
                    //先处理错误
                    SetLoadingState(RequestLoadingState.Failed);
                    return false;
                }
                errCode = HttpStatusCode.OK;
            }
            catch (System.Exception ex)
            {
                if (response != null)
                {
                    errCode = response.StatusCode;
                    errorDesc = response.StatusDescription;
                    response.Close();
                    response = null;
                }
                else
                {
                    errCode = HttpStatusCode.BadGateway;
                    errorDesc = "catch response:" + ex.Message;
                }

                tryTimes++;
                if (request != null)
                {
                    request.Abort();
                }
                CheckNetError();
                UploadError();
                return false;
            }

            //检测回包内容
            Stream responseStream = null;
            try
            {
                responseStream = response.GetResponseStream();
                if (responseStream == null)
                {
                    errorDesc = "responseStream == null";
                    //should never in here
                    SetLoadingState(RequestLoadingState.Failed);
                    return false;
                }
            }
            catch (System.Exception ex)
            {
                errorDesc = "catch responseStream:" + ex.Message;

                tryTimes++;
                responseStream.Close();
                responseStream.Dispose();
                responseStream = null;
                if (request != null)
                {
                    request.Abort();
                }
#if NETWORK_LOG
                Debug.LogError("[Http Response Exception]" + mCurUrl + "\n" + ex);
#endif
                CheckNetError();
                UploadError();
                return false;
            }

            try
            {
                //开始读取
                memoryStream = new MemoryStream();
                var readStartTime = DateTime.Now.Ticks;
                using (BinaryReader responseStreamReader = new BinaryReader(responseStream))
                {
                    byte[] bytes = responseStreamReader.ReadBytes(1024);
                    while (bytes.Length > 0)
                    {
                        memoryStream.Write(bytes, 0, bytes.Length);
                        bytes = responseStreamReader.ReadBytes(1024);

                        var TimeSpan = new TimeSpan(DateTime.Now.Ticks - readStartTime);
                        if (TimeSpan.Seconds >= 6)
                        {
                            tryTimes = 100;//不重连
                            throw new Exception("read bytes timeout");
                            break;
                        }
                    }
                }

                memoryStream.Position = 0;
                response.Close();
                responseStream.Close();
                responseStream.Dispose();

                response = null;
                responseStream = null;

                request.Abort();
                return true;
            }
            catch (System.Exception ex)
            {
                errorDesc = "catch responseStreamReader:" + ex.Message;

                if (memoryStream != null)
                {
                    memoryStream.Close();
                    memoryStream.Dispose();
                    memoryStream = null;
                }

                if (response != null)
                {
                    response.Close();
                    response = null;
                }

                if (responseStream != null)
                {
                    responseStream.Close();
                    responseStream.Dispose();
                    responseStream = null;
                }

                tryTimes++;
                if (request != null)
                {
                    request.Abort();
                    request = null;
                }
                CheckNetError();
                UploadError();
                return false;
            }
        }
    }
}