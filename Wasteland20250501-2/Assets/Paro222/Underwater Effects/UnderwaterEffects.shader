Shader "Paro222/UnderwaterEffects"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _color ("Water Color", Color) = (0, 0.5, 0.8, 1)
        _FogDensity ("Fog Density", Range(0, 10)) = 1
        _alpha ("Alpha", Range(0, 1)) = 0.5
        _refraction ("Refraction", Range(0, 1)) = 0.1
        _normalUV ("Normal UV", Vector) = (1, 1, 0.2, 0.1)
    }
    
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZTest Always
        ZWrite Off
        Cull Off
        
        Pass
        {
            Name "UnderwaterPass"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            TEXTURE2D(_NormalMap);
            SAMPLER(sampler_NormalMap);
            
            float4 _color;
            float _FogDensity;
            float _alpha;
            float _refraction;
            float4 _normalUV;
            
            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // 采样法线贴图
                float3 normalMap = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, 
                                   input.uv * _normalUV.xy + _normalUV.zw * _Time.y));
                
                // 扭曲UV坐标
                float2 distortedUV = input.uv + normalMap.xy * _refraction * 0.01;
                
                // 采样场景颜色（从不透明纹理，而非_MainTex）
                half4 sceneColor = SampleSceneColor(distortedUV);
                
                // 采样场景深度
                float sceneDepth = SampleSceneDepth(distortedUV);
                float linearDepth = LinearEyeDepth(sceneDepth, _ZBufferParams);
                
                // 计算水下雾效
                float fogFactor = 1.0 - exp(-_FogDensity * linearDepth);
                
                // 混合水下颜色
                return lerp(sceneColor, _color, saturate(fogFactor + _alpha));
            }
            ENDHLSL
        }
    }
}