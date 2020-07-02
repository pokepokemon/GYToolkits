using GYLib;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectPool))]
public class ObjectPoolInspector : Editor {

    private bool _forceRefreshPerFrame = false;
    private bool _keepSort = true;

    private string _filterString = string.Empty;

    private bool _isExpandDetail = true;

    public override void OnInspectorGUI()
    {
        //获取脚本对象
        ObjectPool pool = target as ObjectPool;
        EditorGUILayout.BeginVertical();

        _forceRefreshPerFrame = EditorGUILayout.Toggle("force refresh", _forceRefreshPerFrame);
        _keepSort = EditorGUILayout.Toggle("keep sort", _keepSort);
        _filterString = EditorGUILayout.TextField("filter", _filterString);

        _isExpandDetail = EditorGUILayout.Foldout(_isExpandDetail, "detail");
        if (_isExpandDetail)
        {
            Dictionary<string, Queue<object>> objDict = pool.GetObjectDict();
            Dictionary<string, int> limitDict = pool.GetObjectLimitDict();

            if (_keepSort)
            {
                //先构建后排序
                List<string> keyList = new List<string>();
                Dictionary<string, string> printLine1Map = new Dictionary<string, string>();
                Dictionary<string, string> printLine2Map = new Dictionary<string, string>();
                foreach (var key in objDict.Keys)
                {
                    int limit;
                    Queue<object> objQueue = objDict[key];
                    if (limitDict.TryGetValue(key, out limit))
                    {
                        if (!string.IsNullOrEmpty(_filterString))
                        {
                            if (!key.Contains(_filterString))
                            {
                                continue;
                            }
                        }
                        keyList.Add(key);
                        printLine1Map[key] = ("count : " + objQueue.Count);
                        printLine2Map[key] = ("limit : " + limit);
                    }
                }
                keyList.Sort();
                foreach (var key in keyList)
                {
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField(key);
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(printLine1Map[key]);
                    EditorGUILayout.LabelField(printLine2Map[key]);

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
            }
            else
            {
                //hash构建
                foreach (var key in objDict.Keys)
                {
                    int limit;
                    Queue<object> objQueue = objDict[key];
                    if (limitDict.TryGetValue(key, out limit))
                    {
                        if (!string.IsNullOrEmpty(_filterString))
                        {
                            if (!key.Contains(_filterString))
                            {
                                continue;
                            }
                        }
                        EditorGUILayout.BeginVertical();
                        EditorGUILayout.LabelField(key);
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField("count : " + objQueue.Count);
                        EditorGUILayout.LabelField("limit : " + limit);

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }
                }
            }
        }

        EditorGUILayout.EndVertical();
        if (_forceRefreshPerFrame)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
