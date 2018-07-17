Shader "Hidden/Occipital/GreenGrid"
{
Properties {
	_u_alpha_("Alpha", Range (0.0, 1.0)) = 1.0
}
SubShader {
	Tags { "Queue"="Transparent+5000" "IgnoreProjector"="True" "RenderType"="Transparent" }
	LOD 100

	ZWrite On
	Blend SrcAlpha OneMinusSrcAlpha 
	
	Pass
	{
		CGPROGRAM
		#include "Occipital.cginc"
		#pragma vertex vertCollAvoid
		#pragma fragment fragGreenGrid
		ENDCG
	}
}
}
