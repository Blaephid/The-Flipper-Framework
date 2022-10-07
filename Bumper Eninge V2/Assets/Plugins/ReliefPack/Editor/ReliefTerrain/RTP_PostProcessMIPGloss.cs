using UnityEditor;
using UnityEngine;
using System.Collections;

// special treatment of gloss maps - baking sepcular power & normal variance in MIP levels
public class RTP_PostProcessMIPGloss : AssetPostprocessor {

	// 1 - no bias, >1 smear more specularity at higher MIP levels
	const float filterGlossBias = 1.05f; // beware - tweaking this might be very tricky depending on the texture type !
	const float specPowExponent=16.0f; // closer to Unity's PBR
	const float varianceFilteringFactor=0.4f;// 40% variance accounting for MIP0 level

	//////////////////////////////////////////////////////////////////////////////////////
	int progress_count_max;
	int progress_count_current;
	int progress_granulation=1;
	string progress_description="";

	public void OnPostprocessTexture (Texture2D texture) {
        string txtPath = System.IO.Path.GetFileName(assetPath);
        ReliefTerrain[] rts = GameObject.FindObjectsOfType<ReliefTerrain>();
        for(int i=0; i<rts.Length; i++)
        {
            if (rts[i].globalSettingsHolder != null && rts[i].globalSettingsHolder.bakeJobArray != null)
            {
                if (AssetDatabase.GetAssetPath(rts[i].globalSettingsHolder.bakeJobArray[0]) == assetPath)
                {
                    if (rts[i].globalSettingsHolder.bakeJobArray.Length == 2)
                    {
                        // single set
                        PrepareMIPGlossMap((Texture2D)texture, txtPath, rts[i].globalSettingsHolder.bakeJobArray[1]); // Texture2D NormalMap
                        texture.Apply(false);
                    }
                    else
                    {
                        // atlas
                    }
                    rts[i].globalSettingsHolder.bakeJobArray = null;
                    break;
                }
            }
        }
	}

	public void PrepareMIPGlossMap(Texture2D GlossSpecTexture, string name, Texture2D NormalMap) {
		if (GlossSpecTexture==null) return;

		Color32[] _NormalMap=new Color32[1];
		int nSize=0;
		if (NormalMap!=null) {
			ResetProgress( GlossSpecTexture.mipmapCount-1, "Prebaking gloss "+name+" with variance");
			nSize=NormalMap.width;
			_NormalMap=NormalMap.GetPixels32(0);
		} else {
			ResetProgress( GlossSpecTexture.mipmapCount-1+((NormalMap!=null) ? 1:0), "Prebaking gloss "+name );
		}

		Color32[] GlossMapMIP0cols=GlossSpecTexture.GetPixels32(0);

		// level0 variance
		if (NormalMap!=null) {
			CheckProgress();
			int idx=0;
			float sizeInv=1.0f/GlossSpecTexture.width;
			for(int j=GlossSpecTexture.width-1; j>=0; j--) {
				for(int i=0; i<GlossSpecTexture.width; i++) {
					float glossiness=GlossMapMIP0cols[(GlossSpecTexture.width-1-j)*GlossSpecTexture.width+i].a/255.0f;
					float variance=BakeGlossinessVsVariance( i*sizeInv, j*sizeInv, 2, glossiness, _NormalMap, nSize);
					variance=Mathf.Lerp((GlossMapMIP0cols[idx].a/255.0f), variance, varianceFilteringFactor);
					GlossMapMIP0cols[idx].a = (byte)(variance*255.0f);
					idx++;
				}
			}
			GlossSpecTexture.SetPixels32(GlossMapMIP0cols, 0);
		}

//		// gloss preprocess (ranging)
//		float min = 0;
//		float max = 1;
//		for(int i=0; i<GlossMapMIP0cols.Length; i++) {
//			float glossiness=Mathf.Clamp01(Mathf.Lerp(min, max, GlossMapMIP0cols[i].a/255.0f));
//			GlossMapMIP0cols[i].a=(byte)(glossiness*255.0f);
//		}
//		GlossSpecTexture.SetPixels32(GlossMapMIP0cols, 0);
		int size=GlossSpecTexture.width>>1;
		Color32[] prevCols = GlossMapMIP0cols;
		for(int mipLevel=1; size>0; mipLevel++) {
			CheckProgress();

			int idx=0;
			byte[] glossMIPData=new byte[size*size];
			int texelFootprint = 1 << mipLevel;
			float sizeInv=1.0f/size;
			for(int j=size-1; j>=0; j--) {
				for(int i=0; i<size; i++) {
					float glossiness=MedianGlossiness(i, j, mipLevel, GlossMapMIP0cols);
					if (NormalMap!=null) {
						glossMIPData[idx] = (byte)(BakeGlossinessVsVariance(i*sizeInv, j*sizeInv, texelFootprint, glossiness, _NormalMap, nSize)*255.0f);
					} else {
						glossMIPData[idx] = (byte)(glossiness*255.0f); // median gloss only
					}
					idx++;
				}
			}

			Color32[] cols=new Color32[size * size];
			int len=size * size;
			for(int i=0; i<len; i++) {
				int ix=i%size;
				int iy=i/size;
				int idx0=iy*size*4+ix*2;
				int idx1=idx0+1;
				int idx2=idx0+size*2;
				int idx3=idx2+1;
				// musimy rcznie obliczyc kolejne MIPmapy koloru, GetPixels(dla miplevel>0) potrafi wywalic Unity (cos z przydziaem pamici powaznie zaczyna szwankowac)
				cols[i].r=(byte)((prevCols[idx0].r+prevCols[idx1].r+prevCols[idx2].r+prevCols[idx3].r)>>2);
				cols[i].g=(byte)((prevCols[idx0].g+prevCols[idx1].g+prevCols[idx2].g+prevCols[idx3].g)>>2);
				cols[i].b=(byte)((prevCols[idx0].b+prevCols[idx1].b+prevCols[idx2].b+prevCols[idx3].b)>>2);
				cols[i].a=glossMIPData[i];
			}
			prevCols=cols;
			GlossSpecTexture.SetPixels32(cols, mipLevel);

			size=size>>1;
		}

		EditorUtility.ClearProgressBar();
	}
	
	float MedianGlossiness(int texelPosX, int texelPosY, int mipLevel, Color32[] cols) {
		int texelFootprint = 1 << mipLevel;
		texelPosX*=texelFootprint;
		texelPosY*=texelFootprint;
		int size=Mathf.FloorToInt(Mathf.Sqrt(1.0f*cols.Length));
		texelPosY=size-texelPosY-texelFootprint;
		if(mipLevel == 0) {
			return cols[texelPosY*size+texelPosX].a/255.0f;
		} else {
			int tpX = texelPosX;
			int tpY = texelPosY;
			float avg_specPower=0;
			for(int y = 0; y < texelFootprint; y++) {
				for(int x = 0; x < texelFootprint; x++) {
					int samplePosX = tpX + x;
					int samplePosY = tpY + y;
					float gloss=cols[samplePosY*size+samplePosX].a/255.0f;
					avg_specPower+=((Mathf.Pow(2, gloss*specPowExponent+1) - 1.75f) + 8.0f ) / (8.0f * Mathf.PI);
				}
			}
            avg_specPower /= (float)(texelFootprint * texelFootprint);// *filterGlossBias;
			return (Mathf.Log(32*avg_specPower*Mathf.PI-25)-3*Mathf.Log(2))/(Mathf.Log(2)*specPowExponent); // rozwiazalem rownanie ze wzgl na szukane gloss przy pomocy equation solvera ze strony numberempire
		}
	}
	
	float BakeGlossinessVsVariance(float texelPosX, float texelPosY, int texelFootprint, float glossiness, Color32[] NormalMap, int nSize) {
		if(texelFootprint == 0) {
			return glossiness;
		} else {
			int tpX=Mathf.FloorToInt(texelPosX*nSize);
			int tpY=Mathf.FloorToInt((1-texelPosY)*nSize - texelFootprint);
			Vector3 avgNormal = Vector3.zero;
			for(int y = 0; y < texelFootprint; y++) {
				for(int x = 0; x < texelFootprint; x++) {
					int samplePosX = (tpX + x)%nSize;
					int samplePosY = (tpY + y)%nSize;
					Color32 normPix=NormalMap[((nSize+samplePosY-1)%nSize)*nSize+samplePosX];
					Vector3 sampleNormal = new Vector3(normPix.a/255.0f*2-1, normPix.g/255.0f*2-1, 0);
					sampleNormal.z=1-sampleNormal.x*sampleNormal.x-sampleNormal.y*sampleNormal.y;
					if (sampleNormal.z<0) sampleNormal.z=0; else sampleNormal.z=Mathf.Sqrt(sampleNormal.z);
					sampleNormal.Normalize();
					avgNormal += sampleNormal;
				}
			}
			avgNormal /= (float)(texelFootprint * texelFootprint);
			float variance=0;
			for(int y = 0; y < texelFootprint; y++) {
				for(int x = 0; x < texelFootprint; x++) {
					int samplePosX = (tpX + x)%nSize;
					int samplePosY = (tpY + y)%nSize;
					Color32 normPix=NormalMap[((nSize+samplePosY-1)%nSize)*nSize+samplePosX];
					Vector3 sampleNormal = new Vector3(normPix.a/255.0f*2-1, normPix.g/255.0f*2-1, 0);
					sampleNormal.z=1-sampleNormal.x*sampleNormal.x-sampleNormal.y*sampleNormal.y;
					if (sampleNormal.z<0) sampleNormal.z=0; else sampleNormal.z=Mathf.Sqrt(sampleNormal.z);
					sampleNormal.Normalize();
					float dot=Vector3.Dot(avgNormal, sampleNormal);
					//variance+=(1 - Mathf.Abs(dot));
					variance+=(1 - dot*dot);
				}
			}			
			variance /= (float)(texelFootprint * texelFootprint)/(1+(filterGlossBias-1)*2);
			float spec_power=( (Mathf.Pow(2, glossiness*specPowExponent+1)-1.75f) );//+ 8.0f) / (Mathf.PI * 8.0f);
			float spec_powerMax=( (Mathf.Pow(2, 1*specPowExponent+1)-1.75f) );//+ 8.0f) / (Mathf.PI * 8.0f);
			float varianceP=variance+1/(1+spec_power);
			float variancePP=Mathf.Clamp(varianceP, 1.0f/(1.0f+spec_powerMax), 0.5f);
			float new_glossiness=Mathf.Log(1.0f/variancePP - 1) / Mathf.Log(spec_powerMax);
			return new_glossiness;
		}
	}

	private void ResetProgress(int progress_count, string _progress_description="") {
		progress_count_max=progress_count;
		progress_count_current=0;
		progress_description=_progress_description;
	}

	private void CheckProgress() {
		if ( ((progress_count_current++) % progress_granulation) == (progress_granulation-1) )
		{
			EditorUtility.DisplayProgressBar( "Processing...", progress_description, 1.0f*progress_count_current/progress_count_max );
		}
	}	
}