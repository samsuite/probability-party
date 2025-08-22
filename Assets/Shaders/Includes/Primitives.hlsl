// signed distance functions and intersection functions for various primitives

float SphereSignedDistance (float3 samplePos, float3 center, float radius) {
    return distance(samplePos, center)-radius;
}

float BoxSignedDistance (float3 samplePos, float3 center, float3 boxSize) {
    float3 p = samplePos-center;
    float3 q = abs(p) - boxSize;
    return length(max(q,0.0)) + min(max(q.x,max(q.y,q.z)),0.0);
}

float PlaneSignedDistance (float3 samplePos, float3 planeNormal, float planeHeight) {
    return dot(samplePos, planeNormal) + planeHeight;
}



float2 SphereIntersection (float3 rayOrigin, float3 rayDirection, float3 center, float radius) {
    float3 oc = rayOrigin - center;
    float b = dot( oc, rayDirection );
    float c = dot( oc, oc ) - radius*radius;
    float h = b*b - c;
    if( h<0.0 ) return 999999; // no intersection
    h = sqrt( h );
    return float2( -b-h, -b+h );
}

float2 BoxIntersection (float3 rayOrigin, float3 rayDirection, float3 center, float3 boxSize, out float3 outNormal) {
    float3 m = 1.0/rayDirection;        // can precompute if traversing a set of aligned boxes
    float3 n = m*(rayOrigin-center);    // can precompute if traversing a set of aligned boxes
    float3 k = abs(m)*boxSize;
    float3 t1 = -n - k;
    float3 t2 = -n + k;
    float tN = max( max( t1.x, t1.y ), t1.z );
    float tF = min( min( t2.x, t2.y ), t2.z );

    if ( tN>tF || tF<0.0) {
        return float2(999999,999999); // no intersection
    }

    outNormal = (tN>0.0) ? step(tN.xxx,t1) :  // rayOrigin outside the box
                            step(t2,tF.xxx);  // rayOrigin inside the box

    outNormal *= -sign(rayDirection);
    return float2( tN, tF );
}

float PlaneIntersection (float3 rayOrigin, float3 rayDirection, float4 p) {
    return -(dot(rayOrigin,p.xyz)+p.w)/dot(rayDirection,p.xyz);
}
