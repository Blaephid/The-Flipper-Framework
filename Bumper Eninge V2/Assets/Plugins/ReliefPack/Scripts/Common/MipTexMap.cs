using UnityEngine;
using System.Collections;

public class MipTexMap : MonoBehaviour {

	private static Texture2D mipFilterTex64;
	private static Texture2D mipFilterTex128;
	private static Texture2D mipFilterTex256;
	private static Texture2D mipFilterTex512;
	private static Texture2D mipFilterTex1024;
	private static Texture2D mipFilterTex2048;
	
	private static void BuildMipFilterTex(int size) {
		size=Mathf.ClosestPowerOfTwo(size);
		Texture2D mipFilterTex;
		mipFilterTex=new Texture2D (size, size, TextureFormat.Alpha8, true);
		mipFilterTex.anisoLevel=3;
		mipFilterTex.filterMode=FilterMode.Trilinear;
		mipFilterTex.mipMapBias=0;
		for(int mip=0; mip<mipFilterTex.mipmapCount; mip++) {
			int len=size*size;
			Color[] cols=new Color[len];
			float cval=1.0f*mip/(mipFilterTex.mipmapCount-1);
			Color col=new Color(cval, cval, cval, cval);
			for(int i=0; i<len; i++) {
				cols[i]=col;
			}
			mipFilterTex.SetPixels(cols, mip);
		}
		mipFilterTex.Apply(false,true);
		switch(size) {
			case 64: mipFilterTex64=mipFilterTex; break;
			case 128: mipFilterTex128=mipFilterTex; break;
			case 256: mipFilterTex256=mipFilterTex; break;
			case 512: mipFilterTex512=mipFilterTex; break;
			case 1024: mipFilterTex1024=mipFilterTex; break;
			case 2048: mipFilterTex2048=mipFilterTex; break;
			default: mipFilterTex512=mipFilterTex; break;
		}
	}	
	
	public static Texture2D GetTex(int size) {
		size=Mathf.ClosestPowerOfTwo(size);
		switch(size) {
			case 64: if (mipFilterTex64) { return mipFilterTex64; } else { BuildMipFilterTex(size); return mipFilterTex64; };
			case 128: if (mipFilterTex128) { return mipFilterTex128; } else { BuildMipFilterTex(size); return mipFilterTex128; };
			case 256: if (mipFilterTex256) { return mipFilterTex256; } else { BuildMipFilterTex(size); return mipFilterTex256; };
			case 512: if (mipFilterTex512) { return mipFilterTex512; } else { BuildMipFilterTex(size); return mipFilterTex512; };
			case 1024: if (mipFilterTex1024) { return mipFilterTex1024; } else { BuildMipFilterTex(size); return mipFilterTex1024; };
			case 2048: if (mipFilterTex2048) { return mipFilterTex2048; } else { BuildMipFilterTex(size); return mipFilterTex2048; };
			default: if (mipFilterTex512)  { return mipFilterTex512; } else { BuildMipFilterTex(size); return mipFilterTex512; };
		}
	}
}
