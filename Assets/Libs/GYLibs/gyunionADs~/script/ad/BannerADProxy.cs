using UnityEngine;
using System.Collections;
using ByteDance.Union;
using GYLib.Utils;
using System.Collections.Generic;

/// <summary>
/// Banner广告代理
/// </summary>
public class BannerADProxy : MonoBehaviour
{
    private bool _needShow;

    private bool _isLoading;

    private bool _isShowing;

    private bool _alreadyShow;

#if UNITY_ANDROID
    private string _adUnitId = "945251441";
#elif UNITY_IPHONE
    private string _adUnitId = "945251441";
#else
    private string _adUnitId = "945251441";
#endif

#if UNITY_IOS
    private ExpressBannerAd _iosBannerAd;
#else
    private object _iosBannerAd;
    private ExpressAd _bannerAd;
#endif

    private int _curPos = 0;
    public void SetPlayDirty(int pos = 0)
    {
        Debug.Log("SetPlayerAD");
        _curPos = pos;
        _needShow = true;
    }

    public void SetCloseDirty()
    {
        _needShow = false;
        CloseAD();
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
            if (_isLoading && (_bannerAd != null || _iosBannerAd != null))
            {
                _isLoading = false;
            }

            //未开始加载
            if (_needShow)
            {
                if (!_isLoading && (_bannerAd == null && _iosBannerAd == null))
                {
                    _isLoading = true;
                    LoadBannerAd();
                }
            }

            //是否播放或停止
            if (_needShow)
            {
                if (!_isShowing && (_bannerAd != null || _iosBannerAd != null))
                {
                    ShowBannerAd();
                }
            }
            else
            {
                if (_bannerAd != null || _iosBannerAd != null)
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
    public void LoadBannerAd()
    {
#if UNITY_IOS
        if (this._iosBannerAd != null)
        {
            Debug.LogError("AD already loaded!");
            return;
        }
#else
        if (this._bannerAd != null)
        {
            Debug.LogError("AD already loaded!");
            return;
        }
#endif

        Debug.Log("Load AD!");
        var adSlot = new AdSlot.Builder()
#if UNITY_IOS
            .SetCodeId(_adUnitId)
#else
            .SetCodeId(_adUnitId)
#endif
            .SetExpressViewAcceptedSize(640, 100)
            .SetSupportDeepLink(true)
            .SetImageAcceptedSize(1920, 1080)
            .SetAdCount(1)
            .SetOrientation(AdOrientation.Horizontal)
            .Build();
        _isLoading = true;
        _alreadyShow = true;

        Debug.Log("Set already : " + this._alreadyShow);
        ADManager.Instance.AdNative.LoadExpressBannerAd(adSlot, new ExpressAdListener(this));
        
    }

    /// <summary>
    /// 展示
    /// </summary>
    public void ShowBannerAd()
    {
        _isShowing = true;
#if UNITY_IOS
       if (_iosBannerAd == null)
        {
            Debug.LogError("Load AD first!");
            return;
        }
        this._iosBannerAd.ShowNativeAd();
#else
        if (_bannerAd == null)
        {
            Debug.LogError("Load AD first!");
            return;
        }

        Debug.Log("Show AD! _needShow " + _needShow);
        //设置轮播间隔 30s--120s;不设置则不开启轮播
        //this._bannerAd.SetSlideIntervalTime(30 * 1000);
        ExpressAdInteractionListener expressAdInteractionListener = new ExpressAdInteractionListener(this);
        ExpressAdDislikeCallback dislikeCallback = new ExpressAdDislikeCallback(this);
        
        this._bannerAd.SetDownloadListener(
            new AppDownloadListener(this));

        this._alreadyShow = true;
        //NativeAdManager.Instance().ShowExpressBannerAd(ADManager.Instance.activity, _bannerAd.handle, expressAdInteractionListener, dislikeCallback, _curPos);
        NativeAdManager.Instance().ShowExpressBannerAd(ADManager.Instance.activity, _bannerAd.handle, expressAdInteractionListener, dislikeCallback);
#endif
    }

    /// <summary>
    /// 关闭广告
    /// </summary>
    public void CloseAD()
    {
        _isShowing = false;
        if (_bannerAd == null)
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
        NativeAdManager.Instance().DestoryExpressAd(_bannerAd.handle);
        _bannerAd = null;
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
        private BannerADProxy _proxy;

        public ExpressAdListener(BannerADProxy proxy)
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
                this._proxy._bannerAd = enumerator.Current;
            }
        }
#if UNITY_IOS

        public void OnExpressBannerAdLoad(ExpressBannerAd ad)
        {
            Debug.Log("OnExpressBannerAdLoad");
            this._proxy._iosBannerAd = ad;
        }
#else
#endif
    }


    private sealed class ExpressAdDislikeCallback : IDislikeInteractionListener
    {
        private BannerADProxy _proxy;

        public ExpressAdDislikeCallback(BannerADProxy proxy)
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
        private BannerADProxy _proxy;

        public ExpressAdInteractionListener(BannerADProxy proxy)
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
        private BannerADProxy _proxy;

        public AppDownloadListener(BannerADProxy proxy)
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
