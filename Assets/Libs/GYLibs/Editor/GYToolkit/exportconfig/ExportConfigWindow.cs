using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExportConfigWindow : EditorWindow
{
    private Vector2 _scrollPos;
    public ToggleFileName[] pathList;

    private bool _toggleOpen;
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
        string path = Application.dataPath;
        path = path.Replace("\\", "/");
        path = path.Remove(path.LastIndexOf("/") + 1);
        string configPath = path + "Config/";
        path += "Config/excel/";

        string[] strArr = Directory.GetFiles(path, "*.xlsx", SearchOption.AllDirectories);
        _tmpCacheList.Clear();
        for (int i = 0; i < strArr.Length; i++)
        {
            string strName = strArr[i].Substring(strArr[i].LastIndexOf("/") + 1);
            if (!strName.StartsWith("~"))
            {
                _tmpCacheList.Add(new ToggleFileName()
                {
                    fileName = strName,
                    toggle = true,
                });
            }
        }

        //format
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            path = path.Replace("\\", "/");
            configPath = configPath.Replace("\\", "/");
        }
        else
        {
            path = path.Replace("/", "\\");
            configPath = configPath.Replace("/", "\\");
        }

        this.SetXlsxList(configPath, path, _tmpCacheList.ToArray());
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

    private void OnGUI()
    {
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("GenterateAll", new GUILayoutOption[] { GUILayout.MinHeight(50) }))
        {
            EditorUtility.DisplayProgressBar("Waiting", "Waiting for export", 0.5f);
            try
            {
                ExportAll();
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
                    ExportOneItem(toggle.fileName);
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
        AssetDatabase.Refresh();
    }
    
    private void ExportSelected()
    {
        int counter = 0;
        foreach (var item in pathList)
        {
            if (item.toggle)
            {
                ExportOneItem(item.fileName);
                counter++;
            }
        }
        AssetDatabase.Refresh();
    }

    private void ExportOneItem(string fileName)
    {
        string srcExcelFile = Application.platform == RuntimePlatform.OSXEditor ? (_rootPath + "excel/" + fileName) : (_rootPath + "excel\\" + fileName);
        FileManager.CheckAndCreateDoc(srcExcelFile);

        string tarExcelFile = _rootPath + Path.GetFileNameWithoutExtension(fileName) + ".xlsx";
        File.Copy(srcExcelFile, tarExcelFile, true);

        string tarJsonFile = Application.dataPath + "/Resources/config/" + Path.GetFileNameWithoutExtension(fileName);
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            tarJsonFile = tarJsonFile.Replace("\\", "/");
        }
        else
        {
            tarJsonFile = tarJsonFile.Replace("/", "\\");
        }

        Process process = new Process();

        // redirect the output stream of the child process.
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;

        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            process.StartInfo.FileName = _rootPath + "/tools/excel2json.exe";
            process.StartInfo.WorkingDirectory = _rootPath + "/tools/";
        }
        else
        {
            process.StartInfo.FileName = _rootPath + "\\tools\\excel2json.exe";
            process.StartInfo.WorkingDirectory = _rootPath + "\\tools\\";
        }
        process.StartInfo.Arguments = "--excel " + tarExcelFile +
            " --json " + tarJsonFile + ".json.txt --header 3 -a";

        string output = "";
        int exitCode = -1;
        try
        {
            process.Start();

            // do not wait for the child process to exit before
            // reading to the end of its redirected stream.
            // process.WaitForExit();

            // read the output stream first and then wait.
            StreamReader reader = new StreamReader(process.StandardOutput.BaseStream, Encoding.GetEncoding("gb2312"));
            output = reader.ReadToEnd();
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

        if (File.Exists(tarExcelFile))
        {
            File.Delete(tarExcelFile);
        }
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
