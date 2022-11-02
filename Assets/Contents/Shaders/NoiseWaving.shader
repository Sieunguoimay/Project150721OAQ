Shader "Custom/NoiseWaving"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _NoiseTex ("Noise", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        [Header(Waving)]
        _Speed ("Speed", Range(0,1)) = 0.0
        _NoiseWindowFactor ("Noise Window Factor", Range(0,1)) = 0.01
        _AmplitudeFactor ("Amplitude Factor", Range(0,1)) = 0.5
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
        sampler2D _NoiseTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        half _Speed;
        half _NoiseWindowFactor;
        half _AmplitudeFactor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input o)
        {
            float objectWorldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).x;
            const float waving_factor = distance(v.vertex, float4(0, 0, 0, 1));
            v.vertex.z += _AmplitudeFactor*waving_factor * (tex2Dlod(
                _NoiseTex, float4(_Time.y * _Speed + waving_factor * _NoiseWindowFactor + 1.0, objectWorldPos, 0,
                                  0)).r - 0.25);
            v.vertex.x += _AmplitudeFactor*waving_factor * (tex2Dlod(
                    _NoiseTex, float4(_Time.y * _Speed + waving_factor * _NoiseWindowFactor, objectWorldPos, 0, 0))
                .r - 0.25);

            UNITY_INITIALIZE_OUTPUT(Input, o);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}