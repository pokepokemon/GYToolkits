using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GYLibs.control
{
    public class PCOnClickController : IInputController
    {
        private const float CLICK_INTERVAL = 0.05f;

        private UnityAction<Vector2, int> _onClick;
        public UnityAction<Vector2, int> OnClick {
            get => _onClick;
            set => _onClick = value; 
        }

        private UnityAction<Vector2, int> _onDown;
        public UnityAction<Vector2, int> OnDown
        {
            get => _onDown;
            set => _onDown = value;
        }

        private bool _isLeftMouseDown = false;
        private bool _isRightMouseDown = false;
        private Vector2 _leftMouseDownPosition;
        private Vector2 _rightMouseDownPosition;

        public UnityAction<Vector2> OnClickLeft;
        public UnityAction<Vector2> OnClickRight;

        public void Update()
        {
            // 处理左键逻辑
            HandleMouseInput(
                Input.GetMouseButton(0),
                ref _isLeftMouseDown,
                ref _leftMouseDownPosition,
                OnClickInputType.Left
            );

            // 处理右键逻辑
            HandleMouseInput(
                Input.GetMouseButton(1),
                ref _isRightMouseDown,
                ref _rightMouseDownPosition,
                OnClickInputType.Right
            );
        }

        private void HandleMouseInput(bool isButtonDown, ref bool isMouseDown, ref Vector2 mouseDownPosition, int clickArg)
        {
            if (isMouseDown)
            {
                // 按键释放
                if (!isButtonDown)
                {
                    isMouseDown = false;
                    Vector2 currentMousePosition = Input.mousePosition;
                    Vector2 deltaPosition = currentMousePosition - mouseDownPosition;
                    Vector2 deltaScaled = new Vector2(deltaPosition.x / Screen.width, deltaPosition.y / Screen.height);

                    // 检查是否为点击事件
                    if (deltaScaled.magnitude <= CLICK_INTERVAL)
                    {
                        _onClick?.Invoke(currentMousePosition, clickArg);
                    }
                }
            }
            else
            {
                // 按键按下
                if (isButtonDown && !IsTouchUI())
                {
                    isMouseDown = true;
                    mouseDownPosition = Input.mousePosition;
                    _onDown?.Invoke(mouseDownPosition, clickArg);
                }
            }
        }

        public void Stop()
        {
            _isLeftMouseDown = false;
            _isRightMouseDown = false;
        }

        private bool IsTouchUI()
        {
            return EventSystem.current.IsPointerOverGameObject();
        }
    }
}
