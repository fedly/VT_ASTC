using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Threading;
using VirtualTexture;
using UnityEngine.UI;

public class TestThreadFileIO : MonoBehaviour
{
    public string FileDir = "D:/";
    public int FileCount = 8;
    public RawImage rawImage;
    private FileLoaderAsync m_Loader;
    private TiledTextureAstc m_TextureAstc;
    private Stopwatch stopWatch = new Stopwatch();

    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return new WaitForSeconds(3);
        m_Loader = GetComponent<FileLoaderAsync>();
        m_Loader.OnLoadBytesComplete += OnLoadBytesComplete;
        m_TextureAstc = GetComponent<TiledTextureAstc>();
        //m_Loader.Request(2, 0, 0, 5);
        stopWatch.Reset();
        stopWatch.Restart();
        Random.InitState(0);
        for (int i = 0; i < testLoadCount; i++)
        {
            var texIndex = Random.Range(0, VTManager.Instance.textures.Count);
            var mip = Random.Range(0, VTManager.Instance.MaxMipLevel + 1);
            var mipCount = VTManager.Instance.GetBlockCountInMip(mip);
            var x = Random.Range(0, mipCount);
            var y = Random.Range(0, mipCount);
            if (m_Loader.Request(texIndex, x, y, mip) != default)
            {
                requestCount++;
            }
        }
    }
    public int testLoadCount = 10000;
    private int requestCount = 0;
    private int loadCount = 0;
    void OnLoadBytesComplete(LoadRequest request, byte[] bytes)
    {
        //var newTex = new Texture2D(VTManager.Instance.RealBlockSize, VTManager.Instance.RealBlockSize, TextureFormat.ASTC_4x4, false);
        //newTex.SetPixelData(bytes, 0);
        //newTex.Apply(false, true);
        //rawImage.texture = newTex;
        _loadedBytes.Add(bytes);

        if (++loadCount >= requestCount)
        {
            isLoadedAll = true;
            stopWatch.Stop();
            UnityEngine.Debug.Log($"{requestCount}");
            UnityEngine.Debug.Log($"{stopWatch.ElapsedMilliseconds}, ");
        }
    }
    private bool isLoadedAll = false;
    private List<byte[]> _loadedBytes = new List<byte[]>();
    private void Update()
    {
        if (isLoadedAll)
        {
            isLoadedAll = false;
            var index = 0;
            for (var i = 0; i < 10; i++)
            {
                for (var j = 0; j < 10; j++)
                {
                    if (index < _loadedBytes.Count)
                    {
                        m_TextureAstc.UpdateTileBytes(new Vector2Int(i, j), _loadedBytes[index++]);
                    }
                }
            }
            m_TextureAstc.ApplyTiledTexture();
            rawImage.texture = m_TextureAstc.TiledTexture;
        }
    }
}
