using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using GYLib.Utils;
using UnityEngine.Advertisements;
using System.Collections.Generic;

/// <summary>
/// 处理激励广告的代理(一次性)
/// </summary>
public class RewardADProxy : MonoBehaviour, IUnityAdsListener
{
    private HashSet<string> _adUnitIdReadyMap = new HashSet<string>();

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
    private string _adUnitId = "rewardedVideo";
#elif UNITY_IPHONE
    private string _adUnitId = "rewardedVideo";
#else
    private string _adUnitId = "unexpected_platform";
#endif

    private void Start()
    {
        Advertisement.AddListener(this);
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

        Debug.Log("placement state : " + _adUnitId + " is " + Advertisement.GetPlacementState(_adUnitId) + ", isLoading = " + _isLoading + ", isLoadFail = " + _isLoadFail);
        if (_isLoadFail && !_isLoading)
        {
            InitRewardAD();
        }
        else if (!GetIsReady(_adUnitId) && !_isLoading)
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
                InitRewardAD();
            }
        }

        //广告是否加载成功
        if (_isLoading && GetIsReady(_adUnitId))
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
        Debug.Log("IsReady : " + GetIsReady(_adUnitId) + ", adUnitId : " + _adUnitId);
        if (GetIsReady(_adUnitId) && _needPlayAD)
        {
            _needPlayAD = false;
            _ADLoadingStartTime = -1;
            _isPlaying = true;
            _adUnitIdReadyMap.Remove(_adUnitId);
            Advertisement.Show(_adUnitId);
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
        if (!GetIsReady(_adUnitId))
        {
            Debug.Log("Start load AD : " + _adUnitId + ", IsReady : " + GetIsReady(_adUnitId));
            _ADLoadingStartTime = TimeUtil.shareRealTimeSincePlay;
            _isLoadFail = false;
            _isLoading = true;
            Advertisement.Load(_adUnitId);
        }
        else
        {
            Debug.Log("Start but is ready : " + _adUnitId);
        }
    }

    /// <summary>
    /// 广告加载完成
    /// </summary>
    /// <param name="placementId"></param>
    public void OnUnityAdsReady(string placementId)
    {
        Debug.Log("OnADsReady : " + placementId);
        _adUnitIdReadyMap.Add(_adUnitId);
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

        if (_needPlayAD || _isPlaying)
        {
            _needPlayAD = false;
            _isPlaying = false;
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
        Debug.Log("ads start");
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
                _isPlaying = false;
                CheckCloseAD();
                OnPlayADEnd();

                _ADLoadingStartTime = -1;
                InitRewardAD();
            }
            else if (showResult == ShowResult.Skipped)
            {
                Debug.Log("Video completed - skip gain no reward");
                _isPlaying = false;
                CheckCloseAD();
                _ADLoadingStartTime = -1;
                InitRewardAD();
            }
            else if (showResult == ShowResult.Failed)
            {
                Debug.Log("Fail to show");
                _isLoadFail = true;
                _isLoading = false;
                _isPlaying = false;
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

    /// <summary>
    /// 广告是否准备完成
    /// </summary>
    /// <param name="adUnitId"></param>
    /// <returns></returns>
    public bool GetIsReady(string adUnitId)
    {
        return _adUnitIdReadyMap.Contains(adUnitId) || Advertisement.IsReady(_adUnitId);
    }

    public string adUnit
    {
        set
        {
            if (_adUnitId != value)
            {
                _adUnitId = value;
                Debug.Log("Change adUnit : " + value);
                if (!Advertisement.isShowing)
                {
                    _isLoading = false;
                    _isLoadFail = false;
                    InitRewardAD();
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (this.gameObject != null)
        {
            Advertisement.RemoveListener(this);
        }
    }
}
