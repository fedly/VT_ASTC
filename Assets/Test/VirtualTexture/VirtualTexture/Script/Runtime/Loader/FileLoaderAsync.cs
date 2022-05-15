using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualTexture
{
    public class FileLoaderAsync : MonoBehaviour, ILoader
    {
        public int MaxThread = 4;

        public event Action<LoadRequest, Texture2D[]> OnLoadComplete;
        public event Action<LoadRequest, byte[]> OnLoadBytesComplete;

        private ConcurrentDictionary<LoadRequest, bool> m_PendingRequests = new ConcurrentDictionary<LoadRequest, bool>();
        private ConcurrentDictionary<LoadRequest, bool> m_RuningRequests = new ConcurrentDictionary<LoadRequest, bool>();
        private ConcurrentDictionary<LoadRequest, byte[]> m_LoadedRequests = new ConcurrentDictionary<LoadRequest, byte[]>();

        private Dictionary<string, bool> requests = new Dictionary<string, bool>();

        public LoadRequest Request(int texIndex, int x, int y, int mip)
        {
            var key = $"{texIndex}-{x}-{y}-{mip}";
            if (requests.ContainsKey(key)) return null;

            requests.Add(key, true);
            // 是否已经在请求队列中
            foreach (var r in m_RuningRequests)
            {
                if (r.Key.TexureIndex == texIndex && r.Key.PageX == x && r.Key.PageY == y && r.Key.MipLevel == mip)
                    return null;
            }
            foreach (var r in m_PendingRequests)
            {
                if (r.Key.TexureIndex == texIndex && r.Key.PageX == x && r.Key.PageY == y && r.Key.MipLevel == mip)
                    return null;
            }

            // 加入待处理列表
            var request = new LoadRequest(texIndex, x, y, mip);
            m_PendingRequests.TryAdd(request, true);
            return request;
        }

        async Task<byte[]> LoadBlockBytesAsync(LoadRequest request)
        {
            if (VTManager.Instance.vtNameMap.TryGetValue(request.TexureIndex, out var name))
            {
                var path = Application.streamingAssetsPath + "/" + name + ".bytes";
                var count = VTManager.Instance.RealBlockSize * VTManager.Instance.RealBlockSize;
                var offset = (VTManager.Instance.VTMipsCount[request.MipLevel] + request.PageY * VTManager.Instance.GetBlockCountInMip(request.MipLevel) + request.PageX) * count;
                m_RuningRequests.TryAdd(request, true);
                using (var fs = File.Open(path, FileMode.Open))
                {
                    var bytes = new byte[count];
                    fs.Seek(offset, SeekOrigin.Begin);
                    var c = await fs.ReadAsync(bytes, 0, count);
                    m_RuningRequests.TryRemove(request, out var value);
                    Debug.Assert(c == count);
                    return bytes;
                }
            }
            return null;
        }

        // Start is called before the first frame update
        void Start()
        {
            Task.Run(async () =>
            {
                await Task.Yield();
                while (true)
                {
                    if (m_PendingRequests.Count > 0 && m_RuningRequests.Count < MaxThread)
                    {
                        var r = default(LoadRequest);
                        foreach (var request in m_PendingRequests.Keys)
                        {
                            _ = LoadBlockBytesAsync(request).ContinueWith(t =>
                            {
                                var bytes = t.Result;
                                m_LoadedRequests.TryAdd(request, bytes);
                            });
                            r = request;
                            break;
                        }
                        m_PendingRequests.TryRemove(r, out var value);
                    }
                    else
                    {
                        Thread.Sleep(0);
                    }
                }
            });
        }

        // Update is called once per frame
        void Update()
        {
            foreach (var request in m_LoadedRequests.Keys)
            {
                if (m_LoadedRequests.TryRemove(request, out var bytes))
                {
                    OnLoadBytesComplete(request, bytes);
                }
            }
        }
    }
}
