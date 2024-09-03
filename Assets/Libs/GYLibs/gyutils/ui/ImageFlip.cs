using System.Collections;
using UnityEngine;

namespace GYLibs.ui
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    [RequireComponent(typeof(Graphic))]
    public class ImageFlip : UIBehaviour, IMeshModifier
    {
        [SerializeField]
        private bool _isFlipX = true;

        [SerializeField]
        private bool _isFlipY = true;

        private List<UIVertex> uiVertexList = new List<UIVertex>();
        private RectTransform _rectTransform;

#if UNITY_EDITOR
        public new void OnValidate()
        {
            GetComponent<Graphic>().SetVerticesDirty();
            Awake();
        }

#endif

        protected override void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        public void ModifyMesh(Mesh mesh)
        {
        }

        public void ModifyMesh(VertexHelper verts)
        {
            uiVertexList.Clear();
            verts.GetUIVertexStream(uiVertexList);
            var count = uiVertexList.Count;
            for (var i = 0; i < count; ++i)
            {
                var vertex = uiVertexList[i];
                if (_isFlipX)
                {
                    vertex.position.x += Mathf.Lerp(-_rectTransform.rect.width, _rectTransform.rect.width, _rectTransform.pivot.x);
                    vertex.position.x *= -1;
                }

                if (_isFlipY)
                {
                    vertex.position.y += Mathf.Lerp(-_rectTransform.rect.height, _rectTransform.rect.height, _rectTransform.pivot.y);
                    vertex.position.y *= -1;
                }

                uiVertexList[i] = vertex;
            }

            verts.Clear();
            verts.AddUIVertexTriangleStream(uiVertexList);
        }

        public void SetFlipX(bool value)
        {
            _isFlipX = value;
        }

        public void SetFlipY(bool value)
        {
            _isFlipY = value;
        }
    }
}