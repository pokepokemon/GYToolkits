using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using static ExportConfigWindow;

public class ExportConfigWindow : EditorWindow
{
    private Vector2 _scrollPos;
    public ToggleFileName[] pathList;
    public ToggleFileName[] _allPathList;

    private bool _toggleOpen;

    private string _filterName;

    private string _rootPath;
    private string _excelPath;
    private string _excelExecutePath = "excel.exe";

    [MenuItem("Tools/GYLibs/ExportConfig")] 
    public static void ShowExportWindow()
    {
        ExportConfigWindow window = EditorWindow.GetWindow(typeof(ExportConfigWindow)) as ExportConfigWindow;
        window.titleContent = new GUIContent("ExportWindow");
        window.RefreshSource();
        window.Show();
    }

    private List<ToggleFileName> _tmpCacheList = new List<ToggleFileName>();
    public void RefreshSource()
    {
        (string, string) pathPair = GetConfigBatchPathAndExcelPath();
        string excelPath = pathPair.Item1;
        string configRootPath = pathPair.Item2;

        // init toggle items
        _tmpCacheList.Clear();
        string[] strArr = Directory.GetFiles(excelPath, "*.xlsx", SearchOption.AllDirectories);
        for (int i = 0; i < strArr.Length; i++)
        {
            string tmpStr = strArr[i];
            ExportConfigEditorUtils.FormatSystemPath(ref tmpStr);
            string strName = tmpStr.Substring(excelPath.Length);
            string fileName = tmpStr.Substring(tmpStr.LastIndexOf("\\") + 1);
            if (!fileName.StartsWith("~"))
            {
                _tmpCacheList.Add(new ToggleFileName()
                {
                    fileName = strName,
                    toggle = true,
                });
            }
        }

        //format
        ExportConfigEditorUtils.FormatSystemPath(ref excelPath);
        ExportConfigEditorUtils.FormatSystemPath(ref configRootPath);

        _allPathList = _tmpCacheList.ToArray();
        this.SetXlsxList(configRootPath, excelPath, GetXlsxListWithFilter(_tmpCacheList));
    }

    /// <summary>
    /// 获取配置文件夹
    /// </summary>
    /// <returns></returns>
    public (string, string) GetConfigBatchPathAndExcelPath()
    {
        string path = Application.dataPath;
        path = path.Replace("\\", "/");
        path = path.Remove(path.LastIndexOf("/") + 1);
        string configPath = path + "Config/";
        path += "Config/excel/";

        return (path, configPath);
    }

    private string GetExcelPath()
    {
        string path = Application.dataPath;
        path = path.Replace("\\", "/");
        path = path.Remove(path.LastIndexOf("/") + 1);
        string configPath = path + "Config/";
        path += "Config/excel/";

        return path;
    }

    //设置数据源
    private void SetXlsxList(string rootPath, string sourcePath, ToggleFileName[] path)
    {
        _rootPath = rootPath;
        _excelPath = sourcePath;
        pathList = path;
    }


    /// <summary>
    /// 过滤关键字
    /// </summary>
    /// <param name="names"></param>
    /// <returns></returns>
    private ToggleFileName[] GetXlsxListWithFilter(List<ToggleFileName> names)
    {
        if (!string.IsNullOrEmpty(_filterName))
        {
            return names.Where(x => x.fileName.Contains(_filterName)).ToArray();
        }
        else
        {
            return names.ToArray();
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("GenterateAll", new GUILayoutOption[] { GUILayout.MinHeight(50) }))
        {
            EditorUtility.DisplayProgressBar("Waiting", "Waiting for export", 0.5f);
            try
            {
                ExportConfigEditorUtils.ExportAll(_rootPath);
                AssetDatabase.Refresh();
            } catch (Exception e)
            { }
            EditorUtility.ClearProgressBar();
        }
        
        if (GUILayout.Button("RefreshFiles", new GUILayoutOption[] { GUILayout.MinHeight(50) }))
        {
            RefreshSource();
            EditorUtility.SetDirty(this);
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.HelpBox(new GUIContent("generate single config"));
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("SelectAll", new GUILayoutOption[] { GUILayout.MinHeight(40) }))
        {
            SelectAll();
        }
        if (GUILayout.Button("SelectNone", new GUILayoutOption[] { GUILayout.MinHeight(40) }))
        {
            SelectNone();
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        _toggleOpen = GUILayout.Toggle(_toggleOpen, "Show direct open");
        _excelExecutePath = EditorGUILayout.TextField(_excelExecutePath);
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        DrawItemList();
        GUILayout.EndScrollView();
        EditorGUILayout.HelpBox(new GUIContent("generate selected config"));
        if (GUILayout.Button("Genterate Selected", new GUILayoutOption[] { GUILayout.MinHeight(50) }))
        {
            EditorUtility.DisplayProgressBar("Waiting", "Waiting for export", 0.5f);
            try
            {
                ExportSelected();
            }
            catch (Exception e)
            { }
            EditorUtility.ClearProgressBar();
        }
        GUILayout.Space(10);

        string oldFilterName = _filterName;
        _filterName = GUILayout.TextField(_filterName);
        if (_filterName != oldFilterName)
        {
            RefreshSource();
        }
        GUILayout.EndVertical();
    }

    private void DrawItemList()
    {
        GUILayout.BeginVertical();

        GUILayout.Space(10);
        for (int i = 0; i < pathList.Length; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            ToggleFileName toggle = pathList[i];
            
            if (_toggleOpen && GUILayout.Button("Open", new GUILayoutOption[] { GUILayout.MaxWidth(80) }))
            {
                var itemPath = GetExcelPath() + pathList[i].fileName;
                itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
                System.Diagnostics.Process.Start(_excelExecutePath, '\"' + itemPath + '\"');
            }

            if (GUILayout.Button("Export", new GUILayoutOption[] { GUILayout.MaxWidth(80) }))
            {
                EditorUtility.DisplayProgressBar("Waiting", "Waiting for export", 0.5f);
                try
                {
                    ExportConfigEditorUtils.ExportOneItem(_rootPath, toggle.fileName);
                    ExportConfigEditorUtils.PostCheckGenMultiConfig(GetToggleFileNames(_allPathList), toggle.fileName);
                }
                catch (Exception e)
                { }
                EditorUtility.ClearProgressBar();
                AssetDatabase.Refresh();
            }
            GUILayout.Space(15);
            toggle.toggle = EditorGUILayout.ToggleLeft(toggle.fileName, toggle.toggle);
            
            GUILayout.EndHorizontal();
            GUILayout.Space(3);
        }

        GUILayout.EndVertical();
    }

    private string[] GetToggleFileNames(ToggleFileName[] toggles)
    {
        string[] fileNames = new string[toggles.Length];
        for (int i = 0; i < fileNames.Length; i++)
        {
            fileNames[i] = toggles[i].fileName.Remove(toggles[i].fileName.LastIndexOf("."));
        }
        return fileNames;
    }
    
    private void ExportAll()
    {
        Process process = new Process();

        // redirect the output stream of the child process.
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.FileName = _rootPath + "ToJson.bat";
        process.StartInfo.WorkingDirectory = _rootPath;

        string output = "";
        int exitCode = -1;
        try
        {
            process.Start();

            // do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // process.WaitForExit();

            // read the output stream first and then wait.
            output = process.StandardOutput.ReadToEnd();
            UnityEngine.Debug.Log(output);
            process.WaitForExit();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Run error" + e.ToString()); // or throw new Exception
        }
        finally
        {
            exitCode = process.ExitCode;

            process.Dispose();
            process = null;
        }

        ExportConfigEditorUtils.PostCheckGenMultiConfig(GetToggleFileNames(_allPathList));
        AssetDatabase.Refresh();
    }
    
    private void ExportSelected()
    {
        int counter = 0;
        string singlePath = null;
        foreach (var item in pathList)
        {
            if (item.toggle)
            {
                ExportConfigEditorUtils.ExportOneItem(_rootPath, item.fileName);
                singlePath = item.fileName;
                counter++;
            }
        }

        if (counter == 1)
        {
            ExportConfigEditorUtils.PostCheckGenMultiConfig(GetToggleFileNames(_allPathList), singlePath);
        }
        else
        {
            ExportConfigEditorUtils.PostCheckGenMultiConfig(GetToggleFileNames(_allPathList));
        }
        AssetDatabase.Refresh();
    }

    private void SelectAll()
    {
        foreach (var item in pathList)
        {
            item.toggle = true;
        }
    }
    
    private void SelectNone()
    {
        foreach (var item in pathList)
        {
            item.toggle = false;
        }
    }

    [Serializable]
    public class ToggleFileName
    {
        public bool toggle;
        public string fileName;
    }
}
