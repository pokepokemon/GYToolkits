using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// 通过文本组适配图片宽高
/// </summary>
public class ImageResizeTextList : MonoBehaviour
{
    public Image image;
    public List<Text> textList;

    public bool isHorizon = true;
    public bool needUpdate = true;

    public int expandWidth;
    public int expandHeight;

    // Use this for initialization
    void Start()
    {
        CheckResize();
    }

    // Update is called once per frame
    void Update()
    {
        if (needUpdate)
        {
            CheckResize();
        }
    }

    /// <summary>
    /// 调整宽高
    /// </summary>
    private void CheckResize()
    {
        if (textList != null && image != null)
        {
            float contentX = 0;
            float contentY = 0;
            for (int i = 0; i < textList.Count; i++)
            {
                RectTransform rect = textList[i].rectTransform;
                if (isHorizon)
                {
                    contentX += rect.rect.width;
                    if (contentY < rect.rect.height)
                    {
                        contentY = rect.rect.height;
                    }
                }
                else
                {
                    contentY += rect.rect.height;
                    if (contentX < rect.rect.width)
                    {
                        contentX = rect.rect.width;
                    }
                }
            }
            image.rectTransform.sizeDelta = new Vector2(contentX + expandWidth, contentY + expandHeight);
        }
    }
}
