﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteAtlasUpdate : Editor
{
#if UNITY_2017_3_OR_NEWER
    const string SPRITE_FOLDER = "Resources/UI/RawRes/";

    [MenuItem("Tools/Update UIAtlas")]
    public static void CreateAtlasBySprite()
    {
        string spriteSrcDir = Application.dataPath + "/" + SPRITE_FOLDER;
        DirectoryInfo rootDirInfo = new DirectoryInfo(spriteSrcDir);
        spriteSrcDir = spriteSrcDir.Replace("\\", "/");
        //add sprite

        List<Sprite> spts = new List<Sprite>();
        foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
        {
            string curFolderPath = dirInfo.FullName.Replace("\\", "/");

            spts.Clear();
            foreach (FileInfo pngFile in dirInfo.GetFiles("*.png", SearchOption.AllDirectories))
            {
                string allPath = pngFile.FullName;
                string assetPath = allPath.Substring(allPath.IndexOf("Assets")).Replace("\\", "/");
                assetPath.Remove(assetPath.LastIndexOf("."));
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                if (IsPackable(sprite))
                {
                    spts.Add(sprite);
                }
            }
            string atlasName = dirInfo.Name + ".spriteatlas";
            Debug.Log("atlasName " + atlasName);
            SpriteAtlas sa = CheckCreateAtlas(dirInfo.Name);
            SpriteAtlasPackingSettings settings = sa.GetPackingSettings();
            settings.enableTightPacking = false;
            sa.SetPackingSettings(settings);
            for (int i = 0; i < spts.Count; i++)
            {
                if (!sa.CanBindTo(spts[i]))
                {
                    sa.Add(new Object[] { spts[i] });
                }
            }
            AssetDatabase.SaveAssets();
        }

        AssetDatabase.Refresh();
    }

    private static SpriteAtlas CheckCreateAtlas(string atlasName)
    {
        string path = Application.dataPath + "/" + SPRITE_FOLDER + atlasName + "/" + atlasName + ".spriteatlas";
        if (File.Exists(path))
        {
            string assetPath = "Assets" + "/" + SPRITE_FOLDER + atlasName + "/" + atlasName + ".spriteatlas";
            SpriteAtlas sa = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
            return sa;
        }
        else
        {
            return CreateAtlas(atlasName);
        }
    }

    private static SpriteAtlas CreateAtlas(string atlasName)
    {
        SpriteAtlas sa = new SpriteAtlas();
        string assetPath = "Assets" + "/" + SPRITE_FOLDER + atlasName + "/" + atlasName + ".spriteatlas";
        Debug.Log("create path : " + assetPath);
        AssetDatabase.CreateAsset(sa, assetPath);
        return sa;
    }

    static bool IsPackable(Object o)
    {
        return o != null && (o.GetType() == typeof(Sprite) || 
            o.GetType() == typeof(Texture2D) || 
            (o.GetType() == typeof(DefaultAsset) && ProjectWindowUtil.IsFolder(o.GetInstanceID()))
            );
    }
#endif
}
