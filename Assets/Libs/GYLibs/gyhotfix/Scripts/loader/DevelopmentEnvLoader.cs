using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace GYHotfix.Load
{
    public class DevelopmentEnvLoader : ILoadBridge
    {
        bool ILoadBridge.CanLoadSync(string assetName)
        {
            throw new System.NotImplementedException();
        }

        void ILoadBridge.LoadAsset(string assetName, UnityAction<string, Object> callback, int priority)
        {
            throw new System.NotImplementedException();
        }

        Object ILoadBridge.LoadAssetSync(string assetName)
        {
            throw new System.NotImplementedException();
        }

        void ILoadBridge.LoadScene(string sceneName, UnityAction<string> callback, int priority)
        {
            throw new System.NotImplementedException();
        }
    }
}
