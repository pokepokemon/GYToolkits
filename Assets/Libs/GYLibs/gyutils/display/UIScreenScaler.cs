using GYLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 保持比例做分辨率最小比例的缩放
/// </summary>
public class UIScreenScaler : MonoBehaviour
{
    public RectTransform rectTrans;
    public int standardWidth;
    public int standardHeight;

    public bool needUpdate;

    private bool _isInited = false;
    private float _originWidth;
    private float _originHeight;

    public bool scaleWidth;
    public bool scaleHeight;

    private void OnEnable()
    {
        RefreshSize();
    }
    
    private void RefreshSize()
    {
        if (!_isInited)
        {
            _originWidth = rectTrans.rect.width;
            _originHeight = rectTrans.rect.height;
            _isInited = true;
        }
        float ratioX = (float)Screen.width / standardWidth;
        float ratioY = (float)Screen.height / standardHeight;

        float minRatio = 1;
        if (!scaleWidth && scaleHeight)
        {
            minRatio = ratioY;
        }
        else if (scaleWidth && !scaleHeight)
        {
            minRatio = ratioX;
        }
        else
        {
            minRatio = Mathf.Min(ratioX, ratioY);
        }
        rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _originWidth * minRatio * LayerManager.instance.uiContainer.canvas.scaleFactor);
        rectTrans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _originHeight * minRatio * LayerManager.instance.uiContainer.canvas.scaleFactor);
    }

    private void Update()
    {
        if (needUpdate)
        {
            RefreshSize();
        }
    }
}
