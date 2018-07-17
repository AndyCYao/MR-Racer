Shader "Hidden/Occipital/LiveCamProjection"
{
	Properties {
		[MaterialToggle] _shadowsEnabled("Shadows Enabled", Float) = 1
		_shadowedValue("Shadow Ambient Power", Range (0.0, 1.0)) = 0.5
	}

	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			// compile shader into multiple variants, with and without shadows
            // (we don't care about any lightmaps yet, so skip these variants)
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            // shadow helper functions and macros
            #include "AutoLight.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 uv : TEXCOORD1;
				float4 vertex : SV_POSITION;
				SHADOW_COORDS(2) // put shadows data into TEXCOORD2
                
			};

			sampler2D _u_CameraTex;
			float4x4 _colorCameraViewProjMatrix;

			half _shadowedValue;
			fixed _shadowsEnabled;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = mul(_colorCameraViewProjMatrix, mul(unity_ObjectToWorld, v.vertex));
				if (_shadowsEnabled) {
					TRANSFER_SHADOW(o)
				}
               	return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = 0;

				{
					float2 uv = i.uv.xy / i.uv.w;
					bool isVisible = (uv.x >= 0.0) && (uv.x <= 1.0) && (uv.y >= 0.0) && (uv.y <= 1.0);

					// Weight of this keyframe. 0.0 if not visible or out of bounds, 1.0 otherwise.
					float w0 = float(isVisible);
					col.rgb = tex2D(_u_CameraTex, uv);

					if (_shadowsEnabled) {
						fixed shadow = max(1 - _shadowedValue, SHADOW_ATTENUATION(i));
						col.rgb = col.rgb * shadow;
					}

					col.a = w0;// * _u_alpha_;
				}

				return col;
			}
			ENDCG
		}
	}
}
