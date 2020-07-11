using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using GYLib.Utils;

/// <summary>
/// 处理激励广告的代理(一次性)
/// </summary>
public class RewardADProxy : MonoBehaviour
{
    /// <summary>
    /// 是否正在播放广告
    /// </summary>
    private bool _isPlaying = false;

    /// <summary>
    /// 是否正在加载广告中
    /// </summary>
    private bool _isLoading = false;

    /// <summary>
    /// 上次加载广告失败
    /// </summary>
    private bool _isLoadFail = false;

    /// <summary>
    /// 加载失败时间
    /// </summary>
    private float _ADFailTime = -1;

    /// <summary>
    /// 开始加载时间点
    /// </summary>
    private float _ADLoadingStartTime = -1;

    /// <summary>
    /// 播放标志位,为true时只要一加载完广告立刻播放
    /// </summary>
    private bool _needPlayAD = false;

    public UnityAction OnPlayADEnd;

    public UnityAction OnPlayClosed;

    public UnityAction<string> OnPlayFailed;

    private int _maskID = -1;
    
#if UNITY_ANDROID
    private string _adUnitId = "DefaultRewardedVideo";
#elif UNITY_IPHONE
    private string _adUnitId = "DefaultRewardedVideo";
#else
    private string _adUnitId = "unexpected_platform";
#endif

    private void Start()
    {
        IronSourceEvents.onRewardedVideoAdOpenedEvent += RewardedVideoAdOpenedEvent;
        IronSourceEvents.onRewardedVideoAdClosedEvent += RewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += RewardedVideoAvailabilityChangedEvent;
        IronSourceEvents.onRewardedVideoAdStartedEvent += RewardedVideoAdStartedEvent;
        IronSourceEvents.onRewardedVideoAdEndedEvent += RewardedVideoAdEndedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += RewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += RewardedVideoAdShowFailedEvent;

        InitRewardAD();
    }

    /// <summary>
    /// 若加载完成则播放广告
    /// </summary>
    public void SetPlayADDirty()
    {
        _needPlayAD = true;
        string content = LocalizationConfig.Instance.GetStringWithSelf("广告加载中...");
        _maskID = CommonUI.ShowUIMask(content);
        
        bool available = IronSource.Agent.isRewardedVideoAvailable();
        if (_isLoadFail && !_isLoading)
        {
            InitRewardAD();
        }
        else if (!available && !_isLoading)
        {
            InitRewardAD();
        }
    }

    private const float _AD_RETRY_INTERVAL = 28f;
    private const float _AD_LOADING_TIME_OUT = 18f;
    private void Update()
    {
        //加载失败的情况每隔一段时间会开始加载
        if (_ADFailTime != -1 && _isLoadFail && !_isLoading)
        {
            float curTime = TimeUtil.shareRealTimeSincePlay;
            if (curTime - _ADFailTime > _AD_RETRY_INTERVAL)
            {
                _ADFailTime = -1;
                InitRewardAD();
            }
        }

        
        bool available = IronSource.Agent.isRewardedVideoAvailable();
        if (_isLoading && available)
        {
            _isLoading = false;
            _isLoadFail = false;
            _ADLoadingStartTime = -1;
        }
        //加载成功并且需要播放广告了
        if (!_isLoadFail && !_isLoading && _needPlayAD)
        {
            StartCallPlayAD();
        }

        if (_isLoading && _ADLoadingStartTime != -1)
        {
            float curTime = TimeUtil.shareRealTimeSincePlay;
            if (curTime - _ADLoadingStartTime > _AD_LOADING_TIME_OUT)
            {
                CheckCloseAD();
                if (_needPlayAD)
                {
                    OnPlayFailed("Load AD time out");
                    _needPlayAD = false;
                }
                _isLoading = false;
                _isLoadFail = true;
                _ADLoadingStartTime = -1;
            }
        }
    }

    /// <summary>
    /// 正式播放广告
    /// </summary>
    private void StartCallPlayAD()
    {
        bool available = IronSource.Agent.isRewardedVideoAvailable();
        Debug.Log("IsReady : " + available + ", adUnitId : " + _adUnitId);
        if (available && _needPlayAD)
        {
            _needPlayAD = false;
            _ADLoadingStartTime = -1;
            _isPlaying = true;
            IronSource.Agent.showRewardedVideo(_adUnitId);
        }
    }

    /// <summary>
    /// 检查并关闭广告遮罩
    /// </summary>
    private void CheckCloseAD()
    {
        _isPlaying = false;
        if (_maskID != -1)
        {
            CommonUI.CloseUIMask(_maskID);
            _maskID = -1;
            BgmManager.Instance.Resume();
            if (OnPlayClosed != null)
            {
                OnPlayClosed.Invoke();
            }
        }
    }

    /// <summary>
    /// 初始化奖励广告
    /// </summary>
    public void InitRewardAD()
    {
        bool available = IronSource.Agent.isRewardedVideoAvailable();
        if (!available)
        {
            Debug.Log("Start load AD : " + _adUnitId + ", IsReady : " + available);
            _ADLoadingStartTime = TimeUtil.shareRealTimeSincePlay;
            _isLoadFail = false;
            _isLoading = true;
            //Advertisement.Load(_adUnitId);
        }
    }

    /// <summary>
    /// 广告加载完成
    /// </summary>
    /// <param name="placementId"></param>
    public void OnUnityAdsReady(string placementId)
    {
        Debug.Log("ad placement isReady : " + placementId);
        if (_adUnitId == placementId)
        {
            _isLoading = false;
            _isLoadFail = false;
            if (_needPlayAD)
            {
                StartCallPlayAD();
            }
        }
    }

    /// <summary>
    /// 广告出错
    /// </summary>
    /// <param name="message"></param>
    public void OnUnityAdsDidError(string message)
    {
        Debug.Log("HandleADFailedToLoad");
        _isLoadFail = true;
        _ADFailTime = TimeUtil.shareRealTimeSincePlay;
        _ADLoadingStartTime = -1;
        _isLoading = false;

        if (_needPlayAD)
        {
            _needPlayAD = false;
            CheckCloseAD();
            OnPlayFailed("load AD failed : " + message);
        }
    }

    /// <summary>
    /// 广告开始
    /// </summary>
    /// <param name="placementId"></param>
    public void OnUnityAdsDidStart(string placementId)
    {
        if (placementId == _adUnitId)
        {
            BgmManager.Instance.Pause();
            _isLoading = false;
            _isLoadFail = false;
        }
    }
    
    /// <summary>
    /// 广告播放完成(或播放失败)
    /// </summary>
    /// <param name="placementId"></param>
    /// <param name="showResult"></param>
    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (placementId == _adUnitId)
        {
            if (showResult == ShowResult.Finished)
            {
                Debug.Log("Video completed - Offer a reward to the player");
                CheckCloseAD();
                OnPlayADEnd();

                _ADLoadingStartTime = -1;
                InitRewardAD();
            }
            else if (showResult == ShowResult.Skipped)
            {
                Debug.Log("Video completed - skip gain no reward");
                CheckCloseAD();
                _ADLoadingStartTime = -1;
                InitRewardAD();
            }
            else if (showResult == ShowResult.Failed)
            {
                _isLoadFail = true;
                _isLoading = false;
                _ADFailTime = TimeUtil.shareRealTimeSincePlay;
                _ADLoadingStartTime = -1;

                if (_needPlayAD)
                {
                    _needPlayAD = false;
                    CheckCloseAD();
                    OnPlayFailed("Play AD failed");
                }
            }
            else if (showResult == ShowResult.Closed)
            {
                CheckCloseAD();
                _ADLoadingStartTime = -1;
            }
        }
    }
    public string adUnit
    {
        set
        {
            if (_adUnitId != value)
            {
                _adUnitId = value;
                if (!_isPlaying)
                {
                    _isLoading = false;
                    _isLoadFail = true;
                    InitRewardAD();
                }
            }
        }
    }

    public enum ShowResult
    {
        Finished = 1,

        Skipped = 2,

        Failed = 3,

        Closed = 4,
    }

    private void OnDestroy()
    {
        if (this.gameObject != null)
        {
            IronSourceEvents.onRewardedVideoAdOpenedEvent -= RewardedVideoAdOpenedEvent;
            IronSourceEvents.onRewardedVideoAdClosedEvent -= RewardedVideoAdClosedEvent;
            IronSourceEvents.onRewardedVideoAvailabilityChangedEvent -= RewardedVideoAvailabilityChangedEvent;
            IronSourceEvents.onRewardedVideoAdStartedEvent -= RewardedVideoAdStartedEvent;
            IronSourceEvents.onRewardedVideoAdEndedEvent -= RewardedVideoAdEndedEvent;
            IronSourceEvents.onRewardedVideoAdRewardedEvent -= RewardedVideoAdRewardedEvent;
            IronSourceEvents.onRewardedVideoAdShowFailedEvent -= RewardedVideoAdShowFailedEvent;
        }
    }

    #region IronSource

    //Invoked when the RewardedVideo ad view has opened.
    //Your Activity will lose focus. Please avoid performing heavy 
    //tasks till the video ad will be closed.
    void RewardedVideoAdOpenedEvent()
    {
    }

    //Invoked when the RewardedVideo ad view is about to be closed.
    //Your activity will now regain its focus.
    void RewardedVideoAdClosedEvent()
    {
        OnUnityAdsDidFinish(_adUnitId, ShowResult.Closed);
    }

    //Invoked when there is a change in the ad availability status.
    //@param - available - value will change to true when rewarded videos are available. 
    //You can then show the video by calling showRewardedVideo().
    //Value will change to false when no videos are available.
    void RewardedVideoAvailabilityChangedEvent(bool available)
    {
        //Change the in-app 'Traffic Driver' state according to availability.
        bool rewardedVideoAvailability = available;
        if (available)
        {
            OnUnityAdsReady(_adUnitId);
        }
    }

    //Invoked when the user completed the video and should be rewarded. 
    //If using server-to-server callbacks you may ignore this events and wait for 
    // the callback from the  ironSource server.
    //@param - placement - placement object which contains the reward data
    void RewardedVideoAdRewardedEvent(IronSourcePlacement placement)
    {
        OnUnityAdsDidFinish(_adUnitId, ShowResult.Finished);
    }

    //Invoked when the Rewarded Video failed to show
    //@param description - string - contains information about the failure.
    void RewardedVideoAdShowFailedEvent(IronSourceError error)
    {
        OnUnityAdsDidFinish(_adUnitId, ShowResult.Failed);
    }

    // ----------------------------------------------------------------------------------------
    // Note: the events below are not available for all supported rewarded video ad networks. 
    // Check which events are available per ad network you choose to include in your build. 
    // We recommend only using events which register to ALL ad networks you include in your build. 
    // ----------------------------------------------------------------------------------------

    //Invoked when the video ad starts playing. 
    void RewardedVideoAdStartedEvent()
    {
    }

    //Invoked when the video ad finishes playing. 
    void RewardedVideoAdEndedEvent()
    {
        OnUnityAdsDidFinish(_adUnitId, ShowResult.Closed);
    }

    #endregion
}
