﻿using UnityEngine;
using System.Collections;
using System;

namespace GYLib.Utils
{
    /// <summary>
    /// 定时任务管理器
    /// </summary>
    public class BaseEnterFrameManager : IFrame
    {
        protected float _interval = 60;
        private float _lastTime = 0;
		private bool _isStart = false;

        public void Start()
        {
			if (!_isStart) {
				EnterFrame.instance.add (this);
				_lastTime = Time.timeSinceLevelLoad;
				_isStart = true;
			}
        }

        public void Update()
        {
            if (getTaskLength() != 0)
            {
                float deltaTime = (Time.timeSinceLevelLoad - _lastTime) * 1000;
                int frameElapsed = Convert.ToInt32(Math.Floor(deltaTime / _interval));
                //做个限制避免大循环
                if (frameElapsed > 1 && frameElapsed < 100)
                {
                    for (int i = 0; i < frameElapsed; i++)
                    {
                        oneFrameWork();
                    }
                    _lastTime = Time.timeSinceLevelLoad;
                }
                else if (frameElapsed >= 1)
                {
                    oneFrameWork();
                    _lastTime = Time.timeSinceLevelLoad;
                }
            }
			checkUpdate ();
        }

        public void dispose()
        {
			if (_isStart) {
				_isStart = false;
				EnterFrame.instance.remove (this);
			}
        }

        protected virtual void oneFrameWork()
        {
           
        }

        protected void checkUpdate()
        {
            if (getTaskLength () != 0) {
				Start ();
			} else {
				dispose();
			}
        }

        protected virtual int getTaskLength()
        {
            return 0;
        }
    }
}
