Shader "Soyya/CyberpunkSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.01, 0.01, 0.05, 1)
        _HorizonColor ("Horizon Color", Color) = (0.05, 0.02, 0.15, 1)
        _NeonColor1 ("Neon Glow 1", Color) = (1, 0.2, 0.6, 1)
        _NeonColor2 ("Neon Glow 2", Color) = (0, 0.8, 1, 1)
        _StarDensity ("Star Density", Range(0, 1)) = 0.3
        _NeonIntensity ("Neon Intensity", Range(0, 2)) = 0.5
        _FogHeight ("Fog Height", Range(0, 0.5)) = 0.15
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 viewDir : TEXCOORD0;
            };

            float4 _TopColor;
            float4 _HorizonColor;
            float4 _NeonColor1;
            float4 _NeonColor2;
            float _StarDensity;
            float _NeonIntensity;
            float _FogHeight;

            // 疑似ランダム
            float hash(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.viewDir = input.positionOS.xyz;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 dir = normalize(input.viewDir);
                float y = dir.y;

                // 基本グラデーション（上→地平線）
                float t = saturate(y * 2.0 + 0.3);
                float3 col = lerp(_HorizonColor.rgb, _TopColor.rgb, t);

                // 地平線付近のネオンフォグ
                float fogMask = exp(-abs(y) / max(_FogHeight, 0.01));
                float neonMix = sin(dir.x * 3.0 + _Time.y * 0.1) * 0.5 + 0.5;
                float3 neonFog = lerp(_NeonColor1.rgb, _NeonColor2.rgb, neonMix) * _NeonIntensity;
                col += neonFog * fogMask * 0.3;

                // 星
                if (y > 0.1)
                {
                    float2 starUV = dir.xz / (y + 0.001) * 100.0;
                    float2 starGrid = floor(starUV);
                    float starHash = hash(starGrid);

                    if (starHash > (1.0 - _StarDensity))
                    {
                        float2 starCenter = starGrid + 0.5;
                        float dist = length(frac(starUV) - 0.5);
                        float star = exp(-dist * 15.0) * (0.5 + starHash * 0.5);
                        float twinkle = sin(_Time.y * (2.0 + starHash * 5.0)) * 0.3 + 0.7;
                        col += star * twinkle * float3(0.8, 0.85, 1.0);
                    }
                }

                // 下半球は暗く
                if (y < 0)
                {
                    col *= saturate(1.0 + y * 2.0);
                }

                return float4(col, 1.0);
            }
            ENDHLSL
        }
    }
}
