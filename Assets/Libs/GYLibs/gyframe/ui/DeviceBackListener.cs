using UnityEngine;
using System.Collections;
using GYLib;
using UnityEngine.Events;
using System.Collections.Generic;
using GYLib.GYFrame;

public class DeviceBackListener : MonoSingleton<DeviceBackListener>
{
    /// <summary>
    /// 第一次按下
    /// </summary>
    private bool _isFirstPress;

    /// <summary>
    /// 按下的时间
    /// </summary>
    private float _lastDownTime;

    private List<Object> _targetList = new List<Object>();
    private List<UnityAction> _callbackList = new List<UnityAction>();

    public int count;
    
    /// <summary>
    /// 是否在引导中
    /// </summary>
    public bool isGuiding = false;

    public void Init()
    {

    }

    public void AddListener(Object target, UnityAction callback)
    {
        for (int i = _targetList.Count - 1; i >= 0; i--)
        {
            if (_targetList[i] == null || _callbackList[i] == null)
            {
                _targetList.RemoveAt(i);
                _callbackList.RemoveAt(i);
                count--;
            }
        }
        count++;
        _targetList.Add(target);
        _callbackList.Add(callback);
    }

    void Update()
    {
        ExitDetection(); //调用 退出检测函数
    }

    public void RemoveListener(Object target)
    {
        for (int i = 0; i < _targetList.Count; i++)
        {
            if (_targetList[i] == target)
            {
                _targetList.RemoveAt(i);
                _callbackList.RemoveAt(i);

                count--;
            }
        }
    }

    /// <summary>
    /// 退出检测
    /// </summary>
    private void ExitDetection()
    {
        if (Input.GetKeyDown(KeyCode.Escape))            //如果按下退出键
        {
            if (!HandleExistPanel())
            {
                if (_lastDownTime == 0)                          //当倒计时时间等于0的时候
                {
                    _lastDownTime = Time.time;                   //把游戏开始时间，赋值给 CountDown
                    _isFirstPress = true;                        //开始计时
                    CommonUI.ShowToast("Click one more time to quit");
                }
                else
                {
                    Debug.Log("call quit");
#if !UNITY_EDITOR
                    //保存游戏资源
                    if (SaveProcessor.IsInited)
                    {
                        ModuleEventManager.instance.dispatchEvent(new MEvent_GameSaveFlushImmediately());
                        ModuleEventManager.instance.dispatchEvent(new MEvent_GameSaveImmediately());
                    }

                    CommonUI.ShowConfirm(
                        LocalizationConfig.Instance.GetStringWithSelf("提示"),
                        LocalizationConfig.Instance.GetStringWithSelf("离开游戏?"),
                        true,
                        delegate ()
                        {
                            Application.Quit();
                        });
#endif
                    _lastDownTime = 0;
                    _isFirstPress = false;
                }
            }
        }

        if (_isFirstPress)
        {
            if ((Time.time - _lastDownTime) > 2.0)           //两次点击时间间隔
            {
                _lastDownTime = 0;                           //归零
                _isFirstPress = false;
            }
        }
    }

    private bool HandleExistPanel()
    {
        if (isGuiding)
        {
            return false;
        }
        for (int i = _targetList.Count - 1; i >= 0; i--)
        {
            object obj = _targetList[i];
            UnityAction callback = _callbackList[i];
            _targetList.RemoveAt(i);
            _callbackList.RemoveAt(i);
            count--;
            if (obj == null || callback == null)
            {
                continue;
            }
            else
            {
                callback.Invoke();
                return true;
            }
        }
        return false;
    }

}
