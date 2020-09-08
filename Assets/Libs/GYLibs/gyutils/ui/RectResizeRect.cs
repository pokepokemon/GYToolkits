using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 通过文本适配图片宽高
/// </summary>
public class RectResizeRect : MonoBehaviour
{
    public RectTransform myRect;
    public RectTransform sourceRect;

    public int expandWidth;
    public int expandHeight;

    public bool skipWidth = false;
    public bool skipHeight = false;

    // Use this for initialization
    void Start()
    {

    }

    private float _lastResultW = -1f;
    private float _lastResultH = -1f;
    // Update is called once per frame
    void Update()
    {
        if (myRect != null && sourceRect != null)
        {
            float rsWidth = skipWidth ? myRect.sizeDelta.x : sourceRect.sizeDelta.x + expandWidth;
            float rsHeight = skipHeight ? myRect.sizeDelta.y : sourceRect.sizeDelta.y + expandHeight;
            if (_lastResultW != rsWidth || _lastResultH != rsHeight)
            {
                myRect.sizeDelta = new Vector2(rsWidth, rsHeight);
                _lastResultW = rsWidth;
                _lastResultH = rsHeight;
            }
        }
    }
}
