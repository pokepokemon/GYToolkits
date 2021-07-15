using UnityEngine;
using System.Collections;
using GYLib;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.Advertisements;
using GYLib.GYFrame;
using AnyThinkAds.Api;

public class ADManager : MonoSingleton<ADManager>
{
    public bool isInited { get; private set; }

    private List<UnityAction> _actionList = new List<UnityAction>();
    private List<UnityAction> _failActionList = new List<UnityAction>();

    private RewardADProxy _proxy;

    private bool _testMode = false;
#if UNITY_ANDROID
    private const string APP_ID = "a60b88d82cca19";
    private const string APP_KEY = "486b510ae3c00d4f339f3654b429c4e1";
#elif UNITY_IPHONE
    private const string APP_ID = "a60b88d82cca19";
    private const string APP_KEY = "486b510ae3c00d4f339f3654b429c4e1";
#else
    private const string APP_ID = "a60b88d82cca19";
    private const string APP_KEY = "486b510ae3c00d4f339f3654b429c4e1";
#endif

    public void Init()
    {
#if UNITY_EDITOR
        isInited = true;
        return;
#endif
        if (Advertisement.isSupported)
        {
            isInited = true;
            Debug.Log("ADManager init!" + APP_ID + "," + _testMode);

            ATSDKAPI.initSDK(APP_ID, APP_KEY);
            ATSDKAPI.setLogDebug(_testMode);
            GameAnalyticsSDK.GameAnalytics.SubscribeTopOnImpressions();
            CreateRewardAD();
        }
        else
        {
            Debug.Log("Advertisement no support");
        }
    }

    private string _reason;
    /// <summary>
    /// 播放广告后调用callback
    /// </summary>
    /// <param name="callback"></param>
    public void PlayAD(UnityAction callback, string ADReason, UnityAction failCallback = null)
    {
        if (isInited)
        {
            _actionList.Add(callback);
            if (failCallback != null)
            {
                _failActionList.Add(failCallback);
            }
            _reason = ADReason;

#if UNITY_EDITOR
            FinishPlayAD();
            return;
#else
            if (PlayerData.Instance.isPayAD)
            {
                FinishPlayAD();
            }
            else
            {
                if (_proxy != null)
                {
                    GAEventProxy.Instance.ADStart(_reason);
                    _proxy.SetPlayADDirty();
                }
            }
#endif
        }
        else
        {
            CommonUI.ShowAlert("", LocalizationConfig.Instance.GetStringWithSelf("广告模块未能初始化"));
        }
    }

    /// <summary>
    /// 播放广告结束,开始回调
    /// </summary>
    private void FinishPlayAD()
    {
        Debug.Log("finish AD");
        if (_actionList.Count > 0)
        {
            for (int i = 0; i < _actionList.Count; i++)
            {
                _actionList[i].Invoke();
            }
        }

        if (!PlayerData.Instance.isPayAD)
        {
            if (_proxy != null)
            {
                if (!string.IsNullOrEmpty(_proxy.returnADSourceID))
                {
                    Tracking.Instance.setTrackAdShow("unity", _proxy.returnADSourceID, true);
                }
            }
        }
        GAEventProxy.Instance.ADFinish(_reason);
        _reason = null;
        _failActionList.Clear();
        _actionList.Clear();
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
                _proxy.OnPlayFailed = FailToPlayAD;
            }
        }
    }

    /// <summary>
    /// 播放失败回调
    /// </summary>
    private void FailToPlayAD(string reason)
    {
        Debug.Log("Play failed : " + reason);

        GAEventProxy.Instance.ADRewardFailToShow(_reason, reason);
        string titleStr = LocalizationConfig.Instance.GetStringWithSelf("是否要重试播放广告?");
        CommonUI.ShowConfirm("", titleStr, true, delegate ()
        {
            if (_proxy != null)
            {
                _proxy.SetPlayADDirty();
            }
        }, delegate ()
        {
            Debug.Log("Play fail action : " + _failActionList.Count);
            if (_failActionList.Count > 0)
            {
                for (int i = 0; i < _failActionList.Count; i++)
                {
                    _failActionList[i].Invoke();
                }
                _failActionList.Clear();
            }
            _actionList.Clear();
            _reason = null;
        },
        LocalizationConfig.Instance.GetStringWithSelf("重试"),
        LocalizationConfig.Instance.GetStringWithSelf("取消"));
    }
    
}
