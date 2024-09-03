using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GYLibs
{
    /// <summary>
    /// 根据屏幕实际尺寸来调整当前根Canvas缩放的参考值
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    [RequireComponent(typeof(RectTransform))]
    public sealed class AutoAdaptionUICanvas : MonoBehaviour
    {
        private static readonly float _precision = 0.01f;
        private bool _updateRefreshed;

        private CanvasScaler canvasScaler;
        private CanvasScaler CanvasScaler
        {
            get
            {
                if (null == canvasScaler)
                {
                    canvasScaler = GetComponent<CanvasScaler>();
                }
                return canvasScaler;
            }
        }

        private RectTransform RectTr
        {
            get { return transform as RectTransform; }
        }

        private void RefreshMatchSetting()
        {
            bool matchWidth = RectTr.rect.width / RectTr.rect.height - CanvasScaler.referenceResolution.x / CanvasScaler.referenceResolution.y > _precision;
            canvasScaler.matchWidthOrHeight = matchWidth ? 1.0f : 0.0f;
        }

        private void Start()
        {
            RefreshMatchSetting();
        }

        private void LateUpdate()
        {
            if (!_updateRefreshed)
            {
                RefreshMatchSetting();
                _updateRefreshed = true;
            }
        }

        private void OnDestroy()
        {
        }

#if UNITY_EDITOR
        [Sirenix.OdinInspector.Button()]
        private void UpdateWindow()
        {
            RefreshMatchSetting();
        }
#endif
    }
}