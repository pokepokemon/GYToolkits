using UnityEngine;
using System.Collections;

namespace GYHotfix.Load
{
    public enum LoadStepConst
    {
        None,
        Error,
        WaitingDownload,
        WaitingLoadAB,
        WaitingLoadAsset,
        Completed,
    }
}
