// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Abedo (RGB)", 2D) = "white" {}
        _ReflectionTex ("Reflection (RGB)", 2D) = "white" {}
        _RefractionTex ("Refraction (RGB)", 2D) = "white" {}
        _NormalMap ("Normal (RGB)", 2D) = "white" {}
        _FlowMap ("Flow (RGB)", 2D) = "white" {}
        _CausticsTex ("Caustics (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _MaxDepth("Max Depth", Range(0,30)) = 9.3
        _MinDepth("Min Depth", Range(0,10)) = 9.3
        [Toggle]_Debug_Depth("Toggle Debug Depth",Float) = 0
        [Toggle]_Debug_Normal_Map("Toggle Normal Map",Float) = 0
        [Toggle]_Caustics_Chromatic("Toggle Caustics Chromatic",Float) = 0
        _FresnelFactor("Fresnel Factor", Range(0,5)) = 1

        _Speed ("Speed", Float) = 1
        _Tiling ("Tiling", Float) = 1
        _CausticsTiling ("CausticsTiling", Float) = 1
        _CausticsStrength ("CausticsStrength", Float) = 1
        _CausticsSplit ("CausticsSplit", Range(0,0.5)) = 0.004
        //        _TilingModulated ("Tiling, Modulated", Float) = 1
        //        _FlowStrength ("Flow Strength", Float) = 1
        //        _GridResolution ("Grid Resolution", Float) = 10
        //        _HeightScale ("Height Scale", Float) = 1
        //        _HeightScaleModulated ("Height Scale Modulated", Float) = 1
    }
    SubShader
    {
        Tags
        {
            //            "RenderType" = "Opaque"
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        LOD 200

        //        GrabPass
        //        {
        //            "_WaterBackground"
        //        }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha //finalcolor:ResetAlpha//fullforwardshadows
        #pragma vertex vert
        #pragma multi_compile __ _DEBUG_DEPTH_ON
        #pragma multi_compile __ _DEBUG_NORMAL_MAP_ON
        #pragma multi_compile __ _CAUSTICS_CHROMATIC_ON

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _ReflectionTex;
        sampler2D _RefractionTex;
        sampler2D _MainTex;
        sampler2D _CausticsTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
            float4 refparam; //r:fresnel,g:none,b:none,a:none
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        sampler2D _CameraDepthTexture;
        // sampler2D _LastCameraDepthTexture;
        float4 _CameraDepthTexture_TexelSize;
        // sampler2D _WaterBackground;
        float _MaxDepth, _MinDepth;
        float _FresnelFactor;
        sampler2D _NormalMap;
        sampler2D _FlowMap;

        float _Tiling, _Speed; //, _TilingModulated,  _FlowStrength, _GridResolution;
        float _CausticsTiling, _CausticsStrength, _CausticsSplit;
        // float _HeightScale, _HeightScaleModulated;

        // float2 AlignWithGrabTexel(float2 uv)
        // {
        //     #if UNITY_UV_STARTS_AT_TOP
        //     if (_CameraDepthTexture_TexelSize.y < 0)
        //     {
        //         uv.y = 1 - uv.y;
        //     }
        //     #endif
        //
        //     return
        //         (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) *
        //         abs(_CameraDepthTexture_TexelSize.xy);
        // }
        half4x4 _MainLightDirection;

        half2 Panner(half2 uv, half speed, half tiling)
        {
            return (half2(1, 0) * _Time.y * speed) + (uv * tiling);
        }
        #if _CAUSTICS_CHROMATIC_ON
        half3 SampleCaustics(half2 uv, half split)
        {
            half2 uv1 = uv + half2(split, split);
            half2 uv2 = uv + half2(split, -split);
            half2 uv3 = uv + half2(-split, -split);

            half r = tex2D(_CausticsTex, uv1).r;
            half g = tex2D(_CausticsTex, uv2).r;
            half b = tex2D(_CausticsTex, uv3).r;

            return half3(r, g, b);
        }
        #endif
        float3 ColorBelowWater(float4 screenPos, float3 tangentSpaceNormal, float fresnel, float3 worldPos)
        {
            float2 uvOffset = tangentSpaceNormal.xz; //* _RefractionStrength;
            uvOffset.y *=
                _CameraDepthTexture_TexelSize.z * abs(_CameraDepthTexture_TexelSize.y);
            float2 uv = (screenPos.xy + uvOffset) / screenPos.w;


            float backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv)); //
            const float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
            float depthDifference = backgroundDepth - surfaceDepth;

            //
            // uvOffset *= saturate(depthDifference);
            // uv = AlignWithGrabTexel((screenPos.xy + uvOffset) / screenPos.w);
            // backgroundDepth =
            //     LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
            // depthDifference = backgroundDepth - surfaceDepth;


            // float3 backgroundColor = fracol;

            // float fogFactor = easeOutCubic(exp2(-depthDifference * _AbsorptionStrength));
            // backgroundColor = lerp(_Color.rgb, backgroundColor, fogFactor);
            float waterDepth = saturate((depthDifference - _MinDepth) / (_MaxDepth - _MinDepth));

            uv = (screenPos.xy + uvOffset * waterDepth) / screenPos.w;

            half4 flecol = tex2D(_ReflectionTex, uv);
            half4 fracol = tex2D(_RefractionTex, uv);

            backgroundDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv)); //
            depthDifference = backgroundDepth - surfaceDepth;

            float depthFator = saturate(exp2(-depthDifference * _Color.a));

            float3 positionWS = _WorldSpaceCameraPos + backgroundDepth * normalize(worldPos.xyz - _WorldSpaceCameraPos);
            // float3 positionOS = mul(unity_WorldToObject, positionWS);
            // float boundingBoxMask = all(step(positionOS, 0.5) * (1 - step(positionOS, -0.5)));
            half2 causticsUV = mul(positionWS, _MainLightDirection).xy;
            half2 movingCausticsUV1 = Panner(causticsUV, .75 * _Speed, 1 / _CausticsTiling);
            half2 movingCausticsUV2 = Panner(causticsUV, _Speed, -1 / _CausticsTiling);

            #if _CAUSTICS_CHROMATIC_ON
            float3 caustics1 = SampleCaustics(movingCausticsUV1, _CausticsSplit * waterDepth);
            float3 caustics2 = SampleCaustics(movingCausticsUV2, _CausticsSplit * waterDepth);
            #else
            float3 caustics1 = tex2D(_CausticsTex, movingCausticsUV1);
            float3 caustics2 = tex2D(_CausticsTex, movingCausticsUV2);
            #endif
            half3 caustics = min(caustics1, caustics2) * _CausticsStrength;

            #if _DEBUG_DEPTH_ON
            return waterDepth;
            #else
            // return lerp(_Color.rgb, lerp(flecol, fracol, pow(fresnel, _FresnelFactor)), depthFator) + caustics.rgb *
            //     (1-depthFator);
            return lerp(_Color.rgb, lerp(flecol, fracol, _FresnelFactor), depthFator) + caustics.rgb * (1 - depthFator);
            #endif
        }

        //
        // float3 UnpackDerivativeHeight(float4 textureData)
        // {
        //     float3 dh = textureData.agb;
        //     dh.xy = dh.xy * 2 - 1;
        //     return dh;
        // }
        //
        // float2 DirectionalFlowUV(float2 uv, float3 flowVectorAndSpeed, float tiling, float time, out float2x2 rotation)
        // {
        //     float2 dir = normalize(flowVectorAndSpeed.xy);
        //     rotation = float2x2(dir.y, dir.x, -dir.x, dir.y);
        //     uv = mul(float2x2(dir.y, -dir.x, dir.x, dir.y), uv);
        //     uv.y -= time * flowVectorAndSpeed.z;
        //     return uv * tiling;
        // }
        //
        // float3 FlowCell(float2 uv, float2 offset, float time)
        // {
        //     float2 shift = 1 - offset;
        //     shift *= 0.5;
        //     offset *= 0.5;
        //     float2x2 derivRotation;
        //     float2 uvTiled =
        //         (floor(uv * _GridResolution + offset) + shift) / _GridResolution;
        //     float3 flow = tex2D(_FlowMap, uvTiled).rgb;
        //     flow.xy = flow.xy * 2 - 1;
        //     flow.z *= _FlowStrength;
        //     float tiling = flow.z * _TilingModulated + _Tiling;
        //     float2 uvFlow = DirectionalFlowUV(
        //         uv, flow, tiling, time,
        //         derivRotation
        //     );
        //     float3 dh = UnpackDerivativeHeight(tex2D(_NormalMap, uvFlow));
        //     dh.xy = mul(derivRotation, dh.xy);
        //     dh *= flow.z * _HeightScaleModulated + _HeightScale;
        //     return dh;
        // }

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 r = normalize(ObjSpaceViewDir(v.vertex));

            float d = saturate(dot(r, normalize(v.normal))); //r+(1-r)*pow(d,5)				
            o.refparam = float4(d, 0, 0, 0);
            o.worldPos = UnityObjectToClipPos(v.vertex);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            // o.Albedo = c.rgb;
            float time = _Time.y * _Speed;

            // float2 uv = IN.uv_MainTex;
            // float3 dhA = FlowCell(uv, float2(0, 0), time);
            // float3 dhB = FlowCell(uv, float2(1, 0), time);
            // float3 dhC = FlowCell(uv, float2(0, 1), time);
            // float3 dhD = FlowCell(uv, float2(1, 1), time);
            // float2 t = abs(2 * frac(uv * _GridResolution) - 1);
            // float wA = (1 - t.x) * (1 - t.y);
            // float wB = t.x * (1 - t.y);
            // float wC = (1 - t.x) * t.y;
            // float wD = t.x * t.y;
            // float3 dh = dhA * wA + dhB * wB + dhC * wC + dhD * wD;
            // o.Normal = normalize(float3(-dh.xy, 1)); //
            // 
            // float3 caustics = tex2D(_CausticsTex, IN.uv_MainTex);
            #if _DEBUG_NORMAL_MAP_ON
            fixed4 c = tex2D(_NormalMap, IN.uv_MainTex * _Tiling - time);
            o.Normal = normalize(c.rgb);
            #endif

            o.Albedo = ColorBelowWater(IN.screenPos, o.Normal, IN.refparam.r, IN.worldPos);
            // + caustics; // * _Color; // + _Color.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1;
        }

        // void ResetAlpha(Input IN, SurfaceOutputStandard o, inout fixed4 color)
        // {
        //     color.a = 1;
        // }
        ENDCG
    }
    //    FallBack "Diffuse"
}