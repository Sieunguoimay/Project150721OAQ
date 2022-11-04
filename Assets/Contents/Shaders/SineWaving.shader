Shader "Custom/SineWaving"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5

        [Toggle] _Anim ("Anim",Float) = 0

        [ShowIf(_Anim)]_Speed ("Speed", Range(0,5)) = 0.0
        _Frequency ("Frequency", Range(0,5)) = 0.0
        _AmplitudeX ("AmplitudeX", Range(0,5)) = 0.0
        _AmplitudeY ("AmplitudeY", Range(0,5)) = 0.0
        _AmplitudeZ ("AmplitudeZ", Range(0,5)) = 0.0
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
        #pragma surface surf Standard fullforwardshadows addshadow
        #pragma vertex vert
        #pragma multi_compile __ _ANIM_ON

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        
        #if _ANIM_ON
        half _Speed;
        half _Frequency;
        half _AmplitudeX;
        half _AmplitudeY;
        half _AmplitudeZ;
        #endif
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            #if _ANIM_ON
            const float distance = (v.vertex.y * v.vertex.y + v.vertex.x * v.vertex.x + v.vertex.z * v.vertex.z);
            const float c = cos((distance + _Time.y * _Speed) * _Frequency) * distance;
            v.vertex.x += c * _AmplitudeX;
            v.vertex.y += c * _AmplitudeY;
            v.vertex.z += c * _AmplitudeZ;
            #endif
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