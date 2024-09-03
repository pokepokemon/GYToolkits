using UnityEngine;
using System.Collections;
using System;
using System.ComponentModel;

namespace GYLib.GYFrame
{
    public class ModuleEvent
    {
        private string typeName = string.Empty;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(typeName))
            {
                typeName = this.GetType().Name;
            }
            return typeName;
        }
    }
}
