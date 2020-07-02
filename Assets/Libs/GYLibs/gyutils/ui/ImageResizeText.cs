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
            image.rectTransform.sizeDelta = new Vector2(rect.sizeDelta.x + expandWidth, rect.sizeDelta.y + expandHeight);
        }
    }
}
