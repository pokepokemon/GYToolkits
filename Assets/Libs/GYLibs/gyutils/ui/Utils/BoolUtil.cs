using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GYLib.UI
{
    /// <summary>
    /// author 
    /// </summary>
    public class BoolUtil
    {
        public static bool IsTure(object obj)
        {
            if (obj is bool)
                return (bool)obj;
            else if (obj == null)
                return false;
            else if (obj is string)
            {
                if ((string)obj != "" && (string)obj != "false" && (string)obj != "False")
                    return true;
                return false;
            }
            else if (obj is ValueType)
            {
                if (float.Parse(obj.ToString()) == 0)
                    return false;
                return true;
            }
            return true;
        }
        public static bool IsFalse(object obj)
        {
            return !IsTure(obj);
        }
    }
}
