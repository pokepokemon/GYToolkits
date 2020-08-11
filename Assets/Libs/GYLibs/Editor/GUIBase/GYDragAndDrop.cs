using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GYLibs.UnityEditor
{
    public class GYDragAndDrop
    {
        public static object[] DropZone(string title, int w, int h)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(title, GUILayout.Width(w), GUILayout.Height(h));
            Rect dropRect = GUILayoutUtility.GetLastRect();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EventType eventType = Event.current.type;
            bool isAccepted = false;

            if (eventType == EventType.DragUpdated || eventType == EventType.DragPerform)
            {
                if (dropRect.Contains(Event.current.mousePosition))
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (eventType == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        isAccepted = true;
                        //Debug.Log("Consuming drop event in inspector. " + Event.current.mousePosition + " rect" + dropRect);
                        Event.current.Use();
                    }
                }
            }

            return isAccepted ? DragAndDrop.objectReferences : null;
        }
    }
}