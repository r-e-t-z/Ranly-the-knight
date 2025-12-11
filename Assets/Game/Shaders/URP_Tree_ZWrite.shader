Shader "Custom/URP_Tree_ZWrite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="TransparentCutout" 
            "Queue"="AlphaTest" 
            "RenderPipeline" = "UniversalPipeline"
            "PreviewType"="Plane"
        }

        // ВКЛЮЧАЕМ запись в глубину
        ZWrite On
        Cull Off

        Pass
        {
            Tags { "LightMode" = "SRPDefaultUnlit" } // Работает без света

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half4 _Color;
            half _Cutoff;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half4 finalColor = texColor * IN.color;
                
                // Вырезаем прозрачность
                clip(finalColor.a - _Cutoff);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}