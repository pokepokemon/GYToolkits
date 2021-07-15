using UnityEngine;
using System.Collections;
using AnyThinkAds.Api;
using AnyThinkAds.ThirdParty.MiniJSON;
using System.Collections.Generic;
using UnityEngine.Events;

public class RewardADProxy : MonoBehaviour
{
    private const string STEP_WAITING_LOADING = "WaitingLoading";

    private const string STEP_WAITING_LOADED = "WaitingLoaded";

    private const string STEP_READY = "Ready";
    private const string STEP_PLAYING = "Playing";
    private const string STEP_REWARD = "Reward";

#if UNITY_ANDROID
    public string placementID = "b60b88d9be226e";
#elif UNITY_IOS || UNITY_IPHONE
    public string placementID = "b60b88d9be226e";//"b5b44a0f115321";
#else
    public string placementID = "b60b88d9be226e";
#endif

    public string returnADSourceID;

    private ATRewardedVideo _rewardedVideo;
    private static ATCallbackListener callbackListener;

    public string curStep { private set; get; } = STEP_WAITING_LOADING;
    public const float TIME_OUT = 5f;

    private bool _isNeedShow = false;

    private bool _isClickAD = false;

    private bool _hasReward = false;

    public UnityAction OnPlayADEnd;

    private string _errorMsg = "";
    public UnityAction<string> OnPlayFailed;

    public void SetPlayADDirty()
    {
        Debug.Log("Current AD Status : " + curStep);
        _isNeedShow = true;
        if (!IsADMaskShowing())
        {
            OpenADMask();
        }
    }

    public void SetClickDirty()
    {
        _isClickAD = true;
    }

    public void OnLoadFailed(string error)
    {
        _errorMsg = error;
        Debug.Log("load error : " + error);
        curStep = STEP_WAITING_LOADING;
    }

    public void OnClose()
    {
        curStep = STEP_WAITING_LOADING;
    }


    private float _lastOnlienCheckTime = -30f;
    private const float _CHECK_ONLINE_INTERVAL = 2.5f;
    private void Update()
    {
        float curTime = Time.realtimeSinceStartup;
        if (curStep == STEP_WAITING_LOADING)
        {
            if (curTime - _lastOnlienCheckTime > _CHECK_ONLINE_INTERVAL)
            {
                _lastOnlienCheckTime = curTime;

                UpdateSwitchStatus();
            }
            UpdateForCheckMask();
        }
        else
        {
            _lastOnlienCheckTime = curTime;
            UpdateSwitchStatus();
            UpdateForCheckMask();
        }
        UpdateForCheckClickAD();
    }

    private void UpdateForCheckMask()
    {
        if (IsADMaskShowing())
        {
            float curTime = Time.realtimeSinceStartup;
            if (curTime - _lastOpenMaskTime >= TIME_OUT)
            {
                CloseADMask();
                _isNeedShow = false;
                OnPlayFailed?.Invoke(!string.IsNullOrEmpty(_errorMsg) ? _errorMsg : "TimeOut");
                _errorMsg = string.Empty;
            }
        }
    }

    private void UpdateForCheckClickAD()
    {
        if (_isClickAD)
        {
            if (!string.IsNullOrEmpty(returnADSourceID))
            {
                Tracking.Instance.setTrackAdClick("unity", returnADSourceID);
            }
            _isClickAD = false;
        }
    }

    #region status

    private void UpdateSwitchStatus()
    {
        switch (curStep)
        {
            case STEP_WAITING_LOADING:
                UpdateWhenWaitingLoading();
                break;
            case STEP_WAITING_LOADED:
                UpdateWhenWaitingPlay();
                break;
            case STEP_READY:
                UpdateWhenReady();
                break;
            case STEP_PLAYING:
                break;
            case STEP_REWARD:
                UpdateWhenReward();
                break;
        }
    }

    private void UpdateWhenWaitingLoading()
    {
        curStep = STEP_WAITING_LOADED;
        LoadVideo();
    }

    private void UpdateWhenWaitingPlay()
    {
        bool result = ATRewardedVideo.Instance.hasAdReady(placementID);
        if (result)
        {
            curStep = STEP_READY;
        }
    }

    private void UpdateWhenReady()
    {
        if (_isNeedShow)
        {
            _isNeedShow = false;
            CloseADMask();
            ShowVideo();
        }
    }

    private void UpdateWhenPlaying()
    {
    }
    
    private void UpdateWhenReward()
    {
        if (curStep == STEP_REWARD)
        {
            OnPlayADEnd();
            curStep = STEP_WAITING_LOADING;
        }
    }

    #endregion

    public void OnRewardGot()
    {
        curStep = STEP_REWARD;
    }

    private int _maskID = -1;
    private float _lastOpenMaskTime = -1f;
    private void OpenADMask()
    {
        CloseADMask();
        float curTime = Time.realtimeSinceStartup;
        _lastOpenMaskTime = curTime;

        _maskID = CommonUI.ShowUIMask(LocalizationConfig.Instance.GetStringWithSelf("广告加载中"));
    }

    private void CloseADMask()
    {
        if (_maskID != -1)
        {
            CommonUI.CloseUIMask(_maskID);
            _maskID = -1;
            _lastOpenMaskTime = -1f;
        }
        _errorMsg = "";
    }

    private bool IsADMaskShowing()
    {
        return _maskID != -1;
    }

    private void LoadVideo()
    {
        if (callbackListener == null)
        {
            callbackListener = new ATCallbackListener(this);
            Debug.Log("Developer init video....");
            ATRewardedVideo.Instance.setListener(callbackListener);
        }

        Dictionary<string, string> jsonmap = new Dictionary<string, string>();
        ATRewardedVideo.Instance.loadVideoAd(placementID, jsonmap);
    }

    private void ShowVideo()
    {
        curStep = STEP_PLAYING;

        Debug.Log("Show video");
        Dictionary<string, string> jsonmap = new Dictionary<string, string>();
        ATRewardedVideo.Instance.showAd(placementID, jsonmap);
    }

    /// <summary>
    /// 广告就绪
    /// </summary>
    /// <param name="adUnit"></param>
    /// <returns></returns>
    public bool IsADReady(string adUnit)
    {
        bool result = ATRewardedVideo.Instance.hasAdReady(placementID);
        return result;
    }
}

class ATCallbackListener : ATRewardedVideoListener
{
    private RewardADProxy _proxy;

    public ATCallbackListener(RewardADProxy proxy)
    {
        _proxy = proxy;
    }

    public void onRewardedVideoAdLoaded(string placementId)
    {
    }

    public void onRewardedVideoAdLoadFail(string placementId, string code, string message)
    {
        _proxy.OnLoadFailed("Loading:" + code + " , " + message);
    }

    public void onRewardedVideoAdPlayStart(string placementId, ATCallbackInfo callbackInfo)
    {
        _proxy.returnADSourceID = callbackInfo.adsource_id;
    }

    public void onRewardedVideoAdPlayEnd(string placementId, ATCallbackInfo callbackInfo)
    {
    }

    public void onRewardedVideoAdPlayFail(string placementId, string code, string message)
    {
        _proxy.OnLoadFailed("Playing:" + code + " , " + message);
    }

    public void onRewardedVideoAdPlayClosed(string placementId, bool isReward, ATCallbackInfo callbackInfo)
    {
        Debug.Log("onRewardedVideoAdPlayClosed : " + isReward.ToString());
        if (!isReward)
        {
            _proxy.OnClose();
        }
    }

    public void onRewardedVideoAdPlayClicked(string placementId, ATCallbackInfo callbackInfo)
    {
        _proxy.SetClickDirty();
    }

    public void onReward(string placementId, ATCallbackInfo callbackInfo)
    {
        _proxy.OnRewardGot();
    }
}