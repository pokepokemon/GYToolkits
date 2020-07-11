using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace GYHotfix.Load
{
    /// <summary>
    /// 加载使用到的配置相关类
    /// </summary>
    public class LoadConfig
    {
        private Dictionary<string, string> _asset2BundleKey;
        private Dictionary<string, int> _ab2LocalVersion;
        private Dictionary<string, int> _ab2NewVersion;
        private Dictionary<string, BundleSourceType> _ab2SourceType;
        private Dictionary<string, BundleInfo> _cfgMap;
        private Dictionary<string, BundleBlockInfo> _blockMap;

        public bool isLocalVersionDirty { private set; get; } = false;
        private AssetBundleManifest _manifest;

        private string _persistencePrefix = string.Empty;
        public LoadConfig()
        {
            _persistencePrefix = Application.persistentDataPath + "/";
        }

        /// <summary>
        /// 获取依赖关系
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string[] GetDependencyRefs(string key)
        {
            string[] result = _manifest.GetAllDependencies(key);
            return result;
        }

        public void InitByJson(string json)
        {
            _cfgMap = JsonMapper.ToObject<Dictionary<string, BundleInfo>>(json);
            string abName;
            string[] assetNames;
            char[] splitChar = new char[] { '|' };
            foreach (var key in _cfgMap.Keys)
            {
                BundleInfo bundleInfo = _cfgMap[key];
                abName = bundleInfo.ab;
                assetNames = bundleInfo.asns.Split(splitChar, System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var name in assetNames)
                {
                    _asset2BundleKey[name] = abName;
                }
            }
        }

        /// <summary>
        /// 本地文件版本号和加载源
        /// </summary>
        /// <param name="wholeFile"></param>
        public void InitLocalVersion(string wholeFile)
        {
            _ab2LocalVersion = new Dictionary<string, int>();
            _ab2SourceType = new Dictionary<string, BundleSourceType>();
            char[] splitLineChar = new[] { '\n' };
            char[] splitWordChar = new[] { '\t' };
            string[] lineArr = wholeFile.Replace("\r", "").Split(splitLineChar, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lineArr)
            {
                string[] wordArr = line.Split(splitWordChar, StringSplitOptions.RemoveEmptyEntries);
                if (wordArr.Length >= 3)
                {
                    string abKey = wordArr[0];
                    _ab2LocalVersion[abKey] = int.Parse(wordArr[1]);
                    BundleSourceType srcType;
                    int version;
                    if (Enum.TryParse<BundleSourceType>(wordArr[2], out srcType) && int.TryParse(wordArr[1], out version))
                    {
                        _ab2SourceType[abKey] = srcType;
                        _ab2LocalVersion[abKey] = version;
                    }
                }
            }
        }

        /// <summary>
        /// 本地文件在包内偏移量
        /// </summary>
        /// <param name="wholeFile"></param>
        public void InitBundleOffset(string wholeFile)
        {
            if (!string.IsNullOrEmpty(wholeFile))
            {
                _blockMap = JsonMapper.ToObject<Dictionary<string, BundleBlockInfo>>(wholeFile);
            }
            else
            {
                _blockMap = new Dictionary<string, BundleBlockInfo>();
            }
        }

        /// <summary>
        /// 设置某AB在本地的版本号和来源(用于下载完之后)
        /// </summary>
        /// <param name="abKey"></param>
        /// <param name="version"></param>
        /// <param name="srcType"></param>
        public void SetAssetbundleLocalVersion(string abKey, int version, BundleSourceType srcType = BundleSourceType.Persistent)
        {
            _ab2SourceType[abKey] = srcType;
            _ab2LocalVersion[abKey] = version;
            isLocalVersionDirty = true;
        }

        /// <summary>
        ///  将本地版本管理文件写入流
        /// </summary>
        /// <param name="sw"></param>
        public void FlushStream(StreamWriter sw)
        {
            foreach (string abName in _ab2LocalVersion.Keys)
            {
                int version = _ab2LocalVersion[abName];
                string srcType = _ab2SourceType[abName].ToString();
                sw.WriteLine(string.Format($"{abName}\t{version}\t{srcType}"));
            }
        }

        /// <summary>
        /// 获取Assetbundle的Key
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public string GetAssetbundleKey(string assetName)
        {
            string abKey;
            if (_asset2BundleKey.TryGetValue(assetName, out abKey))
            {
                return abKey;
            }
            return string.Empty;
        }

        /// <summary>
        /// 获取本地的AB版本号
        /// </summary>
        /// <param name="abKey"></param>
        /// <returns></returns>
        public int GetAssetbundleLocalVersion(string abKey)
        {
            int version;
            if (_ab2LocalVersion.TryGetValue(abKey, out version))
            {
                return version;
            }
            return -1;
        }
        
        /// <summary>
        /// 获取最新的AB版本号
        /// </summary>
        /// <param name="abKey"></param>
        /// <returns></returns>
        public int GetAssetbundleNewVersion(string abKey)
        {
            int version;
            if (_ab2NewVersion.TryGetValue(abKey, out version))
            {
                return version;
            }
            return -1;
        }

        /// <summary>
        /// 是否需要下载该AB
        /// </summary>
        /// <param name="abKey"></param>
        /// <returns></returns>
        public bool NeedDownload(string abKey)
        {
            int localVer = GetAssetbundleLocalVersion(abKey);
            if (localVer != -1)
            {
                int newVer = GetAssetbundleNewVersion(abKey);
                if (newVer != -1)
                {
                    return newVer > localVer;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 获取指定assetbundle本地加载路径
        /// </summary>
        /// <param name="abKey"></param>
        /// <returns></returns>
        public string GetAssetbundlePath(string abKey)
        {
            return _persistencePrefix + abKey;
        }

        public string GetAssetbundleUrl(string abUrl)
        {
            return string.Empty;
        }
    }
}
