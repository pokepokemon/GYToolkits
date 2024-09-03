using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using GYLib;
using Sirenix.OdinInspector;
using System.Collections.Generic;

/// <summary>
/// 动态加载go容器
/// </summary>
public class DynamicPrefabContainer : MonoBehaviour
{
    public delegate void HandleReplaceLocalize(GameObject go);
    public static HandleReplaceLocalize OnLoadLocalize;

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
    private Object _asset;

    public UnityAction<GameObject> onCompleted;

    [ShowInInspector, ReadOnly]
    private bool _isLoading = false;

    private Dictionary<string, int> _poolKeyDict = null;
    
    [ShowInInspector, ReadOnly]
    private bool _usePool = false;
    private const string _POOL_KEY_PREFIX = "DPC_";

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
        CleanInstance(true);
        if (!string.IsNullOrEmpty(loadPath) && _objInstance == null && !_isLoading)
        {
            if (_usePool)
            {
                string key = GetInstancePoolKey(loadPath);
                GameObject go = ObjectPool.Instance.Get(key) as GameObject;
                if (go != null)
                {
                    SetInstanceGo(go);
                    return;
                }
            }
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
            _asset = obj;
            if (obj != null)
            {
                GameObject go = GameObject.Instantiate<GameObject>(obj as GameObject);
                SetInstanceGo(go);
            }
            else
            {
                Debug.LogError("fail to load : [" + path + "]");
            }
        }
        else
        {
            GameLoader.Instance.Unload(obj);
        }
    }

    /// <summary>
    /// 设置实例
    /// </summary>
    /// <param name="go"></param>
    private void SetInstanceGo(GameObject go)
    {
        go.transform.SetParent(this.transform, false);
        _objInstance = go;
        onCompleted?.Invoke(go);
        OnLoadLocalize?.Invoke(go);
    }

    /// <summary>
    /// 获得需要加载的池Key
    /// </summary>
    /// <returns></returns>
    private static string GetInstancePoolKey(string path)
    {
        return _POOL_KEY_PREFIX + path;
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

    public void CleanInstance(bool keepPath = false)
    {
        if (_objInstance != null)
        {
            CheckRecycleInstance();
            _objInstance = null;
        }
        if (_asset != null)
        {
            GameLoader.Instance.Unload(_asset);
            _asset = null;
        }
        if (!keepPath)
        {
            loadPath = null;
        }
    }

    /// <summary>
    /// 操作池
    /// </summary>
    private void CheckRecycleInstance()
    {
        if (_usePool)
        {
            string key = GetInstancePoolKey(loadPath);
            ObjectPool.Instance.Push(key, _objInstance);

            if (_poolKeyDict != null)
            {
                int refCount;
                if (!_poolKeyDict.TryGetValue(loadPath, out refCount))
                {
                    refCount = 0;
                }
                refCount++;
                _poolKeyDict[loadPath] = refCount;
            }
        }
        else
        {
            Destroy(_objInstance);
        }
    }

    /// <summary>
    /// 设置是否使用池
    /// </summary>
    /// <param name="poolKeySet">会把关联的池Key存入Set中</param>
    /// <param name="usePool"></param>
    public void SetUsePool(Dictionary<string, int> poolKeySet, bool usePool)
    {
        if (usePool)
        {
            _poolKeyDict = poolKeySet;
        }
        else
        {
            _poolKeyDict = null;
        }
        _usePool = usePool;
    }

    public void Dispose()
    {
        StopLoading();
        CleanInstance();
    }

    public bool IsLoading()
    {
        return _isLoading;
    }

    public bool IsPathLoadingOrLoaded(string path)
    {
        if (loadPath == path && (_isLoading || _objInstance != null))
        {
            return true;
        }
        return false;
    }

    private void OnDestroy()
    {
        if (!SingletonManager.quitting)
        {
            Dispose();
        }
    }

    /// <summary>
    /// 释放所有相关池的资源
    /// </summary>
    /// <param name="dict"></param>
    public static void ReleaseAllPoolAssets(Dictionary<string, int> dict)
    {
        foreach (string path in dict.Keys)
        {
            int refCount = dict[path];
            string poolKey = GetInstancePoolKey(path);
            for (int i = 0; i < refCount; i++)
            {
                GameLoader.Instance.UnloadByName(path);
            }
            ObjectPool.Instance.Clear(poolKey);
        }
        dict.Clear();
    }
}
