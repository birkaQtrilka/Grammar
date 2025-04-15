Shader "Triplanar/Simple" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}

		_Glossiness("Smoothness", Range(0, 1)) = 0.5
		_Metallic("Metallic", Range(0, 1)) = 0
		_TextureScale("TextureScale", Range(0.1,10)) = 1.0

		_Sharpness("Sharpness", Range(1,10)) = 1.0
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		#include "UnityStandardUtils.cginc"

		sampler2D _MainTex;

		half _Glossiness;
		half _Metallic;

		half _TextureScale;
		half _Sharpness;

		struct Input {
			float3 worldPos;
			float3 worldNormal;	INTERNAL_DATA
		};

		void surf(Input IN, inout SurfaceOutputStandard o) {
			// Transform world position to object (local) space
			float3 localPos = mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz;

			// Convert world normal to object (local) space
			float3 localNormal = mul((float3x3)unity_WorldToObject, IN.worldNormal);

			// Use absolute values of the local normal for blending
			float3 blend = abs(localNormal);
			blend = pow(blend, _Sharpness);
			blend /= (blend.x + blend.y + blend.z + 1e-5); // small value to avoid div by zero

			// Compute triplanar UVs using localPos
			float2 uvX = localPos.zy / _TextureScale;
			float2 uvY = localPos.xz / _TextureScale;
			float2 uvZ = localPos.xy / _TextureScale;

			// Sample texture
			fixed4 colX = tex2D(_MainTex, uvX);
			fixed4 colY = tex2D(_MainTex, uvY);
			fixed4 colZ = tex2D(_MainTex, uvZ);

			// Blend based on normal direction
			fixed4 col = colX * blend.x + colY * blend.y + colZ * blend.z;

			// Output surface properties
			o.Albedo = col.rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
		}
		ENDCG
	}
	FallBack "Diffuse"
}