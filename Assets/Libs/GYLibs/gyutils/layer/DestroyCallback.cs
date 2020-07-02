using UnityEngine;
using System.Collections;

public class DestroyCallback : MonoBehaviour
{
    public delegate void destroyCallback();

    public destroyCallback callback;

    private void OnDestroy()
    {
        if (callback != null)
        {
            callback();
        }
    }
}
