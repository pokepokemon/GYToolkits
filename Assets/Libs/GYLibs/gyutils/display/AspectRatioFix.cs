using UnityEngine;
using System.Collections;

public class AspectRatioFix : MonoBehaviour
{
    public SpriteRenderer rink;
    public bool needUpdate;

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
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = rink.bounds.size.x / rink.bounds.size.y;

        if (screenRatio >= targetRatio)
        {
            Camera.main.orthographicSize = rink.bounds.size.y / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            Camera.main.orthographicSize = rink.bounds.size.y / 2 * differenceInSize;
        }
    }
}
