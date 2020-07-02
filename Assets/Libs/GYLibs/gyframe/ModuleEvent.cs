using UnityEngine;
using System.Collections;
using System;
using System.ComponentModel;

namespace GYLib.GYFrame
{
    public class ModuleEvent
    {
        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}
