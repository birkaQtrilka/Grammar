Shader "Unlit/VolumetricFog"
{
    Properties
    {
        _CutoffStart("CutoffStart", float) = 5
        _CutoffEnd("CutoffEnd", float) = 7
        _Color("Color", Color) = (1,1,1,1)
        _MaxDistance("Max distance", float) = 100
        _StepSize("StepSize", Range(0.1, 20)) = 100
        _DensityMultiplier("Density Multiplier", Range(0,10)) = 1
        //_NoiseOffset("Noise Offset", float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            float _CutoffStart;
            float _CutoffEnd;
            float4 _Color;
            float _MaxDistance;
            float _StepSize;
            float _DensityMultiplier;
            //float _NoiseOffset;

            float get_density()
            {
                return _DensityMultiplier;
            }
            float InverseLerp(float a, float b, float value)
            {
                return (value - a) / (b - a);
            }
            half4 frag(Varyings IN) :   SV_Target
            {
                float depth = SampleSceneDepth(IN.texcoord);
                float3 worldPos = ComputeWorldSpacePosition(IN.texcoord, depth, UNITY_MATRIX_I_VP);
                float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, IN.texcoord);

                float3 entryPoint = _WorldSpaceCameraPos;
                float3 viewDir = worldPos - _WorldSpaceCameraPos;
                float viewLength = length(viewDir);
                float3 rayDir = normalize(viewDir);

                float pixelCoords = IN.texcoord * _BlitTexture_TexelSize.zw;
                float distLimit = min(viewLength, _MaxDistance);
                float distTraveled = 0;// = InterleavedGradientNoise
                // (
                //     pixelCoords, 
                //     (int)( _Time.y / max(HALF_EPS, unity_DeltaTime.x) ) 
                // )
                // * _NoiseOffset;

                float transmittance = 1;


                while(distTraveled < distLimit)
                {
                    float density = get_density();
                    if(density > 0)
                    {
                        transmittance *= exp(-density * _StepSize);
                    }
                    distTraveled += _StepSize;
                }

                //space effect
                float cutoff_tb = 1- clamp(InverseLerp(_CutoffStart, _CutoffEnd, worldPos.y),0,1);
                //screenEffect
                float cutoff_t = 1- clamp(InverseLerp(_CutoffStart, _CutoffEnd, entryPoint.y),0,1);
                //float cutoff_t = 1 - clamp(entryPoint.y ,_CutoffStart,_CutoffEnd) / _CutoffEnd;    
                //preventing overlap of effects
                //cutoff_t *=  cutoff_tb;
                //return col * cutoff_tb;
                //return lerp(col, _Color, (1- saturate(transmittance)) * (cutoff_t + cutoff_tb) * 0.5f);
                return lerp(col, _Color, (1- saturate(transmittance)) * (cutoff_tb + cutoff_t)*.5f * _Color.w);
                //return lerp(col, _Color, (1- saturate(transmittance)) );
                //float4 endCol = _Color * (cutoff_t + cutoff_tb) * 0.5f;
                //endCol.w = 1;
                //return lerp(col, endCol, (1- saturate(transmittance))) ;
            }
            ENDHLSL
        }
    }
}
