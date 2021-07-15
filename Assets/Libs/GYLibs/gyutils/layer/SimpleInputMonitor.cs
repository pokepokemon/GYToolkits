using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GYLibs.control
{
    public class SimpleInputMonitor
    {
        private bool _isRunning = false;
        private Dictionary<int, Touch> _touchIdMap = new Dictionary<int, Touch>();
        private const float CLICK_INTERVAL = 0.05f;

        /// <summary>
        /// 触摸回调
        /// </summary>
        public UnityAction<Vector2> OnTouch;

        /// <summary>
        /// 是否为单指操作
        /// </summary>
        public bool isSingleTouch = true;

        // Use this for initialization
        public void Start()
        {
            _isRunning = true;
        }

        // Update is called once per frame
        private HashSet<int> _tmpHashBuffer = new HashSet<int>();
        private List<int> _deleteBufferList = new List<int>();
        private List<Touch> _touchNotifyBuffer = new List<Touch>();
        public void Update()
        {
            if (!_isRunning)
            {
                return;
            }
#if UNITY_EDITOR
            UpdateInPC();
#else
            UpdateInMobile();
#endif
        }

        private bool _isPCTouchDown = false;
        private Vector2 _posPCTouch;
        private void UpdateInPC()
        {
            bool isMouseDown = Input.GetMouseButton(0);
            if (_isPCTouchDown && !isMouseDown)
            {
                _isPCTouchDown = false;
                if (!IsTouchUI())
                {
                    Vector2 curPos = Input.mousePosition;
                    Vector2 deltaPos = curPos - _posPCTouch;
                    Vector2 deltaPosScaled = new Vector2(deltaPos.x / Screen.width, deltaPos.y / Screen.height);
                    if (deltaPosScaled.magnitude <= CLICK_INTERVAL)
                    {
                        OnTouch?.Invoke(curPos);
                    }
                    else
                    {
                    }
                }
            }
            else if (!_isPCTouchDown && isMouseDown && !IsTouchUI())
            {
                _isPCTouchDown = true;
                _posPCTouch = Input.mousePosition;
            }
        }

        private void UpdateInMobile()
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

                //检查未检测到的项
                foreach (var touchId in _touchIdMap.Keys)
                {
                    if (!_tmpHashBuffer.Contains(touchId))
                    {
                        _deleteBufferList.Add(touchId);
                    }
                }

                //删除未用项
                if (_deleteBufferList.Count > 0)
                {
                    foreach (var id in _deleteBufferList)
                    {
                        _touchIdMap.Remove(id);
                    }
                    _deleteBufferList.Clear();
                }

                //响应
                if (_touchNotifyBuffer.Count > 0)
                {
                    if (isSingleTouch)
                    {
                            OnTouch?.Invoke(_touchNotifyBuffer[0].position);
                    }
                    else
                    {
                        foreach (var touch in _touchNotifyBuffer)
                        {
                            OnTouch?.Invoke(touch.position);
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

        /// <summary>
        /// 处理触摸信息
        /// </summary>
        /// <param name="touch"></param>
        private void ApplyTouch(Touch touch)
        {
            int touchId = touch.fingerId;
            _tmpHashBuffer.Add(touchId);
            if (touch.phase == TouchPhase.Ended)
            {
                Touch touchedData;
                if (_touchIdMap.TryGetValue(touchId, out touchedData) && !IsTouchUIByTouch(touch))
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
                Touch touchedData;
                if (_touchIdMap.TryGetValue(touchId, out touchedData))
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
            _isRunning = false;
            _isPCTouchDown = false;
            _touchIdMap.Clear();
        }
        
        private bool IsTouchUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
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

    internal enum TouchStaus
    {
        Normal,
        Touched,
    }
}