Shader "Custom/StarfieldShader"
{
    Properties
    {
        _StarDensity ("Star Density", Float) = 0.05
        _StarSizeMin ("Star Size Min", Float) = 0.01
        _StarSizeMax ("Star Size Max", Float) = 0.1
        _CameraOffset ("Camera Offset", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _StarDensity;
            float _StarSizeMin;
            float _StarSizeMax;
            float4 _CameraOffset;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };


            // Более плавное смещение и псевдослучайное распределение
            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // Меньший множитель для плавности
                float2 offset = (_WorldSpaceCameraPos.xz + _CameraOffset.xz) * 0.01;
                o.uv = v.uv + offset;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = frac(i.uv);
                float starSeed = hash(uv * 100.0);
                if (starSeed > 1.0 - _StarDensity)
                {
                    float size = lerp(_StarSizeMin, _StarSizeMax, starSeed);
                    return fixed4(1, 1, 1, size * 0.7);
                }
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}