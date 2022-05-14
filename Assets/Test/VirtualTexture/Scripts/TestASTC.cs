using System.Diagnostics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.IO;
using UnityEngine.UI;

public class TestASTC : MonoBehaviour
{
    public Texture2D astc;
    public RawImage rawImage;
    public int blockX = 0;
    public int blockY = 0;
    // Start is called before the first frame update
    void Start()
    {
        //SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.RGBA_ASTC4X4_UNorm, UnityEngine.Experimental.Rendering.FormatUsage.Sample);
        //var rawBytes = astc.GetRawTextureData();
        var pixelDatas = astc.GetPixelData<byte>(0);
        var mipPixelDatas = default(NativeArray<byte>);
        //var colors = astc.GetPixels(0);
        var newTex = new Texture2D(blockSize + 2 * borderSize, blockSize + 2 * borderSize, TextureFormat.ASTC_4x4, false);
        var blockDatas = LoadTexBlock(astc, ref mipPixelDatas, (blockX, blockY), 5);
        newTex.SetPixelData(blockDatas, 0);
        //var nativeArray = new NativeArray<byte>(blockSize * blockSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        //var subPixelBytes = new List<byte>(blockSize * blockSize);
        //subPixelBytes.AddRange(pixelDatas.GetSubArray(0, 32));
        //subPixelBytes.AddRange(pixelDatas.GetSubArray(64, 32));
        //newTex.SetPixelData(subPixelBytes.ToArray(), 0);

        //unsafe
        //{
        //    var dst = (byte*)nativeArray.GetUnsafePtr();
        //    var src = (byte*)pixelDatas.GetUnsafePtr();
        //    UnsafeUtility.MemCpy(dst, src, 32);
        //    dst += sizeof(byte) * 32;
        //    src += sizeof(byte) * 64;
        //    UnsafeUtility.MemCpy(dst, src, 32);

        //    newTex.SetPixelData(nativeArray, 0);
        //}

        //newTex.SetPixelData<byte>(pixelDatas.GetSubArray(16, 16), 0);
        //newTex.SetPixelData<byte>(pixelDatas.GetSubArray(32, 16), 0);
        //newTex.SetPixelData<byte>(pixelDatas.GetSubArray(48, 16), 0);


        //var row = blockSize / 4;
        //var line = blockSize / 4;
        //for (var i = 0; i < row; i++)
        //{
        //    for (var j = 0; j < line; j++)
        //    {
        //        var start = 0;
        //        var length = 0;
        //        newTex.SetPixelData<byte>(pixelDatas.GetSubArray(start, length), 0);
        //    }
        //}
        //newTex.LoadRawTextureData(pixelDatas.GetSubArray(startIndex * 16, blockSize * blockSize));
        newTex.Apply(false, true);
        rawImage.texture = newTex;
        rawImage.uvRect = new Rect(borderSize / (float)(blockSize + 2 * borderSize), borderSize / (float)(blockSize + 2 * borderSize), blockSize / (float)(blockSize + 2 * borderSize), blockSize / (float)(blockSize + 2 * borderSize));
        //newTex.SetPixelData(rawBytes, 0);
    }

    public static int borderSize = 4;
    public static int blockSize = 128;
    // TODO add border to promote sample rating
    public static NativeArray<byte> LoadTexBlock(Texture2D astc, ref NativeArray<byte> pixelDatas, (int x, int y) index, int mip = 0)
    {
        const int formatBlock = 4;
        if (astc.format != TextureFormat.ASTC_4x4) return new NativeArray<byte>();
        if (astc.width != astc.height) return new NativeArray<byte>();
        if ((astc.width / blockSize) <= index.x || (astc.width / blockSize) <= index.y) return new NativeArray<byte>();
        if (astc.width < blockSize) return astc.GetPixelData<byte>(mip);

        if (pixelDatas == default) pixelDatas = astc.GetPixelData<byte>(mip);
        var realBlockSize = blockSize + 2 * borderSize;
        var nativeArray = new NativeArray<byte>(realBlockSize * realBlockSize, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
        var byteSize = sizeof(byte);
        var mipWidth = astc.width / (int)Mathf.Pow(2, mip);
        var mipFormatCount = mipWidth / formatBlock;

        unsafe
        {
            var pixel = (byte*)pixelDatas.GetUnsafePtr();
            var src = pixel;
            var dst = (byte*)nativeArray.GetUnsafePtr();

            for (var i = 0; i < realBlockSize / formatBlock; i++)
            {
                var formatBlockY = index.y * blockSize / formatBlock + i - 1;
                if (formatBlockY < 0) formatBlockY = mipFormatCount - 1;
                if (formatBlockY >= mipFormatCount) formatBlockY = 0;
                for (var j = 0; j < realBlockSize / formatBlock; j++)
                {
                    var formatBlockX = index.x * blockSize / formatBlock + j - 1;
                    if (formatBlockX < 0) formatBlockX = mipFormatCount - 1;
                    if (formatBlockX >= mipFormatCount) formatBlockX = 0;
                    src = pixel + mipWidth * formatBlock * byteSize * formatBlockY + formatBlock * formatBlock * byteSize * formatBlockX;
                    UnsafeUtility.MemCpy(dst, src, formatBlock * formatBlock * byteSize);
                    dst += formatBlock * formatBlock * byteSize;
                }
            }
        }

        return nativeArray;
    }
}
