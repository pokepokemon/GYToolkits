using System.Collections.Generic;
using UnityEngine;

public class SpriteCutBound
{
    static Color alphaColor = new Color(0, 0, 0, 0);

    private Texture2D _tex;
    private Color _defaultColor;
    private int _width;
    private int _height;
    private Dictionary<int, Dictionary<int, bool>> _dict = new Dictionary<int, Dictionary<int, bool>>();
    private int _count;

    public void Start(Texture2D tex)
    {
        _count = 0;
        
        _tex = tex;
        _defaultColor = tex.GetPixel(_width - 1, _height - 1);
        _width = tex.width;
        _height = tex.height;
        
        initDict();
        
        for (int i = 0; i < _width; i++)
        {
            Search(i, 0);
            Search(i, _height - 1);
        }
        for (int i = 0; i < _height; i++)
        {
            Search(0, i);
            Search(_width - 1, i);
        }
    }

    private void initDict()
    {
        _dict.Clear();
        for (int i = 0; i < _width; i++)
        {
            Dictionary<int, bool> tmpDict = new Dictionary<int, bool>();
            _dict.Add(i, tmpDict);
            for (int j = 0; j < _height; j++)
            {
                tmpDict.Add(j, false);
            }
        }
    }

    /// <summary>
    /// 用循环实现。递归会溢出
    /// </summary>
    /// <param name="rootX"></param>
    /// <param name="rootY"></param>
    private void Search(int rootX, int rootY)
    {
        if (_defaultColor == alphaColor)
            return;

        Stack<int> xStack = new Stack<int>();
        Stack<int> yStack = new Stack<int>();

        xStack.Push(rootX);
        yStack.Push(rootY);

        while (xStack.Count != 0)
        {
            int x = xStack.Pop();
            int y = yStack.Pop();
            if (x >= _width || x < 0 || y >= _height || y < 0)
                continue;

            if (!_dict[x][y])
            {
                _count++;
                _dict[x][y] = true;
            }
            else
                continue;


            Color color = _tex.GetPixel(x, y);
            if (FilterUtils.CompareColor(color, _defaultColor))
            {
                _tex.SetPixel(x, y, alphaColor);

                AddToStack(xStack, yStack, x + 1, y + 1);
                AddToStack(xStack, yStack, x + 1, y - 1);
                AddToStack(xStack, yStack, x - 1, y + 1);
                AddToStack(xStack, yStack, x - 1, y - 1);
                AddToStack(xStack, yStack, x + 1, y);
                AddToStack(xStack, yStack, x - 1, y);
                AddToStack(xStack, yStack, x, y + 1);
                AddToStack(xStack, yStack, x, y - 1);
            }
        }
    }

    private void AddToStack(Stack<int> xStack, Stack<int> yStack, int x, int y)
    {
        if (x >= _width || x < 0 || y >= _height || y < 0)
            return;
        if (!_dict[x][y])
        {
            xStack.Push(x);
            yStack.Push(y);
        }
    }
}
