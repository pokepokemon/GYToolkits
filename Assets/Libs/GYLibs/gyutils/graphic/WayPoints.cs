using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoints : MonoBehaviour {

#if UNITY_EDITOR
    public bool showSharpInEditor = true;
    public bool showTextInEditor = true;
    public Color editorGizmosColor = new Color(1f, 0f, 0f, 1f);
    public Color editorSphereColor = new Color(1f, 1f, 0f, 1f);

    [RangeAttribute(10, 40)]
    public int editorTextSize = 25;
#endif

    public List<Transform> wayPoints = new List<Transform>();

	// Use this for initialization
	void Start () {
		
	}

    private Vector3[] _posList = null;
    public Vector3[] GetPosList()
    {
        if (_posList == null)
        {
            List<Vector3> tmpList = new List<Vector3>();
            for (int i = 0; i < wayPoints.Count; i++)
            {
                tmpList.Add(wayPoints[i].position);
            }
            _posList = tmpList.ToArray();
        }
        return _posList;
    }

#if UNITY_EDITOR

    protected void OnDrawGizmos()
    {
        Transform lastTf = null;
        Transform tf = null;
        for (int i = 0; i < wayPoints.Count; i++)
        {
            tf = wayPoints[i];
            if (tf != null)
            {
                DrawGUIText(tf, this.name + ":" + i);
                // 画 Gizmos
                Gizmos.color = editorGizmosColor;
                Gizmos.DrawCube(tf.position, Vector3.one * 0.9f);


                if (!showSharpInEditor)
                {
                    continue;
                }
                if (lastTf != null)
                {
                    Gizmos.DrawLine(tf.position, lastTf.position);
                }
                lastTf = tf;
            }
        }
        DrawMoveSphere();
    }

    /// <summary>
    /// 画文字
    /// </summary>
    /// <param name="tf"></param>
    protected virtual void DrawGUIText(Transform tf, string content)
    {
        if (!showTextInEditor)
        {
            return;
        }
        UnityEditor.Handles.BeginGUI();
        var restoreColor = GUI.color;
        GUI.color = editorGizmosColor;
        var view = UnityEditor.SceneView.currentDrawingSceneView;
        Vector3 screenPos = view.camera.WorldToScreenPoint(tf.position);

        if (screenPos.y >= 0 && screenPos.y <= Screen.height && screenPos.x >= 0 && screenPos.x <= Screen.width && screenPos.z >= 0)
        {
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(content));
            GUIStyle style = new GUIStyle();
            style.fontSize = editorTextSize;
            style.normal.textColor = GUI.color;
            //调整位置
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x * 20, size.y * 20), content, style);
        }

        GUI.color = restoreColor;
        UnityEditor.Handles.EndGUI();
    }

    
    private Vector3 _editorMovePos;
    private Vector3 _editorStartPos;
    private Vector3 _editorEndPos;
    private Vector3 _editorMoveOffset;
    private int _editorMoveIndex = 0;

    [RangeAttribute(2, 100)]
    public int editorSphereSplitTimes = 15;
    private int _lastEditorSphereSplit = 15;
    /// <summary>
    /// 画轨迹
    /// </summary>
    protected virtual void DrawMoveSphere()
    {
        if (!showSharpInEditor)
        {
            return;
        }
        if (wayPoints != null && wayPoints.Count > 1)
        {
            for (int i = 0; i < wayPoints.Count; i++)
            {
                if (wayPoints[i] == null)
                {
                    return;
                }
            }
            Gizmos.color = editorSphereColor;
            if (_editorMoveIndex + 1 >= wayPoints.Count)
            {
                _editorMoveIndex = 0;
            }
            if (_editorStartPos == wayPoints[_editorMoveIndex].position 
                && _editorEndPos == wayPoints[_editorMoveIndex + 1].position 
                && editorSphereSplitTimes == _lastEditorSphereSplit)
            {
                if (Vector3.Distance(_editorMovePos, _editorEndPos) < 0.1f)
                {
                    if (_editorMoveIndex + 2 < wayPoints.Count)
                    {
                        _editorMoveIndex++;
                    }
                    else
                    {
                        _editorMoveIndex = 0;
                    }

                    _editorStartPos = wayPoints[_editorMoveIndex].position;
                    _editorEndPos = wayPoints[_editorMoveIndex + 1].position;
                    _editorMovePos = _editorStartPos;
                    _editorMoveOffset = (_editorEndPos - _editorStartPos) / editorSphereSplitTimes;
                }
                else
                {
                    _editorMovePos = _editorMovePos + _editorMoveOffset;
                }
            }
            else
            {
                _editorMoveIndex = 0;
                _editorStartPos = wayPoints[_editorMoveIndex].position;
                _editorEndPos = wayPoints[_editorMoveIndex + 1].position;
                _editorMovePos = _editorStartPos;
                _editorMoveOffset = (_editorEndPos - _editorStartPos) / editorSphereSplitTimes;

                _lastEditorSphereSplit = editorSphereSplitTimes;
            }
            Gizmos.DrawSphere(_editorMovePos, 0.9f);
        }
    }
#endif
}
