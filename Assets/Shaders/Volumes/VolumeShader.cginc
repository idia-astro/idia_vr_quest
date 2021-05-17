#include "UnityCG.cginc"

// Adapted from https://docs.unity3d.com/Manual/class-Texture3D.html
#define EPSILON 0.00001f
#define SQRT2 1.4142135623f

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
float _DataMin;
float _DataMax;
float _Threshold;
float4 _Color;
int _MaxSteps;


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

    const float stepSize = SQRT2 / _MaxSteps;
    float3 samplePosition = rayOrigin;

    float x = _DataMin;
    // Perform the actual raymarching. Early-existing from this loop seems to slow things down on Quest2
    for (int i = 0; i < _MaxSteps; i++)
    {
        // Accumulate color only within unit cube bounds
        if(max(abs(samplePosition.x), max(abs(samplePosition.y), abs(samplePosition.z))) < 0.5f + EPSILON)
        {
            const float sampleValue = tex3D(_MainTex, samplePosition + float3(0.5f, 0.5f, 0.5f)).r;
            x = max(x, sampleValue);
            samplePosition += rayDirection * stepSize;
        }
    }

    float range = _DataMax - _DataMin;
    // Scale to [0, 1] based on data min/max range
    x = (x - _DataMin) / range;
    if (x >= _Threshold)
    {
        return x * _Color;       
    }
    return 0;
}
