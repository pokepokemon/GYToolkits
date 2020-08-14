﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class AssetsLoadGroup
{
    public Dictionary<string, UnityEngine.Object> assetDict = new Dictionary<string, Object>();

    private List<string> _pathList;
    private List<System.Type> _typeList;
    private HashSet<string> _loadingSet = new HashSet<string>();
    private Dictionary<string, System.Type> _typeMap = new Dictionary<string, System.Type>();

    public UnityAction<AssetsLoadGroup> OnCompleted;

    public bool isDisposed { private set; get; } = false;
    public bool isCompleted { private set; get; } = false;

    public AssetsLoadGroup(List<string> pathList, List<System.Type> typeList)
    {
        _pathList = pathList;
        _typeList = typeList;
        _loadingSet.Clear();
        _typeMap.Clear();
    }

    public void StartLoad()
    {
        for (int i = 0; i < _pathList.Count; i++)
        {
            string path = _pathList[i];
            System.Type type = _typeList[i];
            if (!_loadingSet.Contains(path))
            {
                GameLoader.Instance.LoadObject(path, type, HandleLoadOne);
                _loadingSet.Add(_pathList[i]);
                _typeMap[path] = type;
            }
        }
    }
    
    private void HandleLoadOne(UnityEngine.Object asset, string assetName)
    {
        if (!isDisposed)
        {
            assetDict[assetName] = asset;
            _loadingSet.Remove(assetName);
            CheckLoadComplete();
        }
    }

    private void CheckLoadComplete()
    {
        if (_loadingSet.Count == 0)
        {
            isCompleted = true;
            if (OnCompleted != null)
            {
                OnCompleted(this);
            }
        }
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            foreach (var asset in assetDict.Values)
            {
                GameLoader.Instance.Unload(asset);
            }
            assetDict.Clear();
            assetDict = null;

            foreach (var path in _loadingSet)
            {
                GameLoader.Instance.Cancel(path, _typeMap[path], HandleLoadOne);
            }
            _typeMap.Clear();
            _loadingSet.Clear();
        }
    }
}
