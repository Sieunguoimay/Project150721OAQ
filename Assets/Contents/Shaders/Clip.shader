Shader "Custom/Clip"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NormalMap ("Normal", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _LocalClipBoxMin ("_LocalClipBoxMin", Vector) = (-10,-10,-10)
        _LocalClipBoxMax ("_LocalClipBoxMax", Vector) = (10,10,10)
        _ClipLerp ("_ClipLerp", Range(0,1)) = .5
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        #pragma vertex vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;

        struct Input
        {
            float2 uv_MainTex;
            float3 localPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        fixed4 _LocalClipBoxMin;
        fixed4 _LocalClipBoxMax;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            o.localPos = v.vertex.xyz;
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 lowerBounds = lerp(_LocalClipBoxMin.xyz, _LocalClipBoxMax.xyz, _LocalClipBoxMin.w);
            float3 upperBounds = lerp(_LocalClipBoxMax.xyz, _LocalClipBoxMin.xyz, _LocalClipBoxMax.w);
            clip(IN.localPos - lowerBounds);
            clip(upperBounds - IN.localPos);
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
        }
        ENDCG
    }
    FallBack "Diffuse"
}