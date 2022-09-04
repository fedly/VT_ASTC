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
        public event Action<LoadRequest, Texture2D[]> OnLoadComplete;
        public event Action<LoadRequest, byte[]> OnLoadBytesComplete;

        private ConcurrentDictionary<LoadRequest, int> m_PendingRequests = new ConcurrentDictionary<LoadRequest, int>();
        //private ConcurrentDictionary<LoadRequest, bool> m_RuningRequests = new ConcurrentDictionary<LoadRequest, bool>();
        private ConcurrentDictionary<LoadRequest, byte[]> m_LoadedRequests = new ConcurrentDictionary<LoadRequest, byte[]>();
        private volatile LoadRequest _runningRequest;

        public LoadRequest Request(int texIndex, int x, int y, int mip)
        {
            var request = new LoadRequest(texIndex, x, y, mip);
            if (!VTManager.Instance.vtNameMap.TryGetValue(request.TexureIndex, out var name))
            {
                UnityEngine.Debug.LogError("can't find texture");
                return null;
            }
            if (m_LoadedRequests.ContainsKey(request) || request.Equals(_runningRequest))
            {
                return null;
            }

            if (m_PendingRequests.AddOrUpdate(request, (r) => 0, (r, v) => v + 1) == 0)
            {
                if (threadWaitOne)
                {
                    threadWaitOne = false;
                    autoResetEvent.Set();
                }

                return request;
            }

            return null;
        }

        byte[] LoadBlockBytesAsync(LoadRequest request)
        {
            VTManager.Instance.vtNameMap.TryGetValue(request.TexureIndex, out var fileName);
            var path = Application.streamingAssetsPath + "/" + fileName + ".bytes";
            var count = VTManager.Instance.RealBlockSize * VTManager.Instance.RealBlockSize;
            var offset = (VTManager.Instance.VTMipsCount[request.MipLevel] + request.PageY * VTManager.Instance.GetBlockCountInMip(request.MipLevel) + request.PageX) * count;
            //m_RuningRequests.TryAdd(request, true);
            _runningRequest = request;
            using (var fs = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                // TODO need bytes pool
                var bytes = new byte[count];
                fs.Seek((int)offset, SeekOrigin.Begin);
                var c = fs.Read(bytes, 0, count);
                //m_RuningRequests.TryRemove(request, out var value);
                _runningRequest = null;
                Debug.Assert(c == count);
                return bytes;
            }
        }

        volatile bool fileLoaderThreading = true;
        volatile bool threadWaitOne = true;
        AutoResetEvent autoResetEvent = new AutoResetEvent(false);
        // Start is called before the first frame update
        void Awake()
        {
            Task.Run(async () =>
            {
                await Task.Yield();
                while (fileLoaderThreading)
                {
                    while (m_PendingRequests.Count > 0)
                    {
                        var keys = m_PendingRequests.Keys;
                        foreach (var request in keys)
                        {
                            try
                            {
                                var bytes = LoadBlockBytesAsync(request);
                                m_LoadedRequests.TryAdd(request, bytes);
                                m_PendingRequests.TryRemove(request, out var value);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning(ex.ToString());
                            }
                        }
                    }

                    threadWaitOne = true;
                    autoResetEvent.WaitOne();
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

        void OnDestroy()
        {
            fileLoaderThreading = false;
            m_PendingRequests.Clear();
            m_LoadedRequests.Clear();
        }
    }
}
