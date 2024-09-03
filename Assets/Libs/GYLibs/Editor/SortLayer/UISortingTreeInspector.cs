using GYLib.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UISortingTree))]
public class UISortingTreeInspector : Editor
{
    public UISortingTree sortTree;

    private void OnEnable()
    {
        sortTree = target as UISortingTree;
    }

    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Sort in EditorMode", new GUILayoutOption[] { GUILayout.MinHeight(40) }))
        {
            sortTree.Rebuild();
        }
    }
}
