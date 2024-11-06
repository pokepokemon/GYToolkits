using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

/// <summary>
/// 通过文本适配图片宽高
/// </summary>
public class ImageResizeText : MonoBehaviour
{
    public Image image;
    public Text text;

    public int expandWidth;
    public int expandHeight;

    public float? _lastExpandWidth;
    public float? _lastExpandHeight;

    public UnityAction<ImageResizeText> onResize;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (text != null && image != null)
        {
            RectTransform rect = text.rectTransform;

            float targetWidth = expandWidth != 0 ? rect.sizeDelta.x + expandWidth : image.rectTransform.rect.width;
            float targetHeight = expandHeight != 0 ? rect.sizeDelta.y + expandHeight : image.rectTransform.rect.height;
            if (targetHeight >= 0)
            {
                if (_lastExpandHeight == null || _lastExpandWidth.Value != targetHeight)
                {
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
                    _lastExpandHeight = targetHeight;
                    onResize?.Invoke(this);
                }
            }
            if (targetWidth >= 0)
            {
                if (_lastExpandWidth == null || _lastExpandWidth.Value != targetWidth)
                {
                    image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
                    _lastExpandWidth = targetWidth;
                    onResize?.Invoke(this);
                }
            }
        }
    }
}
