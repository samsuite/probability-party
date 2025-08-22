
// a miscellaneous collection of methods that are useful for shaders

#ifndef METHODLIBRARY
#define METHODLIBRARY


#include "MatrixMath.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

float WorldPosToDepth (float3 worldPos) {
    float linearDepth = TransformWorldToView(worldPos).z;
    return (_ProjectionParams.y / (_ProjectionParams.x * linearDepth)) - (_ProjectionParams.y/_ProjectionParams.z);
}

float DepthToLinearDistance (float depth, float4 projParams) {
    return (1.0/((depth + (projParams.y/projParams.z))/projParams.y))/projParams.x;
}

float InverseLerp (float a, float b, float v) {
    return saturate((v-a)/(b-a));
}

float3 ProjectOnPlane (float3 vec, float3 normal) {
    return vec - normal * (dot(vec, normal) / dot(normal, normal));
}

float RandomSample (float3 seed) {
    return frac(sin(dot(seed.xyz, float3(12.9898,78.233,45.5432))) * 43758.5453);
}

float4 AddAlphaLayer (float4 baseLayer, float4 newLayer) {
    float4 combinedColor = 0;
    combinedColor.a = newLayer.a + (baseLayer.a*(1-newLayer.a));
    combinedColor.rgb = ((newLayer.rgb * newLayer.a) + ((baseLayer.rgb*baseLayer.a)*(1-newLayer.a)))/combinedColor.a;

    return combinedColor;
}

float2 Rotate2D (float2 vec, float radians) {
    float2 rotated;
    rotated.x = (vec.x * cos(radians)) - (vec.y * sin(radians));
    rotated.y = (vec.y * cos(radians)) + (vec.x * sin(radians));
    return rotated;
}

// sample a texture projected on each worldspace axis and blend them together
float4 SampleTriplanarUnity (UnityTexture2D tex, float3 worldpos, float3 worldnormal, float scale) {
    half3 blend = abs(worldnormal);
    blend /= dot(blend, 1.0);

    float3 scaled_worldpos = worldpos / scale;

    float4 x_plane_sample = SAMPLE_TEXTURE2D(tex.tex, tex.samplerstate, scaled_worldpos.zy);
    float4 y_plane_sample = SAMPLE_TEXTURE2D(tex.tex, tex.samplerstate, scaled_worldpos.xz);
    float4 z_plane_sample = SAMPLE_TEXTURE2D(tex.tex, tex.samplerstate, scaled_worldpos.xy);

    //return (x_plane_sample * blend.x) + (y_plane_sample * blend.y) + (z_plane_sample * blend.z);
    return max(max((x_plane_sample * blend.x) , (y_plane_sample * blend.y)), (z_plane_sample * blend.z));
}

/*
// sample a texture projected on each worldspace axis and blend them together
float4 SampleTriplanar (sampler2D tex, float3 worldpos, float3 worldnormal, float scale) {
    half3 blend = abs(worldnormal);
    blend /= dot(blend, 1.0);

    float3 scaled_worldpos = worldpos / scale;

    float4 x_plane_sample = tex2D(tex, scaled_worldpos.zy);
    float4 y_plane_sample = tex2D(tex, scaled_worldpos.xz);
    float4 z_plane_sample = tex2D(tex, scaled_worldpos.xy);

    return (x_plane_sample * blend.x) + (y_plane_sample * blend.y) + (z_plane_sample * blend.z);
}
*/

// sample a texture projected onto an imaginary plane based on the normal
float4 SampleTriplanarFast (sampler2D tex, float3 worldpos, float3 origin, float3 worldnormal, float3 worldtangent, float3 worldbinormal, float scale) {

    float4x4 mat = inverse(axis_matrix(worldtangent, worldbinormal, worldnormal));
    float3 newpos = mul(mat, float4(worldpos-origin, 0)).xyz;

    float4 sample = tex2D(tex, newpos.xy/scale);
    return sample;

    /*
    float3 scaled_worldpos = worldpos / scale;

    float3 worldnormal_abs = float3(abs(worldnormal.x), abs(worldnormal.y), abs(worldnormal.z));
    float worldnormal_max = max(max(worldnormal_abs.x, worldnormal_abs.y), worldnormal_abs.z);

    //float x_val = (worldnormal_abs.x >= worldnormal_abs.y) && (worldnormal_abs.x >= worldnormal_abs.z);
    //float y_val = (worldnormal_abs.y >= worldnormal_abs.z) && (worldnormal_abs.y >= worldnormal_abs.x);
    //float z_val = (worldnormal_abs.z >= worldnormal_abs.x) && (worldnormal_abs.z >= worldnormal_abs.y);


    int x_val = saturate(ceil(worldnormal_abs.x - worldnormal_abs.y) * ceil(worldnormal_abs.x - worldnormal_abs.z));
    int y_val = saturate(ceil(worldnormal_abs.y - worldnormal_abs.z) * ceil(worldnormal_abs.y - worldnormal_abs.x));
    int z_val = saturate(ceil(worldnormal_abs.z - worldnormal_abs.x) * ceil(worldnormal_abs.z - worldnormal_abs.y));

    int val = (x_val*1) + (y_val*2) + (z_val*3);

    float4 sample;
    switch(val) {
        case 1:
            sample = tex2D(tex, scaled_worldpos.zy);
        case 2:
            sample = tex2D(tex, scaled_worldpos.xz);
        default:
            sample = tex2D(tex, scaled_worldpos.xy);
    }

    return sample;
    */
}

// sample a texture projected onto the XZ plane
float4 SampleXZ (sampler2D tex, float3 worldpos, float scale) {
    float3 scaled_worldpos = worldpos / scale;
    return tex2D(tex, scaled_worldpos.xz);
}


// decode a normalmap texture and output a normal vector in worldspace
float3 DecodeNormalmap (float4 encoded_normal, float normal_intensity, float3 worldnormal, float3 worldtangent, float3 worldbinormal) {
    float texture_exists_mask = saturate(length(encoded_normal));

    float3 local_coords = float3(2.0 * encoded_normal.w - 1.0, 2.0 * encoded_normal.y - 1.0, 0.0);
    local_coords *= normal_intensity;
    local_coords.z = sqrt(saturate(1.0 - dot(local_coords, local_coords)));
    local_coords = normalize(local_coords);

    local_coords = lerp(float3(0,0,1), local_coords, texture_exists_mask);


    float3 normal = normalize(worldnormal);
    float3x3 local_to_world_transpose = float3x3(worldtangent, worldbinormal, worldnormal);
    float3 mapped_normal = normalize(mul(local_coords, local_to_world_transpose));

    return mapped_normal;
}



#endif