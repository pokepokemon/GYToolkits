using UnityEngine;
using System.Collections;
using GYLib;
using UnityEngine.Events;
using System.Collections.Generic;
using UnityEngine.Advertisements;
using GYLib.GYFrame;

public class ADManager : MonoSingleton<ADManager>
{
    public bool isInited { get; private set; }

    private List<UnityAction> _actionList = new List<UnityAction>();
    private List<UnityAction> _failActionList = new List<UnityAction>();

    private RewardADProxy _proxy;

    private bool _testMode = false;
#if UNITY_ANDROID
    private const string GAME_ID = "caa9b9ed";
#elif UNITY_IPHONE
    private const string GAME_ID = "ca25958d";
#else
    private const string GAME_ID = "";
#endif

    public void Init()
    {
#if UNITY_EDITOR
        isInited = true;
        return;
#endif
        IronSource.Agent.init(GAME_ID, IronSourceAdUnits.REWARDED_VIDEO);
        IronSource.Agent.validateIntegration();
        Debug.Log("Init IronSource");
        isInited = true;
        Debug.Log("ADManager init!" + GAME_ID + "," + _testMode);
        CreateRewardAD();
    }

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

            //_proxy.adUnit = "rewardedVideo";
            ModuleEventManager.instance.dispatchEvent(new MEvent_ADStarted());
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
        if (_actionList.Count > 0)
        {
            for (int i = 0; i < _actionList.Count; i++)
            {
                _actionList[i].Invoke();
            }
        }

        _failActionList.Clear();
        _actionList.Clear();
        if (!PlayerData.Instance.isPayAD)
        {
            PlayerData.Instance.adPlayTimes++;
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
            _proxy.InitRewardAD();
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
        },
        LocalizationConfig.Instance.GetStringWithSelf("重试"),
        LocalizationConfig.Instance.GetStringWithSelf("取消"));
    }
    
    /// <summary>
    /// 展示Banner
    /// </summary>
    /// <param name="pos">0 top 1 bottom</param>
    public void ShowBanner(int pos = 0)
    {
    }

    /// <summary>
    /// 隐藏Banner
    /// </summary>
    public void HideBanner()
    {
    }

    /// <summary>
    /// 展示插页广告
    /// </summary>
    public void ShowInterstitial()
    {
        //_proxy.adUnit = "InterstitialVideo";

        _failActionList.Clear();
        _actionList.Clear();

        ModuleEventManager.instance.dispatchEvent(new MEvent_ADStarted());
        _proxy.SetPlayADDirty();
    }

    /// <summary>
    /// 展示Feed
    /// </summary>
    /// <param name="pos">0 top 1 bottom</param>
    public void ShowFeed()
    {
    }

    /// <summary>
    /// 隐藏Feed
    /// </summary>
    public void HideFeed()
    {
    }

}
