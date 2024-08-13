// Made with Amplify Shader Editor v1.9.1.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "PA_CloudShader"
{
	Properties
	{
		_Scale("Scale", Float) = 120
		_DistortScale("Distort Scale", Float) = 120
		_CloudSPeed("Cloud SPeed", Vector) = (0,0,0,0)
		_DistortSpeed("Distort Speed", Vector) = (0,0,0,0)
		_CloudsPower("Clouds Power", Float) = 1
		[HDR]_CloudColor("Cloud Color", Color) = (0.8679245,0.8556426,0.8556426,1)
		_CloudAlpha("Cloud Alpha", Float) = 1
		_OffsetIntensity("Offset Intensity", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Cull Off
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Standard alpha:fade keepalpha noshadow vertex:vertexDataFunc 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform float2 _CloudSPeed;
		uniform float _Scale;
		uniform float2 _DistortSpeed;
		uniform float _DistortScale;
		uniform float _CloudsPower;
		uniform float _OffsetIntensity;
		uniform float4 _CloudColor;
		uniform float _CloudAlpha;


		inline float noise_randomValue (float2 uv) { return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453); }

		inline float noise_interpolate (float a, float b, float t) { return (1.0-t)*a + (t*b); }

		inline float valueNoise (float2 uv)
		{
			float2 i = floor(uv);
			float2 f = frac( uv );
			f = f* f * (3.0 - 2.0 * f);
			uv = abs( frac(uv) - 0.5);
			float2 c0 = i + float2( 0.0, 0.0 );
			float2 c1 = i + float2( 1.0, 0.0 );
			float2 c2 = i + float2( 0.0, 1.0 );
			float2 c3 = i + float2( 1.0, 1.0 );
			float r0 = noise_randomValue( c0 );
			float r1 = noise_randomValue( c1 );
			float r2 = noise_randomValue( c2 );
			float r3 = noise_randomValue( c3 );
			float bottomOfGrid = noise_interpolate( r0, r1, f.x );
			float topOfGrid = noise_interpolate( r2, r3, f.x );
			float t = noise_interpolate( bottomOfGrid, topOfGrid, f.y );
			return t;
		}


		float SimpleNoise(float2 UV)
		{
			float t = 0.0;
			float freq = pow( 2.0, float( 0 ) );
			float amp = pow( 0.5, float( 3 - 0 ) );
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(1));
			amp = pow(0.5, float(3-1));
			t += valueNoise( UV/freq )*amp;
			freq = pow(2.0, float(2));
			amp = pow(0.5, float(3-2));
			t += valueNoise( UV/freq )*amp;
			return t;
		}


		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float2 uv_TexCoord3 = v.texcoord.xy + ( _Time.y * _CloudSPeed );
			float simpleNoise2 = SimpleNoise( uv_TexCoord3*_Scale );
			float2 uv_TexCoord14 = v.texcoord.xy + ( _Time.y * _DistortSpeed );
			float simpleNoise19 = SimpleNoise( uv_TexCoord14*_DistortScale );
			float temp_output_8_0 = pow( ( simpleNoise2 * simpleNoise19 ) , _CloudsPower );
			v.vertex.xyz += ( ase_vertexNormal * temp_output_8_0 * _OffsetIntensity );
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_TexCoord3 = i.uv_texcoord + ( _Time.y * _CloudSPeed );
			float simpleNoise2 = SimpleNoise( uv_TexCoord3*_Scale );
			float2 uv_TexCoord14 = i.uv_texcoord + ( _Time.y * _DistortSpeed );
			float simpleNoise19 = SimpleNoise( uv_TexCoord14*_DistortScale );
			float temp_output_8_0 = pow( ( simpleNoise2 * simpleNoise19 ) , _CloudsPower );
			o.Albedo = ( _CloudColor * temp_output_8_0 ).rgb;
			float temp_output_12_0 = ( temp_output_8_0 * _CloudAlpha );
			o.Alpha = temp_output_12_0;
		}

		ENDCG
	}
}
/*ASEBEGIN
Version=19102
Node;AmplifyShaderEditor.RangedFloatNode;9;-626.6534,223.9954;Inherit;False;Property;_CloudsPower;Clouds Power;4;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;8;-336.6534,-5.004639;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;11;-149.6534,-178.0046;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-313.8131,200.1242;Inherit;False;Property;_CloudAlpha;Cloud Alpha;6;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;12;-117.8131,37.12424;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;14;-1235.202,591.8273;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;15;-1747.831,592.5808;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-1491.831,638.5808;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;19;-843.2018,583.8273;Inherit;True;Simple;True;False;2;0;FLOAT2;111,111;False;1;FLOAT;22;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-1168.202,726.8274;Inherit;False;Property;_DistortScale;Distort Scale;1;0;Create;True;0;0;0;False;0;False;120;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;17;-1750.831,698.5808;Inherit;False;Property;_DistortSpeed;Distort Speed;3;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;3;-1719.511,-11.19421;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-1976.14,35.55926;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;6;-2241.14,132.5592;Inherit;False;Property;_CloudSPeed;Cloud SPeed;2;0;Create;True;0;0;0;False;0;False;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;4;-1652.511,123.8058;Inherit;False;Property;_Scale;Scale;0;0;Create;True;0;0;0;False;0;False;120;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;2;-1327.512,-19.19421;Inherit;True;Simple;True;False;2;0;FLOAT2;111,111;False;1;FLOAT;22;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-960.634,71.12885;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;115.2566,420.4245;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;23;-211.837,582.3484;Inherit;False;Property;_OffsetIntensity;Offset Intensity;7;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;21;-259.1434,424.1245;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;5;-2232.14,-10.44076;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;753.4265,-125.8357;Float;False;True;-1;2;;0;0;Standard;PA_CloudShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;0;False;;0;False;;False;0;False;;0;False;;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;False;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;;-1;0;True;_AlphaClip;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.ColorNode;10;-1334.028,-205.6608;Inherit;False;Property;_CloudColor;Cloud Color;5;1;[HDR];Create;True;0;0;0;False;0;False;0.8679245,0.8556426,0.8556426,1;0.8679245,0.8556426,0.8556426,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendOpsNode;33;-957.8435,-353.0298;Inherit;False;Multiply;True;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1;False;1;COLOR;0
WireConnection;8;0;20;0
WireConnection;8;1;9;0
WireConnection;11;0;10;0
WireConnection;11;1;8;0
WireConnection;12;0;8;0
WireConnection;12;1;13;0
WireConnection;14;1;16;0
WireConnection;16;0;15;0
WireConnection;16;1;17;0
WireConnection;19;0;14;0
WireConnection;19;1;18;0
WireConnection;3;1;7;0
WireConnection;7;0;5;0
WireConnection;7;1;6;0
WireConnection;2;0;3;0
WireConnection;2;1;4;0
WireConnection;20;0;2;0
WireConnection;20;1;19;0
WireConnection;22;0;21;0
WireConnection;22;1;8;0
WireConnection;22;2;23;0
WireConnection;0;0;11;0
WireConnection;0;9;12;0
WireConnection;0;11;22;0
WireConnection;33;0;2;0
WireConnection;33;1;10;0
ASEEND*/
//CHKSM=CBE7C048CDF591189C8E00DC9AB1BA4DE6EB1429