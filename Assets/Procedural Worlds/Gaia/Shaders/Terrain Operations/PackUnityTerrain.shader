    Shader "Hidden/Gaia/PackUnityTerrain" {

    Properties {
				//The input texture
				_MainTex ("Input Texture", any) = "" {}				
				 }

    SubShader {

        ZTest Always Cull Off ZWrite Off

        CGINCLUDE

            #include "UnityCG.cginc"
            #include "TerrainTool.cginc"

            sampler2D _MainTex;
			
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
            

        Pass
        {
            Name "PackUnityTerrain"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            float4 frag(v2f i) : SV_Target
            {
                float4 source = tex2D(_MainTex, float2(i.pcUV));
				return PackHeightmap(source.r);
            }
            ENDCG
        }
    }
    Fallback Off
}
