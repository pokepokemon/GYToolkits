using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEngine;
using Newtonsoft.Json;
using GYLib.Utils;
using System.Text;

/// <summary>
/// 补丁和DLC打包
/// </summary>
public class AssetBundleMenu
{
    [MenuItem("Tools/打包补丁")]
    public static void BuildFromMenu()
    {
        AssetBundleMenu buildMenu = new AssetBundleMenu();
        // 输出路径
        buildMenu.outputABPath = "AssetBundleOutput";
        // 定义扫描路径
        buildMenu.sourcePath = "Assets/ResourceExt";
        // Unity的第一级输出
        buildMenu.abName = "Pack" + BundleResHub.BundlePostfix;
        // 构建的LUT文件名
        buildMenu.jsonMapName = "packCfg.json";
        // 最终发布大包
        buildMenu.finalPackName = "GYPack" + BundleResHub.BundlePostfix;
        // 大包输出路径
        buildMenu.outputPackPath = "Assets/StreamingAssets";
        buildMenu.StartBuild();
    }

    public void StartBuild()
    {
        BuildResourceExtToAssetBundle(out BundleLookUpTable lut);
        MergeBigPack(lut);
        Debug.Log("Build Finished!");
    }

    // 输出路径
    private string outputABPath;
    // 定义扫描路径
    private string sourcePath;
    // Unity的第一级输出
    private string abName;
    // 构建的LUT文件名
    private string jsonMapName;
    // 最终发布大包
    private string finalPackName;
    // 大包输出路径
    private string outputPackPath;
    /// <summary>
    /// 打AB包
    /// </summary>
    /// <param name="lut"></param>
    private void BuildResourceExtToAssetBundle(out BundleLookUpTable lut)
    {
        lut = null;
        if (!Directory.Exists(sourcePath))
        {
            Debug.LogError($"Folder '{sourcePath}'not exist! Plz check path");
            return;
        }

        if (!Directory.Exists(outputABPath))
        {
            Directory.CreateDirectory(outputABPath);
        }

        string bundleName = abName;

        AssetBundleBuild build = new AssetBundleBuild
        {
            assetBundleName = bundleName,
            assetNames = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories)
                                    .Where(file => !file.EndsWith(".meta")) // 排除 .meta 文件
                                    .Select(file => file.Replace("\\", "/")) // 替换路径分隔符
                                    .ToArray()
        };

        if (build.assetNames.Length == 0)
        {
            Debug.LogError($"Folder '{sourcePath}' no resource!");
            return;
        }

        AssetBundleBuild[] builds = new[] { build };
        BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.None;
        buildOptions |= BuildAssetBundleOptions.StrictMode;
        buildOptions |= BuildAssetBundleOptions.ChunkBasedCompression;
        // 打包资源build
        CompatibilityBuildPipeline.BuildAssetBundles(outputABPath, builds, buildOptions, BuildTarget.StandaloneWindows);
        CreateJsonConfig(builds, finalPackName, out lut);

        Debug.Log($"Already pack bundle '{outputABPath}/{bundleName}'");
    }

    /// <summary>
    /// 创建json配置管理资产和bundle的关系
    /// </summary>
    /// <param name="builds"></param>
    /// <returns></returns>
    private void CreateJsonConfig(AssetBundleBuild[] builds, string packName, out BundleLookUpTable lut)
    {
        lut = new BundleLookUpTable();
        string postFix = null;
        foreach (AssetBundleBuild build in builds)
        {
            string[] assetNames = build.assetNames;
            foreach (string assetName in assetNames)
            {
                string assetSimplePath = assetName.Substring(sourcePath.Length + 1);
                int postFixIndex = assetSimplePath.LastIndexOf(".");
                if (postFixIndex != -1)
                {
                    postFix = assetSimplePath.Substring(postFixIndex);
                    assetSimplePath = assetSimplePath.Remove(postFixIndex);
                    lut.asset2Bundle[assetSimplePath] = build.assetBundleName;
                    lut.asset2FullName[assetSimplePath] = assetName;
                }
            }
        }
        lut.bundleName2Pack = CreateBundleToBigPack(packName, builds);
        lut.packName = packName;
    }

    /// <summary>
    /// 保存对应关系配置表
    /// </summary>
    /// <param name="lut"></param>
    private void SaveLUT(BundleLookUpTable lut)
    {
        string jsonStr = JsonConvert.SerializeObject(lut);
        GYEncryptCenter center = new GYEncryptCenter();
        jsonStr = center.Encode(jsonStr);
        byte[] testBytes = UTF8Encoding.UTF8.GetBytes(jsonStr);
        File.WriteAllText(GetJsonCfgLocalPath(), jsonStr);
    }

    /// <summary>
    /// 合并大包步骤
    /// </summary>
    /// <param name="lut"></param>
    private void MergeBigPack(BundleLookUpTable lut)
    {
        string jsonPath = GetJsonCfgLocalPath();
        string packPath = GetPackLocalPath();
        string srcDir = GetSrcDirLocalPath();


        List<string> filePathList = new List<string>();
        filePathList.Add(jsonPath);
        filePathList.Add(packPath);
        // 先构造偏移量
        BuildLUTOffset(filePathList, srcDir, lut);
        // 保存文件
        SaveLUT(lut);
        // 之后再用尾文件记录头配置偏移量
        BundleTailData tailData = CreateTailData();
        // 最终合并
        MergeFilesToTargetPath(filePathList, tailData.BuildByteData(), GetOutputBigPackLocalPath(finalPackName), srcDir, lut);
    }


    private const int BufferSize = 8192; // 每次读取的字节数
    /// <summary>
    /// 合并文件
    /// </summary>
    /// <param name="filePathList"></param>
    /// <param name="tailBytes"></param>
    /// <param name="targetPath"></param>
    /// <param name="srcDir">源合并文件列表路径</param>
    /// <param name="lut">用于在合并期间记录偏移量</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public void MergeFilesToTargetPath(List<string> filePathList, byte[] tailBytes, string targetPath, string srcDir, BundleLookUpTable lut)
    {
        if (filePathList == null || filePathList.Count == 0)
        {
            throw new ArgumentException("file path list is empty or null!");
        }

        // 确保目标目录存在
        string targetDirectory = Path.GetDirectoryName(targetPath);
        FileManager.CheckAndCreateDoc(targetDirectory);
        if (!string.IsNullOrEmpty(targetDirectory) && !Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
        }

        using (FileStream outputStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write))
        {
            byte[] buffer = new byte[BufferSize];

            foreach (string filePath in filePathList)
            {
                string tmpFilePath = filePath.Replace("\\", "/");
                if (!File.Exists(tmpFilePath))
                {
                    throw new FileNotFoundException($"File not found ! [{tmpFilePath}]");
                }

                // 分块读取并写入目标文件
                using (FileStream fileStream = new FileStream(tmpFilePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
            // 尾数据
            if (tailBytes != null && tailBytes.Length > 0)
            {
                outputStream.Write(tailBytes, 0, tailBytes.Length);
            }

            // 返回合并后的字节数组（可选，视需求而定）
            outputStream.Flush();
        }
    }

    /// <summary>
    /// 构建偏移量
    /// </summary>
    /// <param name="filePathList"></param>
    /// <param name="srcDir"></param>
    /// <param name="lut"></param>
    private void BuildLUTOffset(List<string> filePathList, string srcDir, BundleLookUpTable lut)
    {
        ulong curOffset = 0;
        foreach (string filePath in filePathList)
        {
            string tmpFilePath = filePath.Replace("\\", "/");
            string shortName = GetFilePathWithoutSourceDir(tmpFilePath, srcDir);
            if (tmpFilePath.EndsWith(BundleResHub.BundlePostfix) && TryGetSizeLength(tmpFilePath, out long fileSize))
            {
                lut.bundleName2Offset[shortName] = curOffset;
                curOffset += (ulong)fileSize;
            }
            else
            {
                lut.bundleName2Offset[shortName] = 0;
            }
        }
    }

    /// <summary>
    /// 如果文件路径位于某根目录下则删除根目录字符串
    /// </summary>
    /// <param name="fileFullPath"></param>
    /// <param name="srcPath"></param>
    /// <returns></returns>
    private string GetFilePathWithoutSourceDir(string fileFullPath, string srcPath)
    {
        if (fileFullPath.StartsWith(srcPath))
        {
            return fileFullPath.Substring(srcPath.Length);
        }
        else
        {
            return fileFullPath;
        }
    }

    /// <summary>
    /// 构造尾数据
    /// </summary>
    /// <returns></returns>
    private BundleTailData CreateTailData()
    {
        BundleTailData tailData = new BundleTailData();
        string jsonPath = GetJsonCfgLocalPath();
        if (TryGetSizeLength(jsonPath, out long size))
        {
            tailData.jsonLength = Convert.ToUInt64(size);
        }

        return tailData;
    }

    /// <summary>
    /// 构造ab和大包对应关系
    /// </summary>
    /// <param name="bigPackName"></param>
    /// <param name="builds"></param>
    /// <returns></returns>
    private Dictionary<string, string> CreateBundleToBigPack(string bigPackName, AssetBundleBuild[] builds)
    {
        Dictionary<string, string> result = new Dictionary<string, string>();
        foreach (var build in builds)
        {
            result.Add(build.assetBundleName, bigPackName);
        }
        return result;
    }

    private bool TryGetSizeLength(string filePath, out long fileSize)
    {
        if (!File.Exists(filePath))
        {
            fileSize = 0;
            return false;
        }
        FileInfo fileInfo = new FileInfo(filePath);
        fileSize = fileInfo.Length; // 返回文件的字节大小
        return true;
    }

    private string GetJsonCfgLocalPath()
    {
        string fileName = jsonMapName;
        string folderPath = GetProjectFolder();
        return folderPath + "/" + outputABPath + "/" + fileName;
    }

    private string GetPackLocalPath()
    {
        string fileName = abName;
        string folderPath = GetProjectFolder();
        return folderPath + "/" + outputABPath + "/" + fileName;
    }

    private string GetSrcDirLocalPath()
    {
        string folderPath = GetProjectFolder();
        return (folderPath + "/" + outputABPath + "/").Replace("\\", "/");
    }

    private string GetOutputBigPackLocalPath(string packName)
    {
        string fileName = packName;
        string folderPath = GetProjectFolder();
        return outputPackPath + "/" + fileName;
    }

    private string GetProjectFolder()
    {
        string dataPath = Application.dataPath.Replace("\\", "/");
        return Application.dataPath.Remove(Application.dataPath.LastIndexOf("/Assets"));
    }
}