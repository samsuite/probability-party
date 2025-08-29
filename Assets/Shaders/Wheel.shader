// Document here
Shader "Custom/Wheel" {
    Properties {
        [NoScaleOffset] _MainTex ("Texture", 2D) = "white" {}
        [NoScaleOffset] _BlueNoise128 ("Blue Noise", 2D) = "gray" {}
        _CenterAngleDifference ("Center Angle Difference", float) = 0
        _CurrentAngle ("Current Angle", float) = 0
        _SpeedBlur ("Speed Blur", Range(0,1)) = 0
        _HueShiftBandFrequency ("Hue Shift Band Frequency", Range(0,1)) = 0
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Blend One Zero
        Cull Back
        ZWrite On
        ZTest LEqual
        Offset 0, 0


        Pass {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Includes/ColorMath.hlsl"


            sampler2D _MainTex;
            sampler2D _BlueNoise128;
            float _CenterAngleDifference;
            float _SpeedBlur;
            float _CurrentAngle;
            float _HueShiftBandFrequency;


            struct meshdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct interp {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };


            interp vert (meshdata v) {
                interp o;
                o.uv = v.uv;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);

                return o;
            }

            fixed4 frag (interp i) : SV_Target {

                const float maxSpeedBlurAngle = 2;

                float2 screenUV = i.screenPos.xy/i.screenPos.w;
                float2 pixelCoord = screenUV * _ScreenParams;

                const float noiseTexWidth = 128;
                float blueNoise = frac(tex2D(_BlueNoise128, (pixelCoord/noiseTexWidth)).r + _Time.w);
                blueNoise = pow(blueNoise,0.5);

                float2 radialUV = 0;
                float2 centeredUV = i.uv - float2(0.5, 0.5);
                clip(0.5-distance(centeredUV,0));

                float twirlAngle = (0.5-(distance(0, centeredUV)*2))*(3.1416/180) * _CenterAngleDifference;
                float blurOffset = blueNoise * _SpeedBlur * maxSpeedBlurAngle;
                twirlAngle += blurOffset;

                float2 twirledUV;
                twirledUV.x = (centeredUV.x * cos(twirlAngle)) - (centeredUV.y * sin(twirlAngle));
                twirledUV.y = (centeredUV.y * cos(twirlAngle)) + (centeredUV.x * sin(twirlAngle));

                float2 hueshiftUV;
                float spinAngle = _CurrentAngle*(3.14159/180);
                hueshiftUV.x = (twirledUV.x * cos(spinAngle)) - (twirledUV.y * sin(spinAngle));
                hueshiftUV.y = (twirledUV.y * cos(spinAngle)) + (twirledUV.x * sin(spinAngle));

                radialUV.x = 1-saturate(((1 - distance(0, twirledUV)) * 2) - 1);
                radialUV.y = frac(atan2(twirledUV.y, twirledUV.x) * 0.3183 * 0.5);

                float hueshiftOffset = radialUV.y * _HueShiftBandFrequency;
                hueshiftOffset += _CurrentAngle/400;

                fixed4 col = tex2D(_MainTex, radialUV);
                fixed4 shiftedCol = HueShiftMaintainLuminosity(col, hueshiftOffset);
                col = lerp(col, shiftedCol, 0.75);

                return col;
            }

            ENDHLSL
        }
    }

    Fallback "VertexLit"
}
