Shader "Custom/KenneyPathColor"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _GrassColor ("Grass Color", Color) = (0.533, 0.694, 0.306, 1)
        _PathColor ("Path Color", Color) = (0.770, 0.434, 0.303, 1)
        _Threshold ("Threshold", Range(0, 1)) = 0.2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _GrassColor;
                float4 _PathColor;
                float _Threshold;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                
                // Kenney Palette Colors (Bilinear sampled values from colormap.png)
                float3 paletteGreen = float3(0.129, 0.541, 0.420);
                float3 paletteBrown = float3(0.770, 0.434, 0.303);

                float distGreen = distance(texColor.rgb, paletteGreen);
                float distBrown = distance(texColor.rgb, paletteBrown);

                if (distGreen < _Threshold)
                    return _GrassColor;
                
                // We keep brown as is or tint it if needed. 
                // Using _PathColor allows the user to change path color too.
                if (distBrown < _Threshold)
                    return _PathColor;

                return texColor;
            }
            ENDHLSL
        }
    }
}