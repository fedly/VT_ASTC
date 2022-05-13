#ifndef VIRTUAL_TEXTURE_INCLUDED
#define VIRTUAL_TEXTURE_INCLUDED

struct VTAppdata {
	float4 vertex : POSITION;
	float2 texcoord : TEXCOORD0;
};

struct VTV2f
{
	float4 pos : SV_POSITION;
	float2 uv : TEXCOORD0;
};

// x: page size
// y: vertual texture size
// z: max mipmap level
// w: mipmap level bias
float4 _VTFeedbackParam;

// xy: page count
// z:  max mipmap level
float4 _VTPageParam;

// x: padding size
// y: center size
// zw: 1 / tile count
float4 _VTTileParam;


float _MainTexIndex;


sampler2D _VTLookupTex;

sampler2D _VTTiledTex0;
sampler2D _VTTiledTex1;
sampler2D _VTTiledTex2;
sampler2D _VTTiledTex3;

VTV2f VTVert(VTAppdata v)
{
	VTV2f o;
	UNITY_INITIALIZE_OUTPUT(VTV2f, o);

	o.pos = UnityObjectToClipPos(v.vertex);
	o.uv = v.texcoord;
	return o;
}

// 最多8级mips 8192 4096 2048 1024 512 256 128
// 第一个元素是0
float _VTMipsCount[8];
// 到mip的所有mipscount总数
float GetMipsCount(int mip)
{
	return _VTMipsCount[mip + 1];
}

float2 GetMipBlock(float2 uv, int mip)
{
	float maxMipLevel = _VTPageParam.z;
	int size = exp2(maxMipLevel - mip);
	float2 block = floor(size * uv);
	return block;
}

float2 GetLUTUV(float2 uv, int texIndex, int mip)
{
	float maxMipLevel = _VTPageParam.z;
	int size = exp2(maxMipLevel - mip);
	float2 block = GetMipBlock(uv, mip);
	int blockTexelIndex = texIndex * GetMipsCount(maxMipLevel)
		+ GetMipsCount(mip - 1) + size * block.x + block.y;
	float pageSize = _VTFeedbackParam.x;
	float lutUV = float2(fmod(blockTexelIndex, pageSize), floor(blockTexelIndex / pageSize));
	return lutUV / pageSize;
	return float2(0, 0);
}

float2 VTTransferUV(float2 uv, int texIndex)
{
	float2 uv0 = uv * _VTFeedbackParam.y;
	float2 dx = ddx(uv0);
	float2 dy = ddy(uv0);
	int mip = clamp(int(0.5 * log2(max(dot(dx, dx), dot(dy, dy))) + 0.5 + _VTFeedbackParam.w), 0, _VTFeedbackParam.z);

	float2 uvInt = GetLUTUV(uv, texIndex, mip);
	fixed4 page = tex2D(_VTLookupTex, uvInt) * 255;
	float2 inPageOffset = frac(uv * exp2(_VTPageParam.z - page.b));
	float2 inTileOffset = inPageOffset * _VTTileParam.y + _VTTileParam.x;
	return (page.rg + inTileOffset) * _VTTileParam.zw;
}


fixed4 VTTex2D0(float2 uv)
{
	return tex2D(_VTTiledTex0, uv);
}

fixed4 VTTex2D1(float2 uv)
{
	return tex2D(_VTTiledTex1, uv);
}

fixed4 VTTex2D2(float2 uv)
{
	return tex2D(_VTTiledTex2, uv);
}

fixed4 VTTex2D3(float2 uv)
{
	return tex2D(_VTTiledTex3, uv);
}

fixed4 VTTex2D(float2 uv)
{
	return VTTex2D0(uv);
}

#endif