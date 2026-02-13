Shader "Soyya/NeonGlow"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 0.2, 0.6, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 10)) = 3
        _FresnelPower ("Fresnel Power", Range(0.1, 5)) = 2
        _PulseSpeed ("Pulse Speed", Range(0, 5)) = 1.5
        _PulseMin ("Pulse Min", Range(0, 1)) = 0.5
        _FlickerAmount ("Flicker Amount", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Blend One One
        ZWrite Off
        Cull Off

        Pass
        {
            Name "NeonGlow"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            float4 _BaseColor;
            float _GlowIntensity;
            float _FresnelPower;
            float _PulseSpeed;
            float _PulseMin;
            float _FlickerAmount;

            // 疑似ランダムフリッカー
            float random(float seed)
            {
                return frac(sin(seed * 12.9898) * 43758.5453);
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                output.viewDirWS = normalize(GetCameraPositionWS() - worldPos);
                output.uv = input.uv;
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float3 normal = normalize(input.normalWS);
                float3 viewDir = normalize(input.viewDirWS);

                // フレネル効果（エッジが明るく光る）
                float fresnel = pow(1.0 - saturate(dot(normal, viewDir)), _FresnelPower);

                // パルスアニメーション
                float pulse = lerp(_PulseMin, 1.0, (sin(_Time.y * _PulseSpeed * 3.14159) * 0.5 + 0.5));

                // ランダムフリッカー
                float flickerSeed = floor(_Time.y * 15.0);
                float flicker = 1.0 - _FlickerAmount * step(0.9, random(flickerSeed));

                // 最終カラー
                float intensity = (0.3 + fresnel * 0.7) * _GlowIntensity * pulse * flicker;
                float3 col = _BaseColor.rgb * intensity;

                return float4(col, intensity * 0.5);
            }
            ENDHLSL
        }
    }
}
