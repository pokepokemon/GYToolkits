using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using GYLib.GYFrame;

/// <summary>
/// 界面绑定类
/// </summary>
/// <typeparam name="T"></typeparam>
public class PanelBinder
{
    public string name;

    public string path;

    public string openEvt;

    public string closeEvt;

    public Type panelType;

    public bool needMask = true;

    /// <summary>
    /// 只允许一个
    /// </summary>
    public bool isSingleton = true;

    /// <summary>
    /// 业务层需要特殊处理时加入的配置
    /// </summary>
    public object specialData;

    public static UnityAction<string, UnityAction<UnityEngine.Object, string>> overrideLoadFunc = null;

    public UnityAction<PanelBinder, ModuleEvent> onLoadedCall;
    public UnityAction<PanelBinder, ModuleEvent> onCloseCall;

    public GameObject panelGo { get; private set; }
    public Component panelComponent { get; private set; }

    private bool _isLoading;
    private int _loadingMaskId = -1;
    private ModuleEvent _openEvt;
    private ModuleEvent _closeEvt;
    public void LoadPanel(ModuleEvent evt)
    {
        if (isSingleton && (panelGo != null || _isLoading))
        {
            Debug.LogWarning("singleton panel already opened evt: " + evt.ToString());
            return;
        }
        _openEvt = evt;
        if (!_isLoading)
        {
            _isLoading = true;
            if (needMask)
            {
                _loadingMaskId = CommonUI.ShowUIMask("loading " + name);
            }
            else
            {
                _loadingMaskId = -1;
            }
            if (overrideLoadFunc == null)
            {
                GameLoader.Instance.LoadObject(path, OnLoadPrefab);
            }
            else
            {
                overrideLoadFunc.Invoke(path, OnLoadPrefab);
            }
        }
    }

    private void OnLoadPrefab(UnityEngine.Object prefab, string originPath)
    {
        if (!_isLoading)
        {
            return;
        }

        _isLoading = false;
        if (_loadingMaskId != -1)
        {
            CommonUI.CloseUIMask(_loadingMaskId);
            _loadingMaskId = -1;
        }
        if (prefab == null)
        {
            Debug.Log("path : " + path + " is null!");
        }
        if (panelGo == null && panelComponent == null)
        {
            panelGo = GameObject.Instantiate<GameObject>(prefab as GameObject);
            LocalizationConfig.Instance.ReplaceUIString(panelGo);
            panelComponent = panelGo.GetComponent(panelType);
        }
        if (onLoadedCall != null)
        {
            onLoadedCall.Invoke(this, _openEvt);
        }
        UIProcessor.OnPostLoaded?.Invoke(this);
    }

    public void ClosePanel(ModuleEvent evt)
    {
        _closeEvt = evt;
        if (_isLoading)
        {
            _isLoading = false;
            if (_loadingMaskId != -1)
            {
                CommonUI.CloseUIMask(_loadingMaskId);
                _loadingMaskId = -1;
            }
        }
        else
        {
            if (panelGo != null)
            {
                if (onCloseCall != null)
                {
                    onCloseCall.Invoke(this, _closeEvt);
                    panelGo = null;
                }
            }
            panelComponent = null;
        }
    }
}
