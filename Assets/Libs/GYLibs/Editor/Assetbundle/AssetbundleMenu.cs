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
/// ������DLC���
/// </summary>
public class AssetBundleMenu
{
    [MenuItem("Tools/�������")]
    public static void BuildFromMenu()
    {
        AssetBundleMenu buildMenu = new AssetBundleMenu();
        // ���·��
        buildMenu.outputABPath = "AssetBundleOutput";
        // ����ɨ��·��
        buildMenu.sourcePath = "Assets/ResourceExt";
        // Unity�ĵ�һ�����
        buildMenu.abName = "Pack" + BundleResHub.BundlePostfix;
        // ������LUT�ļ���
        buildMenu.jsonMapName = "packCfg.json";
        // ���շ������
        buildMenu.finalPackName = "GYPack" + BundleResHub.BundlePostfix;
        // ������·��
        buildMenu.outputPackPath = "Assets/StreamingAssets";
        buildMenu.StartBuild();
    }

    public void StartBuild()
    {
        BuildResourceExtToAssetBundle(out BundleLookUpTable lut);
        MergeBigPack(lut);
        Debug.Log("Build Finished!");
    }

    // ���·��
    private string outputABPath;
    // ����ɨ��·��
    private string sourcePath;
    // Unity�ĵ�һ�����
    private string abName;
    // ������LUT�ļ���
    private string jsonMapName;
    // ���շ������
    private string finalPackName;
    // ������·��
    private string outputPackPath;
    /// <summary>
    /// ��AB��
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
                                    .Where(file => !file.EndsWith(".meta")) // �ų� .meta �ļ�
                                    .Select(file => file.Replace("\\", "/")) // �滻·���ָ���
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
        // �����Դbuild
        CompatibilityBuildPipeline.BuildAssetBundles(outputABPath, builds, buildOptions, BuildTarget.StandaloneWindows);
        CreateJsonConfig(builds, finalPackName, out lut);

        Debug.Log($"Already pack bundle '{outputABPath}/{bundleName}'");
    }

    /// <summary>
    /// ����json���ù����ʲ���bundle�Ĺ�ϵ
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
    /// �����Ӧ��ϵ���ñ�
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
    /// �ϲ��������
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
        // �ȹ���ƫ����
        BuildLUTOffset(filePathList, srcDir, lut);
        // �����ļ�
        SaveLUT(lut);
        // ֮������β�ļ���¼ͷ����ƫ����
        BundleTailData tailData = CreateTailData();
        // ���պϲ�
        MergeFilesToTargetPath(filePathList, tailData.BuildByteData(), GetOutputBigPackLocalPath(finalPackName), srcDir, lut);
    }


    private const int BufferSize = 8192; // ÿ�ζ�ȡ���ֽ���
    /// <summary>
    /// �ϲ��ļ�
    /// </summary>
    /// <param name="filePathList"></param>
    /// <param name="tailBytes"></param>
    /// <param name="targetPath"></param>
    /// <param name="srcDir">Դ�ϲ��ļ��б�·��</param>
    /// <param name="lut">�����ںϲ��ڼ��¼ƫ����</param>
    /// <exception cref="ArgumentException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    public void MergeFilesToTargetPath(List<string> filePathList, byte[] tailBytes, string targetPath, string srcDir, BundleLookUpTable lut)
    {
        if (filePathList == null || filePathList.Count == 0)
        {
            throw new ArgumentException("file path list is empty or null!");
        }

        // ȷ��Ŀ��Ŀ¼����
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

                // �ֿ��ȡ��д��Ŀ���ļ�
                using (FileStream fileStream = new FileStream(tmpFilePath, FileMode.Open, FileAccess.Read))
                {
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                    }
                }
            }
            // β����
            if (tailBytes != null && tailBytes.Length > 0)
            {
                outputStream.Write(tailBytes, 0, tailBytes.Length);
            }

            // ���غϲ�����ֽ����飨��ѡ�������������
            outputStream.Flush();
        }
    }

    /// <summary>
    /// ����ƫ����
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
    /// ����ļ�·��λ��ĳ��Ŀ¼����ɾ����Ŀ¼�ַ���
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
    /// ����β����
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
    /// ����ab�ʹ����Ӧ��ϵ
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
        fileSize = fileInfo.Length; // �����ļ����ֽڴ�С
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