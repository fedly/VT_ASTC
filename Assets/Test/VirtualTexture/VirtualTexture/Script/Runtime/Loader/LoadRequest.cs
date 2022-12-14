namespace VirtualTexture
{
	/// <summary>
	/// 加载请求类.
	/// </summary>
    public class LoadRequest
    {
		public int TexureIndex { get; }
		/// <summary>
		/// 页表X坐标
		/// </summary>
        public int PageX { get; }

		/// <summary>
		/// 页表Y坐标
		/// </summary>
        public int PageY { get; }

		/// <summary>
		/// mipmap等级
		/// </summary>
		public int MipLevel { get; }

		/// <summary>
		/// 构造函数
		/// </summary>
        public LoadRequest(int texIndex, int x, int y, int mip)
        {
			TexureIndex = texIndex;
            PageX = x;
            PageY = y;
            MipLevel = mip;
        }

        public override int GetHashCode()
        {
            return TexureIndex * 100000000 + PageX * 1000000 + PageY * 1000 + MipLevel;
		}

        public override bool Equals(object obj)
        {
            return obj != null && this.GetHashCode() == obj.GetHashCode();
        }
    }
}