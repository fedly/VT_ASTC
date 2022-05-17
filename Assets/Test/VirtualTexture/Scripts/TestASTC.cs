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
        var mipPixelDatas = default(NativeArray<byte>);
        var newTex = new Texture2D(blockSize + 2 * borderSize, blockSize + 2 * borderSize, TextureFormat.ASTC_4x4, false);
        var blockDatas = LoadTexBlock(astc, ref mipPixelDatas, (blockX, blockY), 5);
        newTex.SetPixelData(blockDatas, 0);
        newTex.Apply(false, true);
        rawImage.texture = newTex;
        rawImage.uvRect = new Rect(borderSize / (float)(blockSize + 2 * borderSize), borderSize / (float)(blockSize + 2 * borderSize), blockSize / (float)(blockSize + 2 * borderSize), blockSize / (float)(blockSize + 2 * borderSize));
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
