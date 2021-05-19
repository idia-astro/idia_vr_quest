#include "UnityCG.cginc"

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

//UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

sampler3D _MainTex;
sampler2D _ColorMap;
int _NumColorMaps;
float _ColorMapIndex;
float _DataMin;
float _DataMax;
float _Threshold;
float4 _Color;
int _MaxSteps;
float _Jitter;
float3 _SliceMin, _SliceMax;

// Simple pseudo-random number generator from https://github.com/keijiro
float nrand(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

struct Ray
{
    float3 origin;
    float3 direction;
};

struct VertexShaderInput
{
    float4 vertex : POSITION;
};

struct VertexShaderOuput
{
    float4 vertex : SV_POSITION;
    Ray ray : TEXCOORD0;
    float4 projPos : TEXCOORD2;
};

VertexShaderOuput vert (VertexInput input)
{
    VertexShaderOuput v;

    const float4 worldPos = mul(UNITY_MATRIX_M, input.vertex);
    v.vertex = mul(UNITY_MATRIX_VP, worldPos);
    v.ray.direction = -ObjSpaceViewDir(input.vertex);
    v.ray.origin = input.vertex.xyz - v.ray.direction;
    // Adapted from the Unity "Particles/Additive" built-in shader
    v.projPos = ComputeScreenPos(v.vertex);
    COMPUTE_EYEDEPTH(v.projPos.z);
    return v;    
}

// Implementation: NVIDIA. Original algorithm : HyperGraph
// http://www.siggraph.org/education/materials/HyperGraph/raytrace/rtinter3.htm
bool IntersectBox(Ray r, float3 boxmin, float3 boxmax, out float tnear, out float tfar)
{
    // compute intersection of ray with all six bbox planes
    const float3 invR = 1.0 / r.direction;
    const float3 tbot = invR * (boxmin.xyz - r.origin);
    const float3 ttop = invR * (boxmax.xyz - r.origin);

    // re-order intersections to find smallest and largest on each axis
    const float3 tmin = min(ttop, tbot);
    const float3 tmax = max(ttop, tbot);

    // find the largest tmin and the smallest tmax
    float2 t0 = max(tmin.xx, tmin.yz);
    const float largest_tmin = max(t0.x, t0.y);
    t0 = min(tmax.xx, tmax.yz);
    const float smallest_tmax = min(t0.x, t0.y);

    // check for hit
    bool hit;
    if ((largest_tmin > smallest_tmax)) 
        hit = false;
    else
        hit = true;

    tnear = largest_tmin;
    tfar = smallest_tmax;

    return hit;
}

fixed4 frag(VertexShaderOuput input) : SV_Target
{
    // Adapted from the Unity "Particles/Additive" built-in shader
    //const float sceneZ = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(input.projPos)));
    //const float opaqueDepthObjectSpace = length(sceneZ * input.ray.direction / input.projPos.z);
    float tNear, tFar;
    input.ray.direction = normalize(input.ray.direction);
    const bool hit = IntersectBox(input.ray, _SliceMin, _SliceMax, tNear, tFar);
    
    // Early exit of pixels missing the bounding box or occluded by opaque objects
    //if (!hit || tFar < 0 || opaqueDepthObjectSpace < tNear)
    if (!hit || tFar < 0)
    {
        return float4(0, 0, 0, 0);
    }
    
    // Clamp intersection depths between 0 and opaque object depth
    //tFar = min(tFar, opaqueDepthObjectSpace);

    // calculate intersection points    
    float3 pNear = input.ray.origin + input.ray.direction * tNear;
    float3 pFar = input.ray.origin + input.ray.direction * tFar;
    // convert to texture space
    pNear = pNear + 0.5;
    pFar = pFar + 0.5;
        
    float3 currentRayPosition = pNear;
    const float3 rayDelta = pFar - pNear;
    const float totalLength = length(rayDelta);

    const float maxLength = length(_SliceMax - _SliceMin);
    const float stepLength = sqrt(maxLength) / floor(_MaxSteps);
    const float3 stepVector = normalize(rayDelta);
    // Calculate the required number of steps, based on the total path length through the object 
    const int requiredSteps = clamp(totalLength / stepLength, 0, _MaxSteps);
    
    // Shift ray's starting point by a small temporal noise amount to reduce box artefacts
    // Based on code from Ryan Brucks: https://shaderbits.com/blog/creating-volumetric-ray-marcher
    const float3 randVector = nrand(input.vertex.xy + _Time.xy) * stepVector * stepLength * _Jitter;
    currentRayPosition += randVector;

    float x = _DataMin;

    for (int i = 0; i < requiredSteps; i++)
    {
        const float sampleValue =tex3Dlod(_MainTex, float4(currentRayPosition, 0)).r;
        x = max(x, sampleValue);
        currentRayPosition += stepVector * stepLength;
    }

    const float range = _DataMax - _DataMin;
    // Scale to [0, 1] based on data min/max range
    x = (x - _DataMin) / range;
    if (x >= _Threshold)
    {
        float colorMapOffset = 1.0 - (0.5 + _ColorMapIndex) / _NumColorMaps;
        float4 colorMapColor = float4(tex2D(_ColorMap, float2(x, colorMapOffset)).xyz, x);
        return colorMapColor;        
    }
    return 0;
}
