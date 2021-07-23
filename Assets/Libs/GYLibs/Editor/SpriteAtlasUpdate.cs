using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteAtlasUpdate : Editor
{
#if UNITY_2017_3_OR_NEWER
    const string SPRITE_FOLDER = "UIRawRes/";
    const string SPRITE_SPUM_FOLDER = "SPUM/";
    const string SPRITE_SPUM_RES_FOLDER = "Resources/SPUM/";
    const string SPRITE_RESOURCES_FOLDER = "Resources/UI/RawRes/";

    public static HashSet<string> ignoreSet = new HashSet<string>()
    {
        "Font",
        "Texture",
        "atlas_title",
    };

    [MenuItem("Tools/Update UIAtlas")]
    public static void CreateAtlasBySprite()
    {
        CreateInAtlasFolder(SPRITE_FOLDER);
        CreateInAtlasFolder(SPRITE_RESOURCES_FOLDER);
        /*
        CreateInOneFolder(SPRITE_SPUM_FOLDER, new HashSet<string>()
        {
            "SPUM/Res/",
        });*/
        CreateInOneFolder(SPRITE_SPUM_RES_FOLDER);
    }


    #region one folder
    private static void CreateInOneFolder(string parentFolder, HashSet<string> ignoreSubSet = null)
    {
        string spriteSrcDir = Application.dataPath + "/" + parentFolder;
        DirectoryInfo dirInfo = new DirectoryInfo(spriteSrcDir);
        spriteSrcDir = spriteSrcDir.Replace("\\", "/");
        //add sprite

        List<Sprite> spts = new List<Sprite>();
        string curFolderPath = dirInfo.FullName.Replace("\\", "/");
        string atlasName = dirInfo.Name + ".spriteatlas";

        spts.Clear();
        foreach (FileInfo pngFile in dirInfo.GetFiles("*.png", SearchOption.AllDirectories))
        {
            string allPath = pngFile.FullName;
            string folderChildPath = Path.GetDirectoryName(allPath).Replace("\\", "/");
            folderChildPath = folderChildPath.Substring(folderChildPath.IndexOf(parentFolder));
            if (ignoreSubSet != null)
            {
                bool isIgnore = false;
                foreach (var setItem in ignoreSubSet)
                {
                    if (folderChildPath.IndexOf(setItem) != -1)
                    {
                        isIgnore = true;
                        break;
                    }
                }
                if (isIgnore)
                {
                    continue;
                }
            }
            string assetPath = allPath.Substring(allPath.IndexOf("Assets")).Replace("\\", "/");
            assetPath.Remove(assetPath.LastIndexOf("."));
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (Object tmpObj in objs)
            {
                Sprite sprite = tmpObj as Sprite;
                if (IsPackable(sprite))
                {
                    spts.Add(sprite);
                }
            }
        }
        Debug.Log("atlasName " + atlasName);
        SpriteAtlas sa = CheckCreateRootAtlas(parentFolder, dirInfo.Name);
        SpriteAtlasPackingSettings settings = sa.GetPackingSettings();
        settings.enableTightPacking = false;
        settings.enableRotation = false;

        HashSet<int> spriteSet = new HashSet<int>();
        Sprite[] spriteArr = new Sprite[sa.spriteCount];
        sa.GetSprites(spriteArr);
        foreach (var sp in spriteArr)
        {
            spriteSet.Add(sp.GetInstanceID());
        }
        for (int i = 0; i < spts.Count; i++)
        {
            if (!spriteSet.Contains(spts[i].GetInstanceID()) && !sa.CanBindTo(spts[i]))
            {
                sa.Add(new Object[] { spts[i] });
            }
        }

        TextureImporterPlatformSettings texSettings = sa.GetPlatformSettings("iPhone");
        texSettings.overridden = true;
        texSettings.format = TextureImporterFormat.ASTC_4x4;
        sa.SetPlatformSettings(texSettings);
        texSettings = sa.GetPlatformSettings("Android");
        texSettings.format = TextureImporterFormat.ETC2_RGBA8;
        sa.SetPlatformSettings(texSettings);
        sa.SetPackingSettings(settings);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private static SpriteAtlas CheckCreateRootAtlas(string parentFolder, string atlasName)
    {
        string path = Application.dataPath + "/" + parentFolder + atlasName + ".spriteatlas";
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        AssetDatabase.Refresh();

        SpriteAtlas sa = new SpriteAtlas();
        string assetPath = "Assets" + "/" + parentFolder + atlasName + ".spriteatlas";
        AssetDatabase.CreateAsset(sa, assetPath);
        return sa;
    }

    #endregion

    #region atlas folder

    private static void CreateInAtlasFolder(string parentFolder)
    {
        string spriteSrcDir = Application.dataPath + "/" + parentFolder;
        DirectoryInfo rootDirInfo = new DirectoryInfo(spriteSrcDir);
        spriteSrcDir = spriteSrcDir.Replace("\\", "/");
        //add sprite

        List<Sprite> spts = new List<Sprite>();
        foreach (DirectoryInfo dirInfo in rootDirInfo.GetDirectories())
        {
            string curFolderPath = dirInfo.FullName.Replace("\\", "/");
            if (ignoreSet.Contains(dirInfo.Name))
            {
                continue;
            }
            string atlasName = dirInfo.Name + ".spriteatlas";

            spts.Clear();
            foreach (FileInfo pngFile in dirInfo.GetFiles("*.png", SearchOption.AllDirectories))
            {
                string allPath = pngFile.FullName;
                string assetPath = allPath.Substring(allPath.IndexOf("Assets")).Replace("\\", "/");
                assetPath.Remove(assetPath.LastIndexOf("."));
                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (Object tmpObj in objs)
                {
                    Sprite sprite = tmpObj as Sprite;
                    if (IsPackable(sprite))
                    {
                        spts.Add(sprite);
                    }
                }
            }
            Debug.Log("atlasName " + atlasName);
            SpriteAtlas sa = CheckCreateAtlas(parentFolder, dirInfo.Name);
            SpriteAtlasPackingSettings settings = sa.GetPackingSettings();

            settings.enableTightPacking = false;
            settings.enableRotation = false;
            sa.SetPackingSettings(settings);

            HashSet<int> spriteSet = new HashSet<int>();
            Sprite[] spriteArr = new Sprite[sa.spriteCount];
            sa.GetSprites(spriteArr);
            foreach (var sp in spriteArr)
            {
                spriteSet.Add(sp.GetInstanceID());
            }
            for (int i = 0; i < spts.Count; i++)
            {
                if (!spriteSet.Contains(spts[i].GetInstanceID()) && !sa.CanBindTo(spts[i]))
                {
                    sa.Add(new Object[] { spts[i] });
                }
            }
            
            TextureImporterPlatformSettings texSettings = sa.GetPlatformSettings("iPhone");
            texSettings.overridden = true;
            texSettings.format = TextureImporterFormat.ASTC_4x4;
            sa.SetPlatformSettings(texSettings);
            texSettings = sa.GetPlatformSettings("Android");
            texSettings.format = TextureImporterFormat.ETC2_RGBA8;
            sa.SetPlatformSettings(texSettings);

            EditorUtility.SetDirty(sa);
            AssetDatabase.SaveAssets();
        }

        AssetDatabase.Refresh();
    }

    private static SpriteAtlas CheckCreateAtlas(string parentFolder, string atlasName)
    {
        string path = Application.dataPath + "/" + parentFolder + atlasName + "/" + atlasName + ".spriteatlas";
        if (File.Exists(path))
        {
            File.Delete(path);
        }

        return CreateAtlas(parentFolder, atlasName);
    }
    #endregion

    private static SpriteAtlas CreateAtlas(string parentFolder, string atlasName)
    {
        SpriteAtlas sa = new SpriteAtlas();
        string assetPath = "Assets" + "/" + parentFolder + atlasName + "/" + atlasName + ".spriteatlas";
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
