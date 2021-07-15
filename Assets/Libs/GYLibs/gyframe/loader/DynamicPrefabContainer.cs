using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// 动态加载go容器
/// </summary>
public class DynamicPrefabContainer : MonoBehaviour
{
    /// <summary>
    /// 加载路径
    /// </summary>
    public string loadPath;

    /// <summary>
    /// 是否在Start的时候自动加载
    /// </summary>
    public bool loadWhenStart = true;

    /// <summary>
    /// 加载时展示的对象
    /// </summary>
    public GameObject objLoading;

    /// <summary>
    /// 加载完毕实例化对象
    /// </summary>
    private GameObject _objInstance;

    private GameObject _prefabTarget;

    public UnityAction<GameObject> onCompleted;

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
        StopLoading();
        if (_objInstance != null)
        {
            Destroy(_objInstance);
        }
        if (!string.IsNullOrEmpty(loadPath) && _objInstance == null && !_isLoading)
        {
            _isLoading = true;
            if (objLoading != null)
            {
                objLoading.SetActive(true);
            }
            GameLoader.Instance.LoadObject(loadPath, OnLoadCompleted);
        }
    }

    private void OnLoadCompleted(UnityEngine.Object obj, string path)
    {
        if (this != null && this.gameObject != null && _objInstance == null && _isLoading && path == loadPath)
        {
            _isLoading = false;
            if (objLoading != null)
            {
                objLoading.SetActive(false);
            }
            _prefabTarget = obj as GameObject;
            GameObject go = GameObject.Instantiate<GameObject>(_prefabTarget);
            go.transform.SetParent(this.transform, false);
            _objInstance = go;
            _objInstance.SetActive(true);

            if (onCompleted != null)
            {
                onCompleted(go);
            }
        }
        else
        {
            GameLoader.Instance.Unload(obj);
        }
    }    
    
    /// <summary>
    /// 获取加载出的实例
    /// </summary>
    /// <returns></returns>
    public GameObject GetObjInstance()
    {
        return _objInstance;
    }

    /// <summary>
    /// 停止加载
    /// </summary>
    public void StopLoading()
    {
        if (_isLoading)
        {
            _isLoading = false;
            if (objLoading != null)
            {
                objLoading.SetActive(false);
            }
        }
    }

    public bool IsLoading()
    {
        return _isLoading;
    }

    private void OnDestroy()
    {
        _isLoading = false;
        if (GameLoader.Instance != null && _prefabTarget != null)
        {
            GameLoader.Instance.Unload(_prefabTarget);
        }
    }
}
