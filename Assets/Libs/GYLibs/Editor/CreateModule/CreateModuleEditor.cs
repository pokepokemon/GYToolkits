using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;

/// <summary>
/// 快速创建模块代码
/// </summary>
public class CreateModuleEditor : EditorWindow 
{
    [MenuItem("Tools/GYLibs/CreateModule")]
    static void CreateModule()
    {
        //创建窗口
        Rect wr = new Rect(0, 0, 300, 100);
        CreateModuleEditor window = (CreateModuleEditor)EditorWindow.GetWindowWithRect(typeof(CreateModuleEditor), wr, true, "CreateModule");
        window.Show();
    }

    string moduleName = "";
    string authorName = "";

    static string RootPath = "/_script/modules/";

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        moduleName = EditorGUILayout.TextField("ModuleName: ", moduleName);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        authorName = EditorGUILayout.TextField("AuthorName: ", authorName);
        EditorGUILayout.EndHorizontal();
        //EditorGUILayout.BeginHorizontal();
        //isCSharp = EditorGUILayout.Toggle("isCSharpModule:", isCSharp);
        //EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("CreateModule"))
        {
            string moduleNameEndWithoutModuleStr = moduleName;
            if (moduleName.EndsWith("Module"))
            {
                moduleNameEndWithoutModuleStr = moduleNameEndWithoutModuleStr.Replace("Module", "");
            }
            
            CreateModuleByTemplate(moduleNameEndWithoutModuleStr, authorName);
        }
        EditorGUILayout.EndHorizontal();
    }

    void CreateModuleByTemplate(string module, string author)
    {
        if (string.IsNullOrEmpty(module))
            return;

        //1.create folder

        if (!Directory.Exists(Application.dataPath + RootPath + module))
            Directory.CreateDirectory(Application.dataPath + RootPath + module);
        /*
        if (!Directory.Exists(Application.dataPath + RootPath + module + "/ui"))
            Directory.CreateDirectory(Application.dataPath + RootPath + module + "/ui");
        if (!Directory.Exists(Application.dataPath + RootPath + module + "/event"))
            Directory.CreateDirectory(Application.dataPath + RootPath + module + "/event");
        if (!Directory.Exists(Application.dataPath + RootPath + module + "/data"))
            Directory.CreateDirectory(Application.dataPath + RootPath + module + "/data");
            */

        //2.read files
        string sourcePath = Application.dataPath + "/3rdLibs/GYLibs/Editor/CreateModule/";
        string templateModuleName = "TemplateModule";
        string templateProcessor = "TemplateProcessor";
        string moduleContent;
        string processorContent;

        StreamReader reader = new StreamReader(sourcePath + templateModuleName + ".txt");
        moduleContent = reader.ReadToEnd();
        reader.Close();

        reader = new StreamReader(sourcePath + templateProcessor + ".txt");
        processorContent = reader.ReadToEnd();
        reader.Close();


        //3.replace content
        moduleContent = moduleContent.Replace("Template", module);
        if (!string.IsNullOrEmpty(author))
        {
            moduleContent = moduleContent.Replace("xxxAuthor", author);
        }

        processorContent = processorContent.Replace("Template", module);
        if (!string.IsNullOrEmpty(author))
        {
            processorContent = processorContent.Replace("xxxAuthor", author);
        }

        //4.write files
        var tmpPath = Application.dataPath + RootPath + module + "/" + module + "Module.cs";
        StreamWriter writer = new StreamWriter(tmpPath);
        writer.Write(moduleContent);
        writer.Flush();
        writer.Close();

        writer = new StreamWriter(Application.dataPath + RootPath + module + "/" + module + "Processor.cs");
        writer.Write(processorContent);
        writer.Flush();
        writer.Close();

        //5.refresh editor
        AssetDatabase.Refresh();
    }
}
