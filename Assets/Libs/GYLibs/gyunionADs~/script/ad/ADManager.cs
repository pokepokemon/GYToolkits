using UnityEngine;
using System.Collections;
using GYLib;
using UnityEngine.Events;
using System.Collections.Generic;
using ByteDance.Union;

public class ADManager : MonoSingleton<ADManager>
{
    public bool isInited { get; private set; }

    private UnityAction _singleAction;
    private UnityAction _singleFailAction;

    private RewardADProxy _proxy;
    private BannerADProxy _bannerProxy;
    private FeedADProxy _feedProxy;


    private string _ADReason;

    private bool _testMode = true;
#if UNITY_ANDROID
    private const string GAME_ID = "3512585";
#elif UNITY_IPHONE
    private const string GAME_ID = "3512584";
#else
    private const string GAME_ID = "";
#endif

    private AdNative _adNative;
    public AdNative AdNative
    {
        get
        {
            if (this._adNative == null)
            {
                this._adNative = SDK.CreateAdNative();
            }

            return this._adNative;
        }
    }
    
    private AndroidJavaObject _activity;
    public AndroidJavaObject activity
    {
        get
        {
            if (_activity == null)
            {
                var unityPlayer = new AndroidJavaClass(
                "com.unity3d.player.UnityPlayer");
                _activity = unityPlayer.GetStatic<AndroidJavaObject>(
               "currentActivity");
            }
            return _activity;
        }
    }

#if UNITY_ANDROID

    private AndroidJavaObject _nativeAdManager;
    public AndroidJavaObject GetNativeAdManager()
    {
#if UNITY_EDITOR
        return null;
#endif
        if (_nativeAdManager != null)
        {
            return _nativeAdManager;
        }
        var jc = new AndroidJavaClass(
                    "com.bytedance.android.NativeAdManager");
        _nativeAdManager = jc.CallStatic<AndroidJavaObject>("getNativeAdManager");
        return _nativeAdManager;
    }
#endif

    public void Init()
    {
#if UNITY_EDITOR
        isInited = true;
#else
        isInited = true;

        Debug.Log("ADManager init!");
        CreateRewardAD();
#endif
    }

    /// <summary>
    /// 播放广告后调用callback
    /// </summary>
    /// <param name="callback"></param>
    public void PlayAD(UnityAction callback, string name, UnityAction failCallback = null)
    {
        if (isInited)
        {
            _singleAction = callback;
            _singleFailAction = failCallback;
            _ADReason = name;

#if UNITY_EDITOR
            FinishPlayAD();
            return;
#else
            if (_proxy != null)
            {
                _proxy.SetPlayADDirty();
            }
#endif
        }
        else
        {
            CommonUI.ShowAlert("", LocalizationConfig.Instance.GetStringWithSelf("AD module no inited!"));
        }
    }

    /// <summary>
    /// 播放广告结束,开始回调
    /// </summary>
    private void FinishPlayAD()
    {
        bool isDirty = false;
        string name = null;
        if (_singleAction != null)
        {
            _singleAction.Invoke();
            _singleAction = null;
            _singleFailAction = null;
            name = _ADReason;
            _ADReason = null;
            isDirty = true;
        }
        if (isDirty && !string.IsNullOrEmpty(name))
        {
        }
    }

    /// <summary>
    /// 创建激励广告
    /// </summary>
    private void CreateRewardAD()
    {
        Debug.Log("Create new AD!");
        if (_proxy == null)
        {
            GameObject go = new GameObject("ADProxy");
            if (this.gameObject != null)
            {
                go.transform.SetParent(this.transform, false);
                _proxy = go.AddComponent<RewardADProxy>();
                _proxy.OnPlayADEnd = FinishPlayAD;
                _proxy.OnPlayClosed = CreateRewardAD;
                _proxy.OnPlayFailed = FailToPlayAD;
            }
        }
        else
        {
        }
    }

    /// <summary>
    /// 播放失败回调
    /// </summary>
    private void FailToPlayAD(string reason)
    {
        Debug.Log("Play failed : " + reason);
        string titleStr = LocalizationConfig.Instance.GetStringWithSelf("是否要重试播放广告?");
        CommonUI.ShowConfirm("", titleStr, true, delegate ()
        {
            if (_proxy != null)
            {
                _proxy.SetPlayADDirty();
            }
        }, delegate ()
        {
            _singleAction = null;
            _ADReason = null;
            if (_singleFailAction != null)
            {
                _singleFailAction.Invoke();
            }
            _singleFailAction = null;
        },
        LocalizationConfig.Instance.GetStringWithSelf("重试"),
        LocalizationConfig.Instance.GetStringWithSelf("取消"));
    }

    #region banner

    /// <summary>
    /// 展示Banner
    /// </summary>
    /// <param name="pos">0 top 1 bottom</param>
    public void ShowBanner(int pos = 0)
    {
        GetBannerProxy().SetPlayDirty(pos);
    }

    /// <summary>
    /// 隐藏Banner
    /// </summary>
    public void HideBanner()
    {
        GetBannerProxy().SetCloseDirty();
    }

    /// <summary>
    /// 获取Banner代理
    /// </summary>
    /// <returns></returns>
    public BannerADProxy GetBannerProxy()
    {
        if (_bannerProxy == null)
        {
            GameObject go = new GameObject("ADBannerProxy");
            if (this.gameObject != null)
            {
                go.transform.SetParent(this.transform, false);
                _bannerProxy = go.AddComponent<BannerADProxy>();
            }
        }
        return _bannerProxy;
    }

    #endregion


    #region feed

    /// <summary>
    /// 展示Feed
    /// </summary>
    public void ShowInterstitial()
    {
        GetInterstitialProxy().SetPlayDirty();
    }

    /// <summary>
    /// 隐藏Banner
    /// </summary>
    public void HideInterstitial()
    {
        GetInterstitialProxy().SetCloseDirty();
    }

    /// <summary>
    /// 获取Banner代理
    /// </summary>
    /// <returns></returns>
    public FeedADProxy GetInterstitialProxy()
    {
        if (_feedProxy == null)
        {
            GameObject go = new GameObject("ADFeedProxy");
            if (this.gameObject != null)
            {
                go.transform.SetParent(this.transform, false);
                _feedProxy = go.AddComponent<FeedADProxy>();
            }
        }
        return _feedProxy;
    }

    #endregion
}
