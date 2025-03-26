using GYLib.GYFrame;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIProcessor : Processor
{
    private static Dictionary<string, PanelBinder> _panelMap = new Dictionary<string, PanelBinder>();
    private static Dictionary<string, PanelBinder> _openEvtMap = new Dictionary<string, PanelBinder>();
    private static Dictionary<string, PanelBinder> _closeEvtMap = new Dictionary<string, PanelBinder>();
    public static List<ModuleEvent> closeEvtList = new List<ModuleEvent>();
    public static string evtNameSpace = string.Empty;

    /// <summary>
    /// 加载界面的GO结束后触发
    /// </summary>
    public static Action<PanelBinder> OnPostLoaded;

    /// <summary>
    /// 注册Binder后触发
    /// </summary>
    public static Action<PanelBinder> OnRegistBinder;

    public override void Init()
    {
        RegistBinder();
        base.Init();
    }

    public void Regist(PanelBinder binder)
    {
        _panelMap.Add(binder.name, binder);
        _openEvtMap.Add(binder.openEvt, binder);
        _closeEvtMap.Add(binder.closeEvt, binder);
        OnRegistBinder?.Invoke(binder);
    }

    public virtual void RegistBinder()
    {

    }

    protected override void receivedModuleEvent(ModuleEvent evt)
    {
        string name = evt.ToString();

        if (_openEvtMap.ContainsKey(name))
        {
            PanelBinder panel = _openEvtMap[name];
            panel.LoadPanel(evt);
        }
        if (_closeEvtMap.ContainsKey(name))
        {
            PanelBinder panel = _closeEvtMap[name];
            panel.ClosePanel(evt);
        }

        base.receivedModuleEvent(evt);
    }

    /// <summary>
    /// 获取界面实例
    /// </summary>
    /// <param name="panelName"></param>
    /// <returns></returns>
    public T GetPanel<T>() where T : MonoBehaviour
    {
        foreach (var binder in _panelMap.Values)
        {
            if (binder.panelComponent != null && binder.panelComponent is T)
            {
                return binder.panelComponent as T;
            }
        }
        return null;
    }

    /// <summary>
    /// 通知延迟关闭事件
    /// </summary>
    public static void CallAllCloseEvt()
    {
        List<ModuleEvent> cloneList = new List<ModuleEvent>();
        cloneList.AddRange(closeEvtList);
        foreach (var evt in cloneList)
        {
            ModuleEventManager.instance.dispatchEvent(evt);
        }
        closeEvtList.Clear();
    }
}
