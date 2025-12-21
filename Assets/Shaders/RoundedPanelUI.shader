Shader "Custom/RoundedPanelUI"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {} // Для совместимости с UI
        _MainColor ("Fill Color", Color) = (1,1,1,0.5) // Полупрозрачный цвет заполнения
        _OutlineColor ("Outline Color", Color) = (0,0,0,1) // Базовый цвет контура
        _GlowColor ("Glow Color", Color) = (0,0.5,1,1) // Цвет свечения (как лазерный меч)
        _OutlineWidth ("Outline Width", Range(0,0.1)) = 0.01 // Базовая толщина контура
        _Radius ("Corner Radius", Range(0,0.5)) = 0.1 // Радиус закругления
        _Aspect ("Aspect Ratio (Width/Height)", Float) = 1.0 // Аспект для корректного закругления
        
        // Анимация
        _PulseSpeed ("Pulse Speed", Range(0.1, 10)) = 2.0 // Скорость пульсации
        _PulseIntensity ("Pulse Intensity", Range(0,1)) = 0.5 // Интенсивность пульсации (0 - без анимации)
        _GlowStrength ("Glow Strength", Range(0,1)) = 0.5 // Сила свечения
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                half2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _MainColor;
            fixed4 _OutlineColor;
            fixed4 _GlowColor;
            float _OutlineWidth;
            float _Radius;
            float _Aspect;
            float _PulseSpeed;
            float _PulseIntensity;
            float _GlowStrength;
            float4 _ClipRect; // Для UI-клиппинга

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;
                return OUT;
            }

            // Функция для SDF rounded rectangle
            float roundedRectSDF(float2 pos, float2 box, float radius)
            {
                float2 q = abs(pos) - box + radius;
                return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - radius;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // UV от 0 до 1
                half2 uv = IN.texcoord;
                // Центрируем UV в [ -0.5, 0.5 ]
                half2 pos = uv - 0.5;

                // Базовый halfSize
                float2 halfSize = float2(0.5, 0.5);

                // Корректировка для аспекта: нормализуем к "квадратному" пространству
                float normAspect = max(_Aspect, 1.0 / _Aspect); // Для расчёта мин. стороны
                float radius = _Radius / normAspect; // Масштабируем радиус относительно мин. стороны

                if (_Aspect > 1.0) {
                    pos.y *= _Aspect;
                    halfSize.y *= _Aspect;
                } else {
                    pos.x /= _Aspect;
                    halfSize.x /= _Aspect;
                }

                // Пульсация: синусоида по времени
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5; // 0..1
                float animatedWidth = _OutlineWidth * (1.0 + pulse * _PulseIntensity * 0.2); // Лёгкая пульсация ширины
                float animatedIntensity = 1.0 + pulse * _PulseIntensity; // Пульсация яркости

                // Half-size бокса для внутреннего заполнения (теперь с учётом halfSize)
                float2 innerBox = halfSize - radius;

                // SDF для внутреннего заполнения
                float innerDist = roundedRectSDF(pos, innerBox, radius);

                // Half-size и радиус для внешнего контура
                float2 outerBox = innerBox + animatedWidth;
                float outerRadius = radius + animatedWidth;
                float outerDist = roundedRectSDF(pos, outerBox, outerRadius);

                // Для glow: более широкий, размытый слой
                float glowWidth = animatedWidth * 2.0; // Шире контура
                float2 glowBox = innerBox + glowWidth;
                float glowRadius = radius + glowWidth;
                float glowDist = roundedRectSDF(pos, glowBox, glowRadius);

                // Антиалиасинг (динамический для лучшего качества)
                float aa = fwidth(length(pos)) * 0.5;

                // Альфа для заполнения
                float fillAlpha = 1.0 - smoothstep(-aa, aa, innerDist);

                // Альфа для внешнего контура
                float outerAlpha = 1.0 - smoothstep(-aa, aa, outerDist);
                float outlineAlpha = max(0.0, outerAlpha - fillAlpha);

                // Альфа для glow (размытый, внешний)
                float glowAlpha = (1.0 - smoothstep(-aa * 10.0, aa * 10.0, glowDist)) * (1.0 - outerAlpha) * _GlowStrength * (0.5 + pulse * 0.5);

                // Комбинируем цвета
                fixed4 fillColor = _MainColor * fillAlpha;
                fixed4 outlineColor = _OutlineColor * outlineAlpha * animatedIntensity;
                fixed4 glowColor = _GlowColor * glowAlpha * animatedIntensity;

                // Финальный цвет: заполнение + контур + glow
                fixed4 finalColor = fillColor + outlineColor + glowColor;
                finalColor.rgb *= finalColor.a; // Premultiply alpha

                // Клиппинг для UI
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

                return finalColor;
            }
            ENDCG
        }
    }
}