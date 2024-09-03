using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class TilesAtlasUpdate : Editor
{
#if UNITY_2017_3_OR_NEWER
    const string SPRITE_FOLDER = "Resources/map/";

    [MenuItem("Tools/Update TileAtlas")]
    public static void CreateAtlasBySprite()
    {
        string spriteSrcDir = Application.dataPath + "/" + SPRITE_FOLDER;
        DirectoryInfo rootDirInfo = new DirectoryInfo(spriteSrcDir);
        spriteSrcDir = spriteSrcDir.Replace("\\", "/");
        //add sprite

        List<Sprite> spts = new List<Sprite>();
        foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
        {
            string tileStyleName = dirInfo.Name;
            string curFolderPath = dirInfo.FullName.Replace("\\", "/");

            DirectoryInfo[] subDirInfos = dirInfo.GetDirectories("raw_tiles");
            DirectoryInfo subDirInfo = null;
            if (subDirInfos != null && subDirInfos.Length > 0)
            {
                subDirInfo = subDirInfos[0];
                
                spts.Clear();
                foreach (FileInfo pngFile in subDirInfo.GetFiles("*.png", SearchOption.AllDirectories))
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
                string atlasName = tileStyleName + ".spriteatlas";
                Debug.Log("atlasName " + atlasName);
                SpriteAtlas sa = CheckCreateAtlas(tileStyleName);
                SpriteAtlasPackingSettings settings = sa.GetPackingSettings();
                settings.enableTightPacking = false;
                settings.enableRotation = false;

                TextureImporterPlatformSettings texSettings = sa.GetPlatformSettings(BuildTarget.iOS.ToString());
                texSettings.overridden = true;
                texSettings.format = TextureImporterFormat.ASTC_4x4;
                sa.SetPlatformSettings(texSettings);
                texSettings = sa.GetPlatformSettings(BuildTarget.Android.ToString());
                texSettings.format = TextureImporterFormat.ETC2_RGBA8;
                sa.SetPlatformSettings(texSettings);

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
