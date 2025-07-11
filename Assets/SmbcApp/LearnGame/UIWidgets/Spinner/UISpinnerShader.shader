﻿// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/Spinner Shader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _Color2 ("Tint2", Color) = (1,1,1,1)
        _BackGroundColor ("BackGround Color", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
            #include "Assets/SmbcApp/LearnGame/UIWidgets/Spinner/UISpinnerShaderInclude.hlsl"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP


            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 world_position : TEXCOORD1;
                float4 mask : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _Color, _Color2;
            float4 _BackGroundColor;
            float4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            int _UIVertexColorAlwaysGammaSpace;

            half3 ui_gamma_to_linear(half3 value)
            {
                half3 low = 0.0849710 * value - 0.000163029;
                half3 high = value * (value * (value * 0.265885 + 0.736584) - 0.00980184) + 0.00319697;

                // We should be 0.5 away from any actual gamma value stored in an 8 bit channel
                const half3 split = 0.0725490; // Equals 18.5 / 255
                return value < split ? low : high;
            }

            inline bool is_gamma_space()
            {
                #ifdef UNITY_COLORSPACE_GAMMA
                return true;
                #else
                return false;
                #endif
            }

            v2f vert(in appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                float4 v_position = TransformObjectToHClip(v.vertex.xyz);
                OUT.world_position = v.vertex;
                OUT.vertex = v_position;

                float2 pixel_size = v_position.w;
                pixel_size /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));

                float4 clamped_rect = clamp(_ClipRect, -2e10, 2e10);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
                OUT.mask = float4(v.vertex.xy * 2 - clamped_rect.xy - clamped_rect.zw,
                                  0.25 / (0.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixel_size.xy)));


                if (_UIVertexColorAlwaysGammaSpace)
                {
                    if (!is_gamma_space())
                    {
                        v.color.rgb = ui_gamma_to_linear(v.color.rgb);
                    }
                }

                OUT.color = v.color * _Color;
                return OUT;
            }

            float4 spinner_color(in float2 uv, in float4 color)
            {
                uv -= 0.5;
                uv *= 1.6;
                float3 col = _BackGroundColor.rgb;
                const float time = _Time.y + 1.1;

                float a = hex_fest(col, uv, time);
                a += circle_fest(col, uv, time) * 2;
                a = saturate(a);
                float3 mul_col = lerp(_BackGroundColor.rgb, color.rgb, smoothstep(0, 0.1, a));
                return float4(col * mul_col, saturate(a + _BackGroundColor.a));
            }

            float4 frag(in v2f IN) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alpha_precision = half(0xff);
                const half inv_alpha_precision = half(1.0 / alpha_precision);
                IN.color.rgb = lerp(IN.color.rgb, _Color2.rgb, _SinTime.w * 0.5 + 0.5);
                IN.color.a = round(IN.color.a * alpha_precision) * inv_alpha_precision;

                // half4 color = IN.color * (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd);
                half4 color = spinner_color(IN.texcoord, IN.color);

                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(IN.mask.xy)) * IN.mask.zw);
                color.a *= m.x * m.y;
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                color.rgb *= color.a;

                return color;
            }
            ENDHLSL
        }
    }
}