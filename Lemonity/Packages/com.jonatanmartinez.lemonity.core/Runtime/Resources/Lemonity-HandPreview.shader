Shader "Lemonity/Hand Preview"
{
	Properties
	{
		_Color ("Color", Color) = (0.65882355, 0.39607844, 0.27058825, 1)
		_Ambient ("Ambient", Range(0, 1)) = 0.35
		_LightStrength ("Light Strength", Range(0, 1)) = 0.65
		_LightDirection ("Light Direction", Vector) = (0.35, 0.7, 0.6, 0)
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

		Pass
		{
			Name "UniversalForward"
			Tags { "LightMode"="UniversalForward" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			fixed4 _Color;
			fixed _Ambient;
			fixed _LightStrength;
			float4 _LightDirection;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float3 lightDirection = normalize(_LightDirection.xyz);
				fixed light = _Ambient + saturate(dot(normal, lightDirection)) * _LightStrength;
				return fixed4(_Color.rgb * light, _Color.a);
			}
			ENDCG
		}
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			fixed4 _Color;
			fixed _Ambient;
			fixed _LightStrength;
			float4 _LightDirection;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float3 normal = normalize(i.worldNormal);
				float3 lightDirection = normalize(_LightDirection.xyz);
				fixed light = _Ambient + saturate(dot(normal, lightDirection)) * _LightStrength;
				return fixed4(_Color.rgb * light, _Color.a);
			}
			ENDCG
		}
	}

	Fallback Off
}
