using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GYLibs.EditorExportConfig
{
    public class ExportConfigScriptWindow : EditorWindow
    {
        private Vector2 _scrollPos;

        public string fileName = "config/card_base.json";
        public string folderName = "card";
        public string className = "CardConfig";  //CardConfig CardConfigData
        public string comments = "xxx config";
        public List<ConfigItemDefine> defineList;
        public List<Dictionary<string, string>> rawConfigs = null;

        [MenuItem("Tools/GYLibs/ScriptGenerator")]
        public static void ShowExportWindow()
        {
            ExportConfigScriptWindow window = EditorWindow.GetWindow(typeof(ExportConfigScriptWindow)) as ExportConfigScriptWindow;
            window.titleContent = new GUIContent("ExportConfigScriptWindow");
            window.Show();
        }

        private void Clear()
        {
            rawConfigs = null;
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();

            comments = EditorGUILayout.TextField("Comments", comments);
            folderName = EditorGUILayout.TextField("FolderName", folderName);
            fileName = EditorGUILayout.TextField("ConfigPath", fileName);
            className = EditorGUILayout.TextField("ClassName", className);
            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start parse", new GUILayoutOption[] { GUILayout.MinHeight(30) }))
            {
                StartParseOriginJson();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Generate", new GUILayoutOption[] { GUILayout.MinHeight(30) }))
            {
                TryGenerate();
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();

            if (rawConfigs != null)
            {
                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                DrawConfig();
                GUILayout.EndScrollView();
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Clear", new GUILayoutOption[] { GUILayout.MinHeight(30) }))
            {
                Clear();
            }

            GUILayout.EndVertical();
        }

        private void DrawConfig()
        {
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            GUILayout.Label("name");
            GUILayout.Label("type");
            GUILayout.EndHorizontal();
            foreach (var item in defineList)
            {
                GUILayout.BeginHorizontal();

                item.name = GUILayout.TextField(item.name);
                item.defineType = (ItemDefineType)EditorGUILayout.EnumPopup(item.defineType);
    
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("New"))
            {
                ConfigItemDefine define = new ConfigItemDefine();
                defineList.Add(define);
            }
            if (GUILayout.Button("Delete") && defineList.Count > 0)
            {
                defineList.RemoveAt(defineList.Count - 1);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void TryGenerate()
        {
            //读取模板
            string sourcePath = Application.dataPath + "/3rdLibs/GYLibs/Editor/GYToolkit/exportconfig/";
            string templateMain = "TemplateConfig";
            string templateSplit = "TemplateConfigSplitString";
            StreamReader reader = new StreamReader(sourcePath + templateMain + ".txt");

            string mainFile;
            string splitFile;
            mainFile = reader.ReadToEnd();
            reader.Close();

            reader = new StreamReader(sourcePath + templateSplit + ".txt");
            splitFile = reader.ReadToEnd();
            reader.Close();

            //替换关键字
            mainFile = mainFile.Replace("{className}", className);
            mainFile = mainFile.Replace("{comments}", comments);
            mainFile = mainFile.Replace("{filePath}", fileName);

            //组装字段
            StringBuilder contentSb = new StringBuilder();
            bool hasGroup = false;
            foreach (var item in defineList)
            {
                if (item.IsGroup())
                {
                    hasGroup = true;
                }
                string result = item.ToString();
                if (!string.IsNullOrEmpty(result))
                {
                    contentSb.Append(result);
                    contentSb.AppendLine();
                    contentSb.AppendLine();
                }
            }
            mainFile = mainFile.Replace("{ItemList}", contentSb.ToString());
            if (hasGroup)
            {
                mainFile = mainFile.Replace("{splitCheck}", PackageSplitScriptContent(splitFile));
            }
            else
            {
                mainFile = mainFile.Replace("{splitCheck}", "");
            }

            //创建文件
            string rootPath = "/_script/config/";
            string finalPath = Application.dataPath + rootPath + folderName;
            if (!Directory.Exists(finalPath))
                Directory.CreateDirectory(finalPath);

            //写入文件
            StreamWriter writer = new StreamWriter(finalPath + "/" + className + ".cs");
            writer.Write(mainFile);
            writer.Flush();
            writer.Close();

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 构造需要解析的字段
        /// </summary>
        /// <param name="splitFile"></param>
        /// <returns></returns>
        private string PackageSplitScriptContent(string splitFile)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(@"char[] splitSymbol = new char[] { ',' };");
            sb.AppendLine(@"string[] strArr;");
            sb.AppendLine(@"int count;");
            sb.AppendLine();

            foreach (var item in defineList)
            {
                if (item.IsGroup())
                {
                    string itemContent = splitFile;
                    itemContent = itemContent.Replace("{itemName}", item.name);
                    if (item.defineType == ItemDefineType.StringGroup)
                    {
                        itemContent = itemContent.Replace("{itemType}", "string");
                        itemContent = itemContent.Replace("{itemAssign}", @"GetArgument");
                    }
                    else if (item.defineType == ItemDefineType.IntGroup)
                    {
                        itemContent = itemContent.Replace("{itemType}", "int");
                        itemContent = itemContent.Replace("{itemAssign}", @"GetIntArguments");
                    }
                    sb.Append(itemContent);
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }


        /// <summary>
        /// 开始解析配置
        /// </summary>
        private void StartParseOriginJson()
        {
            GameLoader.Instance.LoadConfig(fileName, OnLoadCompleted);
        }

        /// <summary>
        /// 重载加载过程
        /// </summary>
        /// <param name="line"></param>
        private void OnLoadCompleted(string json)
        {
            rawConfigs = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(json);
            if (rawConfigs != null)
            {
                defineList = new List<ConfigItemDefine>();
                if (rawConfigs.Count > 0)
                {
                    var map = rawConfigs[0];
                    foreach (var key in map.Keys)
                    {
                        ConfigItemDefine define = new ConfigItemDefine();
                        define.name = key;
                        define.defineType = ItemDefineType.String;
                        defineList.Add(define);
                    }
                }
            }
        }

        public class ConfigItemDefine
        {
            public string name;
            public ItemDefineType defineType = ItemDefineType.None;

            public override string ToString()
            {
                string result = "";
                switch (defineType)
                {
                    case ItemDefineType.Single:
                        result = "\tpublic float " + name + ";";
                        break;
                    case ItemDefineType.Double:
                        result = "\tpublic double " + name + ";";
                        break;
                    case ItemDefineType.Int32:
                        result = "\tpublic int " + name + ";";
                        break;
                    case ItemDefineType.Int64:
                        result = "\tpublic long " + name + ";";
                        break;
                    case ItemDefineType.String:
                        result = "\tpublic string " + name + ";";
                        break;
                    case ItemDefineType.StringGroup:
                        result = "\tpublic string " + name + ";\n";
                        result += "\tpublic string[] " + name + "Arr;";
                        break;
                    case ItemDefineType.IntGroup:
                        result = "\tpublic string " + name + ";\n";
                        result += "\tpublic int[] " + name + "Arr;";
                        break;
                }
                return result;
            }

            public bool IsGroup()
            {
                return defineType == ItemDefineType.StringGroup || defineType == ItemDefineType.IntGroup;
            }
        }

        public enum ItemDefineType
        {
            None,
            String,
            Int32,
            Int64,
            Single,
            Double,
            StringGroup,
            IntGroup,
        }
    }
}