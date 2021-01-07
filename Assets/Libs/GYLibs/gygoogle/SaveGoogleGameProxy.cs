using UnityEngine;
using System.Collections;
using System;
using Newtonsoft.Json;
using System.Threading;
using GYLib.Utils;
using System.IO;

#if ENABLE_GPS
public class SaveGoogleGameProxy
{
    private GYEncryptCenter encrypt = new GYEncryptCenter();
    private Thread _thread = null;
    private bool _isRunning = false;
    private bool _needSave = false;

    public bool unsupport = false;

    public bool isDataDirty { private set; get; } = false;
    public byte[] bytesSave = null;

    public void StartSave()
    {
        _needSave = true;

        if (!unsupport)
        {
            if (!_isRunning)
            {
                _thread = new Thread(new ThreadStart(LoopSave));
                _isRunning = true;
                _thread.IsBackground = true;
                _thread.Start();
            }
        }
        else
        {
            _needSave = false;
            Save();
        }
    }

    private void LoopSave()
    {
        while (_isRunning)
        {
            try
            {
                if (_needSave)
                {
                    Save();
                    _needSave = false;
                }
            }
            catch (Exception e)
            {
                unsupport = true;
                Stop();
                return;
            }
            Thread.Sleep(1000);
        }
    }

    public void Stop()
    {
        _isRunning = false;
        if (_thread != null)
        {
            if (!_thread.Join(500))//等待0.5秒
            {
#if UNITY_IPHONE
				_thread.Abort();
#else
                _thread.Abort();
#endif
            }
            _thread = null;
        }
    }

    /// <summary>
    /// 立刻保存
    /// </summary>
    public void Save()
    {
        string json = JsonConvert.SerializeObject(GlobalData.Instance.saveDict);
        bytesSave = encrypt.EncodeBytes(json);
        string resultStr = Convert.ToBase64String(bytesSave, 0, bytesSave.Length);

        string bPath = SaveProcessor.SAVE_PATH + ".b";
        FileManager.Instance.SaveString(bPath, resultStr);
        string bFullPath = FileManager.Instance.GetPath(bPath);
        string fullPath = FileManager.Instance.GetPath(SaveProcessor.SAVE_PATH);
        if (File.Exists(bFullPath))
        {
            File.Copy(bFullPath, fullPath, true);
            File.Delete(bFullPath);
        }
        isDataDirty = true;
    }
}
#endif