using GYLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GYLibs
{
    /// <summary>
    /// 挂在UI大底图上面，自适应
    /// </summary>
    
    [RequireComponent(typeof(RectTransform))]
    public sealed class UIRectAdapter : MonoBehaviour
    {
        private static float _screenWidth = 0;
        private static float _screenHeigth = 0;
        private static float _screenWHRate = 0;
        private static bool _isInited = false;
        private static float _uiWidth;
        private static float _uiHeight;
        private static float _realWHRate;

        void Start()
        {
            if(!_isInited)
            {
                PrepareInit();
            }

            RectTransform rectTr = transform as RectTransform;
            if(null == rectTr)  
            {
                return;
            }
            float imageWHRate = rectTr.rect.width / rectTr.rect.height;
            float scale = (_realWHRate > imageWHRate) ? (_uiWidth + 2) / rectTr.rect.width : (_uiHeight + 2) / rectTr.rect.height;
            rectTr.localScale = new Vector3(scale, scale, 1.0f);
        }

        private static void PrepareInit()
        {
            _isInited = true;
            _screenWidth = Screen.width;
            _screenHeigth = Screen.height;
            _screenWHRate = _screenWidth / _screenHeigth;

            GameObject uiRootGo = LayerManager.instance.uiContainer.cam.gameObject;
            if(null == uiRootGo)
            {
                return;
            }
            
            CanvasScaler cScaler = uiRootGo.GetComponent<CanvasScaler>();
            float designWHRate = cScaler.referenceResolution.x / cScaler.referenceResolution.y;
            _realWHRate = Mathf.Max(designWHRate, _screenWHRate);

            if(cScaler.matchWidthOrHeight < 0.5f)
            {
                _uiWidth = cScaler.referenceResolution.x;
                _uiHeight = _uiWidth / _realWHRate;
            }
            else
            {
                _uiHeight = cScaler.referenceResolution.y;
                _uiWidth = _uiHeight * _realWHRate;
            }
        }
    }
}
