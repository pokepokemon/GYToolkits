using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;

public class SpriteCutEditor : EditorWindow {

    [MenuItem("Assets/SpriteCut/替换透明像素(仅边框)", false, 3)]
    /// <summary>
    /// 循环设置选择的贴图，仅从边框染色
    /// </summary>
    static private void SpriteCutEntry()
    {
        Object[] textures = GetSelectedTextures();
        Selection.objects = new Object[0];

        ApplyImporter(textures);
        ReplaceEach(textures);
        AssetDatabase.Refresh();
    }

    [MenuItem("Assets/SpriteCut/替换透明像素(全局)", false, 3)]
    /// <summary>
    /// 循环设置选择的贴图，全局替换
    /// </summary>
    static private void SpriteReplaceEntry()
    {
        Object[] textures = GetSelectedTextures();
        Selection.objects = new Object[0];

        ApplyImporter(textures);
        ReplaceEach(textures, false);
        AssetDatabase.Refresh();
    }


    static private void ReplaceEach(Object[] textures, bool onlyBound = true)
    {
        int startIndex = 0;
        int totalLength = textures.Length;
        EditorApplication.update = delegate ()
        {
            Texture2D texture = textures[startIndex] as Texture2D;
            if (texture != null)
            {
                string path = AssetDatabase.GetAssetPath(texture);

                if (onlyBound)
                    StartCut(texture, path);
                else
                    StartReplace(texture, path);
            }
            bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", texture.name, (float)startIndex / (float)totalLength);

            startIndex++;
            if (isCancel || startIndex >= totalLength)
            {
                EditorUtility.ClearProgressBar();
                EditorApplication.update = null;
                startIndex = 0;
                Debug.Log("匹配结束");
            }
        };
    }

    /// <summary>
    /// 应用Importer
    /// </summary>
    /// <param name="textures"></param>
    static private void ApplyImporter(Object[] textures)
    {
        AssetDatabase.StartAssetEditing();
        foreach (Texture2D texture in textures)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            string guid = AssetDatabase.AssetPathToGUID(path);
            FileInfo info = new FileInfo(path);
            TextureImporter texImporter = TextureImporter.GetAtPath(path) as TextureImporter;
            texImporter.textureType = TextureImporterType.Default;
            texImporter.isReadable = true;
            texImporter.npotScale = TextureImporterNPOTScale.None;
            texImporter.SetPlatformTextureSettings("Standalone", 2048, TextureImporterFormat.ARGB32);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
    }

    static private void StartCut(Texture2D tex, string path)
    {
        try
        {
            Texture2D tmpTex = new Texture2D(tex.width, tex.height);
            tmpTex.LoadImage(tex.EncodeToPNG());

            SpriteCutBound tool = new SpriteCutBound();
            tool.Start(tmpTex);
            byte[] bytes = tmpTex.EncodeToPNG();
            string prefix = Application.dataPath;
            prefix = prefix.Substring(0, prefix.LastIndexOf("/Assets") + 1);
            FileStream fs = File.Create(prefix + path.Replace(".png", "_replace.png"));
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
        }
        catch (System.Exception e)
        {
            Debug.Log("fail to replace : " + path + ", e = " + e.ToString());
            Texture2D tmpTex = new Texture2D(tex.width, tex.height);
            tmpTex.LoadImage(tex.EncodeToPNG());
            
            SpriteReplaceColor tool = new SpriteReplaceColor();
            tool.Start(tmpTex);
            byte[] bytes = tmpTex.EncodeToPNG();

            string prefix = Application.dataPath;
            prefix = prefix.Substring(0, prefix.LastIndexOf("/Assets"));
            FileStream fs = File.Create(prefix + path.Replace(".png", "_replace_1.png"));
            fs.Write(bytes, 0, bytes.Length);
            fs.Flush();
            fs.Close();
        }
    }

    static private void StartReplace(Texture2D tex, string path)
    {
        Texture2D tmpTex = new Texture2D(tex.width, tex.height);
        tmpTex.LoadImage(tex.EncodeToPNG());

        SpriteReplaceColor tool = new SpriteReplaceColor();
        tool.Start(tmpTex);
        byte[] bytes = tmpTex.EncodeToPNG();
        FileStream fs = File.Create(path.Replace(".png", "_replace_1.png"));
        fs.Write(bytes, 0, bytes.Length);
        fs.Flush();
        fs.Close();
    }

    /// <summary>
    /// 获取选择的贴图
    /// </summary>
    /// <returns></returns>
    static private Object[] GetSelectedTextures()
    {
        return Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
    }
}
