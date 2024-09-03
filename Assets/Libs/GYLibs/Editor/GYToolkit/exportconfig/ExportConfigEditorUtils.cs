using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ExportConfigEditorUtils
{

    /// <summary>
    /// 导出单个配置
    /// </summary>
    /// <param name="rootPath"></param>
    /// <param name="fileName"></param>
    public static void ExportOneItem(string rootPath, string fileName)
    {
        string srcExcelFile = Application.platform == RuntimePlatform.OSXEditor ? (rootPath + "excel/" + fileName) : (rootPath + "excel\\" + fileName);
        FileManager.CheckAndCreateDoc(srcExcelFile);
        string srcFolder = fileName.Remove(fileName.IndexOf(Path.GetFileNameWithoutExtension(fileName)));

        string tarExcelFile = rootPath + Path.GetFileNameWithoutExtension(fileName) + ".xlsx";
        File.Copy(srcExcelFile, tarExcelFile, true);

        string tarJsonFile = Application.dataPath + "/Resources/config/" + srcFolder + Path.GetFileNameWithoutExtension(fileName);
        FileManager.CheckAndCreateDoc(tarJsonFile);
        FormatSystemPath(ref tarJsonFile);
        FormatSystemPath(ref srcFolder);

        Process process = new Process();

        // redirect the output stream of the child process.
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;

        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            process.StartInfo.FileName = rootPath + "/tools/excel2json.exe";
            process.StartInfo.WorkingDirectory = rootPath + "/tools/";
        }
        else
        {
            process.StartInfo.FileName = rootPath + "\\tools\\excel2json.exe";
            process.StartInfo.WorkingDirectory = rootPath + "\\tools\\";
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

    private static void FormatSystemPath(ref string path)
    {
        if (Application.platform == RuntimePlatform.OSXEditor)
        {
            path = path.Replace("\\", "/");
        }
        else
        {
            path = path.Replace("/", "\\");
        }
    }

    /// <summary>
    /// 导出所有配置
    /// </summary>
    /// <param name="rootPath"></param>
    public static void ExportAll(string rootPath)
    {
        Process process = new Process();

        // redirect the output stream of the child process.
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.FileName = rootPath + "ToJson.bat";
        process.StartInfo.WorkingDirectory = rootPath;

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

    public static void PostCheckGenMultiConfig(string[] jsonPaths, string singlePath = null)
    {
        Dictionary<string, List<string>> organizedDict = OrganizePathList(jsonPaths, singlePath);
        foreach (string key in organizedDict.Keys)
        {
            List<MultiConfigEditorData> datas = new List<MultiConfigEditorData>();
            List<string> paths = organizedDict[key];
            for (int i = 0; i < paths.Count; i++)
            {
                datas.Add(new MultiConfigEditorData() { id = i + 1, path = paths[i] });
            }
            string jsonStr = JsonConvert.SerializeObject(datas);

            string tarJsonFilePath = Application.dataPath + "/Resources/config/" + key + "_multi_path.json.txt";
            FileManager.CheckAndCreateDoc(tarJsonFilePath);
            FormatSystemPath(ref tarJsonFilePath);
            File.WriteAllText(tarJsonFilePath, jsonStr);
            UnityEngine.Debug.Log("Multi Config Output : " + tarJsonFilePath);
        }
    }

    /// <summary>
    /// 将文件组织成字典
    /// </summary>
    /// <param name="pathList"></param>
    /// <param name="singlePath"></param>
    /// <returns></returns>
    private static Dictionary<string, List<string>> OrganizePathList(string[] pathList, string singlePath = null)
    {
        Dictionary<string, List<string>> organizedDict = new Dictionary<string, List<string>>();
        Regex regex = new Regex(@"^(?<name>.+)_(?<number>\d+)$");
        string fileNameKey = null;
        List<string> nameKeyList = new List<string>();
        string singleFileName = singlePath == null ? null : singlePath.Remove(singlePath.LastIndexOf("."));

        foreach (string path in pathList)
        {
            Match match = regex.Match(Path.GetFileNameWithoutExtension(path));
            if (match.Success)
            {
                string name = match.Groups["name"].Value;
                if (!organizedDict.ContainsKey(name))
                {
                    organizedDict[name] = new List<string>();
                    nameKeyList.Add(name);
                }
                organizedDict[name].Add(path);
                if (!string.IsNullOrEmpty(singlePath) && path == singleFileName)
                {
                    fileNameKey = name;
                }
            }
        }

        // 只生成单个配置时,最小化刷新
        if (singleFileName != null)
        {
            for (int i = 0; i < nameKeyList.Count; i++)
            {
                string nameKey = nameKeyList[i];
                if (nameKey != fileNameKey)
                {
                    organizedDict.Remove(nameKey);
                }
            }
        }

        return organizedDict;
    }

    internal class MultiConfigEditorData
    {
        public int id;
        public string path;
    }
}
