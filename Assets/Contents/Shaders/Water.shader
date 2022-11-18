Shader "Custom/Water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap ("Normal (RGB)", 2D) = "white" {}
        _FlowMap ("Flow (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _AbsorptionStrength("Water Absorption Strength", Range(0,1)) = .1
        _RefractionStrength("Refraction Strength", Range(0,1)) = .1
        [Toggle]_Absorption("Absorption Function",Float) = 0
        _Tiling ("Tiling", Float) = 1
        _TilingModulated ("Tiling, Modulated", Float) = 1
        _Speed ("Speed", Float) = 1
        _FlowStrength ("Flow Strength", Float) = 1
        _GridResolution ("Grid Resolution", Float) = 10
        _HeightScale ("Height Scale", Float) = 1
        _HeightScaleModulated ("Height Scale Modulated", Float) = 1
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        LOD 200

        GrabPass
        {
            "_WaterBackground"
        }

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard alpha finalcolor:ResetAlpha//fullforwardshadows
        #pragma multi_compile __ _ABSORPTION_ON

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float4 screenPos;
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
        float4 _CameraDepthTexture_TexelSize;
        sampler2D _WaterBackground;
        float _AbsorptionStrength, _RefractionStrength;
        sampler2D _NormalMap;
        sampler2D _FlowMap;

        float _Tiling, _TilingModulated, _Speed, _FlowStrength, _GridResolution;
        float _HeightScale, _HeightScaleModulated;

        float3 ColorBelowWaterAbsorb(float4 screenPos)
        {
            float2 uv = screenPos.xy / screenPos.w;
            if (_CameraDepthTexture_TexelSize.y < 0)
            {
                uv.y = 1 - uv.y;
            }
            const float background_depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
            const float surface_depth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
            const float depthValue = (background_depth - surface_depth);

            const float absorptionValue = 1.0 - exp2(-_AbsorptionStrength * depthValue);
            const float3 absorptionColor = float3(1, 1, 1) - _Color.rgb;
            const float3 subtractiveColor = absorptionColor * absorptionValue;
            const float3 backgroundColor = tex2D(_WaterBackground, uv).rgb;

            return backgroundColor - subtractiveColor;
        }

        float2 AlignWithGrabTexel(float2 uv)
        {
            #if UNITY_UV_STARTS_AT_TOP
            if (_CameraDepthTexture_TexelSize.y < 0)
            {
                uv.y = 1 - uv.y;
            }
            #endif

            return
                (floor(uv * _CameraDepthTexture_TexelSize.zw) + 0.5) *
                abs(_CameraDepthTexture_TexelSize.xy);
        }

        float3 ColorBelowWater(float4 screenPos, float3 tangentSpaceNormal)
        {
            float2 uvOffset = tangentSpaceNormal.xy * _RefractionStrength;
            uvOffset.y *=
                _CameraDepthTexture_TexelSize.z * abs(_CameraDepthTexture_TexelSize.y);
            float2 uv = AlignWithGrabTexel((screenPos.xy + uvOffset) / screenPos.w);

            float backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
            float surfaceDepth = UNITY_Z_0_FAR_FROM_CLIPSPACE(screenPos.z);
            float depthDifference = backgroundDepth - surfaceDepth;

            uvOffset *= saturate(depthDifference);
            uv = AlignWithGrabTexel((screenPos.xy + uvOffset) / screenPos.w);
            backgroundDepth =
                LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv));
            depthDifference = backgroundDepth - surfaceDepth;

            float3 backgroundColor = tex2D(_WaterBackground, uv).rgb;
            float fogFactor = exp2(-_AbsorptionStrength * depthDifference);
            return lerp(_Color.rgb, backgroundColor, fogFactor);
        }

        float3 UnpackDerivativeHeight(float4 textureData)
        {
            float3 dh = textureData.agb;
            dh.xy = dh.xy * 2 - 1;
            return dh;
        }

        float2 DirectionalFlowUV(float2 uv, float3 flowVectorAndSpeed, float tiling, float time, out float2x2 rotation)
        {
            float2 dir = normalize(flowVectorAndSpeed.xy);
            rotation = float2x2(dir.y, dir.x, -dir.x, dir.y);
            uv = mul(float2x2(dir.y, -dir.x, dir.x, dir.y), uv);
            uv.y -= time * flowVectorAndSpeed.z;
            return uv * tiling;
        }

        float3 FlowCell(float2 uv, float2 offset, float time)
        {
            float2 shift = 1 - offset;
            shift *= 0.5;
            offset *= 0.5;
            float2x2 derivRotation;
            float2 uvTiled =
                (floor(uv * _GridResolution + offset) + shift) / _GridResolution;
            float3 flow = tex2D(_FlowMap, uvTiled).rgb;
            flow.xy = flow.xy * 2 - 1;
            flow.z *= _FlowStrength;
            float tiling = flow.z * _TilingModulated + _Tiling;
            float2 uvFlow = DirectionalFlowUV(
                uv, flow, tiling, time,
                derivRotation
            );
            float3 dh = UnpackDerivativeHeight(tex2D(_NormalMap, uvFlow));
            dh.xy = mul(derivRotation, dh.xy);
            dh *= flow.z * _HeightScaleModulated + _HeightScale;
            return dh;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            // fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            // o.Albedo = c.rgb;
            float time = _Time.y * _Speed;
            float2 uv = IN.uv_MainTex;
            float3 dhA = FlowCell(uv, float2(0, 0), time);
            float3 dhB = FlowCell(uv, float2(1, 0), time);
            float3 dhC = FlowCell(uv, float2(0, 1), time);
            float3 dhD = FlowCell(uv, float2(1, 1), time);
            float2 t = abs(2 * frac(uv * _GridResolution) - 1);
            float wA = (1 - t.x) * (1 - t.y);
            float wB = t.x * (1 - t.y);
            float wC = (1 - t.x) * t.y;
            float wD = t.x * t.y;
            float3 dh = dhA * wA + dhB * wB + dhC * wC + dhD * wD;
            o.Normal = normalize(float3(-dh.xy, 1));

            #if _ABSORPTION_ON
            o.Albedo = ColorBelowWaterAbsorb(IN.screenPos); // + _Color.rgb;
            #else
            o.Emission = ColorBelowWater(IN.screenPos, o.Normal); // + _Color.rgb;
            #endif
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = _Color.a;
        }

        void ResetAlpha(Input IN, SurfaceOutputStandard o, inout fixed4 color)
        {
            color.a = 1;
        }
        ENDCG
    }
    //    FallBack "Diffuse"
}