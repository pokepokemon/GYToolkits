using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GYLibs.control
{
    public class MobileTouchController : IInputController
    {
        private Dictionary<int, Touch> _touchIdMap = new Dictionary<int, Touch>();
        private HashSet<int> _tmpHashBuffer = new HashSet<int>();
        private List<int> _deleteBufferList = new List<int>();
        private List<Touch> _touchNotifyBuffer = new List<Touch>();
        private const float CLICK_INTERVAL = 0.05f;

        private UnityAction<Vector2, int> _onTouch;
        public bool isSingleTouch = true;

        public UnityAction<Vector2, int> OnClick
        {
            get => _onTouch;
            set => _onTouch = value;
        }

        public void Update()
        {
            if (_touchNotifyBuffer.Count > 0)
            {
                _touchNotifyBuffer.Clear();
            }

            int touchCount = Input.touchCount;
            if (touchCount > 0)
            {
                _tmpHashBuffer.Clear();
                for (int i = 0; i < touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    ApplyTouch(touch);
                }

                // 删除未检测到的触摸
                foreach (var touchId in _touchIdMap.Keys)
                {
                    if (!_tmpHashBuffer.Contains(touchId))
                    {
                        _deleteBufferList.Add(touchId);
                    }
                }

                if (_deleteBufferList.Count > 0)
                {
                    foreach (var id in _deleteBufferList)
                    {
                        _touchIdMap.Remove(id);
                    }
                    _deleteBufferList.Clear();
                }

                // 响应触摸事件
                if (_touchNotifyBuffer.Count > 0)
                {
                    if (isSingleTouch)
                    {
                        _onTouch?.Invoke(_touchNotifyBuffer[0].position, 0);
                    }
                    else
                    {
                        foreach (var touch in _touchNotifyBuffer)
                        {
                            _onTouch?.Invoke(touch.position, 0);
                        }
                    }
                    _touchNotifyBuffer.Clear();
                }
            }
            else
            {
                if (_touchIdMap.Count > 0)
                {
                    _touchIdMap.Clear();
                }
            }
        }

        private void ApplyTouch(Touch touch)
        {
            int touchId = touch.fingerId;
            _tmpHashBuffer.Add(touchId);

            if (touch.phase == TouchPhase.Ended)
            {
                if (_touchIdMap.TryGetValue(touchId, out Touch touchedData))
                {
                    Vector2 deltaPos = touch.position - touchedData.position;
                    Vector2 deltaPosScaled = new Vector2(deltaPos.x / Screen.width, deltaPos.y / Screen.height);
                    if (deltaPosScaled.magnitude <= CLICK_INTERVAL)
                    {
                        _touchNotifyBuffer.Add(touch);
                    }
                    _touchIdMap.Remove(touchId);
                }
            }
            else if (touch.phase == TouchPhase.Began)
            {
                if (!IsTouchUIByTouch(touch))
                {
                    _touchIdMap[touchId] = touch;
                }
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                if (_touchIdMap.TryGetValue(touchId, out Touch touchedData) && !IsTouchUIByTouch(touch))
                {
                    Vector2 deltaPos = touch.position - touchedData.position;
                    Vector2 deltaPosScaled = new Vector2(deltaPos.x / Screen.width, deltaPos.y / Screen.height);
                    if (deltaPosScaled.magnitude > CLICK_INTERVAL)
                    {
                        _touchIdMap.Remove(touchId);
                    }
                }
            }
        }

        public void Stop()
        {
            _touchIdMap.Clear();
        }

        private bool IsTouchUIByTouch(Touch touch)
        {
            int id = touch.fingerId;
            if (EventSystem.current.IsPointerOverGameObject(id))
            {
                return true;
            }
            return false;
        }
    }
}
