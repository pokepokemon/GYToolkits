using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameSettings
{
    public const bool enable = false;

    /// <summary>
    /// 替换路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ReplaceEncodePath(string path)
    {
        return path.Replace("config/", "config_encrypt/");
    }
    /// <summary>
    /// 替换路径
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string ReplaceDecodePath(string path)
    {
        return path.Replace("config_encrypt/", "config/");
    }
}
