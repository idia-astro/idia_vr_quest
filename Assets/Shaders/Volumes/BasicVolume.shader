Shader "IDIA/BasicVolume"
{
	Properties
	{
		_MainTex ("MainTex", 3D) = "white" {}
		_ColorMap("Color Map", 2D) = "white" {}
		_NumColorMaps("Number of Color Maps", Int) = 80
		_ColorMapIndex("Color Map Index", Range(0, 80)) = 0
		_SliceMin("Slice Min", Vector) = (-1,-1,-1,1)
		_SliceMax("Slice Max", Vector) = (1,1,1,1)
		_ThresholdMin("Threshold Min", Range(0,1)) = 0
		_ThresholdMax("Threshold Max", Range(0,1)) = 1
		_ScaleMin("Data Scale Min" , Float) = 0
		_ScaleMax("Data Scale Max" , Float) = 1
		_Jitter("Jitter amount", Range(0,1)) = 0
		_MaxSteps("Maximum step count", Range(16,512)) = 128
	}
		SubShader
		{
			Tags { "Queue" = "Transparent-100" }

			Pass
			{
				Blend SrcAlpha OneMinusSrcAlpha
				Cull Front ZWrite Off ZTest Always

				CGPROGRAM
				#pragma multi_compile __ SHADER_AIP
				#pragma vertex vertexShaderVolume
				#pragma fragment fragmentShaderRayMarch

				#include "UnityCG.cginc"
				#include "BasicVolume.cginc"

				ENDCG
			}
		}
}