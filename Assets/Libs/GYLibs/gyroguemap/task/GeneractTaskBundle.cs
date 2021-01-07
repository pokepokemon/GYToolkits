using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GeneractTaskBundle : TaskBase
{
    public TaskBase curTask { private set; get; }
    private Queue<TaskBase> _taskQueue = new Queue<TaskBase>();

    // Use this for initialization
    public override void Start()
    {
        isRunning = true;
    }

    public void AddTask(TaskBase task)
    {
        _taskQueue.Enqueue(task);
    }

    private void CheckNext()
    {
        if (curTask != null && curTask.isCompleted)
        {
            curTask.End();

            if (curTask.OnCompleted != null)
            {
                curTask.OnCompleted(this.shareData);
            }
            curTask = null;
        }

        if (curTask == null)
        {
            if (_taskQueue.Count > 0)
            {
                curTask = _taskQueue.Dequeue();
                curTask.Start();
            }
            else
            {
                isCompleted = true;
            }
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        if (curTask != null)
        {
            curTask.Update();
        }
        CheckNext();
    }

    public override void End()
    {
        base.End();
    }
}
