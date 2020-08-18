using UnityEngine;
using System.Collections;
using GYLib.Utils;

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

    private string _intersititialAd = null;

    public void Awake()
    {
        IronSourceEvents.onInterstitialAdReadyEvent += InterstitialAdReadyEvent;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += InterstitialAdLoadFailedEvent;
        IronSourceEvents.onInterstitialAdShowSucceededEvent += InterstitialAdShowSucceededEvent;
        IronSourceEvents.onInterstitialAdShowFailedEvent += InterstitialAdShowFailedEvent;
        IronSourceEvents.onInterstitialAdClickedEvent += InterstitialAdClickedEvent;
        IronSourceEvents.onInterstitialAdOpenedEvent += InterstitialAdOpenedEvent;
        IronSourceEvents.onInterstitialAdClosedEvent += InterstitialAdClosedEvent;
    }

    public void SetPlayDirty()
    {
        Debug.Log("InterstitialADProxy SetPlayerAD");
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

#if !UNITY_EDITOR || true
    private float _lastCheckTime = -1f;
    private const float _CHECK_INTERVAL = 1f;
    private void Update()
    {
        float curTime = TimeUtil.shareRealTimeSincePlay;
        if (curTime - _lastCheckTime > _CHECK_INTERVAL)
        {
            _lastCheckTime = curTime;

            //未开始加载
            if (!_isLoading && _intersititialAd == null)
            {
                _isLoading = true;
                LoadIntersititialAd();
            }

            //加载成功
            if (_isLoading && _intersititialAd != null)
            {
                _isLoading = false;
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
        if (this._intersititialAd != null)
        {
            Debug.LogError("AD already loaded!");
            return;
        }

        Debug.Log("Load Intersititial AD!");
        IronSource.Agent.loadInterstitial();

    }

    /// <summary>
    /// 展示
    /// </summary>

    public void ShowIntersititialAd()
    {
        if (_intersititialAd == null)
        {
            Debug.LogError("Load AD first!");
            return;
        }

        Debug.Log("Show IntersititialAd");
        this._alreadyShow = true;
        this._isShowing = true;
        this._needShow = false;
        _isLoading = false;
        IronSource.Agent.showInterstitial();
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

    private void InterstitialAdReadyEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdReadyEvent");
        if (IronSource.Agent.isInterstitialReady())
        {
            _intersititialAd = "OK";
        }
        else
        {
            OnLoadADFail();
        }
    }

    private void InterstitialAdLoadFailedEvent(IronSourceError error)
    {
        Debug.Log("unity-script: I got InterstitialAdLoadFailedEvent, code: " + error.getCode() + ", description : " + error.getDescription());
        OnLoadADFail();
    }

    private void InterstitialAdShowSucceededEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdShowSucceededEvent");
    }

    private void InterstitialAdShowFailedEvent(IronSourceError error)
    {
        Debug.Log("unity-script: I got InterstitialAdShowFailedEvent, code :  " + error.getCode() + ", description : " + error.getDescription());
        CloseAD();
    }

    private void InterstitialAdClickedEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdClickedEvent");
    }

    private void InterstitialAdOpenedEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdOpenedEvent");

    }

    private void InterstitialAdClosedEvent()
    {
        Debug.Log("unity-script: I got InterstitialAdClosedEvent");
        CloseAD();
    }

    #endregion
}
