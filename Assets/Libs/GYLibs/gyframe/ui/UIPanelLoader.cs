using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class UIPanelLoader
{
    public AssetsLoadGroup loadGroup { private set; get; }
    public string panelPath { private set; get; }

    private bool _useDatas;
    private PanelLoadData[] _preloadAssets;
    private string[] _assetPathList;

    public bool isLoading { private set; get; } = false;
    private bool isDisposed = false;

    public UnityAction<UIPanelLoader> OnBeforeLoad;
    public UnityAction<UIPanelLoader> OnLoadCompleted;

    public UIPanelLoader SetPreloadAsset(PanelLoadData[] preloadAssets)
    {
        _preloadAssets = preloadAssets;
        _useDatas = true;
        return this;
    }

    public UIPanelLoader SetPreloadPaths(string[] preloadPaths)
    {
        _assetPathList = preloadPaths;
        _useDatas = false;
        return this;
    }

    public UIPanelLoader SetPanelPath(string path)
    {
        panelPath = path;
        return this;
    }

    /// <summary>
    /// 开始加载
    /// </summary>
    /// <returns></returns>
    private UIPanelLoader StartLoading()
    {
        List<string> pathList = new List<string>();
        List<System.Type> typeList = null;

        //准备数据
        if (_useDatas)
        {
            if (_preloadAssets != null && _preloadAssets.Length > 0)
            {
                typeList = new List<System.Type>();
                foreach (var data in _preloadAssets)
                {
                    pathList.Add(data.path);
                    typeList.Add(data.type);
                }
            }
        }
        else
        {
            if (_assetPathList != null && _assetPathList.Length > 0)
            {
                foreach (var path in _assetPathList)
                {
                    pathList.Add(path);
                }
            }
        }

        if (!string.IsNullOrEmpty(panelPath))
        {
            pathList.Add(panelPath);
        }
        if (pathList.Count > 0)
        {
            loadGroup = new AssetsLoadGroup(pathList, typeList);
            isLoading = true;
            if (OnBeforeLoad != null)
            {
                OnBeforeLoad(this);
            }
            loadGroup.OnCompleted = OnGroupLoadCompleted;
        }
        return this;
    }

    private void OnGroupLoadCompleted(AssetsLoadGroup group)
    {
        if (OnLoadCompleted != null)
        {
            OnLoadCompleted(this);
        }
    }

    /// <summary>
    /// 设置加载准备完成的回调
    /// </summary>
    /// <param name="onCompleted"></param>
    /// <returns></returns>
    public UIPanelLoader SetOnCompleted(UnityAction<UIPanelLoader> onCompleted)
    {
        OnLoadCompleted = onCompleted;
        return this;
    }

    /// <summary>
    /// 设置加载前的回调
    /// </summary>
    /// <param name="onBeforeLoad"></param>
    /// <returns></returns>
    public UIPanelLoader SetOnBeforeLoad(UnityAction<UIPanelLoader> onBeforeLoad)
    {
        OnBeforeLoad = onBeforeLoad;
        return this;
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            if (loadGroup != null)
            {
                loadGroup.Dispose();
            }
            OnLoadCompleted = null;
            OnBeforeLoad = null;
        }
    }

    public struct PanelLoadData
    {
        public string path;
        public System.Type type;
    }
}
