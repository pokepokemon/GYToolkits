﻿using UnityEngine;
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
            GameLoader.Instance.LoadObject(path, OnLoadPrefab);
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
