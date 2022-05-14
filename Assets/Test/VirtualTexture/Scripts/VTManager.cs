using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class VTManager : MonoBehaviour
{
    public Material VTMaterial;
    public List<Texture2D> textures = new List<Texture2D>();

    public Dictionary<int, int> vtIndexMap = new Dictionary<int, int>();

    [SerializeField]
    private int _virtualTextureSize = 8192;
    public const int BLOCK_SIZE = 128;
    public const int BLOCK_BORDER = 4;

    public int VirtualTextureSize { get { return _virtualTextureSize; } }
    public int MaxMipLevel { get { return (int)Mathf.Log(VirtualTextureSize / BLOCK_SIZE, 2); } }

    private static VTManager _instance;
    public static VTManager Instance { get { return _instance; } }

    private void Awake()
    {
        _instance = this;

        var index = 0;
        foreach (var tex in textures)
        {
            vtIndexMap.Add(tex.GetInstanceID(), index++);
        }
    }
}
