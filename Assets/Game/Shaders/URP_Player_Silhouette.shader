Shader "Custom/URP_Player_Silhouette"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _SilhouetteColor ("Silhouette Color", Color) = (0.5, 0.5, 0.5, 0.5) // Цвет тени
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent+100" // Рисуем строго после деревьев
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType"="Plane"
        }

        Cull Off
        ZWrite Off

        // --- ПРОХОД 1: Обычный спрайт ---
        Pass
        {
            Name "Normal"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            // Рисуем, если мы ПЕРЕД деревом (Z больше или равен)
            // (Поскольку ты сказал, что ЗА деревом Z меньше, значит ПЕРЕД деревом Z больше)
            ZTest GEqual 
            
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half4 _Color;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;
                c.rgb *= c.a;
                return c;
            }
            ENDHLSL
        }

        // --- ПРОХОД 2: Силуэт ---
        Pass
        {
            Name "Silhouette"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            // Рисуем, если мы ЗА деревом (Z меньше)
            ZTest Less 
            
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half4 _SilhouetteColor;

            Varyings vert(Attributes IN) {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target {
                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                if (tex.a < 0.1) discard;

                half4 finalColor = _SilhouetteColor;
                finalColor.a *= tex.a;
                finalColor.rgb *= finalColor.a;
                return finalColor;
            }
            ENDHLSL
        }
    }
}