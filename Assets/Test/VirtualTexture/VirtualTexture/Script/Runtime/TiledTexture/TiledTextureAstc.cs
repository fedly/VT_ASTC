using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VirtualTexture;

public class TiledTextureAstc : MonoBehaviour, ITiledTexture
{
    [SerializeField]
    public Vector2Int _regionSize = new Vector2Int(16, 16); // 2K * 2K
    public Vector2Int RegionSize => _regionSize;

    public int TileSize => VTManager.BLOCK_SIZE;

    public int PaddingSize => VTManager.BLOCK_BORDER;

    public int TileSizeWithPadding { get { return TileSize + PaddingSize * 2; } }

    public int LayerCount => 1;

    public event Action<Vector2Int> OnTileUpdateComplete;

    public int TiledTextureWidth => TileSizeWithPadding * RegionSize.x;
    public int TiledTextureHeight => TileSizeWithPadding * RegionSize.y;

    public const int ASTCFormatBlock = 4;

    public Texture2D TiledTexture => _tiledTexture;
    private Texture2D _tiledTexture;
    private byte[] _tileTexBytes;

    public Vector2Int RequestTile()
    {
        throw new NotImplementedException();
    }

    public bool SetActive(Vector2Int tile)
    {
        throw new NotImplementedException();
    }

    public void UpdateTile(Vector2Int tile, Texture2D[] textures)
    {
        throw new NotImplementedException();
    }

    public void UpdateTileBytes(Vector2Int tile, byte[] bytes)
    {
        UnityEngine.Debug.Assert(bytes.Length == TileSizeWithPadding * TileSizeWithPadding);

        //if (!SetActive(tile))
        //    return;

        if (bytes == null)
            return;

        CopyBlockBytes(bytes, _tileTexBytes, tile);

        OnTileUpdateComplete?.Invoke(tile);
    }

    private void CopyBlockBytes(byte[] bytes, byte[] target, Vector2Int tile)
    {
        for (var i = 0; i < TileSizeWithPadding / ASTCFormatBlock; i++)
        {
            var srcOffset = i * TileSizeWithPadding * ASTCFormatBlock;
            var dstOffset = (tile.y * TileSizeWithPadding / ASTCFormatBlock + i) * TiledTextureWidth * ASTCFormatBlock + tile.x * TileSizeWithPadding * ASTCFormatBlock;
            Buffer.BlockCopy(bytes, srcOffset, target, dstOffset, TileSizeWithPadding * ASTCFormatBlock);
        }
    }

    //private void OnPreRender()
    //{
    //    UnityEngine.Debug.Log("OnPreRender");
    //    //ApplyTiledTexture();
    //}

    public void ApplyTiledTexture()
    {
        if (_tiledTexture != null && _tileTexBytes != null)
        {
            _tiledTexture.SetPixelData(_tileTexBytes, 0);
            _tiledTexture.Apply(false, false);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var astcTex = new Texture2D(TiledTextureWidth, TiledTextureHeight, TextureFormat.ASTC_4x4, false);
        astcTex.name = "TiledTextureASTC";
        astcTex.filterMode = FilterMode.Bilinear;
        astcTex.wrapMode = TextureWrapMode.Clamp;
        astcTex.anisoLevel = 8;
        //astcTex.Apply(false, false);
        _tiledTexture = astcTex;

        _tileTexBytes = new byte[TiledTextureWidth * TiledTextureHeight];

        Shader.SetGlobalTexture(
                string.Format("_VTTiledTex{0}", 0),
                astcTex);


        // 设置Shader参数
        // x: padding偏移量
        // y: tile有效区域的尺寸
        // zw: 1/区域尺寸
        Shader.SetGlobalVector(
            "_VTTileParam",
            new Vector4(
                (float)PaddingSize / TileSizeWithPadding,
                (float)TileSize / TileSizeWithPadding,
                1.0f / RegionSize.x,
                1.0f / RegionSize.y));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
