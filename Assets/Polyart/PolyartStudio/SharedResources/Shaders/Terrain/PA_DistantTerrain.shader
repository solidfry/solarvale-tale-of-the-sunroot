// Made with Amplify Shader Editor v1.9.1.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Polyart/Dreamscape/Builtin/Distant Terrain"
{
	Properties
	{
		[SingleLineTexture]_RGBMap("RGB Map", 2D) = "white" {}
		[SingleLineTexture]_NormalMap("Normal Map", 2D) = "white" {}
		[SingleLineTexture]_ORMMap("ORM Map", 2D) = "white" {}
		[SingleLineTexture]_Layer01("Layer 01", 2D) = "white" {}
		[SingleLineTexture]_Layer02("Layer 02", 2D) = "white" {}
		[SingleLineTexture]_Layer03("Layer 03", 2D) = "white" {}
		_TilingSize("Tiling Size", Float) = 1
		_NormalIntensity("Normal Intensity", Range( 0 , 1)) = 1
		_RoughnessIntensity("Roughness Intensity", Range( 0 , 2)) = 1
		_Layer01Contrast("Layer 01 Contrast", Float) = 1
		_Layer02Contrast("Layer 02 Contrast", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _NormalMap;
		uniform float4 _NormalMap_ST;
		uniform float _NormalIntensity;
		uniform sampler2D _Layer03;
		uniform float _TilingSize;
		uniform sampler2D _Layer01;
		uniform float _Layer01Contrast;
		uniform sampler2D _RGBMap;
		uniform float4 _RGBMap_ST;
		uniform sampler2D _Layer02;
		uniform float _Layer02Contrast;
		uniform sampler2D _ORMMap;
		uniform float _RoughnessIntensity;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_NormalMap = i.uv_texcoord * _NormalMap_ST.xy + _NormalMap_ST.zw;
			o.Normal = UnpackScaleNormal( tex2D( _NormalMap, uv_NormalMap ), _NormalIntensity );
			float2 temp_cast_1 = (_TilingSize).xx;
			float2 uv_TexCoord10 = i.uv_texcoord * temp_cast_1;
			float temp_output_1_0_g1 = _Layer01Contrast;
			float2 uv_RGBMap = i.uv_texcoord * _RGBMap_ST.xy + _RGBMap_ST.zw;
			float4 tex2DNode13 = tex2D( _RGBMap, uv_RGBMap );
			float lerpResult7_g1 = lerp( ( 0.0 - temp_output_1_0_g1 ) , ( temp_output_1_0_g1 + 0.0 ) , tex2DNode13.r);
			float clampResult8_g1 = clamp( lerpResult7_g1 , 0.0 , 1.0 );
			float4 lerpResult15 = lerp( tex2D( _Layer03, uv_TexCoord10 ) , tex2D( _Layer01, uv_TexCoord10 ) , clampResult8_g1);
			float temp_output_1_0_g3 = _Layer02Contrast;
			float lerpResult7_g3 = lerp( ( 0.0 - temp_output_1_0_g3 ) , ( temp_output_1_0_g3 + 0.0 ) , tex2DNode13.b);
			float clampResult8_g3 = clamp( lerpResult7_g3 , 0.0 , 1.0 );
			float4 lerpResult18 = lerp( lerpResult15 , tex2D( _Layer02, uv_TexCoord10 ) , clampResult8_g3);
			o.Albedo = lerpResult18.rgb;
			float4 tex2DNode9_g6 = tex2D( _ORMMap, float2( 1,1 ) );
			o.Metallic = ( tex2DNode9_g6.b * 0.0 );
			o.Smoothness = ( 1.0 - ( tex2DNode9_g6.g * _RoughnessIntensity ) );
			float lerpResult10_g6 = lerp( 1.0 , tex2DNode9_g6.r , 0.0);
			o.Occlusion = lerpResult10_g6;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=19102
Node;AmplifyShaderEditor.TexturePropertyNode;11;-1963.791,387.7445;Inherit;True;Property;_RGBMap;RGB Map;0;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;9;-2494.49,-34.77016;Inherit;False;Property;_TilingSize;Tiling Size;6;0;Create;True;0;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;19;-976.8628,765.933;Inherit;True;Property;_NormalMap;Normal Map;1;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;14;-1440.736,588.7823;Inherit;False;Property;_Layer01Contrast;Layer 01 Contrast;9;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;4;-1957.304,-308.7755;Inherit;True;Property;_Layer03;Layer 03;5;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TextureCoordinatesNode;10;-2305.482,-54.16565;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;2;-1958.304,-88.77548;Inherit;True;Property;_Layer01;Layer 01;3;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SamplerNode;13;-1537.736,390.7823;Inherit;True;Property;_TextureSample6;Texture Sample 2;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;3;-1961.201,120.9293;Inherit;True;Property;_Layer02;Layer 02;4;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.FunctionNode;12;-1167.547,419.3898;Inherit;False;PA_SF_CheapContrast;-1;;1;91fe25245faae9e47950226a865e2555;0;2;2;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;20;-732.8159,766.5026;Inherit;True;Property;_TextureSample7;Texture Sample 7;7;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;7;-1545.304,-310.7755;Inherit;True;Property;_TextureSample2;Texture Sample 2;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;22;-713.1656,962.1363;Inherit;False;Property;_NormalIntensity;Normal Intensity;7;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-1100.758,643.4058;Inherit;False;Property;_Layer02Contrast;Layer 02 Contrast;10;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;5;-1549.304,-87.77548;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;25;-441.488,204.4915;Inherit;True;Property;_ORMMap;ORM Map;2;1;[SingleLineTexture];Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.UnpackScaleNormalNode;21;-324.1594,773.0238;Inherit;False;Tangent;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;15;-887.7576,-106.5942;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;16;-862.7576,467.4058;Inherit;False;PA_SF_CheapContrast;-1;;3;91fe25245faae9e47950226a865e2555;0;2;2;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-1553.304,132.2245;Inherit;True;Property;_TextureSample1;Texture Sample 1;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;26;-486.3469,399.5704;Inherit;False;Property;_RoughnessIntensity;Roughness Intensity;8;0;Create;True;0;0;0;False;0;False;1;0;0;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;27;-108.9868,186.7738;Inherit;False;PA_SF_ORM_Unpacker;11;;6;d74c7a4dc46d7b848b37bc309ff6ccaf;0;5;1;FLOAT2;1,1;False;2;SAMPLER2D;;False;4;FLOAT;0;False;5;FLOAT;1;False;6;FLOAT;0;False;3;FLOAT;7;FLOAT;0;FLOAT;8
Node;AmplifyShaderEditor.LerpOp;18;-579.7576,107.4058;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;24;113.3332,732.274;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;348.8728,114.5249;Float;False;True;-1;2;;0;0;Standard;Polyart/Dreamscape/Builtin/Distant Terrain;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;0;False;;0;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;9;0
WireConnection;13;0;11;0
WireConnection;12;2;13;1
WireConnection;12;1;14;0
WireConnection;20;0;19;0
WireConnection;7;0;4;0
WireConnection;7;1;10;0
WireConnection;5;0;2;0
WireConnection;5;1;10;0
WireConnection;21;0;20;0
WireConnection;21;1;22;0
WireConnection;15;0;7;0
WireConnection;15;1;5;0
WireConnection;15;2;12;0
WireConnection;16;2;13;3
WireConnection;16;1;17;0
WireConnection;6;0;3;0
WireConnection;6;1;10;0
WireConnection;27;2;25;0
WireConnection;27;5;26;0
WireConnection;18;0;15;0
WireConnection;18;1;6;0
WireConnection;18;2;16;0
WireConnection;24;0;21;0
WireConnection;0;0;18;0
WireConnection;0;1;24;0
WireConnection;0;3;27;7
WireConnection;0;4;27;0
WireConnection;0;5;27;8
ASEEND*/
//CHKSM=DABCCD3B3B2E0D9723EBFD7076AEDAC3F576D298