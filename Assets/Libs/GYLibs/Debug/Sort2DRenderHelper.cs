using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Sort2DRenderHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    [Button("Sort Sprite By Y")]
    public void DoSortSpriteY()
    {
        Renderer[] rendererList = this.gameObject.GetComponentsInChildren<Renderer>();
        List<Renderer> list = new List<Renderer>();
        list.AddRange(rendererList);
        if (list.Count > 1)
        {
            list.Sort(SortRendererYFunc);
            int minZ = 1;
            int curIndex = -1;
            float lastY = float.MinValue;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].gameObject != this.gameObject)
                {
                    Vector3 pos = list[i].transform.localPosition;
                    if (lastY != pos.y)
                    {
                        curIndex += 1;
                    }
                    list[i].sortingOrder = minZ + curIndex;
                    lastY = pos.y;
                }
            }
        }
    }

    [Button("Sort Group By Y")]
    public void DoSortGroupLayerY()
    {
        SortingGroup[] rendererList = this.gameObject.GetComponentsInChildren<SortingGroup>(false);
        List<SortingGroup> list = new List<SortingGroup>();
        list.AddRange(rendererList);
        if (list.Count > 1)
        {
            list.Sort(SortGroupYFunc);
            int minZ = 1;
            int curIndex = -1;
            float lastY = float.MinValue;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].gameObject != this.gameObject)
                {
                    Vector3 pos = list[i].transform.localPosition;
                    if (lastY != pos.y)
                    {
                        curIndex += 1;
                    }
                    list[i].sortingOrder = minZ + curIndex;
                    lastY = pos.y;
                }
            }
        }
    }

    private int SortGroupYFunc(SortingGroup obj1, SortingGroup obj2)
    {
        Vector3 pos1 = obj1.transform.localPosition;
        Vector3 pos2 = obj2.transform.localPosition;
        if (pos1.y != pos2.y)
        {
            return pos2.y.CompareTo(pos1.y);
        }
        else
        {
            return pos2.x.CompareTo(pos1.x);
        }
    }

    private int SortRendererYFunc(Renderer obj1, Renderer obj2)
    {
        Vector3 pos1 = obj1.transform.localPosition;
        Vector3 pos2 = obj2.transform.localPosition;
        if (pos1.y != pos2.y)
        {
            return pos2.y.CompareTo(pos1.y);
        }
        else
        {
            return pos2.x.CompareTo(pos1.x);
        }
    }
}
