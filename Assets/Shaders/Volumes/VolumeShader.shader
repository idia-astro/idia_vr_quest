Shader "Unlit/VolumeShader"
{
    Properties
    {
        _MainTex ("Texture", 3D) = "white" {}
        _MaxSteps("Maximum step count", Range(16,512)) = 128
        _DataMin("Data Min", float) = 0.0
        _DataMax("Data Max", float) = 1.0
        _Threshold("Scale Threshold", Range(0,1)) = 0.0
        _Color("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        Blend One OneMinusSrcAlpha

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