using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class VTManager : MonoBehaviour
{
    public Material VTMaterial;
    public List<Texture2D> textures = new List<Texture2D>();

    public Dictionary<int, int> vtIndexMap = new Dictionary<int, int>();
    public Dictionary<int, string> vtNameMap = new Dictionary<int, string>();

    [SerializeField]
    private int _virtualTextureSize = 8192;
    public const int BLOCK_SIZE = 128;
    public const int BLOCK_BORDER = 4;

    private int[] _VTMipsCount;
    public int[] VTMipsCount
    {
        get
        {
            if (_VTMipsCount == null)
            {
                _VTMipsCount = new int[8];
                var mip = 0;
                var mipBlockCount = 0;
                var mipSize = VirtualTextureSize;
                do
                {
                    _VTMipsCount[mip++] = mipBlockCount;
                    mipBlockCount += (mipSize / BLOCK_SIZE) * (mipSize / BLOCK_SIZE);
                    mipSize /= 2;
                }
                while (mipSize >= BLOCK_SIZE);
            }
            return _VTMipsCount;
        }
    }
    public int RealBlockSize { get { return BLOCK_SIZE + 2 * BLOCK_BORDER; } }
    public int VirtualTextureSize { get { return _virtualTextureSize; } }
    public int MaxMipLevel { get { return (int)Mathf.Log(VirtualTextureSize / BLOCK_SIZE, 2); } }

    private static VTManager _instance;
    public static VTManager Instance { get { return _instance; } }

    public int GetBlockCountInMip(int mip)
    {
        return (int)Mathf.Pow(2, MaxMipLevel - mip);
    }

    private void Awake()
    {
        _instance = this;

        var index = 0;
        foreach (var tex in textures)
        {
            vtIndexMap.Add(tex.GetInstanceID(), index++);
        }
        index = 0;
        foreach (var tex in textures)
        {
            vtNameMap.Add(index++, tex.name);
        }
    }
}
