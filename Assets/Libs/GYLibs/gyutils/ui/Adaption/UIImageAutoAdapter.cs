using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GYLibs
{
    /// <summary>
    /// UI大底图自适应
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public sealed class UIImageAutoAdapter : MonoBehaviour
    {
        private Sprite _sprite;
        private Image _image;
        private static float _fitHeight = 0;

        private void Start()
        {
            GameObject uiRootGo = GameObject.Find("__UIContainer");
            if (null == uiRootGo)
            {
                return;
            }

            CanvasScaler cScaler = uiRootGo.GetComponent<CanvasScaler>();
            _fitHeight = cScaler.referenceResolution.y;
            _image = this.GetComponent<Image>();
            _sprite = _image.sprite;
            if (_sprite != null)
            {
                ReSizeBg();
            }
        }

        private void Update()
        {
            if (_sprite != _image.sprite && _image.sprite != null)
            {
                _sprite = _image.sprite;
                ReSizeBg();
            }
        }

        private void ReSizeBg()
        {
            float scale = _fitHeight / _sprite.texture.height;
            transform.localScale = Vector2.one * scale * 1.05f;
        }
    }
}
