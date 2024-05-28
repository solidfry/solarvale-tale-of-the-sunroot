// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Hidden/Gaia/StampPreview"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.CompareFunction)] _zTestMode ("Z Test mode", Int) = 4 //4 = LEqual
		_positiveHeightColor ("Positive Height Color",Color) = (.34, .85, .92, 1)
		_negativeHeightColor ("Negative Height Color",Color) = (.34, .85, .92, 1)
		_seaLevelTintColor ("Sea Level Tint Color",Color) = (.34, .85, .92, 1)
		_normalMapColorPower ("Normal Map Color Power",Float) = 0.3
		_seaLevel ("Sea Level",Float) = 0
        _usePartialPreview("Use Partial Preview", Int) = 1
        _partialPreviewCenter("Partial Preview Center", Vector) = (0.5,0.5,0.0,0.0)
        _partialPreviewRadius("Partial Preview Radius", Float) = 0.5
         _worldDesignerMinHeight("World Designer Min Height", Float) = 0.0
        _worldDesignerMaxHeight("World Designer Max Height", Float) = 100.0

        _worldDesignerStampVisColor0 ("World Designer Stamp Vis Color 0",Color) = (.34, .85, .92, 1)
        _worldDesignerStampVisColor1 ("World Designer Stamp Vis Color 1",Color) = (.34, .85, .92, 1)
        _worldDesignerStampVisColor2 ("World Designer Stamp Vis Color 2",Color) = (.34, .85, .92, 1)
        _worldDesignerStampVisColor3 ("World Designer Stamp Vis Color 3",Color) = (.34, .85, .92, 1)
        _worldDesignerStampVisColor4 ("World Designer Stamp Vis Color 4",Color) = (.34, .85, .92, 1)

        _worldDesignerStampVisCurveTexture0("World Designer Stamp Vis Curve Texture", any) = "" {}
        _worldDesignerStampVisCurveTexture1("World Designer Stamp Vis Curve Texture", any) = "" {}
        _worldDesignerStampVisCurveTexture2("World Designer Stamp Vis Curve Texture", any) = "" {}
        _worldDesignerStampVisCurveTexture3("World Designer Stamp Vis Curve Texture", any) = "" {}
        _worldDesignerStampVisCurveTexture4("World Designer Stamp Vis Curve Texture", any) = "" {}
	}

    SubShader
    {
        ZTest [_zTestMode] Cull Back ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        CGINCLUDE
            // Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
            #pragma exclude_renderers gles

            #include "UnityCG.cginc"
            #include "TerrainPreview.cginc"
            #include "../../Terrain.cginc"

            sampler2D _BrushTex;

        ENDCG

        Pass    // 0
        {
            Name "TerrainPreviewProcedural"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct v2f {
                float4 clipPosition : SV_POSITION;
                float3 positionWorld : TEXCOORD0;
                float3 positionWorldOrig : TEXCOORD1;
                float2 pcPixels : TEXCOORD2;
                float2 brushUV : TEXCOORD3;
            };

            v2f vert(uint vid : SV_VertexID)
            {
                // build a quad mesh, with one vertex per paint context pixel (pcPixel)
                float2 pcPixels = BuildProceduralQuadMeshVertex(vid);

                // compute heightmap UV and sample heightmap
                float2 heightmapUV = PaintContextPixelsToHeightmapUV(pcPixels);
                float heightmapSample = InternalUnpackHeightmap(tex2Dlod(_Heightmap, float4(heightmapUV, 0, 0)));

                // compute brush UV
                float2 brushUV = PaintContextPixelsToBrushUV(pcPixels);

                // compute object position (in terrain space) and world position
                float3 positionObject = PaintContextPixelsToObjectPosition(pcPixels, heightmapSample);
                float3 positionWorld = TerrainObjectToWorldPosition(positionObject);

                v2f o;
                o.pcPixels = pcPixels;
                o.positionWorld = positionWorld;
                o.positionWorldOrig = positionWorld;
                o.clipPosition = UnityWorldToClipPos(positionWorld);
                o.brushUV = brushUV;
                return o;
            }


            float4 frag(v2f i) : SV_Target
            {
                float brushSample = InternalUnpackHeightmap(tex2D(_BrushTex, i.brushUV));

                // out of bounds multiplier
                float oob = all(saturate(i.brushUV) == i.brushUV) ? 1.0f : 0.0f;

                // brush outline stripe
                float stripeWidth = 2.0f;       // pixels
                float stripeLocation = 0.2f;    // at 20% alpha
                float brushStripe = Stripe(brushSample, stripeLocation, stripeWidth);

                //float4 color = float4(0.5f, 0.5f, 1.0f, 1.0f) * saturate(brushStripe + 0.5f * brushSample);
				float4 color = float4(1.0f, 1.0f, 1.0f, 1.0f) * saturate(brushStripe + 0.5f * brushSample);
                color.a = 0.6f * saturate(brushSample * 5.0f);
                return color * oob;
            }
            ENDCG
        }

        Pass    // 1
        {
            Name "TerrainPreviewProceduralDeltaNormals"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _HeightmapOrig;

			float4 _positiveHeightColor;
			float4 _negativeHeightColor;
			float4 _seaLevelTintColor;
			float _normalMapColorPower;
            int _usePartialPreview;
            float4 _partialPreviewCenter;
            float _partialPreviewRadius;

			float4 _worldDesignerStampVisColor0;
            float4 _worldDesignerStampVisColor1;
            float4 _worldDesignerStampVisColor2;
            float4 _worldDesignerStampVisColor3;
            float4 _worldDesignerStampVisColor4;

			float _seaLevel;
            float _worldDesignerMinHeight;
            float _worldDesignerMaxHeight;

            sampler2D _worldDesignerStampVisCurveTexture0;
            sampler2D _worldDesignerStampVisCurveTexture1;
            sampler2D _worldDesignerStampVisCurveTexture2;
            sampler2D _worldDesignerStampVisCurveTexture3;
            sampler2D _worldDesignerStampVisCurveTexture4;


            struct v2f {
                float4 clipPosition : SV_POSITION;
                float3 positionWorld : TEXCOORD0;
                float3 positionWorldOrig : TEXCOORD1;
                float2 pcPixels : TEXCOORD2;
                float2 brushUV : TEXCOORD3;
            };

            v2f vert(uint vid : SV_VertexID)
            {
                // build a quad mesh, with one vertex per paint context pixel (pcPixel)
                float2 pcPixels = BuildProceduralQuadMeshVertex(vid);

                // compute heightmap UV and sample heightmap
                float2 heightmapUV = PaintContextPixelsToHeightmapUV(pcPixels);
                float heightmapSample = InternalUnpackHeightmap(tex2Dlod(_Heightmap, float4(heightmapUV, 0, 0)));
                float heightmapSampleOrig = InternalUnpackHeightmap(tex2Dlod(_HeightmapOrig, float4(heightmapUV, 0, 0)));

                // compute brush UV
                float2 brushUV = PaintContextPixelsToBrushUV(pcPixels);

                // compute object position (in terrain space) and world position
                float3 positionObject = PaintContextPixelsToObjectPosition(pcPixels, heightmapSample);
                float3 positionWorld = TerrainObjectToWorldPosition(positionObject);

                float3 positionObjectOrig = PaintContextPixelsToObjectPosition(pcPixels, heightmapSampleOrig);
                float3 positionWorldOrig = TerrainObjectToWorldPosition(positionObjectOrig);

                v2f o;
                o.pcPixels = pcPixels;
                o.positionWorld = positionWorld;
                o.positionWorldOrig = positionWorldOrig;
                o.clipPosition = UnityWorldToClipPos(positionWorld);
                o.brushUV = brushUV;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float brushSample = InternalUnpackHeightmap(tex2D(_BrushTex, i.brushUV));

                // out of bounds multiplier
                float oob = all(saturate(i.brushUV) == i.brushUV) ? 1.0f : 0.0f;

                float deltaHeight = abs(i.positionWorld.y - i.positionWorldOrig.y);

                // normal based coloring
                float3 dx = ddx(i.positionWorld);
                float3 dy = ddy(i.positionWorld);
                float3 normal = normalize(cross(dy, dx));

                float4 color;
				color.r = 1.0f;
				color.g = 1.0f;
				color.b = 1.0f;
				color.a = 1.0f;

				//positive or negative color?
				if(i.positionWorld.y - i.positionWorldOrig.y>=0)
				{
					color.rgb = _positiveHeightColor.rgb;
					color.a = _positiveHeightColor.a;
				}
				else
				{
					color.rgb = _negativeHeightColor.rgb;
					color.a = _negativeHeightColor.a;
				}

                //world designer preview colors
                if(_worldDesignerMaxHeight >0.0f)
                {
                    float color0Sample = tex2D(_worldDesignerStampVisCurveTexture0,float2(smoothstep(_worldDesignerMinHeight, _worldDesignerMaxHeight, i.positionWorld.y),0));
                    float color1Sample = tex2D(_worldDesignerStampVisCurveTexture1,float2(smoothstep(_worldDesignerMinHeight, _worldDesignerMaxHeight, i.positionWorld.y),0));
                    float color2Sample = tex2D(_worldDesignerStampVisCurveTexture2,float2(smoothstep(_worldDesignerMinHeight, _worldDesignerMaxHeight, i.positionWorld.y),0));
                    float color3Sample = tex2D(_worldDesignerStampVisCurveTexture3,float2(smoothstep(_worldDesignerMinHeight, _worldDesignerMaxHeight, i.positionWorld.y),0));
                    float color4Sample = tex2D(_worldDesignerStampVisCurveTexture4,float2(smoothstep(_worldDesignerMinHeight, _worldDesignerMaxHeight, i.positionWorld.y),0));

                    color = lerp(color, _worldDesignerStampVisColor0, _worldDesignerStampVisColor0.a * color0Sample);
                    color = lerp(color, _worldDesignerStampVisColor1, _worldDesignerStampVisColor1.a * color1Sample);
                    color = lerp(color, _worldDesignerStampVisColor2, _worldDesignerStampVisColor2.a * color2Sample);
                    color = lerp(color, _worldDesignerStampVisColor3, _worldDesignerStampVisColor3.a * color3Sample);
                    color = lerp(color, _worldDesignerStampVisColor4, _worldDesignerStampVisColor4.a * color4Sample);
                }

				//apply sea level tint
				if(i.positionWorld.y<_seaLevel)
				{
					color.rgb = (color.rgb * (1-_seaLevelTintColor.a)) + (_seaLevelTintColor.rgb * _seaLevelTintColor.a);
				}
				
				//apply normal map influence
				color.rgb = color.rgb * (1-_normalMapColorPower)  + normal.xzy * _normalMapColorPower;

                //color.rgb = lerp(color.rgb, float3(1.0f, 1.0f, 1.0f), 0.3f);
                color.rgb = color.rgb * (0.1f + 0.9f * dot(0.75f, normal/1.15f));


                float partialPreviewMuliplier = 1.0f;
                if(_usePartialPreview>0)
                {
                    partialPreviewMuliplier  = 1.0f - smoothstep( _partialPreviewRadius * 0.75f, _partialPreviewRadius, distance(i.brushUV, float2(_partialPreviewCenter.x, _partialPreviewCenter.y)));
                }

                color.a = saturate(1.0f * deltaHeight) * color.a * partialPreviewMuliplier;
                return color;
            }
            ENDCG
        }

        Pass    // 2
        {
            Name "TerrainPreviewProceduralDeltaStripes"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _HeightmapOrig;

            struct v2f {
                float4 clipPosition : SV_POSITION;
                float3 positionWorld : TEXCOORD0;
                float3 positionWorldOrig : TEXCOORD1;
                float2 pcPixels : TEXCOORD2;
                float2 brushUV : TEXCOORD3;
            };

            v2f vert(uint vid : SV_VertexID)
            {
                // build a quad mesh, with one vertex per paint context pixel (pcPixel)
                float2 pcPixels = BuildProceduralQuadMeshVertex(vid);

                // compute heightmap UV and sample heightmap
                float2 heightmapUV = PaintContextPixelsToHeightmapUV(pcPixels);
                float heightmapSample = InternalUnpackHeightmap(tex2Dlod(_Heightmap, float4(heightmapUV, 0, 0)));
                float heightmapSampleOrig = InternalUnpackHeightmap(tex2Dlod(_HeightmapOrig, float4(heightmapUV, 0, 0)));

                // compute brush UV
                float2 brushUV = PaintContextPixelsToBrushUV(pcPixels);

                // compute object position (in terrain space) and world position
                float3 positionObject = PaintContextPixelsToObjectPosition(pcPixels, heightmapSample);
                float3 positionWorld = TerrainObjectToWorldPosition(positionObject);

                float3 positionObjectOrig = PaintContextPixelsToObjectPosition(pcPixels, heightmapSampleOrig);
                float3 positionWorldOrig = TerrainObjectToWorldPosition(positionObjectOrig);

                v2f o;
                o.pcPixels = pcPixels;
                o.positionWorld = positionWorld;
                o.positionWorldOrig = positionWorldOrig;
                o.clipPosition = UnityWorldToClipPos(positionWorld);
                o.brushUV = brushUV;
                return o;
            }

            float MultiStripes(in float x, in float freq1, in float freq2)
            {
                float2 derivatives = float2(ddx(x), ddy(x));
                float derivLen = length(derivatives);

                float tweak = 0.5;
                float sharpen = tweak / max(derivLen, 0.00001f);

                float triwave1 = abs(frac(x * freq1) - 0.5f) - 0.25f;
                float triwave2 = abs(frac(x * freq2) - 0.5f) - 0.25f;

                float width = 0.95f;

                float result1 = saturate((triwave1 - width * 0.25f) * sharpen / freq1 + 0.75f);
                float result2 = saturate((triwave2 - width * 0.25f) * sharpen / freq2 + 0.75f);

                return max(result1, result2);
            }

            float4 frag(v2f i) : SV_Target
            {
                float brushSample = InternalUnpackHeightmap(tex2D(_BrushTex, i.brushUV));

                // out of bounds multiplier
                float oob = all(saturate(i.brushUV) == i.brushUV) ? 1.0f : 0.0f;

                // brush outline stripe
                float stripeWidth = 1.0f;       // pixels
                float stripeLocation = 0.2f;    // at 20% alpha
                float heightStripes = MultiStripes(i.positionWorld.y + 0.25f, 0.0625f, 1.0f);
                float xStripes = MultiStripes(i.positionWorld.x, 0.03125f, 0.5f);
                float zStripes = MultiStripes(i.positionWorld.z, 0.03125f, 0.5f);

                float deltaHeight = saturate(abs(i.positionWorld.y - i.positionWorldOrig.y));
                float4 color = lerp(float4(0.5f, 0.5f, 1.0f, 1.0f), float4(0.5f, 1.0f, 0.5f, 1.0f), deltaHeight);
                return color * lerp(deltaHeight * 0.5f, (brushSample > 0.0f ? 1.0f : 0.0f), 0.5f * saturate(heightStripes + xStripes + zStripes));
            }
            ENDCG
        }
    }
    Fallback Off
}
