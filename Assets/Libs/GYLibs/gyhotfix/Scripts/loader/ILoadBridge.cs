using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface ILoadBridge
{
    Object LoadAssetSync(string assetName);
    void LoadAsset(string assetName, UnityAction<string, Object> callback, int priority);
    void LoadScene(string sceneName, UnityAction<string> callback, int priority);
    bool CanLoadSync(string assetName);
}
