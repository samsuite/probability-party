#define _MAIN_LIGHT_SHADOWS_CASCADE;
#include "SimplexNoise.hlsl"
#include "Primitives.hlsl"
#include "MethodLibrary.hlsl"

sampler2D _BlueNoise128;

sampler2D _ScanTexture;
float3 _ScanCameraPosition;
float4 _ScanProjParams;
float _ScanSize;

float Random (float2 uv) {
    return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
}

float4 SampleScanHeight (float3 worldPos) {
    float2 scanUV = (worldPos.xz - (_ScanCameraPosition.xz-_ScanSize*0.5))/_ScanSize;
    float2 scanMask2d = (scanUV >= 0)* (scanUV <= 1);
    float4 scanTex = tex2D(_ScanTexture, scanUV);
    //float scanMask = scanMask2d.x * scanMask2d.y;
    //scanTex *= scanMask;

    float4 linearDistance = (1-scanTex)*_ScanProjParams.z; // _ScanProjParams.z = scan far plane
    linearDistance.r *= (scanTex.r != 0);
    return _ScanCameraPosition.y - linearDistance;
}

// TODO:
/*
float SampleDirectionalShadow (float3 position) {
    float4 shadowCoord = TransformWorldToShadowCoord(position);
    ShadowSamplingData shadowSamplingData = GetMainLightShadowSamplingData();
    half4 shadowParams = GetMainLightShadowParams();

    //half cascadeIndex = ComputeCascadeIndex(position);
    //float4 shadowCoord = mul(_MainLightWorldToShadow[cascadeIndex], float4(position, 1.0));
    //shadowCoord.w = 0;

    //float atten =  real(SAMPLE_TEXTURE2D_SHADOW(_MainLightShadowmapTexture, sampler_LinearClampCompare, shadowCoord.xyz));
    //float atten =  SampleShadowmapFilteredHighQuality(TEXTURE2D_SHADOW_ARGS(_MainLightShadowmapTexture, sampler_LinearClampCompare), shadowCoord, shadowSamplingData);


    float atten = SampleShadowmap(TEXTURE2D_ARGS(_MainLightShadowmapTexture, sampler_LinearClampCompare), shadowCoord, shadowSamplingData, shadowParams, false);
    return atten;

    //float beyondShadowLimitMask = BEYOND_SHADOW_FAR(shadowCoord);
    //float4 resultColor = float4(atten.xxx,1);
    //resultColor = lerp(resultColor, float4(1,0,0,1), beyondShadowLimitMask);
    //return resultColor;
}
*/

float SampleSceneHeightShadow (float3 position) {
    float sceneHeight = SampleScanHeight(position).g;
    return (sceneHeight < position.y);
}

float SampleGodRays (float3 position, float3 lightDir, float distFromSurface) {

    const float godRayScale = 1;
    const float godRayLength = 5;
    const float godRayBrightness = 0.5;
    const float surfaceFadeDistance = 1;
    const float oscillationSpeed = 0.25;

    float3 flattenedLightDir = normalize(float3(lightDir.x, 0, lightDir.z));
    float distFromSurfaceAlongRay = distFromSurface / length(cross(flattenedLightDir, lightDir));

    float3 godRayPosition = float3(position.x, 0, position.z);
    godRayPosition -= distFromSurfaceAlongRay * lightDir;
    godRayPosition /= godRayScale;

    float godRayNoise = SimplexNoise3D(float3(godRayPosition.x, _Time.y*oscillationSpeed, godRayPosition.z));
    godRayNoise = saturate(godRayNoise - 0.1);

    godRayNoise *= InverseLerp(godRayLength, 0, distFromSurfaceAlongRay);
    godRayNoise *= InverseLerp(0, surfaceFadeDistance, distFromSurface);
    godRayNoise *= godRayBrightness;

    return godRayNoise;
}

float SampleFogDensity (float3 position, float surfaceHeight) {
    const float densityCoefficient = 0.5;
    const float surfaceFadeDistance = 1;
    float surfaceFade = 1-InverseLerp(surfaceHeight-surfaceFadeDistance, surfaceHeight, position.y);

    float sampleA = saturate(SimplexNoise3D(position*0.2  +  float3(0,_Time.x*0.5,0)));
    float sampleB = saturate(SimplexNoise3D(position*0.75 +  float3(0,_Time.x*0.75,0)));
    float sampleC = saturate(SimplexNoise3D(position*20   +  float3(0,_Time.x*1.5,0)));

    float combinedNoise = (sampleA*2)+(sampleB*1)+(sampleC*0.2) - 0.6;
    combinedNoise *= surfaceFade;
    combinedNoise *= densityCoefficient;

    // density should never be negative
    return max(combinedNoise, 0);
}

// faster version of fog density that doesn't sample the smallest noise factor. also doesn't bother with surface fade
float SampleApproxFogDensity (float3 position, float surfaceHeight) {

    float sampleA = saturate(SimplexNoise3D(position*0.2  +  float3(0,_Time.x*0.5,0)));
    float sampleB = saturate(SimplexNoise3D(position*0.75 +  float3(0,_Time.x*0.75,0)));

    float combinedNoise = (sampleA*2)+(sampleB*1) - 0.55;
    return max(combinedNoise, 0);
}

float4 SampleFogLighting (float3 position, float surfaceHeight, float4 fogLightColor, float4 fogDarkColor, float4 godRayColor) {
    const float lightCheckDistance = 0.1;
    const float lightPenetration = 0.1;
    const float depthDarkeningRange = 30;
    const float lightAmountInShadow = 0.35;
    const float densityCoefficientInShadow = 0.4;

    float distFromSurface = surfaceHeight - position.y;

    float density = SampleFogDensity(position, surfaceHeight);
    density += 0.1; // add some extra density so we can see light rays in the fog


    // TODO:
    //Light mainLight = GetMainLight();
    float3 lightDir = normalize(float3(0,1,0.7));// -normalize(mainLight.direction);

    float3 lightSamplePosition = position - (lightDir * lightCheckDistance);
    float lightSampleDensity = SampleApproxFogDensity(lightSamplePosition, surfaceHeight);
    float lightAmount = 1-saturate(lightSampleDensity/lightPenetration);

    float depthFactor = InverseLerp(depthDarkeningRange, 0, distFromSurface);
    depthFactor = pow(depthFactor, 2);  // square the depth factor to get a smoother curve
    lightAmount *= depthFactor;

    float shadowSample = SampleSceneHeightShadow(position);

    lightAmount *= saturate(shadowSample + lightAmountInShadow);

    // density appears lower in shadowed areas because less light is bouncing off
    density *= saturate(shadowSample+densityCoefficientInShadow);

    float godRayNoise = SampleGodRays(position, lightDir, distFromSurface);
    godRayNoise *= shadowSample;

    float4 color = lerp(fogDarkColor, fogLightColor, lightAmount);


    // darken shadowed areas
    color.rgb *= saturate(shadowSample+lightAmountInShadow);
    // and deep areas
    color.rgb *= saturate(depthFactor+0.25);

    color.rgb *= 1 + (godRayNoise * 10);
    color.rgb += godRayNoise * godRayColor.rgb * godRayColor.a;

    return float4(color.rgb, color.a*density);
}

float GetOceanIntersection (float3 rayOrigin, float3 rayDirection) {
    float3 normal;

    float distance;
    distance = BoxIntersection(rayOrigin, rayDirection, 0, 20, normal).x;
    distance = min(distance, BoxIntersection(rayOrigin, rayDirection, float3(20,0,0), 5, normal).x);

    return distance;
}

float SampleOceanSDF (float3 samplePos) {

    float distance;
    distance = BoxSignedDistance(samplePos, 0, 15);
    distance = min(distance, BoxSignedDistance(samplePos, float3(20,0,0), 5));

    return -1;//distance;
}


float4 RaymarchBlood (float3 startPos, float3 rayDir, float maxDistance, float2 pixelCoord, float4 fogColor) {

    const float testDensity = 0.5;



    const int numSteps = 7;
    const float fadeStartDistance = 0;
    const float cutoffDistance = 10;

    //float surfaceDistance = GetOceanIntersection(camPos, rayDir);
    //float3 startPos = camPos + (rayDir * surfaceDistance);
    //return float4(frac(startPos+0.1),1);

    float boundaryDistance = min(maxDistance, cutoffDistance);
    float epsilon = boundaryDistance/numSteps;
    float3 boundaryPosition = startPos + rayDir * boundaryDistance;

    const float noiseTexWidth = 128;
    float blueNoise = frac(tex2D(_BlueNoise128, (pixelCoord/noiseTexWidth)).r + _Time.w);
    float startOffset = blueNoise * epsilon;

    return blueNoise;

    float3 accumulatedColor = 0;
    float acculumulatedAlpha = 1;
    float distanceTravelled = startOffset;

    // march from back to front
    // so the closer fog covers the farther fog
    for (int i = 0; i < numSteps; i++) {
        if (distanceTravelled > boundaryDistance) {
            break;
        }
        float3 samplePos = boundaryPosition - (rayDir * distanceTravelled);

        float hit = 1-saturate(floor(SampleOceanSDF(samplePos)));
        float4 sampledColor = float4(fogColor.rgb, fogColor.a * hit * testDensity);

        sampledColor.a *= epsilon;
        sampledColor.a = saturate(sampledColor.a);

        sampledColor.rgb *= sampledColor.a;

        accumulatedColor *= (1-sampledColor.a);
        acculumulatedAlpha *= (1-sampledColor.a);
        accumulatedColor += sampledColor.rgb;

        distanceTravelled += epsilon;
    }

    return float4(accumulatedColor, acculumulatedAlpha);
}

float4 RaymarchBloodFog (float3 startPos, float3 rayDir, float maxDistance, float surfaceHeight, float2 pixelCoord, float4 cloudDarkColor, float4 cloudLightColor, float4 godRayColor) {
    const int numSteps = 7;
    const float fadeStartDistance = 0;
    const float cutoffDistance = 20;
    const float epsilon = cutoffDistance/numSteps;

    float boundaryDistance = min(maxDistance, cutoffDistance);
    float3 boundaryPosition = startPos + rayDir * boundaryDistance;

    const float noiseTexWidth = 128;
    //float2 pixelCoord = (screenPos.xy/screenPos.w) * _ScreenParams.xy;
    float blueNoise = frac(tex2D(_BlueNoise128, (pixelCoord/noiseTexWidth)).r + _Time.w);
    float startOffset = blueNoise * epsilon;

    float4 color = 0;
    float distanceTravelled = startOffset;

    // march from back to front
    // so the closer fog covers the farther fog
    for (int i = 0; i < numSteps; i++) {
        if (distanceTravelled > boundaryDistance) {
            break;
        }
        float3 samplePos = boundaryPosition - (rayDir * distanceTravelled);

        float distanceFade = InverseLerp(0, cutoffDistance-fadeStartDistance, distanceTravelled);

        float4 sampledColor = SampleFogLighting(samplePos, surfaceHeight, cloudLightColor, cloudDarkColor, godRayColor);
        sampledColor.a *= epsilon;
        sampledColor.a *= distanceFade;
        sampledColor.a = saturate(sampledColor.a);

        color = AddAlphaLayer(color, sampledColor);


        //sampledColor.rgb *= sampledColor.a;
        //accumulatedColor *= (1-sampledColor.a);
        //acculumulatedAlpha *= (1-sampledColor.a);
        //accumulatedColor += sampledColor.rgb;

        distanceTravelled += epsilon;
    }


    return color;
}
