// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UI/Blur Modal Shader"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineThickness ("Outline Thickness", Float) = 0.1
        _Alpha ("Alpha", Range(0,1)) = 1

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
            float4 _Color;
            float4 _OutlineColor;
            float4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;
            float _UIMaskSoftnessX;
            float _UIMaskSoftnessY;
            float _Alpha;
            float _OutlineThickness;
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
                OUT.color.a = _Alpha;
                return OUT;
            }

            float4 sample_tex2d(in float2 uv, in float4 col)
            {
                const float threshould = 0.5;
                col = abs(uv.x - 0.5) > threshould || abs(uv.y - 0.5) > threshould ? 0 : col;
                return col;
            }

            float4 frag(in v2f IN) : SV_Target
            {
                //Round up the alpha color coming from the interpolator (to 1.0/256.0 steps)
                //The incoming alpha could have numerical instability, which makes it very sensible to
                //HDR color transparency blend, when it blends with the world's texture.
                const half alpha_precision = half(0xff);
                const half inv_alpha_precision = half(1.0 / alpha_precision);
                IN.color.a = round(IN.color.a * alpha_precision) * inv_alpha_precision;

                const float2 outer_uv = IN.texcoord;
                const float2 inner_uv = outer_uv * _OutlineThickness + (1 - _OutlineThickness) * 0.5;
                float4 outline_col = _OutlineColor * (tex2D(_MainTex, outer_uv) + _TextureSampleAdd);
                float4 color = IN.color * sample_tex2d(inner_uv, tex2D(_MainTex, inner_uv) + _TextureSampleAdd);

                const float2 inner_uv2 = outer_uv * (_OutlineThickness * 1.05) + (1 - _OutlineThickness * 1.05) * 0.5;
                float4 inner_col = sample_tex2d(inner_uv2, tex2D(_MainTex, inner_uv2));

                outline_col = inner_col.a < 0.01 ? outline_col : color;
                color = saturate(outline_col);

                // color = color.a < 0.1 ? outline_col : color;
                // color = outline_col;

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