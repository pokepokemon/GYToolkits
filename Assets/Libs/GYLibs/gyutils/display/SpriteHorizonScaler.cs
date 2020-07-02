using UnityEngine;
using System.Collections;

public class SpriteHorizonScaler : MonoBehaviour
{
    public SpriteRenderer rink;
    public bool needUpdate;

    private float _originRatio = 1;
    private Vector3 _originScale;

    private bool isInited = false;
    // Use this for initialization
    void OnEnable()
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

    private void InitOrigin()
    {
        if (!isInited)
        {
            _originRatio = rink.bounds.size.x / rink.bounds.size.y;
            _originScale = rink.transform.localScale;
            isInited = true;
        }
    }

    private void RefreshFix()
    {
        InitOrigin();
        float screenRatio = (float)Screen.width / (float)Screen.height;
        if (_originRatio != 0)
        {
            rink.transform.localScale = new Vector3(_originScale.x, _originScale.y * screenRatio / _originRatio, _originScale.z);
        }
    }
}
