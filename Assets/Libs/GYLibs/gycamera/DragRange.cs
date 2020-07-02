using UnityEngine;
using UnityEditor;

/// <summary>
/// 摄像机可拖动范围
/// </summary>
public class DragRange : MonoBehaviour
{
#if false
    public Color editorGizmosColor = new Color(1f, 0f, 0f, 1f);
#endif

    public float minX;
    public float minY;
    public float maxX;
    public float maxY;

    public float scaleMinX;

    public float minScale;
    public float maxScale;

    public float originSize;
}