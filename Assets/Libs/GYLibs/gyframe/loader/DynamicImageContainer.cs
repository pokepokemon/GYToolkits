using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.UI;
using GYLib;

/// <summary>
/// 动态加载Image容器
/// </summary>
public class DynamicImageContainer : MonoBehaviour
{
    /// <summary>
    /// 加载路径
    /// </summary>
    public string loadPath;

    public string loadedPath;

    /// <summary>
    /// 需要加载的图片容器
    /// </summary>
    public Image imageTarget;

    /// <summary>
    /// 是否在Start的时候自动加载
    /// </summary>
    public bool loadWhenStart = true;

    /// <summary>
    /// 加载时展示的对象
    /// </summary>
    public GameObject objLoading;

    public UnityAction<string, Sprite> onCompleted;
    private Sprite spAsset;

    private bool _isLoading = false;

    // Use this for initialization
    void Start()
    {
        if (loadWhenStart)
        {
            StartLoading();
        }
    }

    /// <summary>
    /// 开始加载
    /// </summary>
    public void StartLoading()
    {
        if (AlreadyLoadPath(loadPath))
        {
            return;
        }
        StopLoading();
        if (!string.IsNullOrEmpty(loadPath) && !_isLoading)
        {
            _isLoading = true;
            if (objLoading != null)
            {
                objLoading.SetActive(true);
            }
            imageTarget.gameObject.SetActive(false);
            GameLoader.Instance.LoadSprite(loadPath, OnLoadCompleted);
        }
    }

    private void OnLoadCompleted(UnityEngine.Object obj, string path)
    {
        if (this != null && this.gameObject != null && _isLoading && path == loadPath && imageTarget != null)
        {
            _isLoading = false;
            if (objLoading != null)
            {
                objLoading.SetActive(false);
            }
            Sprite sp = obj as Sprite;
            spAsset = sp;
            loadedPath = path;
            if (sp != null)
            {
                imageTarget.gameObject.SetActive(true);
                imageTarget.sprite = sp;
            }

            if (onCompleted != null)
            {
                onCompleted(path, sp);
            }
        }
        else
        {
            GameLoader.Instance.Unload(obj);
            spAsset = null;
            loadedPath = null;
        }
    }

    /// <summary>
    /// 停止加载
    /// </summary>
    public void StopLoading()
    {
        if (_isLoading)
        {
            _isLoading = false;

            if (spAsset != null)
            {
                GameLoader.Instance.Unload(spAsset);
                spAsset = null;
            }
            if (objLoading != null)
            {
                objLoading.SetActive(false);
            }
            loadedPath = null;
        }
    }


    public bool AlreadyLoadPath(string path)
    {
        if (loadedPath == path && !_isLoading && spAsset != null)
        {
            return true;
        }
        return false;
    }

    public void Dispose()
    {
        if (spAsset != null)
        {
            GameLoader.Instance.Unload(spAsset);
            spAsset = null;
        }
    }

    private void Reset()
    {
        if (this.imageTarget == null)
        {
            Image image = this.GetComponent<Image>();
            this.imageTarget = image;
        }
    }

    private void OnDestroy()
    {
        if (!SingletonManager.quitting)
        {
            Dispose();
        }
    }
}
