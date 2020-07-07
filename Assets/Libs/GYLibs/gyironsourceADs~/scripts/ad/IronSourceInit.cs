using UnityEngine;
using System.Collections;

public class IronSourceInit : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
    }
    
    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }
}
