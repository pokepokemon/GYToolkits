using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace GYLib.Utils
{
    public class TimeCounter: IFrame
    {
        public delegate void TimeCounterDelegate();
		//这里要用毫秒
		private float _interval = 1;
		private TimeCounterDelegate _completeFunc;
		private bool _autoDispose;
		private bool _autoRepeat;
		private int _repeatCounter;
		private string _registerName;
		private bool _isRunning;
		private float _lastTime = 0;

		/// <summary>
		/// 自定义帧调用器
		/// </summary>
		private EnterFrame _targetFrame;

        /// <summary>
        /// enter frame 一次性计数器
        /// </summary>
        /// <param name="interval">设置计数器触发事件间隔,当到达该间隔时自动停止记数,单位毫秒</param>
        /// <param name="autoDispose">当达到间隔时自动调用dispose清除所有信息,调用时机为调用自定义间隔函数之后,默认为 false</param>
        /// <param name="func">到达间隔时自动调用的函数,默认 null</param>
        /// <param name="registerName">调用函数时应用的参数,默认 null</param>
        /// <param name="frame">指定帧调用</param>
        public TimeCounter(float interval = 1f, bool autoDispose = false, TimeCounterDelegate func = null, string registerName = null, EnterFrame frame = null)
		{
			_registerName = registerName;
			_autoRepeat = false;
			_repeatCounter = -1;
			this._interval = interval;
			_autoDispose = autoDispose;
            _targetFrame = frame;

            if (func != null)
			{
				this.addListener(func);
			}
		}
		
		/**  当到达计数器设置间隔时自动停止记数  */
		public void Start()
		{
			_isRunning = true;
			(_targetFrame ?? EnterFrame.instance).add (this);
			_lastTime = Time.time;
		}

		/**  停止记数  */
		public void Stop()
		{
			_isRunning = false;
            (_targetFrame ?? EnterFrame.instance).remove (this);
		}
		
		/**  到达间隔时间时自动调用函数
		 *  注 : 1个FrameCounter仅持有一个触发函数
		 *   */
		public void addListener(TimeCounterDelegate onComplete)
		{
			this._completeFunc = onComplete;
		}

        public void Update()
        {
            float deltaTime = (TimeUtil.shareTimeSincePlay - _lastTime) * 1000;
            if (deltaTime >= _interval)
            {
                _lastTime = TimeUtil.shareTimeSincePlay;
                oneFrameWork();
            }
        }
		
		private void oneFrameWork()
		{
			judgeCallFunction();
			judgeRepeat();
		}
		
		/**  重复,当此函数调用时,autoDispose的对象会在重复结束时调用dispose方法,无限次重复时不调用
		 * @param times 次数,当值小于0时为无限次重复
		 *   */
		public void Repeat(int times = -1, bool enableFillFrame = false)
		{
			this._autoRepeat = true;
			this._repeatCounter = times;
			Start();
		}

		public void setInterval(int interval)
		{
			this._interval = interval;
		}
		
		public void dispose()
		{
			Stop ();
            _completeFunc = null;
		}
		
		private void judgeRepeat()
		{
			if (!_autoRepeat || _repeatCounter == 1)
			{
				Stop();
				judgeDispose();
			}
			else
			{
				_repeatCounter--;
			}
		}
		
		private void judgeCallFunction()
		{
			if (_completeFunc != null && _isRunning)
			{
				_completeFunc.Invoke();
			}
		}
		
		private void judgeDispose()
		{
			if (_autoDispose)
			{
				dispose();
			}
		}
    }
}