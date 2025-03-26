using GYLib;
using GYLib.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class BundleResHub : MonoSingleton<BundleResHub>
{
    public const string BundlePostfix = ".assetbundle";

    private Dictionary<string, AssetBundle> _bundleDict;
    private Dictionary<string, BundleTailData> _path2Tail;
    private BundleLookUpTable _bundleLUT;
    private string streamingPathFormat = Application.streamingAssetsPath;

    public void Init()
    {
        _bundleDict = new Dictionary<string, AssetBundle>();
        _path2Tail = new Dictionary<string, BundleTailData>();
        _bundleLUT = new BundleLookUpTable();
        LoadAll();
    }

    private void LoadAll()
    {
        CheckStreamingFolder();
    }

    /// <summary>
    /// 尝试获取资源所在Bundle
    /// </summary>
    /// <param name="resName"></param>
    /// <param name="bundle"></param>
    /// <returns></returns>
    public bool TryGetBundle(string resName, out AssetBundle bundle, out string resFullName)
    {
        if (_bundleLUT != null 
            && _bundleLUT.asset2Bundle.TryGetValue(resName, out string bundleName) 
            && _bundleLUT.asset2FullName.TryGetValue(resName, out resFullName))
        {
            if (_bundleDict.TryGetValue(bundleName, out bundle))
            {
                return true;
            }
        }
        resFullName = null;
        bundle = null;
        return false;
    }

    private void CheckStreamingFolder()
    {
        string folderPath = Application.streamingAssetsPath;
        ScanBundleFile(folderPath);
        ParseBundleFiles(folderPath);
    }

    private string GetStreamingBundle(string fileName)
    {
        return string.Format(streamingPathFormat, fileName);
    }


    /// <summary>
    /// 获取指定路径下最近修改或最近创建的 .dat 文件名
    /// </summary>
    /// <param name="directoryPath">文件夹路径</param>
    /// <param name="useCreationTime">是否使用创建时间（true为创建时间，false为修改时间）</param>
    /// <returns>最近修改或创建的文件名</returns>
    public void ScanBundleFile(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }

        // 获取 .assetbundle 文件
        string[] files = Directory.GetFiles(directoryPath, "*" + BundleResHub.BundlePostfix, SearchOption.TopDirectoryOnly);
        if (files.Length > 0)
        {
            foreach (string file in files)
            {
                if (BundleTailData.CheckFileData(file, out var bytes))
                {
                    BundleTailData tailData = BundleTailData.CreateFromBytes(bytes, BundleTailData.MarkerLength);
                    string tmpFileName = file.Replace("\\", "/");
                    _path2Tail.Add(tmpFileName, tailData);
                }
            }
        }
    }

    private void ParseBundleFiles(string folderPath)
    {
        foreach (var filePath in _path2Tail.Keys)
        {
            BundleLookUpTable table = null;
            var tailData = _path2Tail[filePath];
            using (FileStream stream = new FileStream(filePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    byte[] jsonBytes = reader.ReadBytes(Convert.ToInt32(tailData.jsonLength));
                    string jsonContent = Encoding.UTF8.GetString(jsonBytes);
                    GYEncryptCenter center = new GYEncryptCenter();
                    jsonContent = center.Decode(jsonContent);
                    table = JsonConvert.DeserializeObject<BundleLookUpTable>(jsonContent);
                }
            }
            LoadAssetbundleByLUT(table, filePath, tailData);
        }
    }

    /// <summary>
    /// 通过LUT和指定大包路径加载AB
    /// </summary>
    /// <param name="table"></param>
    /// <param name="filePath"></param>
    private void LoadAssetbundleByLUT(BundleLookUpTable table, string filePath, BundleTailData tailData)
    {
        if (table != null)
        {
            // 合并到LUT
            _bundleLUT.MergeTable(table);
            foreach (var bundleName in table.bundleName2Offset.Keys)
            {
                if (bundleName.EndsWith(BundleResHub.BundlePostfix))
                {
                    ulong offset = table.bundleName2Offset[bundleName];
                    AssetBundle assetbundle = AssetBundle.LoadFromFile(filePath, 0, offset + tailData.jsonLength);

                    if (assetbundle != null)
                    {
                        _bundleDict.Add(bundleName, assetbundle);
                    }
                }
            }
        }
    }

    public void Dispose()
    {
        foreach (var bundle in  _bundleDict.Values)
        {
            bundle.Unload(false);
        }
    }
}
