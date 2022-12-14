using System;
using UnityEngine;

namespace VirtualTexture
{
	/// <summary>
	/// 加载器接口.
	/// </summary>
    public interface ILoader
	{
		/// <summary>
		/// 加载完成的事件回调.
		/// </summary>
        event Action<LoadRequest, Texture2D[]> OnLoadComplete;

		event Action<LoadRequest, byte[]> OnLoadBytesComplete;

		/// <summary>
		/// 新建加载请求
		/// </summary>
		LoadRequest Request(int texIndex, int x, int y, int mip);
    }
}
