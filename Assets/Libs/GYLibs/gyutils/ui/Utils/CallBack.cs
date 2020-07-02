using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GYLib.UI
{
    /// <summary>
    ///  author 
    /// </summary>
    public class CallBack
    {
        public delegate void FunVoid();
        public delegate void FunFloat(float f);
        public delegate void FunInt(int i);
        public delegate void FunLong(long l);
        public delegate void FunString(string s);
        public delegate void FunObject(object o);
        public delegate void FunBool(bool b);
        public delegate void FunGameObject(GameObject b);
        public delegate bool FunClose(GameObject b);
    }
}
