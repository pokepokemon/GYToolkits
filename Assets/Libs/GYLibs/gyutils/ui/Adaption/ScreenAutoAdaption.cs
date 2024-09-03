using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GYLibs
{
    /// <summary>
    /// Control mask position
    /// </summary>
    public class ScreenAutoAdaption : MonoBehaviour
    {

        /// <summary>
        /// Fill with mask root
        /// </summary>
        public Transform[] maskRoots;
        public bool updateRefreshed;
        public static readonly float _designWidth = 1920;
        public static readonly float _designHeight = 1080;
        private static readonly float _precision = 0.01f;

        public bool isVertical = false;
        
        void Start()
        {
            if (isVertical)
            {
                AutoAdapterMaskVertical();
            }
            else
            {
                AutoAdapterMaskHorizon();
            }
        }

        /// <summary>
        /// 获取当前屏幕高度与设计需求高度的比例
        /// </summary>
        /// <returns></returns>
        private static float GetHeightScaleRate(bool isVertical)
        {
            float sWidth = isVertical ? Screen.height : Screen.width;
            float sHeight = isVertical ? Screen.width : Screen.height;
            float whRate = sWidth / sHeight;
            float designedWHRate = _designWidth / _designHeight;
            if (Mathf.Abs(whRate - designedWHRate) > _precision)
            {
                float widthRate = sWidth / _designWidth;
                float renderHeight = widthRate * _designHeight;
                float heightRate = sHeight / renderHeight;
                return heightRate;
            }
            return 1f;
        }
        
        private void AutoAdapterMaskHorizon()
        {
            float heightRate = GetHeightScaleRate(isVertical);
            foreach (var rootTr in maskRoots)
            {
                GameObject maskRootGo = rootTr.gameObject;
                if (heightRate - 1 <= _precision)
                {
                    maskRootGo.SetActive(false);
                    return;
                }
                maskRootGo.SetActive(true);
                float realHeight = heightRate * _designHeight;
                int maskHeight = (int)Mathf.Ceil((realHeight - _designHeight) * 0.5f);
                RectTransform upMaskRectTr = rootTr.Find("ImageUp") as RectTransform;
                RectTransform bottomMaskRectTr = rootTr.Find("ImageBottom") as RectTransform;
                
                upMaskRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maskHeight);
                bottomMaskRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maskHeight);
            }
        }

        private void AutoAdapterMaskVertical()
        {
            float heightRate = GetHeightScaleRate(isVertical);
            foreach (var rootTr in maskRoots)
            {
                GameObject maskRootGo = rootTr.gameObject;
                if (heightRate - 1 <= _precision)
                {
                    maskRootGo.SetActive(false);
                    return;
                }
                maskRootGo.SetActive(true);
                float realHeight = heightRate * _designHeight;
                int maskHeight = (int)Mathf.Ceil((realHeight - _designHeight) * 0.5f);
                RectTransform upMaskRectTr = rootTr.Find("ImageUp") as RectTransform;
                RectTransform bottomMaskRectTr = rootTr.Find("ImageBottom") as RectTransform;

                upMaskRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maskHeight);
                bottomMaskRectTr.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maskHeight);
            }
        }

        void LateUpdate()
        {
            if (!updateRefreshed)
            {
                if (isVertical)
                {
                    AutoAdapterMaskVertical();
                }
                else
                {
                    AutoAdapterMaskHorizon();
                }
                updateRefreshed = true;
            }
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button()]
        private void UpdateWindow()
        {
            if (isVertical)
            {
                AutoAdapterMaskVertical();
            }
            else
            {
                AutoAdapterMaskHorizon();
            }
        }
#endif
    }
}