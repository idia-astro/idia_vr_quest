Shader "Unlit/VolumeShader"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _ColorMap("Color Map", 2D) = "white" {}
		_NumColorMaps("Number of Color Maps", Int) = 80
		_ColorMapIndex("Color Map Index", Range(0, 80)) = 0
        _MaxSteps("Maximum step count", Range(16,512)) = 128
        _DataMin("Data Min", float) = 0.0
        _DataMax("Data Max", float) = 1.0
        _Threshold("Scale Threshold", Range(0,1)) = 0.0
        _Jitter("Temporal Jitter", Range(0,1)) = 1.0
        _SliceMin("Slice Min", Vector) = (-0.5,-0.5,-0.5,1)
		_SliceMax("Slice Max", Vector) = (0.5,0.5,0.5,1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent-100"}
        Blend SrcAlpha OneMinusSrcAlpha
		Cull Front ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "VolumeShader.cginc"
            ENDCG
        }
    }
}