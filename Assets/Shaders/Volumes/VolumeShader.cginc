#include "UnityCG.cginc"

// Adapted from https://docs.unity3d.com/Manual/class-Texture3D.html

#define MAX_STEP_COUNT 128
#define EPSILON 0.00001f

struct VertexInput
{
    float4 vertex : POSITION;
};

struct VertexToFrag
{
    float4 vertex : SV_POSITION;
    float3 objectVertex : TEXCOORD0;
    float3 vectorToSurface : TEXCOORD1;
};

sampler3D _MainTex;
float4 _MainTex_ST;
float _Alpha;
float _StepSize;

VertexToFrag vert (VertexInput input)
{
    VertexToFrag output;   
    output.objectVertex = input.vertex;

    // These need to be adjusted to take into account object positions properly
    const float3 worldVertex = mul(unity_ObjectToWorld, input.vertex).xyz;
    output.vectorToSurface = worldVertex - _WorldSpaceCameraPos;

    output.vertex = UnityObjectToClipPos(input.vertex);
    return output;
}

// Alpha blending between samples
half4 accumulate(half4 dest, half4 source)
{
    dest.rgb += (1.0 - dest.a) * source.a * source.rgb;
    dest.a += (1.0 - dest.a) * source.a;
    return dest;
}

fixed4 frag(VertexToFrag input) : SV_Target
{
    const float3 rayOrigin = input.objectVertex;   
    const float3 rayDirection = mul(unity_WorldToObject, float4(normalize(input.vectorToSurface), 1));

    half4 color = half4(0, 0, 0, 0);
    float3 samplePosition = rayOrigin;

    // Perform the actual raymarching. Early-existing from this loop seems to slow things down on Quest2
    for (int i = 0; i < MAX_STEP_COUNT; i++)
    {
        // Accumulate color only within unit cube bounds
        if(max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
        {
            half4 sampleValue = tex3D(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f));
            sampleValue.a *= _Alpha;
            color = accumulate(color, sampleValue);
            samplePosition += rayDirection * _StepSize;
        }
    }
    
    return color;
}