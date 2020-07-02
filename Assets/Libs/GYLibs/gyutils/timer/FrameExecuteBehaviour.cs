using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace GYLib.Utils
{
    /// <summary>
    /// 逐帧执行器
    /// </summary>
    public class FrameExecuteBehaviour : MonoBehaviour
    {
        public delegate void ExecuteCompleted();
        public ExecuteCompleted onCompleted;

        private Queue<UnityAction> actionList;
        public int progress { private set; get; }
        public int totalCount { private set; get; }

        public int timesPerFrameDo = 1;

        // Use this for initialization
        void Start()
        {
            this.gameObject.name = "FrameExecuteBehaviour start";
            DontDestroyOnLoad(this.gameObject);
        }

        private bool _needStart = false;
        /// <summary>
        /// 开始执行逐帧任务
        /// </summary>
        public void StartDo()
        {
            _needStart = true;
        }

        /// <summary>
        /// 添加需要逐帧的函数任务，按顺序执行
        /// </summary>
        /// <param name="action"></param>
        public void AddCaller(UnityAction action)
        {
            if (actionList == null)
            {
                actionList = new Queue<UnityAction>();
                totalCount = 0;
            }
            actionList.Enqueue(action);
            totalCount++;
        }

        // Update is called once per frame
        void Update()
        {
            if (_needStart)
            {
                progress = totalCount - actionList.Count;
                this.gameObject.name = "FrameExecuteBehaviour doing (" + progress + "/" + totalCount + ")";
                for (int i = 0; i < timesPerFrameDo; i++)
                {
                    if (actionList.Count > 0)
                    {
                        UnityAction action = actionList.Dequeue();
                        action.Invoke();
                    }
                }
                if (actionList.Count == 0 && onCompleted != null)
                {
                    this.onCompleted.Invoke();
                    this.onCompleted = null;
                    GameObject.Destroy(this.gameObject);
                }
            }
        }
    }
}