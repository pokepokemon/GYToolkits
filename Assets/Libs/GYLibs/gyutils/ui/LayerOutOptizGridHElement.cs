using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using GYLib.Utils;

public class LayerOutOptizGridHElement : MonoBehaviour
{
    public float top;
    public float bottom;
    public int gridRowCount;

    public float spaceX;
    public float spaceY;

    public float gridWidth;
    public float gridHeight;

    public LayoutElement layoutElement;

    public void Start()
    {
        Refresh();
    }

    /// <summary>
    /// Cell适配锚点要在上中,AnchorY取1
    /// </summary>
    public void Refresh()
    {
        int count = this.transform.childCount;
        float totalWidth = gridWidth * gridRowCount + spaceX * (gridRowCount - 1);
        
        float totalHeight = bottom;
        float startX = -totalWidth / 2 + gridWidth / 2;
        float curX = startX;
        float curY = -top;

        int gridCounter = 0;
        int rowCount = count / gridRowCount + 1;
        totalHeight = rowCount * gridHeight + (rowCount - 1) * spaceY + top + bottom; 

        for (int i = 0; i < count; i++)
        {
            RectTransform rectTf = this.transform.GetChild(i) as RectTransform;
            if (rectTf != null)
            {
                rectTf.anchoredPosition = new Vector2(curX, curY);
                curX += gridWidth + spaceX;
                gridCounter++;

                if (gridCounter >= gridRowCount)
                {
                    gridCounter = 0;
                    curY -= (gridHeight + spaceY);
                    curX = startX;
                }
            }
        }

        layoutElement.preferredWidth = totalWidth;
        layoutElement.preferredHeight = totalHeight;
    }

    private void Reset()
    {
        layoutElement = this.gameObject.GetComponent<LayoutElement>();
    }
}
