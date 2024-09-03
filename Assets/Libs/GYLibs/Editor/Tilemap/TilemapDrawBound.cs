using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public class VegasModelBakerPreEditor : OdinEditorWindow
{
    private bool _isReady = false;

    private bool _isCreating = false;

    [SceneObjectsOnly]
    public Tilemap tilemap;

    [AssetsOnly]
    public TileBase tileBlock;
    
    [MinValue(0)]
    public int width;

    [MinValue(0)]
    public int height;


    //添加菜单栏用于打开窗口
    [MenuItem("Tools/Tilemap/围边界")]
    static void ShowWindow()
    {
        GetWindow<VegasModelBakerPreEditor>().Show();
    }

    public VegasModelBakerPreEditor()
    {
        this.titleContent = new GUIContent("绘制边界");
        init();
    }

    private void init()
    {
        _isCreating = false;
        _isReady = false;
    }

    /// <summary>
    /// 围边界
    /// </summary>
    [Button("生成边界", 30)]
    [EnableIf("@tilemap != null && tileBlock != null && width != 0 && height != 0")]
    public void GenBound()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || y == 0 || x == width - 1 || y == height - 1)
                {
                    tilemap.SetTile(new Vector3Int() { x = x, y = y }, tileBlock);
                }
            }
        }
    }
}
