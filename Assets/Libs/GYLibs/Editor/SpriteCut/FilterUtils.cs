using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class FilterUtils
{
    /// <summary>
    /// 色值对比
    /// </summary>
    /// <param name="c1"></param>
    /// <param name="c2"></param>
    /// <returns></returns>
    static public bool CompareColor(Color c1, Color c2)
    {
        if (c1 == c2)
            return true;
        else
        {
            float h1, s1, v1;
            float h2, s2, v2;
            Color.RGBToHSV(c1, out h1, out s1, out v1);
            Color.RGBToHSV(c2, out h2, out s2, out v2);

            float r = Mathf.Abs(c1.r - c2.r);
            float g = Mathf.Abs(c1.g - c2.g);
            float b = Mathf.Abs(c1.b - c2.b);
            bool rs = (r + g + b) < 0.1 && Mathf.Abs(v1 - v2) < 0.2;
            return rs;
        }
    }
}
