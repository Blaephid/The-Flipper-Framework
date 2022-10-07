//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// missing defines for LOD/GRAD access
#if UNITY_VERSION >= 560
    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL)
        #define UNITY_USING_SPLIT_SAMPLERS
    #endif
#else
    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_COMPILER_HLSLCC)
        #define UNITY_USING_SPLIT_SAMPLERS
    #endif
#endif
#if !defined(UNITY_USING_SPLIT_SAMPLERS) && (defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(UNITY_COMPILER_HLSLCC) || defined(SHADER_API_PSSL) || (defined(SHADER_TARGET_SURFACE_ANALYSIS) && !defined(SHADER_TARGET_SURFACE_ANALYSIS_MOJOSHADER) && UNITY_VERSION >= 201810))
   #define UNITY_USING_SPLIT_SAMPLERS
#endif

//#if defined(SHADER_API_D3D11) || defined(SHADER_API_XBOXONE) || defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE) || defined(SHADER_API_PSSL)
#if defined(UNITY_USING_SPLIT_SAMPLERS)
	#if !defined(UNITY_SAMPLE_TEX2D_GRAD)
		#define UNITY_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) tex.SampleGrad(sampler##tex,coord,dx,dy)
	#endif
	#if !defined(UNITY_SAMPLE_TEX2D_GRAD_SAMPLER)
		#define UNITY_SAMPLE_TEX2D_GRAD_SAMPLER(tex,samplertex,coord,dx,dy) tex.SampleGrad(sampler##samplertex,coord,dx,dy)
	#endif
    #if defined(UNITY_SAMPLE_TEX2D_LOD)
        #undef UNITY_SAMPLE_TEX2D_LOD
    #endif
    #define UNITY_SAMPLE_TEX2D_LOD(tex,coord) tex.SampleLevel (sampler##tex,(coord).xy,(coord).w)
	#if !defined(UNITY_SAMPLE_TEX2D_LOD_SAMPLER)
		#define UNITY_SAMPLE_TEX2D_LOD_SAMPLER(tex,samplertex,coord) tex.SampleLevel (sampler##samplertex,(coord).xy,(coord).w)
	#endif
	#if !defined(UNITY_SAMPLE_TEX2D_BIAS)
		#define UNITY_SAMPLE_TEX2D_BIAS(tex,coord) tex.SampleBias (sampler##tex,(coord).xy,(coord).w)
	#endif
#else
	#if !defined(UNITY_SAMPLE_TEX2D_GRAD)
		#define UNITY_SAMPLE_TEX2D_GRAD(tex,coord,dx,dy) tex2Dgrad(tex,coord,dx,dy)
	#endif
    #if defined(UNITY_SAMPLE_TEX2D_LOD)
        #undef UNITY_SAMPLE_TEX2D_LOD
    #endif
    #define UNITY_SAMPLE_TEX2D_LOD(tex,coord) tex2Dlod(tex,coord)
    #if !defined(UNITY_SAMPLE_TEX2D_LOD_SAMPLER)
		#define UNITY_SAMPLE_TEX2D_LOD_SAMPLER(tex,samplertex,coord) tex2Dlod(tex,coord)
	#endif
	#if !defined(UNITY_SAMPLE_TEX2D_BIAS)
		#define UNITY_SAMPLE_TEX2D_BIAS(tex,coord) tex2Dbias(tex,coord)
	#endif
#endif

	float4 _MainTex_ST;
	void vert (inout appdata_full v, out Input o) {
	    #if defined(SHADER_API_D3D11) || defined(SHADER_API_D3D11_9X) || defined(UNITY_PI)
			UNITY_INITIALIZE_OUTPUT(Input, o);
		#endif

		o.texCoords_FlatRef.xy=TRANSFORM_TEX(v.texcoord, _MainTex);
	
		float3 Wpos=mul(unity_ObjectToWorld, v.vertex).xyz;
		
		#if defined(RTP_SNOW) || defined(RTP_WETNESS) || defined(RTP_CAUSTICS)
			float3 binormal = cross( v.normal, v.tangent.xyz ) * v.tangent.w;
			float3x3 rotation = float3x3( v.tangent.xyz, binormal, v.normal.xyz );				
		#endif

		#if defined(RTP_SNOW) || defined(RTP_WETNESS) || defined(RTP_CAUSTICS)
			o.texCoords_FlatRef.zw = normalEncode(( mul (rotation, mul(unity_WorldToObject, float4(0,1,0,0)).xyz) ).xyz);
		#endif
	}

	void surf (Input IN, inout SurfaceOutputStandard o) {
		o.Normal = float3(0, 0, 1); o.Albedo = 0; o.Emission = 0; o.Smoothness = 0; o.Alpha = 0;
		o.atten = 1;
		
		float _distance=length(_WorldSpaceCameraPos.xyz-IN.worldPos);
		float _uv_Relief_w=saturate((_distance - _TERRAIN_distance_start_bumpglobal) / _TERRAIN_distance_transition_bumpglobal);
		#if defined(COLOR_MAP) || defined(GLOBAL_PERLIN) || defined(RTP_UV_BLEND)
			float _uv_Relief_z=saturate((_distance - _TERRAIN_distance_start) / _TERRAIN_distance_transition);
			float _uv_Relief_wz_no_overlap=_uv_Relief_w*_uv_Relief_z;
			_uv_Relief_z=1-_uv_Relief_z;
		#else
			float _uv_Relief_z=1-_uv_Relief_w;
		#endif

		#if defined(GLOBAL_PERLIN)		
			float4 global_bump_val=UNITY_SAMPLE_TEX2D(_BumpMapGlobal, IN.texCoords_FlatRef.xy*_BumpMapGlobalScale);
			#if !defined(RTP_SIMPLE_SHADING)
				global_bump_val.rg=global_bump_val.rg*0.6 + UNITY_SAMPLE_TEX2D(_BumpMapGlobal, IN.texCoords_FlatRef.xy*_BumpMapGlobalScale*8).rg*0.4;
			#endif
		#endif

		float2 globalUV=(IN.worldPos.xz-_TERRAIN_PosSize.xy)/_TERRAIN_PosSize.zw;
		#ifdef COLOR_MAP
			float global_color_blend=lerp( lerp(_GlobalColorMapBlendValues.y, _GlobalColorMapBlendValues.x, _uv_Relief_z*_uv_Relief_z), _GlobalColorMapBlendValues.z, _uv_Relief_w);
			#if defined(RTP_SIMPLE_SHADING) || !defined(GLOBAL_PERLIN)
				float4 global_color_value= UNITY_SAMPLE_TEX2D(_ColorMapGlobal, globalUV);
				global_color_value=lerp(UNITY_SAMPLE_TEX2D_LOD(_ColorMapGlobal, float4(globalUV, _GlobalColorMapNearMIP.xx)), global_color_value, _uv_Relief_w);
			#else
				float4 global_color_value= UNITY_SAMPLE_TEX2D(_ColorMapGlobal, globalUV+(global_bump_val.rg-float2(0.5f, 0.5f))*_GlobalColorMapDistortByPerlin);
				global_color_value=lerp(UNITY_SAMPLE_TEX2D_LOD(_ColorMapGlobal, float4(globalUV+(global_bump_val.rg-float2(0.5f, 0.5f))*_GlobalColorMapDistortByPerlin, _GlobalColorMapNearMIP.xx)), global_color_value, _uv_Relief_w);
			#endif
			
			//float perlin2global_color=abs((global_bump_val.r-0.4)*5);
			//perlin2global_color*=perlin2global_color;
			//float GlobalColorMapSaturationByPerlin = saturate( lerp(_GlobalColorMapSaturation, _GlobalColorMapSaturationFar, _uv_Relief_w) -perlin2global_color*_GlobalColorMapSaturationByPerlin);
			float GlobalColorMapSaturationByPerlin = lerp(_GlobalColorMapSaturation, _GlobalColorMapSaturationFar, _uv_Relief_w);
			global_color_value.rgb=lerp(dot(global_color_value.rgb,0.35).xxx, global_color_value.rgb, GlobalColorMapSaturationByPerlin);
			global_color_value.rgb*=lerp(_GlobalColorMapBrightness, _GlobalColorMapBrightnessFar, _uv_Relief_w);
		#endif		
		
		#if defined(GLOBAL_PERLIN)
      		float perlinmask= UNITY_SAMPLE_TEX2D_BIAS(_BumpMapGlobal, float4(IN.texCoords_FlatRef.xy/16, _uv_Relief_w.xx*2)).r;
      	#else
      		#if defined(RTP_WETNESS) && !defined(SIMPLE_WATER)
      			float perlinmask= UNITY_SAMPLE_TEX2D(TERRAIN_FlowingMap, IN.texCoords_FlatRef.xy/8).a;
      		#else
      			float perlinmask=0;
      		#endif
      	#endif
   		float3 norm_far=float3(0,0,1);

		#if defined(TWO_LAYERS)
	      	float2 tH;
	      	tH.x= UNITY_SAMPLE_TEX2D(_HeightMap, IN.texCoords_FlatRef.xy).a;
	    	tH.y= UNITY_SAMPLE_TEX2D(_HeightMap2, IN.texCoords_FlatRef.xy).a;
	      	#if !defined(RTP_SIMPLE_SHADING)
	      	#if defined(GEOM_BLEND)
	      		float eh=max(0.001, _ExtrudeHeight*(1-IN.color.a));
	      	#else
	      		float eh=_ExtrudeHeight;
	      	#endif		      	
	      	float2 uv=IN.texCoords_FlatRef.xy + ParallaxOffset(tH.x, eh, IN.viewDir.xyz);
	      	float2 uv2=IN.texCoords_FlatRef.xy + ParallaxOffset(tH.y, eh, IN.viewDir.xyz);
	      	#endif
	      	float2 control=float2(IN.color.r, 1-IN.color.r);
	      	control*=(tH+0.01);
      		float2 control_orig=control;		
	      	control*=control;
	      	control*=control;
	      	control*=control;
	      	control/=dot(control, 1);
	      	#ifdef NOSPEC_BLEED
				float2 control_nobleed=saturate(control-float2(0.5,0.5))*2;
			#else
				float2 control_nobleed=control;
			#endif
	      	float actH=dot(control, tH);
	    #else
	      	float actH= UNITY_SAMPLE_TEX2D(_HeightMap, IN.texCoords_FlatRef.xy).a;
	      	#if !defined(RTP_SIMPLE_SHADING)
	      	#if defined(GEOM_BLEND)
	      		float eh=max(0.001, _ExtrudeHeight*(1-IN.color.a));
	      	#else
	      		float eh=_ExtrudeHeight;
	      	#endif		      	
	      	float2 uv=IN.texCoords_FlatRef.xy + ParallaxOffset(actH, eh, IN.viewDir.xyz);
	      	#endif
		#endif
      		
      	float2 rayPos;
      	
		// simple fresnel rim (w/o bumpmapping)
		IN.viewDir=normalize(IN.viewDir);
		IN.viewDir.z=saturate(IN.viewDir.z); // czasem wystepuja problemy na krawedziach widocznosci (viewDir.z nie powinien byc jednak ujemny)
		float diffFresnel = exp2(SchlickFresnelApproxExp2Const*IN.viewDir.z); // ca. (1-x)^5
		
		#if defined(RTP_SNOW) || defined(RTP_WETNESS) || defined(RTP_CAUSTICS)
			float3 flat_dir = normalDecode(IN.texCoords_FlatRef.zw);
			#if defined(RTP_WETNESS)
				float wetSlope=1-dot(norm_far, flat_dir.xyz);
			#endif
		#endif
		
		#if defined(GLOBAL_PERLIN)
			norm_far.xy = global_bump_val.rg*3-1.5;
			norm_far.z = sqrt(1 - saturate(dot(norm_far.xy, norm_far.xy)));			
		#endif
		
		#ifdef RTP_CAUSTICS
		float damp_fct_caustics;
   		#if defined(RTP_WETNESS)
			float damp_fct_caustics_inv;
		#endif
		{
			float norm=saturate(1-flat_dir.z);
			norm*=norm;
			norm*=norm;  
			float CausticsWaterLevel=TERRAIN_CausticsWaterLevel+norm*TERRAIN_CausticsWaterLevelByAngle;
			damp_fct_caustics=saturate((IN.worldPos.y-CausticsWaterLevel+TERRAIN_CausticsWaterDeepFadeLength)/TERRAIN_CausticsWaterDeepFadeLength);
			float overwater=saturate(-(IN.worldPos.y-CausticsWaterLevel-TERRAIN_CausticsWaterShallowFadeLength)/TERRAIN_CausticsWaterShallowFadeLength);
			damp_fct_caustics*=overwater;
       		#if defined(RTP_WETNESS)
				damp_fct_caustics_inv=1-overwater;
			#endif
			damp_fct_caustics*=saturate(flat_dir.z+0.1)*0.9+0.1;
		}
		#endif			
		
		// snow initial step
		#ifdef RTP_SNOW
			float3 norm_for_snow=norm_far*0.3;
			norm_for_snow.z+=0.7;
			#if defined(VERTEX_COLOR_TO_SNOW_COVERAGE)
				rtp_snow_strength*=VERTEX_COLOR_TO_SNOW_COVERAGE;
			#endif	
			float snow_const = 0.5*rtp_snow_strength;
			snow_const-=perlinmask;
			float snow_height_fct=saturate((rtp_snow_height_treshold - IN.worldPos.y)/rtp_snow_height_transition)*4;
			snow_height_fct=snow_height_fct<0 ? 0 : snow_height_fct;
			snow_const -= snow_height_fct;
			
			float snow_val;
			#ifdef COLOR_MAP
				snow_val = snow_const + rtp_snow_strength*dot(1-global_color_value.rgb, rtp_global_color_brightness_to_snow.xxx)+rtp_snow_strength*2;
			#else
				rtp_global_color_brightness_to_snow=0;
				snow_val = snow_const + rtp_snow_strength*0.5*rtp_global_color_brightness_to_snow+rtp_snow_strength*2;
			#endif
			snow_val -= rtp_snow_slope_factor*( 1 - dot(norm_for_snow, flat_dir.xyz) );
	
			float snow_depth=snow_val-1;
			snow_depth=snow_depth<0 ? 0:snow_depth*6; 
			
			//float snow_depth_lerp=saturate(snow_depth-rtp_snow_deep_factor);
	
			fixed3 rtp_snow_color_tex=rtp_snow_color.rgb;
		#endif		
		
		#ifdef RTP_UV_BLEND
			float blendVal=(1.0-_uv_Relief_z*0.3);
			#if defined(TWO_LAYERS)
				blendVal *= dot( control, float2(_MixBlend0, _MixBlend1) );
			#else
				blendVal *= _MixBlend0;
			#endif
			#if defined(GLOBAL_PERLIN)
				blendVal*=saturate((global_bump_val.r*global_bump_val.g*2+0.3));
			#endif

			#if defined(TWO_LAYERS)
				float2 MixScaleRouted=float2(_MixScale0, _MixScale1);
			#else
				float MixScaleRouted=_MixScale0;
			#endif
		#endif		
		
		// layer emission - init step
		#ifdef RTP_EMISSION
			#if defined(TWO_LAYERS)
				float emission_valA=dot(control, float2(_LayerEmission0, _LayerEmission1) );
				half3 _LayerEmissionColor=control.x * _LayerEmissionColor0 + control.y * _LayerEmissionColor1;
				float layer_emission = emission_valA;
			#else
				half3 _LayerEmissionColor=_LayerEmissionColor0;
				float layer_emission = _LayerEmission0;
			#endif
		#endif		
		
      	#if defined(RTP_SIMPLE_SHADING)
      		rayPos=IN.texCoords_FlatRef.xy;
      	#else
			#if defined(TWO_LAYERS)		      	
	      		rayPos=lerp(uv, uv2, control.y);
      		#else
    	  		rayPos=uv;
      		#endif
      	#endif
      	
	    #if defined(RTP_WETNESS) || defined(RTP_REFLECTION)
	        float p = 0;
	        float _WaterOpacity=0;
		#endif
		
		////////////////////////////////
		// water
		//
		#ifdef RTP_WETNESS
			#if defined(VERTEX_COLOR_TO_WATER_COVERAGE) || !defined(GLOBAL_PERLIN)
				#if defined(VERTEX_COLOR_TO_WATER_COVERAGE) 
					float water_mask = VERTEX_COLOR_TO_WATER_COVERAGE;
				#else
					float water_mask = 0;
				#endif
			#else
				float mip_selector_tmp=saturate(_uv_Relief_w-1);// bug in compiler for forward pass, we have to specify mip level indirectly (can't be treated constant)
				float water_mask = UNITY_SAMPLE_TEX2D_LOD(_BumpMapGlobal, float4(globalUV*(1-2*_BumpMapGlobal_TexelSize.xx)+_BumpMapGlobal_TexelSize.xx, mip_selector_tmp.xx)).b;
			#endif
			#if defined(TWO_LAYERS)		      	
				float2 water_splat_control=control;
				float2 water_splat_control_nobleed=control_nobleed;
			#endif
			float TERRAIN_LayerWetStrength = saturate(2 * (1 - water_mask - perlinmask*(1 - TERRAIN_GlobalWetness)))*TERRAIN_GlobalWetness;
			float2 roff=0;
			float2 flowOffset=0;

			wetSlope=saturate(wetSlope*TERRAIN_WaterLevelSlopeDamp);
			float _RippleDamp=saturate(TERRAIN_LayerWetStrength*2-1)*saturate(1-wetSlope*4)*_uv_Relief_z;
			TERRAIN_RainIntensity*=_RippleDamp;
			TERRAIN_LayerWetStrength=saturate(TERRAIN_LayerWetStrength*2);
			TERRAIN_WaterLevel=clamp(TERRAIN_WaterLevel + ((TERRAIN_LayerWetStrength - 1) - wetSlope)*2, 0, 2);
			#ifdef RTP_CAUSTICS
				TERRAIN_WaterLevel*=damp_fct_caustics_inv;
			#endif				
			TERRAIN_LayerWetStrength=saturate(TERRAIN_LayerWetStrength - (1-TERRAIN_LayerWetStrength)*actH*0.25);

			p = saturate((TERRAIN_WaterLevel - actH -(1-actH)*perlinmask*0.5)*TERRAIN_WaterEdge);
			p*=p;
	        _WaterOpacity= TERRAIN_WaterColor.a*p;
			#if defined(RTP_EMISSION)
				float wEmission = TERRAIN_WaterEmission*p;
				layer_emission = lerp( layer_emission, wEmission, _WaterOpacity);
				layer_emission = max( layer_emission, wEmission*(1-_WaterOpacity) );
			#endif					
			#if !defined(RTP_SIMPLE_SHADING) && !defined(SIMPLE_WATER)
				float2 flowUV=lerp(IN.texCoords_FlatRef.xy, rayPos.xy, 1-p*0.5)*TERRAIN_FlowScale;
				float _Tim=frac(_Time.x*TERRAIN_FlowCycleScale)*2;
				float ft=abs(frac(_Tim)*2 - 1);
				float2 flowSpeed=clamp((flat_dir.xy+0.01)*4,-1,1)/TERRAIN_FlowCycleScale;
				#ifdef FLOWMAP
					float4 vec= UNITY_SAMPLE_TEX2D(_FlowMap, flowUV)*2-1;
					flowSpeed+=lerp(vec.xy, vec.zw, IN.color.r)*float2(-1,1)*TERRAIN_FlowSpeedMap;
				#endif
				flowUV*=TERRAIN_FlowScale;
				flowSpeed*=TERRAIN_FlowSpeed*TERRAIN_FlowScale;
				float rtp_mipoffset_add = (1-saturate(dot(flowSpeed, flowSpeed)*TERRAIN_mipoffset_flowSpeed))*TERRAIN_mipoffset_flowSpeed;
				rtp_mipoffset_add+=(1-TERRAIN_LayerWetStrength)*8;
				rtp_mipoffset_add+=TERRAIN_FlowMipOffset;
				#if defined(GLOBAL_PERLIN)
					flowOffset= UNITY_SAMPLE_TEX2D_BIAS(_BumpMapGlobal, float4(flowUV+frac(_Tim.xx)*flowSpeed, rtp_mipoffset_add.xx)).rg*2-1;
					flowOffset=lerp(flowOffset, UNITY_SAMPLE_TEX2D_BIAS(_BumpMapGlobal, float4(flowUV+frac(_Tim.xx+0.5)*flowSpeed*1.25, rtp_mipoffset_add.xx)).rg*2-1, ft);
				#else
					flowOffset= UNITY_SAMPLE_TEX2D_BIAS(TERRAIN_FlowingMap, float4(flowUV+frac(_Tim.xx)*flowSpeed, rtp_mipoffset_add.xx)).ag*2-1;
					flowOffset=lerp(flowOffset, UNITY_SAMPLE_TEX2D_BIAS(TERRAIN_FlowingMap, float4(flowUV+frac(_Tim.xx+0.5)*flowSpeed*1.25, rtp_mipoffset_add.xx)).ag*2-1, ft);
				#endif
				#ifdef RTP_SNOW
					flowOffset*=saturate(1-snow_val);
				#endif							
				flowOffset*=lerp(TERRAIN_WetFlow, TERRAIN_Flow, p)*_uv_Relief_z*TERRAIN_LayerWetStrength;
			#endif
			
			#if defined(RTP_WET_RIPPLE_TEXTURE) && !defined(RTP_SIMPLE_SHADING)
				float2 rippleUV = IN.texCoords_FlatRef.xy*TERRAIN_RippleScale + flowOffset*0.1*flowSpeed/TERRAIN_FlowScale;
			    float4 Ripple;
			  	{
			  	 	Ripple = UNITY_SAMPLE_TEX2D(TERRAIN_RippleMap, rippleUV);
				    Ripple.xy = Ripple.xy * 2 - 1;
				
				    float DropFrac = frac(Ripple.w + _Time.x*TERRAIN_DropletsSpeed);
				    float TimeFrac = DropFrac - 1.0f + Ripple.z;
				    float DropFactor = saturate(0.2f + TERRAIN_RainIntensity * 0.8f - DropFrac);
				    float FinalFactor = DropFactor * Ripple.z * sin( clamp(TimeFrac * 9.0f, 0.0f, 3.0f) * 3.1415);
				  	roff = Ripple.xy * FinalFactor * 0.35f;
				  	
				  	rippleUV+=float2(0.25,0.25);
			  	 	Ripple = UNITY_SAMPLE_TEX2D(TERRAIN_RippleMap, rippleUV);
				    Ripple.xy = Ripple.xy * 2 - 1;
				
				    DropFrac = frac(Ripple.w + _Time.x*TERRAIN_DropletsSpeed);
				    TimeFrac = DropFrac - 1.0f + Ripple.z;
				    DropFactor = saturate(0.2f + TERRAIN_RainIntensity * 0.8f - DropFrac);
				    FinalFactor = DropFactor * Ripple.z * sin( clamp(TimeFrac * 9.0f, 0.0f, 3.0f) * 3.1415);
				  	roff += Ripple.xy * FinalFactor * 0.35f;
			  	}
			  	roff*=4*_RippleDamp*lerp(TERRAIN_WetDropletsStrength, 1, p);
			  	#ifdef RTP_SNOW
			  		roff*=saturate(1-snow_val);
			  	#endif
			  	roff+=flowOffset;
			#else
				roff = flowOffset;
			#endif
			
			#if !defined(RTP_SIMPLE_SHADING)
				flowOffset=TERRAIN_Refraction*roff*max(p, TERRAIN_WetRefraction);
				#if !defined(RTP_TRIPLANAR)
					rayPos.xy+=flowOffset;
				#endif
			#endif
		#endif
		// water
		///////////////////////////////////////////	      	
	    
	    //
	    // diffuse color
	    //
		#if defined(TWO_LAYERS)		      	
	      	fixed4 col = control.x*UNITY_SAMPLE_TEX2D(_MainTex, rayPos.xy) + control.y*UNITY_SAMPLE_TEX2D(_MainTex2, rayPos.xy);
	    #else
	      	fixed4 col = UNITY_SAMPLE_TEX2D(_MainTex, rayPos.xy);
	    #endif

	    // UV blend
		#if defined(RTP_UV_BLEND)
			#if defined(TWO_LAYERS)
				fixed4 colBlend = control.x * UNITY_SAMPLE_TEX2D(_MainTex, IN.texCoords_FlatRef.xy * _MixScale0) + control.y * UNITY_SAMPLE_TEX2D(_MainTex2, IN.texCoords_FlatRef.xy * _MixScale1);
				float3 colBlendDes=lerp((dot(colBlend.rgb, 0.33333)).xxx, colBlend.rgb, dot(control, float2(_MixSaturation0, _MixSaturation1)));
				float repl = dot( control, float2(_MixReplace0, _MixReplace1) );
				repl *= _uv_Relief_wz_no_overlap;
		        float3 blendNormal = UnpackNormal(UNITY_SAMPLE_TEX2D(_BumpMap, IN.texCoords_FlatRef.xy*_MixScale0)*control.x + UNITY_SAMPLE_TEX2D(_BumpMap2, IN.texCoords_FlatRef.xy*_MixScale1)*control.y );
				col.rgb=lerp(col.rgb, col.rgb*colBlendDes*dot(control, float2(_MixBrightness0, _MixBrightness1) ), lerp(blendVal, 1, repl));  
			#else
				fixed4 colBlend = UNITY_SAMPLE_TEX2D(_MainTex, IN.texCoords_FlatRef.xy * _MixScale0);
				float3 colBlendDes=lerp((dot(colBlend.rgb, 0.33333)).xxx, colBlend.rgb, _MixSaturation0);
				float repl = _MixReplace0;
				repl *= _uv_Relief_wz_no_overlap;
				float3 blendNormal = UnpackNormal(UNITY_SAMPLE_TEX2D(_BumpMap, IN.texCoords_FlatRef.xy*_MixScale0));
				col.rgb=lerp(col.rgb, col.rgb*colBlendDes*_MixBrightness0, lerp(blendVal, 1, repl));
			#endif
			col.rgb = lerp( col.rgb, colBlend.rgb , repl );
		#endif		    
	    
		#ifdef VERTICAL_TEXTURE
			float2 vert_tex_uv=float2(0, IN.worldPos.y/_VerticalTextureTiling);
			#ifdef GLOBAL_PERLIN
				vert_tex_uv += _VerticalTextureGlobalBumpInfluence*global_bump_val.xy;
			#endif
			half3 vert_tex= UNITY_SAMPLE_TEX2D(_VerticalTexture, vert_tex_uv).rgb;
			#if defined(TWO_LAYERS)
				float _VerticalTextureStrength=dot(control, float2(_VerticalTextureStrength0, _VerticalTextureStrength1));
			#else
				float _VerticalTextureStrength=_VerticalTextureStrength0;
			#endif
			col.rgb=lerp( col.rgb, col.rgb*vert_tex*2, _VerticalTextureStrength );
		#endif
			    
      	fixed3 colAlbedo=0;
      	
      	//
      	// PBL specularity
      	//
      	float glcombined=col.a;
		#if defined(RTP_UV_BLEND)			
			glcombined=lerp(glcombined, colBlend.a, repl*0.5);					
		#endif		    
		#if defined(RTP_COLORSPACE_LINEAR)
		//glcombined=FastToLinear(glcombined);
		#endif
		#if defined(TWO_LAYERS)		      	
			float _Metalness = dot(control_nobleed, float2(_Metalness0, _Metalness1)); // anti-bleed subtraction
			float _GlossMin = dot(control, float2(_GlossMin0, _GlossMin1) );
			float _GlossMax = dot(control, float2(_GlossMax0, _GlossMax1));
		#else
			float _Metalness = _Metalness0;
			float _GlossMin = _GlossMin0;
			float _GlossMax = _GlossMax0;
		#endif
		o.Smoothness = lerp(_GlossMin, _GlossMax, glcombined);
		o.Metallic = _Metalness;

		half colDesat=dot(col.rgb,0.33333);
		#if defined(TWO_LAYERS)		 		
			col.rgb=lerp(colDesat.xxx, col.rgb, dot(control, float2(_LayerSaturation0, _LayerSaturation1)) );	
        #else
			col.rgb=lerp(colDesat.xxx, col.rgb, _LayerSaturation0);
        #endif
		colAlbedo=col.rgb;
		#if defined(TWO_LAYERS)		 		
			col.rgb*=(control.x*_LayerColor0 + control.y*_LayerColor1)*2;
	      	
	        o.Normal = UnpackNormal(UNITY_SAMPLE_TEX2D(_BumpMap, rayPos.xy)*control.x + UNITY_SAMPLE_TEX2D(_BumpMap2, rayPos.xy)*control.y );
        #else
			col.rgb*=_LayerColor0*2;
	      	
	        o.Normal = UnpackNormal(UNITY_SAMPLE_TEX2D(_BumpMap, rayPos.xy) );
        #endif
		#if defined(RTP_UV_BLEND)
			o.Normal=lerp(o.Normal, blendNormal, repl);
		#endif        
        #ifdef RTP_SNOW
        	o.Normal = lerp( o.Normal, float3(0,0,1), saturate(snow_depth)*0.5 );
        #endif
      	
		////////////////////////////////
		// water
		//
        #if defined(RTP_WETNESS)
			float3 o_NormalNoWater = normalize(o.Normal);
			#ifdef RTP_CAUSTICS
				TERRAIN_WetGloss*=damp_fct_caustics_inv;
				TERRAIN_WaterGloss*=damp_fct_caustics_inv;
			#endif
	  		float porosity = 1-saturate(o.Smoothness * 2 - 1);
			float wet_fct = saturate(TERRAIN_LayerWetStrength*2-0.4);
			float glossDamper=lerp( (1-TERRAIN_WaterGlossDamper), 1, _uv_Relief_z); // odlegosc>near daje cakowite tumienie
			o.Smoothness = max(o.Smoothness, lerp(o.Smoothness, lerp(TERRAIN_WetGloss, TERRAIN_WaterGloss, p)*glossDamper, wet_fct)); // water glossiness
			o.Metallic = max(o.Metallic, lerp(o.Metallic, TERRAIN_WaterMetallic, wet_fct));
	  		
	  		// col - saturation, brightness
	  		half3 col_sat=col.rgb*col.rgb + half3(0.001, 0.001, 0.001); // saturation z utrzymaniem jasnosci
	  		col_sat*=dot(col.rgb,1)/dot(col_sat,1);
	  		wet_fct=saturate(TERRAIN_LayerWetStrength*(2-perlinmask));
	  		porosity*=0.5;
	  		col.rgb=lerp(col.rgb, col_sat, wet_fct*porosity);
			col.rgb*=1-wet_fct*TERRAIN_WetDarkening*(porosity+0.5);
					  		
	        // col - colorisation
	        col.rgb *= lerp(half3(1,1,1), TERRAIN_WaterColor.rgb, p*p);
	        
 			// col - opacity
			col.rgb = lerp(col.rgb, TERRAIN_WaterColor.rgb, _WaterOpacity );
			colAlbedo=lerp(colAlbedo, col.rgb, _WaterOpacity); // potrzebne do spec color				
	        
	        o.Normal = lerp(o.Normal, float3(0,0,1), max(p*0.7, _WaterOpacity));
	        o.Normal.xy+=roff;
        #endif
		// water
		////////////////////////////////      	
		
		float3 norm_snowCov=o.Normal;

				
		#if defined(TWO_LAYERS)
			float _BumpMapGlobalStrengthPerLayer=dot(control, float2(_BumpMapGlobalStrength0, _BumpMapGlobalStrength1));
		#else
			float _BumpMapGlobalStrengthPerLayer=_BumpMapGlobalStrength0;
		#endif
		#if !defined(RTP_SIMPLE_SHADING)
			{
			float3 tangentBase = normalize(cross(float3(0.0,1.0,0.0), norm_far));
			float3 binormalBase = normalize(cross(norm_far, tangentBase));
			float3 combinedNormal = tangentBase * o.Normal.x + binormalBase * o.Normal.y + norm_far * o.Normal.z;
			o.Normal = lerp(o.Normal, combinedNormal, lerp(rtp_perlin_start_val,1, _uv_Relief_w)*_BumpMapGlobalStrengthPerLayer);
			}
		#else
			o.Normal+=norm_far*lerp(rtp_perlin_start_val,1, _uv_Relief_w)*_BumpMapGlobalStrengthPerLayer;	
		#endif


		#ifdef COLOR_MAP
			float colBrightness=dot(col,1);
			#ifdef RTP_WETNESS
				global_color_blend*=(1-_WaterOpacity);
			#endif
			
			// basic global colormap blending
			#ifdef RTP_COLOR_MAP_BLEND_MULTIPLY
				col.rgb=lerp(col.rgb, col.rgb*global_color_value.rgb*2, global_color_blend);
			#else
				col.rgb=lerp(col.rgb, global_color_value.rgb, global_color_blend);
			#endif

			#ifdef RTP_IBL_DIFFUSE
				half3 colBrightnessNotAffectedByColormap=col.rgb*colBrightness/max(0.01, dot(col.rgb,float3(1,1,1)));
			#endif
		#else
			#ifdef RTP_IBL_DIFFUSE
				half3 colBrightnessNotAffectedByColormap=col.rgb;
			#endif
		#endif		
		
		#ifdef RTP_SNOW
			//rayPos.xy=lerp(rayPos.xy, IN.texCoords_FlatRef.xy, snow_depth_lerp);
		
			#ifdef COLOR_MAP
				snow_val = snow_const + rtp_snow_strength*dot(1-global_color_value.rgb, rtp_global_color_brightness_to_snow.xxx)+rtp_snow_strength*2;
			#else
				snow_val = snow_const + rtp_snow_strength*0.5*rtp_global_color_brightness_to_snow+rtp_snow_strength*2;
			#endif
			
			snow_val -= rtp_snow_slope_factor*saturate( 1 - dot( (norm_snowCov*0.7+norm_for_snow*0.3), flat_dir.xyz) - 0*dot( norm_for_snow, flat_dir.xyz));
			
			snow_val=saturate(snow_val);
			//snow_val=pow(abs(snow_val), rtp_snow_edge_definition); // replaced with dissolve functionality

			// UBER - micro bumps and dissolve
			float3 microSnowTex = UNITY_SAMPLE_TEX2D(_SparkleMap, rayPos.xy*rtp_snow_MicroTiling).gba;
			float dissolveMaskValue = microSnowTex.r;
			half dissolveMask = 1.08 - dissolveMaskValue;
			dissolveMask = lerp(0.15, dissolveMask, rtp_snow_edge_definition);// rtp_snow_Dissolve);
			half sv = snow_val * 6>dissolveMask ? saturate(snow_val * 6 - dissolveMask) : 0;

			half3 snowNormalMicro;
			snowNormalMicro.xy = microSnowTex.gb * 2 - 1;
			snowNormalMicro.xy *= lerp(sv * 6 * dissolveMaskValue, 1, snow_val) * rtp_snow_BumpMicro;
			snowNormalMicro.z = sqrt(1.0 - saturate(dot(snowNormalMicro.xy, snowNormalMicro.xy)));
			snowNormalMicro = normalize(snowNormalMicro);
			snow_val = sv;
			o.snow_val = snow_val;
			//////////////////////

			#ifdef COLOR_MAP
				half3 global_color_value_desaturated=dot(global_color_value.rgb, 0.37);//0.3333333); // bdzie troch jasniej
				#ifdef RTP_COLOR_MAP_BLEND_MULTIPLY
					rtp_snow_color_tex=lerp(rtp_snow_color_tex, rtp_snow_color_tex*global_color_value_desaturated.rgb*2, min(0.4,global_color_blend*0.7) );
				#else
					rtp_snow_color_tex=lerp(rtp_snow_color_tex, global_color_value_desaturated.rgb, min(0.4,global_color_blend*0.7) );
				#endif
			#endif

			col.rgb=lerp( col.rgb, rtp_snow_color.rgb, rtp_snow_Frost*0.05); // UBER Frost
			col.rgb=lerp( col.rgb, rtp_snow_color_tex, snow_val );
			
			float3 snow_normal=o.Normal;
			snow_normal=norm_for_snow + 2*snow_normal*(_uv_Relief_z*0.5+0.5);
			
			snow_normal=normalize(snow_normal);
			// blend with UBER like micro bumps
			snow_normal = BlendNormals(snow_normal, snowNormalMicro);

			o.Normal=lerp(o.Normal, snow_normal, snow_val);
			o.Metallic=lerp(o.Metallic, rtp_snow_metallic, snow_val);
			// przeniesione pod emisj (ktora zalezy od specular materiau _pod_ sniegiem)
			//o.Smoothness=lerp(o.Smoothness, rtp_snow_gloss, snow_val);
			float snow_damp=saturate(1-snow_val*2);
		#endif
				
		// emission of layer (inside)
		#ifdef RTP_EMISSION
			#ifdef RTP_SNOW
				layer_emission *= snow_damp*0.9+0.1; // delikatna emisja poprzez snieg
			#endif
			
			#if defined(RTP_WETNESS)
				layer_emission *= lerp(o.Smoothness, 1, p) * 2;
				// zroznicowanie koloru na postawie animowanych normalnych
				#ifdef RTP_FUILD_EMISSION_WRAP
					float norm_fluid_val=lerp( 0.7, saturate(dot(o.Normal.xy*4, o.Normal.xy*4)), _uv_Relief_z);
					o.Emission += (col.rgb + _LayerEmissionColor.rgb ) * ( norm_fluid_val*p+0.15 ) * layer_emission * 4;
				#else
					float norm_fluid_val=lerp( 0.5, (o.Normal.x+o.Normal.y), _uv_Relief_z);
					o.Emission += (col.rgb + _LayerEmissionColor.rgb ) * ( saturate(norm_fluid_val*2*p)*1.2+0.15 ) * layer_emission * 4;
				#endif
			#else
				layer_emission *= o.Smoothness * 2;
				o.Emission += (col.rgb + _LayerEmissionColor.rgb*0.2 ) * layer_emission * 4;
			#endif
			layer_emission = max(0, 1 - layer_emission);
			o.Smoothness *= layer_emission;
			col.rgb *= layer_emission;
		#endif		
		
		// przeniesione pod emisj (ktora zalezy od specular materiau _pod_ sniegiem)
		#ifdef RTP_SNOW
			o.Smoothness=lerp(o.Smoothness, rtp_snow_gloss, snow_val);
		#endif	
			
		o.Normal=normalize(o.Normal);
		o.Albedo=col.rgb;
	

		//
		// occlusion approximation
		//
		#if ( defined(RTP_WETNESS) && !defined(UNITY_PASS_SHADOWCASTER) )
			o_NormalNoWater = lerp(o_NormalNoWater, o.Normal, _uv_Relief_w); // no water ripples in distance

			o.Occlusion = lerp(o.Normal.z * o.Normal.z, o_NormalNoWater.z * o_NormalNoWater.z, p);
			// pass wetness value
			o.Wetness = saturate(p);
		#else
			o.Occlusion = o.Normal.z * o.Normal.z;
		#endif
		o.Occlusion *= o.Occlusion;
		o.Occlusion = lerp(o.Occlusion, 1, actH*actH);
		o.Occlusion *= o.Occlusion;
		#if ( defined(RTP_SNOW) && !defined(UNITY_PASS_SHADOWCASTER) )
			o.Occlusion = lerp(o.Occlusion, 1, (1-rtp_snow_occlusionStrength)*snow_val );
		#endif
		o.Occlusion = lerp(1, o.Occlusion, _occlusionStrength);
		//o.Smoothness = o.Occlusion;


		#if defined(VERTEX_COLOR_AO_DAMP)
			o.atten=VERTEX_COLOR_AO_DAMP;
		#endif
				
		// ^4 shaped diffuse fresnel term for soft surface layers (grass)
		#if defined(TWO_LAYERS)		
			float _DiffFresnel=dot( control, float2(RTP_DiffFresnel0, RTP_DiffFresnel1) );
		#else
			float _DiffFresnel=RTP_DiffFresnel0;
		#endif
		// diffuse fresnel term for snow
		#ifdef RTP_SNOW
			_DiffFresnel=lerp(_DiffFresnel, rtp_snow_diff_fresnel, snow_val);
		#endif
		float diffuseScatteringFactor=1.0 + diffFresnel*_DiffFresnel;
		o.Albedo *= diffuseScatteringFactor;
		#ifdef RTP_IBL_DIFFUSE
			colBrightnessNotAffectedByColormap *= diffuseScatteringFactor;
		#endif
		
		#ifdef RTP_WETNESS
			colAlbedo=lerp(colAlbedo, o.Albedo, p);
		#endif
		#if defined(TWO_LAYERS)
			o.Smoothness = lerp( saturate(o.Smoothness+4*dot(control_nobleed, float2(_FarSpecCorrection0,_FarSpecCorrection1) )), o.Smoothness, (1-_uv_Relief_w)*(1-_uv_Relief_w) );
		#else
			o.Smoothness = lerp( saturate(o.Smoothness + 4 * _FarSpecCorrection0), o.Smoothness, (1-_uv_Relief_w)*(1-_uv_Relief_w) );
		#endif
		
		float3 normalW=WorldNormalVector(IN, o.Normal);
		o.Albedo += normalW*0.0001; // HACK-ish workaround - Unity skip's passing variables for WorldNormalVector() in some cases (probably when results are used conditionally)

		#if defined(RTP_GLITTER) && (defined(UNITY_PASS_FORWARDBASE) || defined(UNITY_PASS_DEFERRED))
			#if defined(TWO_LAYERS)
				float glitter_thickness = dot(control, float2(_GlitterStrength0, _GlitterStrength1));
			#else
				float glitter_thickness = _GlitterStrength0;
			#endif
			Glitter(o, snow_val, normalW, rayPos.xy, ddx(rayPos.xy), ddy(rayPos.xy), IN.worldPos, glitter_thickness);
		#endif

		#ifdef RTP_CAUSTICS
		{
			float tim=_Time.x*TERRAIN_CausticsAnimSpeed;
			rayPos.xy=IN.worldPos.xz*TERRAIN_CausticsTilingScale;
			#ifdef RTP_VERTALPHA_CAUSTICS
				float3 _Emission= UNITY_SAMPLE_TEX2D(TERRAIN_CausticsTex, rayPos.xy+float2(tim, tim) ).aaa;
				_Emission+= UNITY_SAMPLE_TEX2D(TERRAIN_CausticsTex, rayPos.xy+float2(-tim, -tim*0.873) ).aaa;
				_Emission+= UNITY_SAMPLE_TEX2D(TERRAIN_CausticsTex, rayPos.xy*1.1+float2(tim, 0) ).aaa;
				_Emission+= UNITY_SAMPLE_TEX2D(TERRAIN_CausticsTex, rayPos.xy*0.5+float2(0, tim*0.83) ).aaa;
			#else
				float3 _Emission= UNITY_SAMPLE_TEX2D(TERRAIN_CausticsTex, rayPos.xy+float2(tim, tim) ).rgb;
				_Emission+= UNITY_SAMPLE_TEX2D(TERRAIN_CausticsTex, rayPos.xy+float2(-tim, -tim*0.873) ).rgb;
				_Emission+= UNITY_SAMPLE_TEX2D(TERRAIN_CausticsTex, rayPos.xy*1.1+float2(tim, 0) ).rgb;
				_Emission+= UNITY_SAMPLE_TEX2D(TERRAIN_CausticsTex, rayPos.xy*0.5+float2(0, tim*0.83) ).rgb;
			#endif
			_Emission=saturate(_Emission-1.55);
			_Emission*=_Emission;
			_Emission*=_Emission;
			_Emission*=TERRAIN_CausticsColor.rgb*8;
			_Emission*=damp_fct_caustics;
			//_Emission*=(1-_uv_Relief_w);
			o.Emission+=_Emission;
		} 
		#endif		
		
		#if defined(GEOM_BLEND)
			#if defined(BLENDING_HEIGHT)
				float4 terrain_coverage= UNITY_SAMPLE_TEX2D(_TERRAIN_Control, globalUV);
				float2 tiledUV=(IN.worldPos.xz-_TERRAIN_PosSize.xy+_TERRAIN_Tiling.zw)/_TERRAIN_Tiling.xy;
				float4 splat_control1=terrain_coverage * UNITY_SAMPLE_TEX2D(_TERRAIN_HeightMap, tiledUV) * IN.color.a;
				#if defined(TWO_LAYERS)
					float4 splat_control2=float4(control_orig, 0, 0) * (1-IN.color.a);
				#else
					float4 splat_control2=float4(actH+0.01, 0, 0, 0) * (1-IN.color.a);
				#endif

				float blend_coverage=dot(terrain_coverage, 1);
				if (blend_coverage>0.1) {

					splat_control1*=splat_control1;
					splat_control1*=splat_control1;
					splat_control1*=splat_control1;
					splat_control2*=splat_control2;
					splat_control2*=splat_control2;
					splat_control2*=splat_control2;

					float normalize_sum=dot(splat_control1, 1)+dot(splat_control2, 1);
					splat_control1 /= normalize_sum;
					splat_control2 /= normalize_sum;

					o.Alpha=dot(splat_control2,1);
					o.Alpha=lerp(1-IN.color.a, o.Alpha, saturate((blend_coverage-0.1)*4) );
				} else {
					o.Alpha=1-IN.color.a;
				}
			#else
				o.Alpha=1-IN.color.a;
			#endif		
	
		#endif
		
		// HACK-ish workaround - Unity skips passing color if IN.color is not explicitly used here in shader
		o.Albedo+=IN.color.xyz*0.0001;	
	}