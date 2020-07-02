using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using GYLib.Utils;
using UnityEngine.Advertisements;
using ByteDance.Union;

/// <summary>
/// 处理激励广告的代理(一次性)
/// </summary>
public class RewardADProxy : MonoBehaviour
{
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

    private bool _isADLoaded = false;

    private bool _isShowing = false;

#if UNITY_ANDROID
    private string _adUnitId = "945248999";
#elif UNITY_IPHONE
    private string _adUnitId = "945248999";
#else
    private string _adUnitId = "945248999";
#endif

    public RewardVideoAd rewardAd;

    private void Start()
    {
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


        if (_isLoadFail && !_isLoading)
        {
            InitRewardAD();
        }
        else if (!_isADLoaded && !_isLoading)
        {
            InitRewardAD();
        }
    }

    private const float _AD_RETRY_INTERVAL = 40f;
    private const float _AD_LOADING_TIME_OUT = 30f;
    private void Update()
    {
        //加载失败的情况每隔一段时间会开始加载
        if (_ADFailTime != -1 && _isLoadFail && !_isLoading)
        {
            float curTime = TimeUtil.shareRealTimeSincePlay;
            if (curTime - _ADFailTime > _AD_RETRY_INTERVAL)
            {
                InitRewardAD();
            }
        }

        //加载成功并且需要播放广告了
        if (!_isLoadFail && !_isLoading && _needPlayAD)
        {
            StartCallPlayAD();
        }
        if (_isLoading && _isADLoaded)
        {
            _isLoading = false;
            _isLoadFail = false;
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
                _ADLoadingStartTime = -1;
            }
        }
    }

    /// <summary>
    /// 正式播放广告
    /// </summary>
    private void StartCallPlayAD()
    {
        Debug.Log("IsReady : " + _isADLoaded + ", adUnitId : " + _adUnitId);
        bool isReady = _isADLoaded;
        if (_isADLoaded && _needPlayAD)
        {
            _needPlayAD = false;
            _isADLoaded = false;
            ShowRewardAd();
        }
    }

    /// <summary>
    /// 检查并关闭广告遮罩
    /// </summary>
    private void CheckCloseAD()
    {
        if (_maskID != -1)
        {
            CommonUI.CloseUIMask(_maskID);
            _maskID = -1;
            BgmManager.Instance.Resume();
            _isShowing = false;
            if (OnPlayClosed != null)
            {
                OnPlayClosed.Invoke();
            }
        }

        if (this.rewardAd != null)
        {
            this.rewardAd.Dispose();
            this.rewardAd = null;
        }
    }

    /// <summary>
    /// 初始化奖励广告
    /// </summary>
    public void InitRewardAD()
    {
        if (!_isADLoaded)
        {
            Debug.Log("Start load AD : " + _adUnitId + ", IsReady : " + _isADLoaded);
            _ADLoadingStartTime = TimeUtil.shareRealTimeSincePlay;
            _isLoadFail = false;
            _isLoading = true;
            LoadRewardAd();
        }
    }


    /// <summary>
    /// Load the reward Ad.
    /// </summary>
    public void LoadRewardAd()
    {
        if (this.rewardAd != null)
        {
            Debug.LogError("AD already load");
            return;
        }

        var adSlot = new AdSlot.Builder()
#if UNITY_IOS
            .SetCodeId(_adUnitId)
#else
            .SetCodeId(_adUnitId)
#endif
            .SetSupportDeepLink(true)
            .SetImageAcceptedSize(750, 1334)
            .SetRewardName("RewardAD") // 奖励的名称
            .SetRewardAmount(1) // 奖励的数量
            .SetUserID(SystemInfo.deviceUniqueIdentifier) // 用户id,必传参数
            .SetMediaExtra("media_extra") // 附加参数，可选
            .SetOrientation(AdOrientation.Vertical) // 必填参数，期望视频的播放方向
            .Build();

        ADManager.Instance.AdNative.LoadRewardVideoAd(
            adSlot, new RewardVideoAdListener(this));
    }

    /// <summary>
    /// Show the reward Ad.
    /// </summary>
    public void ShowRewardAd()
    {
        if (this.rewardAd == null)
        {
            Debug.LogError("Need to load a AD first!");
            return;
        }
        _isShowing = true;
        this.rewardAd.ShowRewardVideoAd();
    }

    /// <summary>
    /// 广告加载完成
    /// </summary>
    /// <param name="placementId"></param>
    public void OnUnityAdsReady(string placementId)
    {
        if (_adUnitId == placementId)
        {
            _isLoading = false;
            _isLoadFail = false;
            _isADLoaded = true;
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
        _isADLoaded = false;

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
    private void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (placementId == _adUnitId)
        {
            if (showResult == ShowResult.Finished)
            {
                Debug.Log("Video completed - Offer a reward to the player");
                CheckCloseAD();
                OnPlayADEnd();
                InitRewardAD();
            }
            else if (showResult == ShowResult.Skipped)
            {
                Debug.Log("Video completed - skip gain no reward");
                CheckCloseAD();
                InitRewardAD();
            }
            else if (showResult == ShowResult.Failed)
            {
                _isLoadFail = true;
                _ADFailTime = TimeUtil.shareRealTimeSincePlay;
                _ADLoadingStartTime = -1;

                if (_needPlayAD)
                {
                    _needPlayAD = false;
                    CheckCloseAD();
                    OnPlayFailed("Play AD failed");
                }
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
                if (!_isShowing)
                {
                    _isLoading = false;
                    _isLoadFail = true;
                    InitRewardAD();
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (this.gameObject != null)
        {
            if (this.rewardAd != null)
            {
                this.rewardAd.Dispose();
            }
        }
    }

    private enum ShowResult
    {
        Finished,
        Skipped,
        Failed
    }

    /// <summary>
    /// 激励广告监听器
    /// </summary>
    private sealed class RewardVideoAdListener : IRewardVideoAdListener
    {
        private RewardADProxy _proxy;
        private string _unitId;

        public RewardVideoAdListener(RewardADProxy proxy)
        {
            _proxy = proxy;
            _unitId = _proxy._adUnitId;
        }

        public void OnError(int code, string message)
        {
            Debug.LogError("OnRewardError: " + message);
            _proxy.OnUnityAdsDidError(string.Format("errorcode [{0}]: {1}", code, message));
        }

        public void OnRewardVideoAdLoad(RewardVideoAd ad)
        {
            Debug.Log("OnRewardVideoAdLoad");
            _proxy.rewardAd = ad;
            _proxy.OnUnityAdsReady(_unitId);
            ad.SetRewardAdInteractionListener(
                new RewardAdInteractionListener(_proxy, _unitId));
        }

        public void OnExpressRewardVideoAdLoad(ExpressRewardVideoAd ad)
        {
        }

        public void OnRewardVideoCached()
        {
            Debug.Log("OnRewardVideoCached");
        }
    }

    /// <summary>
    /// 激励广告交互
    /// </summary>
    private sealed class RewardAdInteractionListener : IRewardAdInteractionListener
    {
        private RewardADProxy _proxy;
        private string _unitId;

        public RewardAdInteractionListener(RewardADProxy proxy, string unitId)
        {
            this._proxy = proxy;
            this._unitId = unitId;
        }

        public void OnAdShow()
        {
            _proxy.OnUnityAdsDidStart(_unitId);
            Debug.Log("rewardVideoAd show");
        }

        public void OnAdVideoBarClick()
        {
            Debug.Log("rewardVideoAd bar click");
        }

        public void OnAdClose()
        {
            Debug.Log("rewardVideoAd close");
        }

        public void OnVideoComplete()
        {
            Debug.Log("rewardVideoAd complete");
            _proxy.OnUnityAdsDidFinish(_unitId, ShowResult.Skipped);
        }

        public void OnVideoError()
        {
            Debug.LogError("rewardVideoAd error");
            _proxy.OnUnityAdsDidFinish(_unitId, ShowResult.Failed);
        }

        public void OnRewardVerify(
            bool rewardVerify, int rewardAmount, string rewardName)
        {
            Debug.Log("verify:" + rewardVerify + " amount:" + rewardAmount +
                " name:" + rewardName);
            _proxy.OnUnityAdsDidFinish(_unitId, ShowResult.Finished);
        }
    }
}
