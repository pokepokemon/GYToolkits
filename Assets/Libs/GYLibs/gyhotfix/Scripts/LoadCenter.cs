using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GYHotfix.Load
{
    public class LoadCenter
    {
        public static readonly LoadCenter Instance = new LoadCenter();

        public ILoadBridge load;

        public LoadConfig config;
    }
}