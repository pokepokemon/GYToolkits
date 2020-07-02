using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SpriteReplaceColor
{
    static Color alphaColor = new Color(0, 0, 0, 0);

    private Texture2D _tex;
    private Color _defaultColor;
    private int _width;
    private int _height;

    public void Start(Texture2D tex)
    {
        _tex = tex;
        _defaultColor = tex.GetPixel(_width - 1, _height - 1);
        _width = tex.width;
        _height = tex.height;

        for (int x = 0; x < _width; x++)
            for (int y = 0; y < _height; y++)
            {
                Color tmpColor = tex.GetPixel(x, y);
                if (FilterUtils.CompareColor(tmpColor ,_defaultColor))
                    tex.SetPixel(x, y, alphaColor);
            }
        
    }

    
}