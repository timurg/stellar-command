Shader "Custom/ShieldRipplesOnlyWavesURP_NoST"
{
    Properties
    {
        _MainTex ("Shield Mask (Alpha)", 2D) = "white" {}
        _WaveColor ("Wave Color", Color) = (0.25,0.75,1,1)

        _Speed ("Wave Speed (UV/sec)", Range(0, 10)) = 2.0
        _Freq  ("Wave Frequency", Range(0, 200)) = 70.0
        _Width ("Ring Width", Range(0.001, 0.2)) = 0.03
        _Decay ("Decay", Range(0.1, 20)) = 7.0

        _Intensity ("Overall Intensity", Range(0, 10)) = 2.0
        _Localize ("Localize", Range(0, 200)) = 0.0

        _WaveAlpha ("Wave Alpha Boost", Range(0, 5)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "RenderPipeline"="UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _WaveColor;

                float _Speed;
                float _Freq;
                float _Width;
                float _Decay;

                float _Intensity;
                float _Localize;
                float _WaveAlpha;

                float4 _Impulses[16]; // xy UV, z startTime, w amplitude
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float4 color      : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float4 color       : COLOR;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;       // Без _ST для 2D SRP Batcher совместимости
                OUT.color = IN.color; // SpriteRenderer tint
                return OUT;
            }

            float Ring(float d, float r, float w)
            {
                float x = (d - r) / max(w, 1e-5);
                return exp(-x * x);
            }

            float WaveContribution(float2 uv, float2 p, float t, float t0, float amp)
            {
                float dt = t - t0;
                if (dt <= 0.0 || amp <= 0.0) return 0.0;

                float d = distance(uv, p);
                float r = dt * _Speed;

                float ring = Ring(d, r, _Width);

                // КРИТИЧНО: cos, чтобы на фронте (d≈r) было значение 1, а не 0.
                float osc  = cos((d - r) * _Freq);

                float fade = exp(-dt * _Decay);

                float local = (_Localize <= 0.0) ? 1.0 : exp(-d * d * _Localize);

                return ring * osc * fade * amp * local;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y;
                float2 uv = IN.uv;

                // Маска по альфе
                half4 maskTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                float mask = maskTex.a;

                // Если маска пустая — ничего не рисуем
                mask = 1.0;

                float sum = 0.0;
                UNITY_LOOP
                for (int i = 0; i < 16; i++)
                {
                    float4 imp = _Impulses[i];
                    sum += WaveContribution(uv, imp.xy, t, imp.z, imp.w);
                }

                float waves = abs(sum) * _Intensity;

                // Цвет и альфа — только от волн
                half3 rgb = (half3)_WaveColor.rgb * waves * (half3)IN.color.rgb;
                half  a   = saturate(waves * _WaveAlpha) * mask;

                return half4(rgb, a);
            }
            ENDHLSL
        }
    }
}
