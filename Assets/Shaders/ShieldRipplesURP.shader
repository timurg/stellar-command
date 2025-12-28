Shader "Custom/ShieldRipplesURP"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color   ("Tint", Color) = (0.25,0.75,1,0.25)

        _EdgePower ("Edge Power", Range(0.1, 8)) = 2.0
        _EdgeBoost ("Edge Boost", Range(0, 5)) = 1.0

        _Speed ("Wave Speed (UV/sec)", Range(0, 10)) = 2.0
        _Freq  ("Wave Frequency", Range(0, 200)) = 60.0
        _Width ("Ring Width", Range(0.001, 0.2)) = 0.03
        _Decay ("Decay", Range(0.1, 20)) = 5.0

        _Intensity ("Overall Intensity", Range(0, 5)) = 1.0
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
                float4 _MainTex_ST;
                float4 _Color;

                float _EdgePower;
                float _EdgeBoost;

                float _Speed;
                float _Freq;
                float _Width;
                float _Decay;

                float _Intensity;

                // 16 импульсов: xy = UV (0..1), z = startTime, w = amplitude
                float4 _Impulses[16];
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
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            // Гауссово кольцо по фронту: exp(-((d-r)/w)^2)
            float Ring(float d, float r, float w)
            {
                float x = (d - r) / max(w, 1e-5);
                return exp(-x*x);
            }

            float WaveContribution(float2 uv, float2 p, float t, float t0, float amp)
            {
                float dt = t - t0;
                if (dt <= 0.0) return 0.0;

                float d = distance(uv, p);
                float r = dt * _Speed;

                float ring = Ring(d, r, _Width);
                float osc  = sin((d - r) * _Freq);         // волновая “зебра”
                float fade = exp(-dt * _Decay);            // затухание

                return ring * osc * fade * amp;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y;
                float2 uv = IN.uv;

                half4 baseCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                baseCol *= (half4)_Color;
                baseCol *= IN.color; // SpriteRenderer tint

                // Edge/Fresnel-like: по краям сильнее
                float2 c = uv - 0.5;
                float edge = pow(saturate(length(c) * 2.0), _EdgePower); // 0 в центре, 1 по краям
                float edgeBoost = 1.0 + edge * _EdgeBoost;

                float sum = 0.0;
                UNITY_LOOP
                for (int i = 0; i < 16; i++)
                {
                    float4 imp = _Impulses[i];
                    // imp.w == 0 => слот пуст
                    sum += WaveContribution(uv, imp.xy, t, imp.z, imp.w);
                }

                // Превращаем синусоидальные вкладки в читаемую подсветку:
                // 1) абсолютное значение дает “белые” волны без смены знака
                float waves = abs(sum) * _Intensity;

                // Альфа и цвет щита усиливаются локально
                half3 col = baseCol.rgb * (1.0 + waves * edgeBoost);
                half  a   = saturate(baseCol.a + waves * 0.35);

                return half4(col, a);
            }
            ENDHLSL
        }
    }
}
