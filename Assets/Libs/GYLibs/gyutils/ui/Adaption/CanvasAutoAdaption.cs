using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GYLibs
{
    /// <summary>
    /// Attach to UIRoot GameObject, Control rectTransform canvas size to fit mask.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public sealed class CanvasAutoAdaption : MonoBehaviour
    {
        public bool updateRefreshed;
        private static bool _inited = false;
        private static bool _isSpringDevice = false;
        private static readonly float _precision = 0.01f;
        private static readonly float _rateWHThreshold = 2.0f;

        public bool isVertical = false;

        private static bool useFullScreen;
        public static bool IsFullScreen
        {
            get
            {
                return useFullScreen;
            }
        }

        private void Start()
        {
            if (!_inited)
            {
                _isSpringDevice = isVertical ? 
                    (1.0f * Screen.height / Screen.width - _rateWHThreshold > _precision) : 
                    (1.0f * Screen.width / Screen.height - _rateWHThreshold > _precision);
                _inited = true;
            }
            DoAdaption();
        }

        private void DoAdaption()
        {
            if (_isSpringDevice)
            {
                var name = transform.parent.name;
                bool isUIRealRoot = (name.Equals("__UIContainer") || name.Equals("TopUIRoot"));
                //某些UI在异形屏幕下要往中间挤，远离边缘
                bool needMoveToCenter = name.Contains("ToCenter");
                if (isVertical)
                {
                    SetVerticalSize(isUIRealRoot, needMoveToCenter);
                }
                else
                {
                    SetHorizonSize(isUIRealRoot, needMoveToCenter);
                }
            }
        }

        private void SetHorizonSize(bool isUIRealRoot, bool needMoveToCenter)
        {
            RectTransform rectTr = transform as RectTransform;
            if (isUIRealRoot || needMoveToCenter)
            {
                rectTr.offsetMin = new Vector2(80f, -540f);
                rectTr.offsetMax = new Vector2(-80f, 540f);
                useFullScreen = true;
            }
            else
            {
                rectTr.offsetMin = new Vector2(-80f, rectTr.offsetMin.y);
                rectTr.offsetMax = new Vector2(80f, rectTr.offsetMax.y);
                useFullScreen = false;
            }
        }

        private void SetVerticalSize(bool isUIRealRoot, bool needMoveToCenter)
        {
            RectTransform rectTr = transform as RectTransform;
            if (isUIRealRoot || needMoveToCenter)
            {
                rectTr.offsetMin = new Vector2(-540f, 80f);
                rectTr.offsetMax = new Vector2(540f, -80f);
                useFullScreen = true;
            }
            else
            {
                rectTr.offsetMin = new Vector2(rectTr.offsetMin.y, -80f);
                rectTr.offsetMax = new Vector2(rectTr.offsetMax.y, 80f);
                useFullScreen = false;
            }
        }

        void LateUpdate()
        {
            if (!updateRefreshed)
            {
                DoAdaption();
                updateRefreshed = true;
            }
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button()]
        private void UpdateWindow()
        {
            DoAdaption();
        }
#endif
    }
}

