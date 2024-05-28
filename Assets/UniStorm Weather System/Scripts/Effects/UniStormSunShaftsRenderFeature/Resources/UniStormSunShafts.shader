Shader "UniStorm/URP/UniStormSunShafts" {
	Properties{
		_MainTex("Base", 2D) = "" {}
	//_ColorBuffer ("Color", 2D) = "" {}
	_Skybox("Skybox", 2D) = "" {}
	}

		CGINCLUDE

#include "UnityCG.cginc"

		struct v2f {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
#if UNITY_UV_STARTS_AT_TOP
		float2 uv1 : TEXCOORD1;
#endif		

		UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
			UNITY_VERTEX_OUTPUT_STEREO //Insert
	};

	struct v2f_radial {
		float4 pos : SV_POSITION;
		float2 uv : TEXCOORD0;
		float2 blurVector : TEXCOORD1;

		UNITY_VERTEX_INPUT_INSTANCE_ID //Insert
			UNITY_VERTEX_OUTPUT_STEREO //Insert
	};

	UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex); //Insert UNITY_DECLARE_TEX2D(_MainTex);
	UNITY_DECLARE_SCREENSPACE_TEXTURE(_ColorBuffer); //sampler2D
	sampler2D _Skybox;
	UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

	uniform half4 _SunThreshold;

	uniform half4 _SunColor;
	uniform half4 _BlurRadius4;
	uniform half4 _SunPositionArray[2];
	uniform half4 _MainTex_TexelSize;
	half4 _MainTex_ST;
	half4 _ColorBuffer_ST;
	half4 _Skybox_ST;
	half4 _CameraDepthTexture_ST;


#define SAMPLES_FLOAT 6.0f
#define SAMPLES_INT 6

	v2f vert(appdata_img v) {
		v2f o;

		UNITY_SETUP_INSTANCE_ID(v); //Insert
		UNITY_INITIALIZE_OUTPUT(v2f, o); //Insert
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert

		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv = v.texcoord.xy;

#if UNITY_UV_STARTS_AT_TOP
		o.uv1 = v.texcoord.xy;
		if (_MainTex_TexelSize.y < 0)
			o.uv1.y = 1 - o.uv1.y;
#endif				

		return o;
	}

	half4 fragScreen(v2f i) : SV_Target{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert

		half4 colorA = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _MainTex_ST)); //tex2D UNITY_SAMPLE_TEX2D
		#if UNITY_UV_STARTS_AT_TOP
		half4 colorB = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ColorBuffer, UnityStereoScreenSpaceUVAdjust(i.uv1.xy, _ColorBuffer_ST)); //tex2D
		#else
		half4 colorB = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ColorBuffer, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _ColorBuffer_ST)); //tex2D
		#endif
		half4 depthMask = saturate(colorB * _SunColor);
		return 1.0f - (1.0f - colorA) * (1.0f - depthMask);
	}

		half4 fragAdd(v2f i) : SV_Target{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert

			half4 colorA = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _MainTex_ST)); //tex2D UNITY_SAMPLE_TEX2D
			#if UNITY_UV_STARTS_AT_TOP
			half4 colorB = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ColorBuffer, UnityStereoScreenSpaceUVAdjust(i.uv1.xy, _ColorBuffer_ST)); //tex2D
			#else
			half4 colorB = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_ColorBuffer, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _ColorBuffer_ST)); //tex2D
			#endif
			half4 depthMask = saturate(colorB * _SunColor);
			return colorA + depthMask;
	}

		v2f_radial vert_radial(appdata_img v) {
		v2f_radial o;

		UNITY_SETUP_INSTANCE_ID(v); //Insert
		UNITY_INITIALIZE_OUTPUT(v2f_radial, o); //Insert
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); //Insert

		o.pos = UnityObjectToClipPos(v.vertex);

		o.uv.xy = v.texcoord.xy;
		half4 _SunPosition = _SunPositionArray[unity_StereoEyeIndex];
		o.blurVector = (_SunPosition.xy - v.texcoord.xy) * _BlurRadius4.xy;

		return o;
	}

	half4 frag_radial(v2f_radial i) : SV_Target
	{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert

		half4 color = half4(0,0,0,0);
		for (int j = 0; j < SAMPLES_INT; j++)
		{
			half4 tmpColor = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _MainTex_ST)); //tex2D UNITY_SAMPLE_TEX2D
			color += tmpColor;
			i.uv.xy += i.blurVector;
		}
		return color / SAMPLES_FLOAT;
	}

		half TransformColor(half4 skyboxValue) {
		return dot(max(skyboxValue.rgb - _SunThreshold.rgb, half3(0, 0, 0)), half3(1, 1, 1)); // threshold and convert to greyscale
	}

	half4 frag_depth(v2f i) : SV_Target{
		UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert

		#if UNITY_UV_STARTS_AT_TOP
		float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust(i.uv1.xy, _CameraDepthTexture_ST));
		#else
		float depthSample = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _CameraDepthTexture_ST));
		#endif

		half4 tex = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _MainTex_ST)); //tex2D UNITY_SAMPLE_TEX2D

		depthSample = Linear01Depth(depthSample);

		// consider maximum radius
		half4 _SunPosition = _SunPositionArray[unity_StereoEyeIndex];

		#if UNITY_UV_STARTS_AT_TOP
		half2 vec = _SunPosition.xy - i.uv1.xy;
		#else
		half2 vec = _SunPosition.xy - i.uv.xy;
		#endif
		half dist = saturate(_SunPosition.w - length(vec.xy));

		half4 outColor = 0;

		// consider shafts blockers
		if (depthSample > 0.99)
			outColor = TransformColor(tex) * dist;

		return outColor;
	}

		half4 frag_nodepth(v2f i) : SV_Target{
			UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); //Insert

			#if UNITY_UV_STARTS_AT_TOP
			float4 sky = (tex2D(_Skybox, UnityStereoScreenSpaceUVAdjust(i.uv1.xy, _Skybox_ST)));
			#else
			float4 sky = (tex2D(_Skybox, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _Skybox_ST)));
			#endif

			float4 tex = (UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, UnityStereoScreenSpaceUVAdjust(i.uv.xy, _MainTex_ST)));//tex2D UNITY_SAMPLE_TEX2D

			// consider maximum radius
			half4 _SunPosition = _SunPositionArray[unity_StereoEyeIndex];

			#if UNITY_UV_STARTS_AT_TOP
			half2 vec = _SunPosition.xy - i.uv1.xy;
			#else
			half2 vec = _SunPosition.xy - i.uv.xy;
			#endif
			half dist = saturate(_SunPosition.w - length(vec));

			half4 outColor = 0;

			// find unoccluded sky pixels
			// consider pixel values that differ significantly between framebuffer and sky-only buffer as occluded
			if (Luminance(abs(sky.rgb - tex.rgb)) < 0.2)
				outColor = TransformColor(sky) * dist;

			return outColor;
	}



		ENDCG

		Subshader {


		Pass{
			 ZTest Always Cull Off ZWrite Off

			 CGPROGRAM

			 #pragma vertex vert
			 #pragma fragment fragScreen
	   #pragma enable_d3d11_debug_symbols

			 ENDCG
		}

			Pass{
				 ZTest Always Cull Off ZWrite Off

				 CGPROGRAM

				 #pragma vertex vert_radial
				 #pragma fragment frag_radial
		   #pragma enable_d3d11_debug_symbols

				 ENDCG
		}

			Pass{
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag_depth
		  #pragma enable_d3d11_debug_symbols

				ENDCG
		}

			Pass{
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag_nodepth
		  #pragma enable_d3d11_debug_symbols

				ENDCG
		}

			Pass{
				ZTest Always Cull Off ZWrite Off

				CGPROGRAM

				#pragma vertex vert
				#pragma fragment fragAdd
		  #pragma enable_d3d11_debug_symbols

				ENDCG
		}
	}

	Fallback off

} // shader
