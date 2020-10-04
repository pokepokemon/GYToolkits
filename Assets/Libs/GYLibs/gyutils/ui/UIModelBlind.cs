using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Recheck position follow 3D Model
/// </summary>
public class UIModelBlind : MonoBehaviour {
    public Vector3 vecMax = new Vector3(99999, 99999, 0);

    public Transform modelTransform;

    public RectTransform screenUITransform;

    public Canvas canvas;

    public float offsetX = 0;
    public float offsetY = 0;

    private bool _isReset = false;

    private Camera _cameraCache;

    // Use this for initialization
    void Start () {
        _cameraCache = Camera.main; 
        ResetPos();
    }

    private void OnEnable()
    {
        if (_cameraCache == null)
        {
            _cameraCache = Camera.main;
            ResetPos();
        }
    }

    /// <summary>
    /// 若model的active发生变化,UI也跟随变化
    /// </summary>
    public bool activeFollowModel = false;

    public bool firstCalc = false;
    float _lastX = 0;
    float _lastY = 0;
	// Update is called once per frame
	void Update () {
        if (modelTransform != null && screenUITransform != null && canvas != null && _cameraCache != null)
        {
            if (activeFollowModel)
            {
                if (!modelTransform.gameObject.activeInHierarchy && screenUITransform.gameObject.activeInHierarchy)
                {
                    screenUITransform.gameObject.SetActive(false);
                }
                else if (modelTransform.gameObject.activeInHierarchy && !screenUITransform.gameObject.activeInHierarchy)
                {
                    screenUITransform.gameObject.SetActive(true);
                }
            }
            float scaleFactor = canvas.scaleFactor;
            Vector2 screentPosition = _cameraCache.WorldToScreenPoint(modelTransform.position);
            screentPosition.x = screentPosition.x - Screen.width / 2 + offsetX * scaleFactor;
            screentPosition.y = screentPosition.y - Screen.height / 2 + offsetY * scaleFactor;
            if (!firstCalc && !_isReset)
            {
                if (_lastX == screentPosition.x && _lastY == screentPosition.y)
                {
                    return;
                }
            }
            else
            {
                firstCalc = false;
                _isReset = false;
            }
            
            Vector2 convertPos = new Vector2(screentPosition.x / scaleFactor, screentPosition.y / scaleFactor);
            screenUITransform.anchoredPosition = convertPos;
            _lastX = screentPosition.x;
            _lastY = screentPosition.y;
        }
        else
        {
            ResetPos();
        }
    }

    public void ResetPos()
    {
        if (screenUITransform != null)
        {
            screenUITransform.localPosition = vecMax;
        }
        _isReset = true;
    }
}
