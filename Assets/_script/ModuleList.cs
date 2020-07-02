using UnityEngine;
using System.Collections;
using GYLib.GYFrame;
using System.Collections.Generic;

public class ModuleList
{
    public static readonly ModuleList instance = new ModuleList();
    
    /**
    * 启动所有模块
    * 
    */
    public void setup()
    {
        //ModuleInUnity.Instance.AddModule<StartModule>();
        //ModuleInUnity.Instance.AddModule<SaveModule>();
    }
}