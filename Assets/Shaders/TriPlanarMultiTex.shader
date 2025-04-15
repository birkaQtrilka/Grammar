Shader "Triplanar/MultiTex" {
	Properties{
		_MainTexX("Albedo X (RGB)", 2D) = "white" {}
		_MainTexY("Albedo Y (RGB)", 2D) = "white" {}
		_MainTexZ("Albedo Z (RGB)", 2D) = "white" {}

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

			sampler2D _MainTexX;
			sampler2D _MainTexY;
			sampler2D _MainTexZ;

			half _Glossiness;
			half _Metallic;

			half _TextureScale;
			half _Sharpness;

			struct Input {
				float3 worldPos;
				float3 worldNormal;	INTERNAL_DATA
			};

			void surf(Input IN, inout SurfaceOutputStandard o) {
				// Use absolute values of the world normal vector as start point for the blend factors:
				float3 blend = abs(IN.worldNormal);

				// Higher power leads to sharper texture boundaries & less blending:
				blend = pow(blend, _Sharpness);

				// For a correct blend between textures, set the sum of blend factors to 1:
				blend /= (blend.x + blend.y + blend.z); 

				// calculate triplanar uvs:
				float2 uvX = IN.worldPos.zy / _TextureScale;
				float2 uvY = IN.worldPos.xz / _TextureScale;
				float2 uvZ = IN.worldPos.xy / _TextureScale;

				// Simple albedo textures:
				fixed4 colX = tex2D(_MainTexX, uvX);
				fixed4 colY = tex2D(_MainTexY, uvY);
				fixed4 colZ = tex2D(_MainTexZ, uvZ);

				// Final color: a blend between the three textures:
				fixed4 col = colX * blend.x + colY * blend.y + colZ * blend.z;

				// set surface output properties:
				o.Albedo = col.rgb;
				o.Metallic = _Metallic;
				o.Smoothness = _Glossiness;
			}
			ENDCG
		}
		FallBack "Diffuse"
}