    Shader "Hidden/Gaia/FilterDistanceMask" {

    Properties {
				//The input texture
				_InputTex ("Input Texture", any) = "" {}				
				//1-pixel high distance mask texture representing an animation curve
				_DistanceMaskTex ("Distance Mask Texture", any) = "" {}
				//Int to determine the axis mode, 0 = XZ, 1= X, 2=Z
				_AxisMode("Axis Mode", Int) = 0
			    //X-Offset on the terrain for the center of this distance mask
				_XOffset("X Offset", Float) = 0
				//Z-Offset on the terrain for the center of this distance mask
				_ZOffset("Z Offset", Float) = 0
				//1-pixel height transform texture representing an animation curve
				_HeightTransformTex ("Height Transform Texture", any) = "" {}
				//Scaling along the X-axis
				_XScale("X Scale", Float) = 1
				//Scaling along the Z-axis
				_ZScale("Z Scale", Float) = 1
				//Rotation
				_Rotation("Rotation", Float) = 0
				//Flag to determine if tiling is on (=1) or off(=0)
				_Tiling("Tiling", Int) = 0
				//Extend for the square shaped XZ mask
				_SquareRoundness("SquareRoundness", Float) = 0.25
				 }

    SubShader {

        ZTest Always Cull Off ZWrite Off

        CGINCLUDE

            #include "UnityCG.cginc"
            #include "TerrainTool.cginc"
			#include "../Terrain.cginc"

            sampler2D _InputTex;
			sampler2D _DistanceMaskTex;
			sampler2D _HeightTransformTex;
			int _AxisMode;
			float _XOffset;
			float _ZOffset;
			float _XScale;
			float _ZScale;
			float _Rotation;
			int _Tiling;
			float _SquareRoundness;
			
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

			float GetFilter(v2f i)
			{
				float2 UVOffset = i.pcUV;

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
				else
				{
					//for the XZ-Squared AxisMode we cut off when out of bounds, does not work well with the square formula otherwise.
					if (_AxisMode == 3)
					{
						if(UVOffset.x > 1.0f || UVOffset.x < 0.0f || UVOffset.y > 1.0f || UVOffset.y < 0.0f)
						{
							return 0.0f;
						}
					}
				}

				float filter = 0.0f;

				if (_AxisMode == 0)
				{
					//X-Z-Circle
					filter = InternalUnpackHeightmap(tex2D(_DistanceMaskTex, smoothstep(0, 0.5f, distance(UVOffset, float2(0.5f, 0.5f)))));
				}
				else if (_AxisMode == 1)
				{
					//X-Axis
					filter = InternalUnpackHeightmap(tex2D(_DistanceMaskTex, float2(UVOffset.x, 0)));
				}
				else if (_AxisMode == 2)
				{
					//Z-Axis
					filter = InternalUnpackHeightmap(tex2D(_DistanceMaskTex, float2(UVOffset.y, 0)));
				}
				else
				{
					//X-Z-Square
					 UVOffset *=  1.0f - UVOffset.yx;   
					float square = pow(UVOffset.x*UVOffset.y, _SquareRoundness); 
					filter = InternalUnpackHeightmap(tex2D(_DistanceMaskTex, float2(1.0f-(square * 2.0f), 0)));
				}
				return filter;
			}
		ENDCG

         Pass    // 0 Multiply
        {
            Name "Distance Mask Multiply"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment DistanceMaskMultiply

            float4 DistanceMaskMultiply(v2f i) : SV_Target
            {
				float height = InternalUnpackHeightmap(tex2D(_InputTex, i.pcUV));
				float filter = GetFilter(i);
				float transformedHeight = InternalUnpackHeightmap(tex2D(_HeightTransformTex, filter));
				float result = height * transformedHeight;
				return InternalPackHeightmap(result);
            }
            ENDCG
        }
	
		
		Pass    // 1 Distance Mask Greater Than
        {
            Name "Distance Mask Greater Than"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment DistanceMaskGreaterThan

            float4 DistanceMaskGreaterThan(v2f i) : SV_Target
            {
				float height = InternalUnpackHeightmap(tex2D(_InputTex, i.pcUV));
				float filter = GetFilter(i);
				float transformedHeight = InternalUnpackHeightmap(tex2D(_HeightTransformTex, filter));
				float result = height;
				if (transformedHeight > height)
				{
					result = transformedHeight;
				}
				return InternalPackHeightmap(result);
            }
            ENDCG
        }

	
	   Pass    // 2 Distance Mask Smaller Than
		{
			Name "Distance Mask Smaller Than"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment DistanceMaskSmallerThan

			float4 DistanceMaskSmallerThan(v2f i) : SV_Target
			{
				float height = InternalUnpackHeightmap(tex2D(_InputTex, i.pcUV));
				float filter = GetFilter(i);
				float transformedHeight = InternalUnpackHeightmap(tex2D(_HeightTransformTex, filter));
				float result = height;
				if (transformedHeight < height)
				{
					result = transformedHeight;
				}
				return InternalPackHeightmap(result);
			}
			ENDCG
		}

		Pass    // 3 Distance Mask Add
		{
			Name "Distance Mask Add"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment DistanceMaskAdd

			float4 DistanceMaskAdd(v2f i) : SV_Target
			{
				float height = InternalUnpackHeightmap(tex2D(_InputTex, i.pcUV));
				float filter = GetFilter(i);
				float transformedHeight =InternalUnpackHeightmap(tex2D(_HeightTransformTex, filter));
				float result = height + transformedHeight;
				return InternalPackHeightmap(result);
			}
			ENDCG
		}

		Pass    // 4 Distance Mask Subtract
		{
			Name "Distance Mask Subtract"

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment DistanceMaskSubtract

			float4 DistanceMaskSubtract(v2f i) : SV_Target
			{
				float height = InternalUnpackHeightmap(tex2D(_InputTex, i.pcUV));
				float filter = GetFilter(i);
				float transformedHeight =InternalUnpackHeightmap(tex2D(_HeightTransformTex, filter));
				float result = height - transformedHeight;
				return InternalPackHeightmap(result);
			}
			ENDCG
		}

    }
    Fallback Off
}
