using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class TaskBase
{
    public UnityAction<BaseShareData> OnCompleted;
    public bool isRunning { protected set; get; }
    public bool isCompleted { protected set; get; }

    public BaseShareData shareData;

    public virtual void Start()
    {
        isRunning = true;
    }

    public virtual void Update()
    {
    }

    public virtual void End()
    {
        isRunning = false;
    }
}
