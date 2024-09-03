using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 通过文本适配图片宽高
/// </summary>
public class ImageResizeText : MonoBehaviour
{
    public Image image;
    public Text text;

    public int expandWidth;
    public int expandHeight;

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
                image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetHeight);
            }
            if (targetWidth >= 0)
            {
                image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);
            }
        }
    }
}
