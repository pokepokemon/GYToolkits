using UnityEngine;
using UnityEngine.Events;

namespace GYLibs.control
{
    public class SimpleInputMonitor
    {
        private bool _isRunning = false;

        public IInputController inputController { private set; get; }

        public void Start()
        {
            _isRunning = true;
#if UNITY_EDITOR || UNITY_STANDALONE
            inputController = new PCOnClickController();
#else
            _inputController = new MobileTouchController();
#endif
        }

        public void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            inputController.Update();
        }

        public void Stop()
        {
            _isRunning = false;
            inputController.Stop();
        }
    }
}