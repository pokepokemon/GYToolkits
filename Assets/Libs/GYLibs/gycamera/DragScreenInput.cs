﻿using GYLib;
using GYLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;

/// <summary>
/// 拖动屏幕
/// </summary>
public class DragScreenInput : MonoSingleton<DragScreenInput>
{

    //有状态,但是不复杂,不使用模式了
    internal enum InputStatus
    {
        None,
        Drag,
        Scale,
    };
    
    private bool _isRunning = false;
    private Camera _lastCamera = null;
    private Transform _cameraRigid = null;
    private Transform _tranControl = null;

    private InputStatus _status = InputStatus.None;

    public bool touchEnable = true;
    public bool dragEnable = true;

    public Vector2 lastClickPos;
    public UnityAction<Vector2> onClick;

    private Vector3 _originCameraPos;
    private float _originCameraY;

    private float _scaleRate = 1;

    public void SetRunning(bool value)
    {
        if (_isRunning != value)
        {
            _isRunning = value;

#if !(UNITY_EDITOR || UNITY_STANDALONE_WIN)
            Input.multiTouchEnabled = true;
            Input.simulateMouseWithTouches = false;
            linearFactor = 0.4f;
#endif

            KillTween();
            ResetStatus();

            if (_isRunning)
            {
                _lastCamera = Camera.main;
                if (_lastCamera != null)
                {
                    Transform tranParent = Camera.main.transform.parent;
                    if (tranParent != null)
                    {
                        GameObject go = tranParent.gameObject;
                        if (go.GetComponent<CameraRigid>() != null)
                        {
                            _cameraRigid = tranParent;
                        }
                    }
                    _tranControl = _cameraRigid ?? _lastCamera.transform;
                }
                
                if (!_lastCamera.orthographic)
                {
                    _originCameraPos = _lastCamera.transform.localPosition;

                    _originCameraY = _lastCamera.transform.localPosition.y;
                }
                InitRange();
            }
        }
    }

    public void KillTween()
    {
        if (_tranControl != null)
        {
            _tranControl.DOKill();
        }
    }
    
    private void Update()
    {
        if (_isRunning)
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
            UpdateForMouseDown();
#endif
            if (_status == InputStatus.None)
            {
                UpdateInNone();
            }
            else if (_status == InputStatus.Drag)
            {
                UpdateInDrag();
            }
            else if (_status == InputStatus.Scale)
            {
                UpdateInScale();
            }
        }
    }

    private void ResetStatus()
    {
        _status = InputStatus.None;
    }

    private void UpdateInNone()
    {
        bool isSingleTouchDown = Input.touchCount == 1;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        isSingleTouchDown = isSingleTouchDown || _isMouseDown;
#endif

        if (isSingleTouchDown)
        {
            EnterDrag();
        }
        else if (Input.touchCount == 2)
        {
            EnterScale();
        }
    }

    private bool _hasRange = false;
    private float _lastCameraMinX;
    private float _lastCameraMinY;
    private float _lastCameraMaxX;
    private float _lastCameraMaxY;
    private float _lastCameraMinScale;
    private float _lastCameraMaxScale;
    private float _originCameraSize;
    private float _scalePixelRate;
    /// <summary>
    /// 判定点击间隔
    /// </summary>
    public float CLICK_TIME_INTERVAL = 0.2f;

    /// <summary>
    /// 缓动时间
    /// </summary>
    public float TWEEN_INTERVAL = 0.4f;

    /// <summary>
    /// 拖动移动比例
    /// </summary>
    public float linearFactor = 0.4f;

    /// <summary>
    /// 是否移动过
    /// </summary>
    private bool _isMove = false;

    /// <summary>
    ///  上一次按下的时间
    /// </summary>
    private float _lastDownTime;
#if (UNITY_EDITOR || UNITY_STANDALONE_WIN)

    private const float _CLICK_DISTANCE = 30f;
    private Vector3 _lastMousePt;
    private float _moveDistance;

    /// <summary>
    /// 进行拖拽监听状态
    /// </summary>
    private void EnterDrag(bool resetMove = true)
    {
        if (resetMove)
        {
            _isMove = false;
            _moveDistance = 0;
        }
        _lastDownTime = Time.realtimeSinceStartup;
        _lastMousePt = Input.mousePosition;
        _status = InputStatus.Drag;

        KillTween();
    }
    
    /// <summary>
    /// 拖拽 轮询
    /// </summary>
    private void UpdateInDrag()
    {
        if (_isMouseDown)
        {
            Vector2 deltaPosition = (Input.mousePosition - _lastMousePt) * linearFactor; 
            //Drag move

            Vector3 targetPt = new Vector3(
                _tranControl.localPosition.x - deltaPosition.x,
                _tranControl.localPosition.y,
                _tranControl.localPosition.z - deltaPosition.y);
            if (_hasRange)
            {
                targetPt.x = targetPt.x >= GetMaxX() ? GetMaxX() : targetPt.x;
                targetPt.x = targetPt.x <= GetMinX() ? GetMinX() : targetPt.x;
                targetPt.z = targetPt.z >= GetMaxY() ? GetMaxY() : targetPt.z;
                targetPt.z = targetPt.z <= GetMinY() ? GetMinY() : targetPt.z;
            }

            if (dragEnable)
            {
                _tranControl.DOKill();
                _tranControl.DOLocalMove(targetPt, TWEEN_INTERVAL);
            }
            _lastMousePt = Input.mousePosition;
            _moveDistance += Vector2.SqrMagnitude(deltaPosition);
        }
        else
        {
            float deltaTime = Time.realtimeSinceStartup - _lastDownTime;
            if (_moveDistance < _CLICK_DISTANCE)
            {
                // on click call
                if (onClick != null)
                {
                    lastClickPos = Input.mousePosition;
                    if (touchEnable)
                    {
                        onClick.Invoke(Input.mousePosition);
                    }
                }
            }
            ResetStatus();
        }
    }
#else
    private int _touchSingleId;
    
    private float _moveDistance;
    /// <summary>
    /// 进行拖拽监听状态
    /// </summary>
    private void EnterDrag(bool resetMove = true)
    {
        if (resetMove)
        {
            _isMove = false;
            _moveDistance = 0;
        }
        bool overUI = IsTouchUI();
        if (!overUI)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled)
            {
                _touchSingleId = touch.fingerId;
                _lastDownTime = Time.realtimeSinceStartup;
                _status = InputStatus.Drag;
                KillTween();
            }
            else
            {
                ResetStatus();
            }
        }
        else
        {
            ResetStatus();
        }
    }

    private const float _CLICK_DISTANCE = 100f;
    private Vector3 _lastTouchPosition;
    /// <summary>
    /// 拖拽 轮询
    /// </summary>
    private void UpdateInDrag()
    {
        int len = Input.touchCount;
        if (len == 0 || len > 2)
        {
            if (len == 0 && (_moveDistance < _CLICK_DISTANCE))
            {
                // on click call
                if (onClick != null)
                {
                    lastClickPos = Input.mousePosition;
                    if (touchEnable)
                    {
                        onClick.Invoke(_lastTouchPosition);
                    }
                }
            }
            ResetStatus();
        }
        else if (len == 2)
        {
            _isMove = true;
            EnterScale();
        }
        else
        {
            Touch touch = Input.GetTouch(0);
            if (touch.fingerId == _touchSingleId)
            {
                _isMove = true;
                Vector2 deltaPosition = touch.deltaPosition * linearFactor;
                _moveDistance = _moveDistance + Vector2.SqrMagnitude(deltaPosition);
                //Drag move
                Vector3 targetPt = new Vector3(
                    _tranControl.localPosition.x - deltaPosition.x,
                    _tranControl.localPosition.y,
                    _tranControl.localPosition.z - deltaPosition.y);
                if (_hasRange)
                {
                    targetPt.x = targetPt.x >= GetMaxX() ? GetMaxX() : targetPt.x;
                    targetPt.x = targetPt.x <= GetMinX() ? GetMinX() : targetPt.x;
                    targetPt.z = targetPt.z >= GetMaxY() ? GetMaxY() : targetPt.z;
                    targetPt.z = targetPt.z <= GetMinY() ? GetMinY() : targetPt.z;
                }
    
                if (dragEnable)
                {
                    _tranControl.DOKill();
                    _tranControl.DOLocalMove(targetPt, TWEEN_INTERVAL);
                }
                _lastTouchPosition = touch.position;
            }
        }
    }
#endif

    public void ResetPos()
    {
        if (_tranControl != null && _lastCamera != null)
        {
            _tranControl.transform.DOKill();
            _tranControl.transform.localEulerAngles = Vector3.zero;
            _lastCamera.transform.DOKill();
            if (!_lastCamera.orthographic)
            {
                _lastCamera.transform.localPosition = _originCameraPos;
            }
        }
    }

    /// <summary>
    /// 重置控制位置 Tween
    /// </summary>
    /// <param name="time"></param>
    public void ResetPosWithTween(float time)
    {
        if (_tranControl != null && _lastCamera != null)
        {
            _tranControl.transform.DOKill();
            _tranControl.transform.localEulerAngles = Vector3.zero;
            _lastCamera.transform.DOKill();
            if (!_lastCamera.orthographic)
            {
                _lastCamera.transform.DOLocalMove(_originCameraPos, time);
            }
        }
    }

    public void MoveTo(float x, float y, float interval)
    {
        if (_lastCamera != null)
        {
            Vector3 targetPt = new Vector3(x, _tranControl.localPosition.y, y);
            targetPt.x = targetPt.x >= GetMaxX() ? GetMaxX() : targetPt.x;
            targetPt.x = targetPt.x <= GetMinX() ? GetMinX() : targetPt.x;
            targetPt.z = targetPt.z >= GetMaxY() ? GetMaxY() : targetPt.z;
            targetPt.z = targetPt.z <= GetMinY() ? GetMinY() : targetPt.z;
            _tranControl.DOKill();
            _tranControl.DOLocalMove(targetPt, interval);
        }
    }

    public void ScaleTo(float targetScale, bool withTween = false, float tweenTime = 0.5f)
    {
        float oldScale = _scaleRate;
        float resultRate = targetScale;
        resultRate = resultRate > _lastCameraMaxScale ? _lastCameraMaxScale : resultRate;
        resultRate = resultRate < _lastCameraMinScale ? _lastCameraMinScale : resultRate;
        _scaleRate = resultRate;

        if (_lastCamera.orthographic)
        {
            _lastCamera.orthographicSize = _scaleRate * _originCameraSize;
        }
        else
        {
            Vector3 pos = _originCameraPos * _scaleRate;
            if (withTween)
            {
                _lastCamera.transform.DOLocalMove(pos, tweenTime);
            }
            else
            {
                _lastCamera.transform.localPosition = pos;
            }
        }
    }

    /// <summary>
    /// 进行缩放监听状态
    /// </summary>
    private Dictionary<int, bool> _multiDict = new Dictionary<int, bool>();
    private float _lastDistance = 0;
    private void EnterScale()
    {
        bool overUI = IsTouchUI();
        if (!overUI)
        {
            _status = InputStatus.Scale;
            _multiDict.Clear();
            for (int i = 0; i < Input.touchCount; i++)
            {
                _multiDict.Add(Input.touches[i].fingerId, true);
            }
            _lastDistance = 0;
        }
        else
        {
            ResetStatus();
        }
    }

    private const float _SCALE_DISTANCE_INTERVAL = 10f;
    private const float _SCALE_DISTANCE_FACTOR = 0.015f;
    /// <summary>
    /// 两个手指缩放轮询
    /// </summary>
    private void UpdateInScale()
    {
        int len = Input.touchCount;
        if (len == 1)
        {
            EnterDrag(false);
            _lastDistance = 0;
        }
        else if (len == 2) 
        {
            float newDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            
            if (_lastDistance == 0)
            {
                _lastDistance = newDistance;
            }
            float deltaDistance = newDistance - _lastDistance;

            float deltaScaleRate = -(deltaDistance / _SCALE_DISTANCE_INTERVAL) * _SCALE_DISTANCE_FACTOR;
            //TO DO
            float oldScale = _scaleRate;
            float resultRate = _scaleRate + deltaScaleRate;
            resultRate = resultRate > _lastCameraMaxScale ? _lastCameraMaxScale : resultRate;
            resultRate = resultRate < _lastCameraMinScale ? _lastCameraMinScale : resultRate;
            _scaleRate = resultRate;

            if (_lastCamera.orthographic)
            {
                _lastCamera.orthographicSize = _scaleRate * _originCameraSize;
            }
            else
            {
                Vector3 pos = _originCameraPos * _scaleRate;
                _lastCamera.transform.DOLocalMove(pos, TWEEN_INTERVAL);
            }
            _lastDistance = newDistance;
        }
        else
        {
            ResetStatus();
            _lastDistance = 0;
        }
    }

    /// <summary>
    /// 初始化移动范围
    /// </summary>
    private void InitRange()
    {
        DragRange range = _lastCamera.gameObject.GetComponent<DragRange>();
        if (range != null)
        {
            _hasRange = true;
            _lastCameraMinX = range.minX;
            _lastCameraMinY = range.minY;
            _lastCameraMaxX = range.maxX;
            _lastCameraMaxY = range.maxY;

            _lastCameraMinScale = range.minScale;
            _lastCameraMaxScale = range.maxScale;
            _originCameraSize = range.originSize;

            float deltaX = range.minX - range.scaleMinX;
            float deltaScale = 1 - range.minScale;
            if (deltaScale != 0)
            {
                _scalePixelRate = deltaX / deltaScale;
            }
            else
            {
                _scalePixelRate = 0;
            }
            _scaleRate = 1;
        }
        else
        {
            _hasRange = false;
        }
    }

    private float GetMaxX()
    {
        return _lastCameraMaxX;
    }

    private float GetMinX()
    {
        return _lastCameraMinX;
    }

    private float GetMaxY()
    {
        return _lastCameraMaxY;
    }

    private float GetMinY()
    {
        return _lastCameraMinY;
    }

    private bool IsTouchUI()
    {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        return EventSystem.current.IsPointerOverGameObject();
#else
        foreach (Touch touch in Input.touches)
        {
            int id = touch.fingerId;
            if (EventSystem.current.IsPointerOverGameObject(id))
            {
                return true;
            }
        }
        
        return false;
#endif
    }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
    private bool _isMouseDown = false;
    private void UpdateForMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isMouseDown = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            _isMouseDown = false;
        }
        if (IsTouchUI())
        {
            _isMouseDown = false;
        }
    }
#endif
}