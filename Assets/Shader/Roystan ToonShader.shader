Shader "ToonShader/RoystanToonShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" { }
        [HDR] _AmbientColor("Ambient Color", Color) = (0.4, 0.4, 0.4, 1)
        [HDR] _SpecularColor("Specular Color", Color) = (0.9, 0.9, 0.9, 1)
        _Glossiness("Glossiness", Float) = 32
        [HDR] _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimAmount("Rim Amount", Range(0, 1)) = 0.716
        _RimThreshold("Rim Threshold", Range(0, 1)) = 0.1
    }

    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline" 
            "LightMode" = "UniversalForward"
        }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldNormal : NORMAL;
                float3 viewDir : TEXCOORD1;

                float4 shadowCoord : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _AmbientColor;
                float _Glossiness;
                half4 _SpecularColor;
                half4 _RimColor;
                float _RimAmount;
                float _RimThreshold;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.worldNormal = TransformObjectToWorldNormal(IN.normal);

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDir = GetWorldSpaceViewDir(positionWS);

                OUT.shadowCoord = TransformWorldToShadowCoord(positionWS);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 viewDir = normalize(IN.viewDir);
                half4 sample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                
                // Main 조명 가져오기
                Light mainLight = GetMainLight(IN.shadowCoord);

                // 노멀 방향과 조명 위치 내적
                float3 normal = normalize(IN.worldNormal);
                float NdotL = dot(mainLight.direction, normal);

                float shadow = mainLight.shadowAttenuation * mainLight.distanceAttenuation;

                //float lightIntensity = NdotL > 0 ? 1 : 0;
                float lightIntensity = smoothstep(0, 0.01, NdotL * shadow);

                // 조명 색 * 조명 강도
                float3 lightColor = mainLight.color * lightIntensity;

                // Specular
                float3 halfVector = normalize(mainLight.direction + viewDir);
                float NdotH = dot(normal, halfVector);

                float specularIntensity = pow(NdotH * lightIntensity, _Glossiness * _Glossiness);
                float specularIntensitySmooth = smoothstep(0.005, 0.01, specularIntensity);
                float4 specular = specularIntensitySmooth * _SpecularColor;

                // 정면에 가까울수록 0이 되고 가장자리일수록 1에 가까워짐
                // -> 정면일수록 rim이 어두워지고  가장자리일수록 밝아짐
                float rimDot = 1 - dot(viewDir, normal);
                float rimMask = rimDot * pow(NdotL, _RimThreshold);
                float rimIntensity = smoothstep(_RimAmount - 0.01, _RimAmount + 0.01, rimMask);
                float4 rim = rimIntensity * _RimColor;
                

                // LightColor
                float3 lighting = lightColor + _AmbientColor + specular + rim;

                // 최종 색상 값
                float3 finalColor = sample.rgb * _BaseColor * lighting;
                float finalAlpha = sample.a * _BaseColor.a;

                return float4(finalColor, finalAlpha);
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            // 그림자 방향 변수 (URP 내부 사용)
            float3 _LightDirection;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                // 1. 월드 좌표 및 노멀 변환
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);

                // 2. [에러 해결] GetShadowPositionHClip 대신 명시적 계산 사용
                // ApplyShadowBias 함수가 Shadow Acne(자글거림)를 방지해줍니다.
                float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

                // 3. 만약 위 코드로도 부족하다면, URP의 기본 그림자 처리 함수 사용
                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
                #endif

                OUT.positionCS = positionCS;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
