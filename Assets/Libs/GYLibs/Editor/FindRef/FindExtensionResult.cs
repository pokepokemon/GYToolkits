using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class FindExtensionResult : EditorWindow
{
    static string mFindingObjName = string.Empty;
    static string mFoundResult = string.Empty;

    static Vector2 mScroll;
    public static void ShowFindExtensionResult(string name, string result)
    {
        mFindingObjName = name;
        mFoundResult = result;
        EditorWindow window = EditorWindow.GetWindow(typeof(FindExtensionResult));
        window.titleContent = new GUIContent("查找结果");
    }

    protected void OnGUI()
    {

        /*
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginScrollView(new Vector2(0, vSbarValue));
                vSbarValue = GUILayout.VerticalScrollbar(vSbarValue, 1.0F, 100.0F, 0.0F);        
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();*/

        GUILayout.BeginHorizontal();
        GUILayout.Space(3f);
        GUILayout.BeginVertical();

        mScroll = GUILayout.BeginScrollView(mScroll);

        //GUILayout.Label("查找结果");
        GUILayout.Label("查找对象 ：" + mFindingObjName);
        GUILayout.Label("引用情况 ：");
        //GUILayout.Label(mFoundResult);
        string[] lines = mFoundResult.Split('\n');
        if (lines.Length > 0)
            GUILayout.Label(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i]))
                continue;

            //EditorGUILayout.BeginHorizontal();
            //GUILayout.Label(lines[i]);
            if (GUILayout.Button(new GUIContent(lines[i], "select")/*, GUILayout.Width(160f)*/))
                Selection.activeGameObject = AssetDatabase.LoadAssetAtPath(lines[i], typeof(GameObject)) as GameObject;

            //EditorGUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(3f);
        GUILayout.EndHorizontal();
    }
}
