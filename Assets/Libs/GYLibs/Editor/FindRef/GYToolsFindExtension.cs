using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

class GYToolsFindExtension
{
    delegate void onReferenceFound();
    static string mCurSeletingObjName = string.Empty;

    [MenuItem("Assets/IdleTools/找引用")]
    static void GetReference()
    {
        if (Selection.objects.Length == 1)
        {
            Object obj = Selection.objects[0];
            mCurSeletingObjName = obj.name;
            string path = AssetDatabase.GetAssetPath(obj);
            string guid = AssetDatabase.AssetPathToGUID(path);

            //目前改造成单文件查找
            List<string> guids = new List<string>();
            guids.Add(guid);
            Find(guids, OutputReference);
        }
    }

    /// <summary>
    /// 打印引用
    /// </summary>
    static private void OutputReference()
    {
        StringBuilder sb_result = new StringBuilder();
        foreach (string key in findResults.Keys)
        {
            StringBuilder sb = new StringBuilder();
            sb_result.Append("guid : " + key);
            sb_result.Append("\n");
            List<string> results = findResults[key];
            for (int i = 0; i < results.Count; i++)
            {
                sb.Append(results[i]);
                sb.Append("\n");
            }
            sb_result.Append(sb.ToString());
            sb_result.Append("\n");
            //Debug.Log(sb.ToString());
        }

        FindExtensionResult.ShowFindExtensionResult(mCurSeletingObjName, sb_result.ToString());
        mCurSeletingObjName = string.Empty;
    }

    static Dictionary<string, List<string>> findResults = new Dictionary<string, List<string>>();
    /// <summary>
    /// GUID GUIDLIST
    /// </summary>
    /// <param name="guids"></param>
    /// <returns></returns>
    static private void Find(List<string> guids, onReferenceFound callback)
    {
        //Debug.Log("start find");
        findResults = new Dictionary<string, List<string>>();

        //目前只在这几类文件找,有需要自行添加
        EditorSettings.serializationMode = SerializationMode.ForceText;
        List<string> withoutExtensions = new List<string>() { ".prefab", ".unity", ".mat", ".asset" };
        string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
        int startIndex = 0;

        //逐帧查找
        EditorApplication.update = delegate ()
        {
            string file = files[startIndex];

            bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

            string fileText = File.ReadAllText(file);
            for (int i = 0; i < guids.Count; i++)
            {
                string guid = guids[i];
                string rePath = GetRelativeAssetsPath(file);
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(guid));
                string name = asset.name;

                //NGUI存在图集二次引用,还需要查找文件名
                if (Regex.IsMatch(fileText, guid) || Regex.IsMatch(fileText, name))
                {
                    if (findResults.ContainsKey(guid))
                    {
                        findResults[guid].Add(rePath);
                    }
                    else
                    {
                        findResults.Add(guid, new List<string>());
                        findResults[guid].Add(rePath);
                    }
                }
            }

            startIndex++;
            if (isCancel || startIndex >= files.Length)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
                //Debug.Log("匹配结束");
                if (callback != null)
                {
                    callback.Invoke();
                }
            }

        };
    }

    /// <summary>
    /// 获得相对路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}
