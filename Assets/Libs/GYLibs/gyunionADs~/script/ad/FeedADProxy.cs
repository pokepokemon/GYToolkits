using UnityEngine;
using System.Collections;
using ByteDance.Union;
using GYLib.Utils;
using System.Collections.Generic;

/// <summary>
/// Feed 信息流广告
/// </summary>
public class FeedADProxy : MonoBehaviour
{
    private bool _needShow;

    private bool _isLoading;

    private bool _isShowing;

    private bool _alreadyShow;

#if UNITY_ANDROID
    private string _adUnitId = "945251448";
#elif UNITY_IPHONE
    private string _adUnitId = "945251448";
#else
    private string _adUnitId = "945251448";
#endif

    private object _iosFeedAd;
    private ExpressAd _feedAd;
    
    /// <summary>
    /// 展示
    /// </summary>
    public void SetPlayDirty()
    {
        Debug.Log("SetPlayerAD");
        _needShow = true;
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void SetCloseDirty()
    {
        _needShow = false;
        CloseAD();
    }

    /// <summary>
    /// 设置广告单元ID
    /// </summary>
    /// <param name="unitId"></param>
    public void SetAdUnitId(string unitId)
    {
        _adUnitId = unitId;
    }

#if !UNITY_EDITOR
    private float _lastCheckTime = -1f;
    private const float _CHECK_INTERVAL = 1f;
    private void Update()
    {
        float curTime = TimeUtil.shareRealTimeSincePlay;
        if (curTime - _lastCheckTime > _CHECK_INTERVAL)
        {
            _lastCheckTime = curTime;

            //加载成功
            if (_isLoading && (_feedAd != null || _iosFeedAd != null))
            {
                _isLoading = false;
            }

            //未开始加载
            if (_needShow)
            {
                if (!_isLoading && (_feedAd == null && _iosFeedAd == null))
                {
                    _isLoading = true;
                    LoadFeedAd();
                }
            }

            //是否播放或停止
            if (_needShow)
            {
                if (!_isShowing && (_feedAd != null || _iosFeedAd != null))
                {
                    ShowFeedAd();
                }
            }
            else
            {
                if (_feedAd != null || _iosFeedAd != null)
                {
                    CloseAD();
                }
            }
        }
    }
#endif

    /// <summary>
    /// 加载广告
    /// </summary>
    public void LoadFeedAd()
    {
#if UNITY_IOS
        if (this._iosFeedAd != null)
        {
            Debug.LogError("AD already loaded!");
            return;
        }
#else
        if (this._feedAd != null)
        {
            Debug.LogError("AD already loaded!");
            return;
        }
#endif
        
        var adSlot = new AdSlot.Builder()
#if UNITY_IOS
                             .SetCodeId(_adUnitId)                   
#else
                             .SetCodeId(_adUnitId)
                             ////期望模板广告view的size,单位dp，//高度设置为0,则高度会自适应
                             .SetExpressViewAcceptedSize(350, 0)
#endif
                             .SetSupportDeepLink(true)
                             .SetImageAcceptedSize(1080, 1920)
                             .SetOrientation(AdOrientation.Horizontal)
                             .SetAdCount(3) //请求广告数量为1到3条
                             .Build();

        this._alreadyShow = true;
        ADManager.Instance.AdNative.LoadNativeExpressAd(adSlot, new ExpressAdListener(this));
    }

    /// <summary>
    /// 展示
    /// </summary>
    public void ShowFeedAd()
    {
        _isShowing = true;
#if UNITY_IOS
       if (_iosFeedAd == null)
        {
            Debug.LogError("Load AD first!");
            return;
        }
        
        this._iosFeedAd.SetExpressInteractionListener(
               new ExpressAdInteractionListener(this));
        this._iosFeedAd.SetDownloadListener(
            new AppDownloadListener(this));
        this._iosFeedAd.ShowExpressAd(5,100);
#else
        if (_feedAd == null)
        {
            Debug.LogError("Load AD first!");
            return;
        }

        Debug.Log("Show AD! _needShow " + _needShow);
        ExpressAdInteractionListener expressAdInteractionListener = new ExpressAdInteractionListener(this);
        ExpressAdDislikeCallback dislikeCallback = new ExpressAdDislikeCallback(this);
        
        this._feedAd.SetDownloadListener(
            new AppDownloadListener(this));

        this._alreadyShow = true;
        NativeAdManager.Instance().ShowExpressFeedAd(ADManager.Instance.activity, _feedAd.handle, expressAdInteractionListener, dislikeCallback);
#endif
    }

    /// <summary>
    /// 关闭广告
    /// </summary>
    public void CloseAD()
    {
        _isShowing = false;
        if (_feedAd == null)
        {
            return;
        }
        else
        {
            if (_isLoading)
            {
                _isLoading = false;
            }
            if (!this._alreadyShow)
            {
                return;
            }
        }
        NativeAdManager.Instance().DestoryExpressAd(_feedAd.handle);
        _feedAd = null;
        _alreadyShow = false;
        Debug.Log("Close AD");
    }
    
    /// <summary>
    /// 加载广告失败 
    /// </summary>
    public void OnLoadADFail()
    {
        _isLoading = false;
    }

#region Listener

    private sealed class ExpressAdListener : IExpressAdListener
    {
        private FeedADProxy _proxy;

        public ExpressAdListener(FeedADProxy proxy)
        {
            this._proxy = proxy;
        }

        public void OnError(int code, string message)
        {
            Debug.LogError(string.Format("errorcode [{0}]: {1}", code, message));
            _proxy.OnLoadADFail();
        }

        public void OnExpressAdLoad(List<ExpressAd> ads)
        {
            Debug.Log("OnExpressAdLoad");
            IEnumerator<ExpressAd> enumerator = ads.GetEnumerator();
            if (enumerator.MoveNext())
            {
                this._proxy._alreadyShow = false;
                this._proxy._feedAd = enumerator.Current;
            }
        }
#if UNITY_IOS
#else
#endif
    }


    private sealed class ExpressAdDislikeCallback : IDislikeInteractionListener
    {
        private FeedADProxy _proxy;

        public ExpressAdDislikeCallback(FeedADProxy proxy)
        {
            this._proxy = proxy;
        }
        public void OnCancel()
        {
            Debug.Log("express dislike OnCancel");
        }

        public void OnSelected(int var1, string var2)
        {
            Debug.Log("express dislike OnSelected:" + var2);
#if UNITY_IOS
#else
            _proxy.SetCloseDirty(); 
#endif
        }
    }


    private sealed class ExpressAdInteractionListener : IExpressAdInteractionListener
    {
        private FeedADProxy _proxy;

        public ExpressAdInteractionListener(FeedADProxy proxy)
        {
            this._proxy = proxy;
        }
        public void OnAdClicked(ExpressAd ad)
        {
            Debug.Log("express OnAdClicked");
        }

        public void OnAdShow(ExpressAd ad)
        {
            Debug.Log("express OnAdShow");
        }

        public void OnAdViewRenderError(ExpressAd ad, int code, string message)
        {
            Debug.LogError(string.Format("express OnAdViewRenderError [{0}] : {1}", code, message));
        }

        public void OnAdViewRenderSucc(ExpressAd ad, float width, float height)
        {
            Debug.Log("express OnAdViewRenderSucc : " + width + "_" + height);
        }

        public void OnAdClose(ExpressAd ad)
        {
            Debug.Log("express OnAdClose" );
        }
    }


    private sealed class AppDownloadListener : IAppDownloadListener
    {
        private FeedADProxy _proxy;

        public AppDownloadListener(FeedADProxy proxy)
        {
            this._proxy = proxy;
        }

        public void OnIdle()
        {
        }

        public void OnDownloadActive(
            long totalBytes, long currBytes, string fileName, string appName)
        {
            Debug.Log("Downloading...Click to pause");
        }

        public void OnDownloadPaused(
            long totalBytes, long currBytes, string fileName, string appName)
        {
            Debug.Log("Download pause, Click to continue...");
        }

        public void OnDownloadFailed(
            long totalBytes, long currBytes, string fileName, string appName)
        {
            Debug.LogError("Download failed, click to try again!");
        }

        public void OnDownloadFinished(
            long totalBytes, string fileName, string appName)
        {
            Debug.Log("Download Completed, click to download again...");
        }

        public void OnInstalled(string fileName, string appName)
        {
            Debug.Log("Install completed.");
        }
    }

#endregion
}
