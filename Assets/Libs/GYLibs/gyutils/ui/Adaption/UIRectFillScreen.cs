using UnityEngine;

namespace GYLibs
{
    public class UIRectFillScreen : MonoBehaviour
    {
        void Start()
        {
            RectTransform selfRect = GetComponent<RectTransform>();
            if (CanvasAutoAdaption.IsFullScreen)
            {
                if (selfRect.anchorMin.x == 0 && selfRect.anchorMax.x == 0)
                {
                    selfRect.offsetMin = new Vector2(-80, selfRect.offsetMin.y);
                }
                else if (selfRect.anchorMin.x == 1 && selfRect.anchorMax.x == 1)
                {
                    selfRect.offsetMax = new Vector2(80, selfRect.offsetMax.y);
                }
                else
                {
                    selfRect.offsetMin = new Vector2(-80, selfRect.offsetMin.y);
                    selfRect.offsetMax = new Vector2(80, selfRect.offsetMax.y);
                }
            }
        }
    }
}
