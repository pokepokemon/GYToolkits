using UnityEngine;
using System.Collections;
using GYLib;
using System;
using System.IO;
using UnityEngine.Events;
using GYLib.Utils;

public class GameLoader : MonoSingleton<GameLoader>
{
    public delegate void OnJsonConfigLoaded(string str);
    public delegate void OnCSVConfigLoaded(string[] arr);
    public delegate void OnSceneAsyncLoaded();

    private GYEncryptCenter _center;

    // Use this for initialization
    void Start()
    {

    }

    /// <summary>
    /// 异步加载UnityEngine.Object
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback">Object,string</param>
    public void LoadObject(string path, UnityAction<UnityEngine.Object, string> callback)
    {
        BundleLoader.Instance.AddTask(path, callback);
    }

    /// <summary>
    /// 异步加载UnityEngine.Object
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback">Object,string</param>
    public void LoadObject(string path, Type type, UnityAction<UnityEngine.Object, string> callback)
    {
        BundleLoader.Instance.AddTaskSetType(path, callback, type);
    }

    /// <summary>
    /// 异步加载UnityEngine.Object
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback">Object,string</param>
    public void LoadSprite(string path, UnityAction<UnityEngine.Object, string> callback)
    {
        BundleLoader.Instance.AddTaskSetType(path, callback, typeof(Sprite));
    }

    /// <summary>
    /// 同步加载UnityEngine.Object
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public UnityEngine.Object LoadObjectSync(string path)
    {
        return BundleLoader.Instance.LoadObjectSync(path);
    }

    public UnityEngine.Sprite LoadSpriteSync(string path)
    {
        return BundleLoader.Instance.LoadSpriteSync(path);
    }

    /// <summary>
    /// 同步加载一串UnityEngine.Object[]
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public T[] LoadAllSpriteSync<T>(string path) where T : UnityEngine.Object
    {
        return Resources.LoadAll<T>(path);
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    public void LoadConfig(string path, OnJsonConfigLoaded callback)
    {
        if (FrameSettings.enable)
        {
            path = FrameSettings.ReplaceEncodePath(path);
        }
        TextAsset obj = Resources.Load<TextAsset>(path) as TextAsset;
        if (!obj)
        {
            Debug.Log("error : " + path + " not found");
        }
        else
        {
            Debug.Log(path + " load completed");

            if (FrameSettings.enable)
            {
                _center = _center ?? new GYEncryptCenter();
                string result = _center.Decode(obj.text);
                callback(result);
            }
            else
            {
                callback(obj.text);
            }
        }
    }

    /// <summary>
    /// 加载csv(FileStream方式)
    /// </summary>
    /// <param name="path"></param>
    /// <param name="callback"></param>
    public void LoadCSV(string path, OnCSVConfigLoaded callback)
    {
        FileStream fs = FileManager.Instance.GetFile(path);
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, bytes.Length);
        fs.Close();

        string content = System.Text.Encoding.UTF8.GetString(bytes);
        string[] arr = content.Split(new string[1] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
        callback(arr);
    }

    private AsyncOperation _tempSceneAsync = null;
    private OnSceneAsyncLoaded _tempSceneLoadedCallback = null;

    public void LoadScene(string sceneName, OnSceneAsyncLoaded callback = null)
    {
        _tempSceneLoadedCallback = callback;
        _tempSceneAsync = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Assets/Resources/" + sceneName + ".unity", UnityEngine.SceneManagement.LoadSceneMode.Single);
        _tempSceneAsync.allowSceneActivation = true;
    }

    private void Update()
    {
        if (_tempSceneAsync != null && _tempSceneAsync.isDone)
        {
            if (_tempSceneLoadedCallback != null)
            {
                OnSceneAsyncLoaded tmpCall = _tempSceneLoadedCallback;
                _tempSceneLoadedCallback = null;
                tmpCall.Invoke();
            }
        }
    }

    /// <summary>
    /// 是否有以参数开始的路径 正在加载中
    /// </summary>
    /// <param name="subStr"></param>
    /// <returns></returns>
    public bool HasStartWithLoadingPath(string subStr, string ignoreStr = "")
    {
        return BundleLoader.Instance.HasStartWithLoadingPath(subStr, ignoreStr);
    }

    public void Unload(UnityEngine.Object asset)
    {

    }

    public void UnloadByName(string assetName)
    {

    }

    public void Cancel(string path, Type type, UnityAction<UnityEngine.Object, string> callback)
    {
        if (BundleLoader.Instance != null)
        {
            BundleLoader.Instance.CancelCallback(path, type, callback);
        }
    }
}
