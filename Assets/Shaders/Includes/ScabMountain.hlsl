#include "MethodLibrary.hlsl"

void StandardFrag_float (float3 worldPosition, float3 worldNormal, UnityTexture2D tex, out float3 albedo, out float3 emission) {
    float4 triplanarTex = SampleTriplanarUnity(tex, worldPosition, worldNormal, 15);


    albedo = float4(saturate(triplanarTex.rrr+0.5),1);
    emission = float3(0, 0, 0);
}


