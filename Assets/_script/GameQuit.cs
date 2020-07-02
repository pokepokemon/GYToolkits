using GYLib.GYFrame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameQuit : MonoBehaviour
{
    /// <summary>
    /// 第一次按下
    /// </summary>
    private bool _isFirstPress;  

    /// <summary>
    /// 按下的时间
    /// </summary>
    private float _lastDownTime; 

    void Update()
    {
        ExitDetection(); //调用 退出检测函数
    }


    /// <summary>
    /// 退出检测
    /// </summary>
    private void ExitDetection()
    {
        if (Input.GetKeyDown(KeyCode.Escape))            //如果按下退出键
        {
            if (_lastDownTime == 0)                          //当倒计时时间等于0的时候
            {
                _lastDownTime = Time.time;                   //把游戏开始时间，赋值给 CountDown
                _isFirstPress = true;                        //开始计时
                //Alert("Click one more times to quit");
            }
            else
            {
                Debug.Log("call quit");
#if !UNITY_EDITOR
                //保存游戏资源 TO DO
                
                Application.Quit();
#endif

                _lastDownTime = 0;
                _isFirstPress = false;
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
}
