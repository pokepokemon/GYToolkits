﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class EditorCopyPath : Editor
{ 
    [MenuItem("Assets/GYTools/打印路径")]
    /// <summary>
    /// 获取鼠标选中的文件夹路径
    /// </summary>
    /// <returns></returns>
    public static void GetSelectionPath()
    {
        string path = "Assets";
        
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                if (path.Contains("."))
                {
                    path = path.Remove(path.LastIndexOf("."));
                }
                string resourcePath = "Assets/Resources/";
                if (path.StartsWith(resourcePath))
                {
                    path = path.Substring(resourcePath.Length);
                }
                break;
            }
        }
        Debug.Log(path);
        GUIUtility.systemCopyBuffer = path;
    }

    [MenuItem("Assets/GYTools/打印本地文件路径")]
    /// <summary>
    /// 获取全路径
    /// </summary>
    public static void GetSelectFileFullPath()
    {
        string path = "Assets";
        string projectPath = Application.dataPath;
        projectPath = projectPath.Remove(projectPath.LastIndexOf("Assets"));
        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                if (path.Contains("."))
                {
                    path = path.Remove(path.LastIndexOf("."));
                }
                break;
            }
        }
        path = projectPath + path;
        Debug.Log(path);
        GUIUtility.systemCopyBuffer = path;
    }

    /// <summary>
    /// 复制Hierarchy中的全路径
    /// </summary>
    [MenuItem("GameObject/Copy Path", priority = 999)]
    public static void GameObjectCopyPath()
    {
        var transform = Selection.activeTransform;
        if (null == transform)
        {
            return;
        }
        GUIUtility.systemCopyBuffer = GetTransformPath(transform);
    }

    public static string GetTransformPath(Transform transform)
    {
        if (null == transform)
        {
            return string.Empty;
        }
        if (null == transform.parent)
        {
            return transform.name;
        }
        return string.Format("{0}/{1}", GetTransformPath(transform.parent), transform.name);
    }
}
