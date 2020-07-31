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
            
            float targetWidth = expandWidth != 0 ? rect.sizeDelta.x + expandWidth : image.rectTransform.sizeDelta.x;
            float targetHeight = expandHeight != 0 ? rect.sizeDelta.y + expandHeight : image.rectTransform.sizeDelta.y;

            image.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);
        }
    }
}
