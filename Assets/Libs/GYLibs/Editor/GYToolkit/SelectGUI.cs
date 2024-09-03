using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
using TMPro;

namespace SK.Framework
{
    [InitializeOnLoad]
    public static class UISelector
    {
        static string[] ComponentEnum = new string[] {      //需要检测的组件添加在这里
            "Image",
            "SingleImage",
            "Button",
            "TextMeshProUGUI",
            "EmptyUIRaycast",
            "Text",
            //"Animator"
        };

        static string[] IgnoreName = new string[] {      
            //"rain_image",
        };


        static Dictionary<string, string> componentEnum2NameDic = new Dictionary<string, string>()
        {
            ["Image"] = "图片",
            ["SingleImage"] = "大图",
            ["Button"] = "按钮",
            ["TextMeshProUGUI"] = "TMP文本",
            ["EmptyUIRaycast"] = "空的点击区域",
            ["LangTxt"] = "多语言脚本",
            ["Animator"] = "动画状态机",
            ["Animation"] = "动画",
            ["Text"] = "文本",
        };


        static UISelector()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            UnityEditor.SceneManagement.PrefabStage prefabStage =  UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                //打开Prefab事件
                UnityEditor.SceneManagement.PrefabStage.prefabStageOpened += OnPrefabStageOpened;
            }
        }

        static string componentName = "";
        static string tips = "";
        static List<string> ComponentType = new List<string>(ComponentEnum);
        static List<string> IgnoreList = new List<string>(IgnoreName);
        static Vector2 scrollPosition;
        static int width = 50;
        static int height = 20;
        static bool isScene = false;
        static IGrouping<string, RectTransform>[] groups;

        private static void OnPrefabStageOpened(UnityEditor.SceneManagement.PrefabStage prefabStage)
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private static void OnPrefabStageClosing(UnityEditor.SceneManagement.PrefabStage prefabStage)
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private static bool state = false;
        private static bool selectState = true;
        private const string menuName = "Tools/scene窗口选中UI";
        [MenuItem(menuName, false, 0)]
        private static void ShowMenuPick()
        {
            state = !state;
            Menu.SetChecked(menuName, state);
        }

        private static void SelectComponent()
        {
            selectState = !selectState;
        }

        public static void OnSceneGUI(SceneView sceneView)
        {
            #region GUI绘制
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(0, 0, 150,200));
            var usePick = GUILayout.Toggle(state,"右键UI选择组件");
            if (usePick != state)
            {
                ShowMenuPick();
            }
            if (usePick)
            {
                GUILayout.Label("输入要检测的控件名");
                //GUIContent GUIcontent = new GUIContent("This is a box","6666666666666666666666666");
                //GUI.Box(new Rect(10, 90, 100, 90), GUIcontent);

                componentName = GUILayout.TextField(componentName, 100, GUILayout.Height(height), GUILayout.Width(100));
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("添加", GUILayout.Height(height), GUILayout.Width(width)))
                {
                    componentName = ToUpperFirst(componentName);            //首字母转换成大写
                    if (ComponentType.Exists(str => str == componentName))
                    {

                        tips = "已经存在这个组件啦";


                    }
                    else if (componentName.Equals(""))
                    {
                        tips = "不要添加空气";
                    }
                    else
                    {
                        if (componentName.Equals("TMP"))
                        {
                            ComponentType.Add("TextMeshProUGUI");
                        }
                        else
                        {
                            ComponentType.Add(componentName);
                        }
                        Debug.Log(componentName);
                    }
                }
                if (GUILayout.Button("清空", GUILayout.Height(height), GUILayout.Width(width)))
                {
                    ComponentType.Clear();
                    tips = "";
                    componentName = "";
                }
                GUILayout.EndHorizontal();
                scrollPosition = GUILayout.BeginScrollView(
                scrollPosition, false, false);
                if (tips == "")
                    tips = "当前检测的组件：";
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.green;
                GUILayout.Label(tips, style);
                for (int i = 0; i < ComponentType.Count; i++)
                {
                    string component = ComponentType[i];
                    var selected = GUILayout.Toggle(selectState, component);
                    if (selected != selectState)
                        ComponentType.Remove(component);
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
            Handles.EndGUI();
            #endregion
            if (!state)
            {
                return;
            }
            var ec = Event.current;
 
            if (ec != null && ec.button == 1 && ec.type == EventType.MouseUp)
            {
                ec.Use();
                // 当前屏幕坐标，左上角是（0，0）右下角（camera.pixelWidth，camera.pixelHeight）
                Vector2 mousePosition = Event.current.mousePosition;
                // Retina 屏幕需要拉伸值
                float mult = EditorGUIUtility.pixelsPerPoint;
                // 转换成摄像机可接受的屏幕坐标，左下角是（0，0，0）右上角是（camera.pixelWidth，camera.pixelHeight，0）
                mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y * mult;
                mousePosition.x *= mult;
                UnityEditor.SceneManagement.PrefabStage prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
                if (prefabStage == null)
                {
                    isScene = true;
                }
                else
                {
                    isScene = false;
                }
                groups = null;
                //运行状态
                if (isScene)
                {
                    var scenes = GetAllScenes();
                    groups = scenes
                        .Where(m => m.isLoaded)
                        .SelectMany(m => m.GetRootGameObjects())
                        .Where(m => m.activeInHierarchy)
                        .SelectMany(m => FindChildrenWithCanvasGroupAlphaNoZero(m))
                        .Where(m => m.gameObject.activeInHierarchy)
                        .Where(m => IgnoreNodeName(m))
                        .Where(m => GetUIComponent(m))
                        .Where(m => RectTransformUtility.RectangleContainsScreenPoint(m, mousePosition, sceneView.camera))
                    .GroupBy(m => m.gameObject.name)
                    .ToArray();
                }
                //prefab状态
                else
                {
                    GameObject prefabRoot = prefabStage.prefabContentsRoot;
                    if (prefabRoot)
                    {
                        RectTransform[] prefabsTrs = prefabRoot.GetComponentsInChildren<RectTransform>();
                        groups = prefabsTrs.Where(m => GetUIComponent(m))
                            .Where(m => RectTransformUtility.RectangleContainsScreenPoint(m, mousePosition, sceneView.camera))
                            .GroupBy(m => m.gameObject.name)
                            .ToArray();
                    }
                }
                //处理子节点
                var gc = new GenericMenu();
                var dic = new Dictionary<string, int>();
                int count = groups.Length;
                Debug.Log(count);
                for (int i = count-1; i >= 0; i--)
                {
                    foreach (var rt in groups[i])
                    {
                        var name = rt.name;
                        var sceneName = rt.gameObject.scene.name;
                        int len = rt.parent.name.Length;
                        var parentName = len != 0 ? rt.parent.name : "";
                        var nameWithParentName = parentName + "/" + name;
                        var isContains = dic.ContainsKey(nameWithParentName);
                        var parentChildCount = rt.parent.childCount;
                        var text = name;
                        if (rt.GetComponent<TextMeshProUGUI>())
                        {
                            text = rt.GetComponent<TextMeshProUGUI>().text;
                        }
                        foreach (string componentType in ComponentType)
                        {
                            if (rt.GetComponent(componentType))
                            {
                                var comname = componentEnum2NameDic[componentType];
                                text = "[" + comname + "]" + text;
                            }
                        }
                        var content = new GUIContent(text);
                        gc.AddItem(content, false, () =>
                        {
                            Selection.activeTransform = rt;
                            EditorGUIUtility.PingObject(rt.gameObject);
                        });
                        if (!isContains)
                        {
                            dic.Add(nameWithParentName, 1);
                        }
                    }
                }
                gc.ShowAsContext();
            }
        }
        private static IEnumerable<Scene> GetAllScenes()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                yield return SceneManager.GetSceneAt(i);
            }
        }

        private static bool IgnoreNodeName(RectTransform rt)
        {
            foreach (string nodeName in IgnoreList)
            {
                if (rt.gameObject.name.Equals(nodeName))
                {
                    return false;
                }
            }
            return true;
        }


        private static bool GetUIComponent(RectTransform m)
        {
            if (m.GetComponent<Image>() && m.GetComponent<Image>().enabled == false)
            {
                return false;
            }
            foreach (string componentType in ComponentType)
            {
                if (m.GetComponent(componentType))
                {
                    return true;
                }
            }
            return false;
        }

        private static RectTransform[] FindChildrenWithCanvasGroupAlphaNoZero(GameObject root)
        {
            RectTransform parent = CheckRectTranform(root);
            Queue<RectTransform> queue = new Queue<RectTransform>();
            List<RectTransform> list = new List<RectTransform>();
            if (parent != null)
            {
                queue.Enqueue(parent);
            }
            while (queue.Count > 0)
            {
                //访问当前的子节点，当它有canvasGroup且alpha == 0时不添加到选择列表
                RectTransform current = queue.Dequeue();
                Debug.Log(current.name);
                if (current.GetComponent<CanvasGroup>() && current.GetComponent<CanvasGroup>().alpha == 0)
                {
                    continue;
                }
                else
                {
                    list.Add(current);
                }
                //如果当前的节点有子节点且为激活状态，则将其加入队列
                for (int i = 0; i < current.childCount; i++)
                {
                    if (current.GetChild(i).GetComponent<RectTransform>() && current.GetChild(i).gameObject.activeInHierarchy)
                    {
                        queue.Enqueue((RectTransform)current.GetChild(i));
                    }
                }
            }
            return list.ToArray();
        }

        private static RectTransform CheckRectTranform(GameObject gameObject)
        {
            RectTransform ret = null;
            if (gameObject.GetComponent<RectTransform>())
            {
                return ret = gameObject.GetComponent<RectTransform>();
            }
            else
            {
                GameObject[] rootObj = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
                for (int i = 0; i < rootObj.Length; i++)
                {
                    if (rootObj[i].GetComponent<RectTransform>())
                    {
                        return ret = rootObj[i].GetComponent<RectTransform>();
                    }
                }
                return ret;
            }
        }


        //首字母大写
        private static unsafe string ToUpperFirst(string str)
        {
            if (str == null) return null;
            string result = string.Copy(str);
            fixed (char* p = result)
                *p = char.ToUpper(*p);
            return result;
        }

    }
}