Shader "Custom2D/FlowMagmaLitSprite"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}
        [HideInInspector]_MaskTex("Mask", 2D) = "white" {}
        [HideInInspector]_NormalMap("Normal Map", 2D) = "bump" {}

        _FlowTex("Flow Texture",2D) = "white"{}
        // _GlowTex("Glow Texture",2D) = "white"{}
        [Toggle]_isWhite("Color Reverse",Float) = 0
        [HDR]_GlowColor("Glow Color",Color) = (1,1,1,1)

        // _FlowColor("Flow Color",Color) = (1,1,1,1)

        // _MagmaTex("Magma Texture",2D) = "white"{}
        _MagmaDistortion("Magma Distortion",Vector) = (0,0,0,0)
        _MagmaDistortionSpeed("Magma Distortion Speed",Range(-10,10)) = 0
        // _MagmaIntensity("Magma Intensity",float) = 1
        // _MagmaVel("Magma Velocity",Vector) = (0,0,0,0)

        // _OutlineSize("Outline Size",float) = 0.2
        // _OutlineColor("Outline Color",Color) = (1,1,1,1)

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    HLSLINCLUDE
    
    
    ENDHLSL

    SubShader
    {
        Tags {"Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex CombinedShapeLightVertex
            #pragma fragment CombinedShapeLightFragment

            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_0 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_1 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_2 __
            #pragma multi_compile USE_SHAPE_LIGHT_TYPE_3 __
            #pragma multi_compile _ DEBUG_DISPLAY

            #pragma shader_feature _ISWHITE_ON

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2  uv          : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS  : SV_POSITION;
                half4   color       : COLOR;
                float2  uv          : TEXCOORD0;
                half2   lightingUV  : TEXCOORD1;
                #if defined(DEBUG_DISPLAY)
                float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/LightingUtility.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            TEXTURE2D(_FlowTex);
            SAMPLER(sampler_FlowTex);
            TEXTURE2D(_MagmaTex);
            SAMPLER(sampler_MagmaTex);

            TEXTURE2D(_GlowTex);
            SAMPLER(sampler_GlowTex);

            half4 _GlowColor;
            half4 _MagmaDistortion;
            half _MagmaDistortionSpeed;
            half4 _MagmaVel;
            half4 _FlowColor;
            half4 _MainTex_ST;
            half _MagmaIntensity;

            half4 _OutlineColor;
            half _OutlineSize;

            #if USE_SHAPE_LIGHT_TYPE_0
            SHAPE_LIGHT(0)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_1
            SHAPE_LIGHT(1)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_2
            SHAPE_LIGHT(2)
            #endif

            #if USE_SHAPE_LIGHT_TYPE_3
            SHAPE_LIGHT(3)
            #endif

            Varyings CombinedShapeLightVertex(Attributes v)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(v.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.lightingUV = half2(ComputeScreenPos(o.positionCS / o.positionCS.w).xy);

                o.color = v.color;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/CombinedShapeLightShared.hlsl"

            float4 OutLine(float2 uv, float value, float4 color)
            {

                value*=0.01;
                float4 mainColor = SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex, uv + float2(-value, value))
                + SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex,  uv + float2(value, -value))
                + SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex,  uv + float2(value, value))
                + SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex,  uv - float2(value, value));
                mainColor.rgb = color;

                float4 addcolor = SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex, uv);
                return addcolor;
                if (mainColor.a > 0.40) { mainColor = color; }
                if (addcolor.a > 0.40) { mainColor.a = 0; }
                return mainColor;
            }
            half2 CalDistortionUV(float2 p, float WaveX,float WaveY, float DistanceX, float DistanceY, float Speed)
            {
                Speed *=_Time*100;
                p.x= p.x+sin(p.y*WaveX + Speed)*DistanceX*0.05;
                p.y= p.y+cos(p.x*WaveY + Speed)*DistanceY*0.05;
                return p;
            }
            half4 CombinedShapeLightFragment(Varyings i) : SV_Target
            {

                // const half4 main = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                // const half4 mask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv);
                // SurfaceData2D surfaceData;
                // InputData2D inputData;

                // InitializeSurfaceData(main.rgb, main.a, mask, surfaceData);
                // InitializeInputData(i.uv, i.lightingUV, inputData);

                // half4 color = CombinedShapeLightShared(surfaceData, inputData);

                half4 color = 0;
                half2 distortionUV = CalDistortionUV(i.uv,_MagmaDistortion.x,_MagmaDistortion.y,
                _MagmaDistortion.z,_MagmaDistortion.w,_MagmaDistortionSpeed);
                half flow = SAMPLE_TEXTURE2D(_FlowTex,sampler_FlowTex,distortionUV).r;
                
                half4 base = half4(1,1,1,1);
                #ifdef _ISWHITE_ON
                flow = 1 - flow;
                half4 glow = _GlowColor * (abs(sin(_Time.y)) + 0.5);
                return flow * glow + (1 - flow) * float4(0,0,0,1); 

                #else
                half4 glow = _GlowColor * (abs(sin(_Time.y)) + 0.5);
                return flow * glow;
                #endif


                // // color = flow.r * _FlowColor;
                // half4 magma = SAMPLE_TEXTURE2D(_MagmaTex,sampler_MagmaTex,distortionUV);
                // magma = half4(magma.r,magma.r,magma.r,magma.r) * _MagmaIntensity;
                // color = flow.r * magma;

                // half lineColor = SAMPLE_TEXTURE2D(_GlowTex,sampler_GlowTex,i.uv).r;
                // // return color;
                // return lineColor * _GlowColor +  color;


                // return color ;

                // return CombinedShapeLightShared(surfaceData, inputData);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "NormalsRendering"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex NormalsRenderingVertex
            #pragma fragment NormalsRenderingFragment

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                float4 tangent      : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                half4   color           : COLOR;
                float2  uv              : TEXCOORD0;
                half3   normalWS        : TEXCOORD1;
                half3   tangentWS       : TEXCOORD2;
                half3   bitangentWS     : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            half4 _NormalMap_ST;  // Is this the right way to do this?

            Varyings NormalsRenderingVertex(Attributes attributes)
            {
                
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                o.uv = TRANSFORM_TEX(attributes.uv, _NormalMap);
                o.color = attributes.color;
                o.normalWS = -GetViewForwardDir();
                o.tangentWS = TransformObjectToWorldDir(attributes.tangent.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * attributes.tangent.w;
                return o;
            }

            #include "Packages/com.unity.render-pipelines.universal/Shaders/2D/Include/NormalsRenderingShared.hlsl"

            half4 NormalsRenderingFragment(Varyings i) : SV_Target
            {
                const half4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                const half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv));

                return NormalsRenderingShared(mainTex, normalTS, i.tangentWS.xyz, i.bitangentWS.xyz, i.normalWS.xyz);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" "Queue"="Transparent" "RenderType"="Transparent"}

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4  positionCS      : SV_POSITION;
                float4  color           : COLOR;
                float2  uv              : TEXCOORD0;
                #if defined(DEBUG_DISPLAY)
                float3  positionWS  : TEXCOORD2;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            Varyings UnlitVertex(Attributes attributes)
            {
                Varyings o = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(attributes);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.positionCS = TransformObjectToHClip(attributes.positionOS);
                #if defined(DEBUG_DISPLAY)
                o.positionWS = TransformObjectToWorld(v.positionOS);
                #endif
                o.uv = TRANSFORM_TEX(attributes.uv, _MainTex);
                o.color = attributes.color;
                return o;
            }

            float4 UnlitFragment(Varyings i) : SV_Target
            {

                float4 mainTex = i.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);


                #if defined(DEBUG_DISPLAY)
                SurfaceData2D surfaceData;
                InputData2D inputData;
                half4 debugColor = 0;

                InitializeSurfaceData(mainTex.rgb, mainTex.a, surfaceData);
                InitializeInputData(i.uv, inputData);
                SETUP_DEBUG_DATA_2D(inputData, i.positionWS);

                if(CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                {
                    return debugColor;
                }
                #endif

                return mainTex;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
