using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.IO;

public static class ProcessVirtualTexture
{
    [MenuItem("VT/Process VT Offline")]
    // Start is called before the first frame update
    static void ProcessVTOffline()
    {
        var vtManager = Object.FindObjectOfType<VTManager>();
        if (vtManager != null)
        {
            blockSize = VTManager.BLOCK_SIZE;
            borderSize = VTManager.BLOCK_BORDER;
            var maxMipLevel = vtManager.MaxMipLevel;
            foreach (var tex in vtManager.textures)
            {
                var blockCount = vtManager.VirtualTextureSize / blockSize;
                var bytesPath = Application.streamingAssetsPath + "/" + tex.name + ".bytes";
                if (File.Exists(bytesPath))
                {
                    File.Delete(bytesPath);
                }
                using (var fs = File.Create(bytesPath))
                {
                    for (int mip = 0; mip <= maxMipLevel; mip++)
                    {
                        var mipPixelDatas = default(NativeArray<byte>);
                        for (int x = 0; x < blockCount; x++)
                        {
                            for (int y = 0; y < blockCount; y++)
                            {
                                UnityEngine.Debug.Log($"{tex.name}, {x}, {y}, {mip}");
                                var blockBytes = LoadTexBlock(tex, ref mipPixelDatas, (x, y), mip);
                                fs.Write(blockBytes.ToArray());
                                fs.Flush();
                                blockBytes.Dispose();
                            }
                        }
                        blockCount /= 2;
                    }
                }
            }
        }
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
