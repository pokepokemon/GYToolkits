using UnityEngine;
using System.Collections;
using ByteDance.Union;
using GYLib.Utils;
using System.Collections.Generic;

/// <summary>
/// Banner广告代理
/// </summary>
public class InterstitialADProxy : MonoBehaviour
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

#if UNITY_IPHONE
    private ExpressInterstitialAd _intersititialAd;
#else
    private ExpressAd _intersititialAd;
#endif

    public void SetPlayDirty()
    {
        Debug.Log("SetPlayerAD");
        _needShow = true;
        if (!_isLoading && _intersititialAd != null)
        {
            CloseAD();
        }
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
            if (_isLoading && _intersititialAd != null)
            {
                _isLoading = false;
            }

            //未开始加载
            if (_needShow)
            {
                if (!_isLoading && _intersititialAd == null)
                {
                    _isLoading = true;
                    LoadIntersititialAd();
                }
            }

            //是否播放或停止
            if (_needShow)
            {
                if (!_isShowing && _intersititialAd != null)
                {
                    ShowIntersititialAd();
                }
            }
            else
            {
                if (_intersititialAd != null)
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
    public void LoadIntersititialAd()
    {
#if UNITY_IOS
        if (this._iosBannerAd != null)
        {
            Debug.LogError("AD already loaded!");
            return;
        }
#else
        if (this._intersititialAd != null)
        {
            Debug.LogError("AD already loaded!");
            return;
        }
#endif

        Debug.Log("Load Intersititial AD!");
        var adSlot = new AdSlot.Builder()
#if UNITY_IOS
                             .SetCodeId(_adUnitId)
                             .SetExpressViewAcceptedSize(200, 300)
#else
                             .SetCodeId(_adUnitId)
                             .SetExpressViewAcceptedSize(350, 0)
                             ////期望模板广告view的size,单位dp，//高度设置为0,则高度会自适应
#endif
                             .SetSupportDeepLink(true)
                             .SetAdCount(1)
                             .SetImageAcceptedSize(1080, 1920)
                             .Build();
        ADManager.Instance.AdNative.LoadExpressInterstitialAd(adSlot, new ExpressAdListener(this, 2));

    }

    /// <summary>
    /// 展示
    /// </summary>

    public void ShowIntersititialAd()
    {
#if UNITY_IOS
        if (intersititialAd == null)
        {
            Debug.LogError("请先加载广告");
            this.information.text = "请先加载广告";
            return;
        }
        _intersititialAd.ShowNativeAd(AdSlotType.InteractionAd);
		_intersititialAd = null;
#else
        if (_intersititialAd == null)
        {
            Debug.LogError("Load AD first!");
            return;
        }

        Debug.Log("Show IntersititialAd");
        this._alreadyShow = true;
        this._isShowing = true;
        _isLoading = false;
        ExpressAdInteractionListener expressAdInteractionListener = new ExpressAdInteractionListener(this, 1);
        this._intersititialAd.SetDownloadListener(
            new AppDownloadListener(this));
        NativeAdManager.Instance().ShowExpressInterstitialAd(ADManager.Instance.activity, _intersititialAd.handle, expressAdInteractionListener);
#endif
    }

    /// <summary>
    /// 关闭广告
    /// </summary>
    public void CloseAD()
    {
        _isShowing = false;
        if (_intersititialAd == null)
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
        _intersititialAd.Dispose();
        _intersititialAd = null;
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
        private InterstitialADProxy _proxy;
        private int type;//0:feed   1:banner  2:interstitial

        public ExpressAdListener(InterstitialADProxy proxy, int type)
        {
            this._proxy = proxy;
            this.type = type;
        }
        public void OnError(int code, string message)
        {
            Debug.LogError("onExpressAdError: " + message + "_" + code);
            _proxy.OnLoadADFail();
        }

        public void OnExpressAdLoad(List<ExpressAd> ads)
        {
            Debug.Log("OnExpressAdLoad");
            IEnumerator<ExpressAd> enumerator = ads.GetEnumerator();
            if (enumerator.MoveNext())
            {
                if (type == 2)
                {
                    this._proxy.CloseAD();
                    this._proxy._intersititialAd = enumerator.Current; 
                }
            }
        }
#if UNITY_IOS

        public void OnExpressBannerAdLoad(ExpressBannerAd ad)
        {
            Debug.Log("OnExpressBannerAdLoad");
        }

        public void OnExpressInterstitialAdLoad(ExpressInterstitialAd ad)
        {
            Debug.Log("OnExpressInterstitialAdLoad");
            ad.SetExpressInteractionListener(
                new ExpressAdInteractionListener(this._proxy, 2));
            ad.SetDownloadListener(
                new AppDownloadListener(this._proxy));
            this._proxy._intersititialAd = ad;
        }
#else
#endif
    }


    private sealed class ExpressAdInteractionListener : IExpressAdInteractionListener
    {
        private InterstitialADProxy _proxy;
        int type;//0:feed   1:banner  2:interstitial

        public ExpressAdInteractionListener(InterstitialADProxy proxy, int type)
        {
            this._proxy = proxy;
            this.type = type;
        }

        public void OnAdClicked(ExpressAd ad)
        {
            Debug.Log("express OnAdClicked,type:" + type);
        }

        public void OnAdShow(ExpressAd ad)
        {
            Debug.Log("express OnAdShow,type:" + type);
        }

        public void OnAdViewRenderError(ExpressAd ad, int code, string message)
        {
            Debug.Log("express OnAdViewRenderError,type:" + type + "_" + code + "_" + message);
            _proxy.CloseAD();
        }

        public void OnAdViewRenderSucc(ExpressAd ad, float width, float height)
        {
            Debug.Log("express OnAdViewRenderSucc,type:" + type);
        }

        public void OnAdClose(ExpressAd ad)
        {
            Debug.Log("express OnAdClose,type:" + type);
            _proxy.CloseAD();
        }
    }


    private sealed class AppDownloadListener : IAppDownloadListener
    {
        private InterstitialADProxy _proxy;

        public AppDownloadListener(InterstitialADProxy proxy)
        {
            this._proxy = proxy;
        }

        public void OnIdle()
        {
        }

        public void OnDownloadActive(
            long totalBytes, long currBytes, string fileName, string appName)
        {
            Debug.Log("OnDownloadActive");
        }

        public void OnDownloadPaused(
            long totalBytes, long currBytes, string fileName, string appName)
        {
            Debug.Log("OnDownloadPaused");
        }

        public void OnDownloadFailed(
            long totalBytes, long currBytes, string fileName, string appName)
        {
            Debug.LogError("OnDownloadFailed");
        }

        public void OnDownloadFinished(
            long totalBytes, string fileName, string appName)
        {
            Debug.Log("OnDownloadFinished");
        }

        public void OnInstalled(string fileName, string appName)
        {
            Debug.Log("OnInstalled");
        }
    }

    #endregion
}
