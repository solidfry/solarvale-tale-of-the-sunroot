    Shader "Hidden/Gaia/MixHeight" {

    Properties {
				//The input texture
				_InputTex ("Input Texture", any) = "" {}	
				//The brush texture containing the stamp & the remaining local changes
				_BrushTex("Brush Texture", any) = "" {}
                //The global brush texture 
                _GlobalBrushTex("Stamp Texture", any) =""{}
                //The level at which the mixing with the brush value takes place
                //0=at the bottom level of the Brush
                //1=at the top level of the Brush 
				 _MixMidPoint("Mix Mid Point", Float) = 0.5
				 //Strength from 0 to 1 to determine how "strong" the effect is applied
				 _Strength ("Strength", Float) = 0
                 //The minimum scalar (0....1) height from the exisiting terrain
                 _WorldHeightMin("WorldHeightMin", Float) = 0
                 //The maximum scalar (0....1) height from the exisiting terrain
                 _WorldHeightMax("WorldHeightMax", Float) = 1
				//X-Offset on the terrain for the center of this mask
				_XOffset("X Offset", Float) = 0
				//Z-Offset on the terrain for the center of this mask
				_ZOffset("Z Offset", Float) = 0
				//Scaling along the X-axis
				_XScale("X Scale", Float) = 1
				//Scaling along the Z-axis
				_ZScale("Z Scale", Float) = 1
				//Rotation
				_Rotation("Rotation", Float) = 0
				//Flag to determine if tiling is on (=1) or off(=0)
				_Tiling("Tiling", Int) = 0



				 }

    SubShader {

        ZTest Always Cull Off ZWrite Off

        CGINCLUDE

            #include "UnityCG.cginc"
            #include "TerrainTool.cginc"
			#include "../Terrain.cginc"

            sampler2D _InputTex;
			sampler2D _BrushTex;
            sampler2D _GlobalBrushTex;
			float _MixMidPoint;
			float _Strength;
            float _WorldHeightMin;
            float _WorldHeightMax;
            float _XOffset;
			float _ZOffset;
			float _XScale;
			float _ZScale;
			float _Rotation;
			int _Tiling;
			
            float4 _MainTex_TexelSize;      // 1/width, 1/height, width, height

            struct appdata_t {
                float4 vertex : POSITION;
                float2 pcUV : TEXCOORD0;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 pcUV : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.pcUV = v.pcUV;
                return o;
            }
		ENDCG
            

         Pass    // 0 Height Mix
        {
            Name "Height Mix"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment HeightMix

            float4 HeightMix(v2f i) : SV_Target
            {
            	float2 brushUV = PaintContextUVToBrushUV(i.pcUV);
				float2 heightmapUV = PaintContextUVToHeightmapUV(i.pcUV);

                float2 UVOffset = brushUV;

				float s = sin ( _Rotation );
				float c = cos ( _Rotation );
				float2x2 rotationMatrix = float2x2( c, -s, s, c);
 
				float offsetX = .5; 
				float offsetY = .5; 
 
				float x = (i.pcUV.x - offsetX - _XOffset) / _XScale; 
				float y = (i.pcUV.y - offsetY - _ZOffset)/ _ZScale; 
 
				UVOffset = mul (float2(x, y), rotationMatrix ) + float2(offsetX, offsetY);

				if(_Tiling > 0)
				{
					if(UVOffset.x>0)
					{
						UVOffset.x %= 1;
					}
					else
					{
						UVOffset.x = 1 + (UVOffset.x % 1);
					}
					if(UVOffset.y>0)
					{
						UVOffset.y %= 1;
					}
					else
					{
						UVOffset.y = 1 + (UVOffset.y % 1);
					}
				}


				
								
                float oob = all(saturate(brushUV) == brushUV) ? 1.0f : 0.0f;
				float oob2 = all(saturate(UVOffset) == UVOffset) ? 1.0f : 0.0f;
				float inputHeight = tex2D(_InputTex, heightmapUV);
				
				float brushStrength = oob * InternalUnpackHeightmap(tex2D(_BrushTex, brushUV));
				float globalBrushStrength = oob * InternalUnpackHeightmap(tex2D(_GlobalBrushTex, brushUV));
                //float stampHeight = oob * InternalUnpackHeightmap(tex2D(_StampTex, UVOffset));

                float target = inputHeight + ((brushStrength - _MixMidPoint) * (_WorldHeightMax - _WorldHeightMin) * _Strength);
				if(oob>0)
				{
					return InternalPackHeightmap(clamp(lerp(inputHeight, target, globalBrushStrength),0.0f,0.5f));
				}
				else
				{
					return inputHeight;
				}
				//return InternalPackHeightmap(lerp(inputHeight, target, brushStrength));*/
				//return target;

            }
            ENDCG
        }

    }
    Fallback Off
}
