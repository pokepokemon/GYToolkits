using UnityEngine;
using System.Collections;

public class AspectRatioFix : MonoBehaviour
{
    public SpriteRenderer rink;
    public bool needUpdate;

    private Camera _cam;

    // Use this for initialization
    void Start()
    {
        RefreshFix();
    }

    private void Update()
    {
        if (needUpdate)
        {
            RefreshFix();
        }
    }

    private void RefreshFix()
    {
        _cam = _cam ?? Camera.main;
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = rink.bounds.size.x / rink.bounds.size.y;

        if (screenRatio >= targetRatio)
        {
            _cam.orthographicSize = rink.bounds.size.y / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            _cam.orthographicSize = rink.bounds.size.y / 2 * differenceInSize;
        }
    }
}
