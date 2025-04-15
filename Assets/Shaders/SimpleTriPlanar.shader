Shader "URP/TriplanarSimple"
{
    Properties
    {
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Metallic("Metallic", Range(0,1)) = 0.0
        _Smoothness("Smoothness", Range(0,1)) = 0.5
        _TextureScale("Texture Scale", Range(0.1, 10)) = 1.0
        _Sharpness("Sharpness", Range(1, 10)) = 2.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 300

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionOS : TEXCOORD2;
                float3 normalOS : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _TextureScale;
                float _Sharpness;
                float _Metallic;
                float _Smoothness;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS.xyz);
                OUT.positionOS = IN.positionOS.xyz;
                OUT.normalOS = IN.normalOS.xyz;
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                float3 localPos = IN.positionOS;
                float3 localNormal = normalize(IN.normalOS);

                // Triplanar blend
                float3 blend = abs(localNormal);
                blend = pow(blend, _Sharpness);
                blend /= (blend.x + blend.y + blend.z + 1e-5);

                // Triplanar UVs
                float2 uvX = localPos.zy / _TextureScale;
                float2 uvY = localPos.xz / _TextureScale;
                float2 uvZ = localPos.xy / _TextureScale;

                float4 colX = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvX);
                float4 colY = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvY);
                float4 colZ = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uvZ);
                float4 albedo = colX * blend.x + colY * blend.y + colZ * blend.z;

                // Lighting
                float3 viewDirWS = normalize(_WorldSpaceCameraPos - IN.positionWS);
                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalize(IN.normalWS);
                inputData.viewDirectionWS = viewDirWS;
                inputData.shadowCoord = TransformWorldToShadowCoord(IN.positionWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo.rgb;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = float3(0, 0, 1); // Flat normal for now
                surfaceData.emission = float3(0, 0, 0);
                surfaceData.occlusion = 1;
                surfaceData.alpha = 1;

                return UniversalFragmentPBR(inputData, surfaceData);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
