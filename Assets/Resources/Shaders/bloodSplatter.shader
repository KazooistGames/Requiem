// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Projector' with 'unity_Projector'
// Upgrade NOTE: replaced '_ProjectorClip' with 'unity_ProjectorClip'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/bloodSplatter"
{
	Properties{
			[NoScaleOffset]_ShadowTex("Cookie", 2D) = "gray" {}
			[NoScaleOffset]_ShadowTex1("Cookie", 2D) = "gray" {}
			[NoScaleOffset]_ShadowTex2("Cookie", 2D) = "gray" {}
			_Radius("Radius", Range(0.1, 10)) = 1
			_PlayerPosition("PlayerPosition", Vector) = (0,0,0,0)
	}
	Subshader
	{

		//Tags {"Queue" = "Transparent"}
		Tags { "RenderType" = "Opaque" }
		Pass {
			ZWrite Off
			ColorMask RGB
			Blend SrcAlpha One
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			struct v2f {
				float4 pos : SV_POSITION;
				float4 uvShadow : TEXCOORD0;
				float4 worldPos : TEXCOORD1;
				float3 worldNormal : TEXCOORD2;
				fixed4 light : COLOR0;
			};

			float4x4 unity_Projector;
			fixed4 _LightColor0;

			v2f vert(appdata_base v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uvShadow = mul(unity_Projector, v.vertex);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.worldNormal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
				o.light = _LightColor0;
				return o;
			}

			sampler2D _ShadowTex;
			sampler2D _ShadowTex1;
			sampler2D _ShadowTex2;
			fixed4 _Color;
			fixed4 _PlayerPosition;
			float _Radius;

			fixed4 frag(v2f i) : SV_TARGET
			{
				float dist = distance(i.worldPos, _PlayerPosition);
				float ratio = (dist / _Radius);
				float rand1 = 0.5 + sin((i.worldPos.x + i.worldPos.z) * 10)/2;
				float rand2 = 0.5 + cos((i.worldPos.x + i.worldPos.z) * 10)/2;
				fixed4 cookie = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow));
				fixed4 cookie1 = tex2Dproj(_ShadowTex1, UNITY_PROJ_COORD(i.uvShadow));
				fixed4 cookie2 = tex2Dproj(_ShadowTex2, UNITY_PROJ_COORD(i.uvShadow));
				fixed4 output = lerp(cookie1, cookie2, rand1);
				output = lerp(output, cookie, rand2);
				//output *= dot(i.worldNormal, normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz));
				output *= saturate(1 - ratio);
				output.a = output.a > 1 ? output.a : output.a*2;
				output.r /= 5;
				//output *= i.light;
				return output;
			}
			ENDCG
		}
	}
	FallBack "Diffuse"
}
