using GYLib.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// UI上的序列帧播放剪辑
/// </summary>
public class ImageMovieClip : MonoBehaviour
{
    public Image image;
    public SpriteRenderer spriteRenderer;
    /// <summary>
    /// 文件播放路径(目前暂时和数据源耦合)
    /// </summary>
    public string sheetPath;
    
    /// <summary>
    /// 文件名的起始编码 Sheet/aaa_{0}.png 编码不需要从0开始,但是必须连续
    /// </summary>
    public int startNumber = 0;

    /// <summary>
    /// 总帧数
    /// </summary>
    public int totalFrame = 0;

    /// <summary>
    /// 每一帧的播放间隔(s)( 小于等于 0 的情况会自动跳一帧)
    /// </summary>
    public float playInterval = -1f;

    /// <summary>
    /// 自动循环
    /// </summary>
    public bool isLoop = false;

    /// <summary>
    /// 开始时自动调用Play
    /// </summary>
    public bool autoPlay = true;

    /// <summary>
    /// 是否在路径变更的时候从第一帧开始
    /// </summary>
    public bool resetWhenPathChange = true;

    /// <summary>
    /// 仅在加速过快使用
    /// </summary>
    public bool canSkipLastFrame = false;

    /// <summary>
    /// 仅在加速过快使用
    /// </summary>
    public bool canSkipFirstFrame = false;

    private int _currentFrame = -1;

    private bool _isPlaying = false;


    private float _lastFrameTime = -1;

    private string _currentBufferName;
    private int _currentBufferStartFrame;

    public UnityAction OnLastFrame;

    private Dictionary<int, Sprite> _spriteBuffers = new Dictionary<int, Sprite>();

    public delegate Sprite[] DelegateGetResource(string path);
    public DelegateGetResource HandleGetResource;

    // Start is called before the first frame update
    void Start()
    {
        _currentFrame = -1;
        if (autoPlay)
        {
            Play();
        }
    }

    /// <summary>
    /// 是否当前状态下可以开始播放
    /// </summary>
    /// <returns></returns>
    private bool CheckCanPlay()
    {
        if (totalFrame > 0 && !string.IsNullOrEmpty(sheetPath))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 播放
    /// </summary>
    /// <returns></returns>
    public void Play()
    {
        if (CheckCanPlay())
        {
            _isPlaying = true;
            _lastFrameTime = TimeUtil.shareTimeSincePlay;
            _currentFrame = 0;
            RefreshImage();
        }
    }

    /// <summary>
    /// 停止(重置到第一帧开始)
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
        _lastFrameTime = -1f;
        _currentFrame = 0;
        RefreshImage();
    }

    /// <summary>
    /// 暂停
    /// </summary>
    public void Pause()
    {
        _isPlaying = false;
    }

    /// <summary>
    /// 从暂停状态中恢复
    /// </summary>
    public void Resume()
    {
        if (!_isPlaying)
        {
            _isPlaying = true;
            CheckPathChange();
            _lastFrameTime = TimeUtil.shareTimeSincePlay - _lastFrameTime;
        }
    }

    private int _lastFrame = -1;
    // Update is called once per frame
    void Update()
    {
        if (_isPlaying)
        {
            CheckPathChange();
            float curTime = TimeUtil.shareTimeSincePlay;
            if (curTime == 0)
            {
                return;
            }
            if (_lastFrameTime == -1)
            {
                _lastFrameTime = curTime;
                _currentFrame = (_currentFrame + 1) % totalFrame;
            }
            else
            {
                float deltaTime = curTime - _lastFrameTime;
                float interval = playInterval == -1 ? deltaTime : playInterval;
                if (interval <= 0)
                {
                    interval = deltaTime;
                }
                //不需要至少换帧
                int step = Mathf.FloorToInt(deltaTime / interval);
                int resultFrame = (_currentFrame + step);
                int minusOne = (totalFrame - 1);

                if (step != 0)
                {
                    //此帧未补足的时间碎片
                    float lastTimeChip = playInterval > 0 ? deltaTime - (step * playInterval) : 0;
                    if (!isLoop && resultFrame >= minusOne)
                    {
                        //不循环的话直接停在最后一帧
                        _currentFrame = minusOne;
                        _isPlaying = false;
                        _lastFrameTime = -1f;
                    }
                    else if (!canSkipLastFrame && _currentFrame != minusOne && resultFrame >= minusOne)
                    {
                        //不跳过最后一帧的播放
                        _currentFrame = minusOne;
                    }
                    else if (!canSkipFirstFrame && _currentFrame != 0 && resultFrame > minusOne)
                    {
                        //不跳过第一帧的播放
                        _currentFrame = 0;
                    }
                    else
                    {
                        //无限制
                        _currentFrame = resultFrame % totalFrame;

                    }
                    _lastFrameTime = curTime - lastTimeChip;
                }
            }

            RefreshImage();
            if (_currentFrame == totalFrame - 1)
            {
                if (_currentFrame != _lastFrame)
                {
                    if (OnLastFrame != null)
                    {
                        OnLastFrame();
                    }
                }
            }
            _lastFrame = _currentFrame;
        }
    }

    /// <summary>
    /// 根据当前帧刷新图片
    /// </summary>
    private void RefreshImage()
    {
        if (_currentFrame >= 0 && _currentFrame < totalFrame)
        {
            Sprite sprite = GetSpriteByBuffer();
            if (image != null)
            {
                image.sprite = sprite;
            }
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = sprite;
            }
        }
    }

    /// <summary>
    /// 检查是否外部改动了路径
    /// </summary>
    private void CheckPathChange()
    {
        if (_currentBufferName != sheetPath || startNumber != _currentBufferStartFrame)
        {
            _spriteBuffers.Clear();
            _currentBufferName = sheetPath;
            _currentBufferStartFrame = startNumber;
            if (resetWhenPathChange)
            {
                Stop();
                Play();
            }
        }
    }

    /// <summary>
    /// 从缓存中获取图片
    /// </summary>
    /// <returns></returns>
    private Sprite GetSpriteByBuffer()
    {
        Sprite tmpSp;
        if (_spriteBuffers.TryGetValue(_currentFrame, out tmpSp))
        {
            return tmpSp;
        }
        else
        {
            string fullPath = string.Format(sheetPath, _currentFrame + startNumber);
            tmpSp = Resources.Load<Sprite>(fullPath);
            if (tmpSp != null)
            {
                _spriteBuffers[_currentFrame] = tmpSp;
            }
            else
            {
                string spritePath = fullPath.Remove(fullPath.LastIndexOf("/"));
                string subPath = fullPath.Substring(fullPath.LastIndexOf("/") + 1);
                Sprite[] iconsAtlas;
                // Load all sprites in atlas
                if (HandleGetResource != null)
                {
                    iconsAtlas = HandleGetResource(spritePath);
                }
                else
                {
                    iconsAtlas = Resources.LoadAll<Sprite>(spritePath);
                }
                // Get specific sprite
                if (iconsAtlas != null)
                {
                    Sprite targetSprite = iconsAtlas.Single(s => s.name == subPath);
                    if (targetSprite != null)
                    {
                        _spriteBuffers[_currentFrame] = targetSprite;
                        tmpSp = targetSprite;
                    }
                }
            }
            return tmpSp;
        }
    }
}
