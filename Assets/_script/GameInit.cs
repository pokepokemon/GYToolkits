using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GYLib.GYFrame;
using GYLib.Utils;
using LitJson;
using System;
using DG.Tweening;

public class GameInit : MonoBehaviour {
    
    // Use this for initialization
    void Start () {
        this.gameObject.AddComponent<GameQuit>();
        
        Application.targetFrameRate = 60;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        TimeUtil.shareRealTimeSincePlay = Time.realtimeSinceStartup;

        RegistJson();
        //InitLanguage();
        FileManager tmpMgr = FileManager.Instance;
        RandomUtils.SetSeed(System.DateTime.Now.Millisecond);
        LayerManager.instance.Init();
        ModuleList.instance.setup();
        //ADManager.Instance.Init();
#if UNITY_ANDROID
        GlobalData.ANDROID_APP_ID = Application.identifier;
#endif
        Debug.Log("Game inited!");
    }

    private void RegistJson()
    {
        JsonMapper.RegisterExporter<float>((obj, writer) => writer.Write(Convert.ToDouble(obj)));
        JsonMapper.RegisterImporter<double, float>(input => Convert.ToSingle(input));
        JsonMapper.RegisterImporter<double, int>(input => Convert.ToInt32(input));
        JsonMapper.RegisterImporter<double, long>(input => Convert.ToInt64(input));
        JsonMapper.RegisterImporter<int, string>(input => input.ToString());
        JsonMapper.RegisterImporter<double, string>(input => input.ToString());
        JsonMapper.RegisterImporter<float, string>(input => input.ToString());
        JsonMapper.RegisterImporter<string, float>(input => Convert.ToSingle(input));
        JsonMapper.RegisterImporter<string, double>(input => Convert.ToDouble(input));
        JsonMapper.RegisterImporter<string, int>(input => Convert.ToInt32(input));
    }

    private void Update()
    {
        TimeUtil.shareRealTimeSincePlay = Time.realtimeSinceStartup;
    }

    private void OnApplicationQuit()
    {
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
        }
    }
}
