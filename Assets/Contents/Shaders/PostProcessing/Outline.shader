Shader "Custom/Outline"
{

    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _SceneTex ("Scene Texture", 2D) = "black" {}
    }
    CustomEditor "OutlineMaterialEditor"

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
        }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature OUTLINE_12 OUTLINE_9 OUTLINE_5

            #include "UnityCG.cginc"


            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2_f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SceneTex;


            float2 _MainTex_TexelSize;

            v2_f vert(const appdata v)
            {
                v2_f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = o.vertex.xy / 2 + 0.5;
                return o;
            }

            fixed4 frag(const v2_f i) : SV_Target
            {
                //if something already exists underneath the fragment, discard the fragment.
                if (tex2D(_MainTex, i.uv.xy).r > 0)
                {
                    return tex2D(_SceneTex, i.uv.xy);
                }
                //arbitrary number of iterations for now

                //split texel size into smaller words
                const float tx_x = _MainTex_TexelSize.x;
                const float tx_y = _MainTex_TexelSize.y;

                //and a final intensity that increments based on surrounding intensities.
                float color_intensity_in_radius = 0.0;

                #if OUTLINE_12
                const int number_of_iterations = 12;
                #elif OUTLINE_9
                const int number_of_iterations = 9;
                #elif OUTLINE_5
                const int number_of_iterations = 5;
                #endif

                //for every iteration we need to do horizontally
                for (int k = 0; k < number_of_iterations; k += 1)
                {
                    //for every iteration we need to do vertically
                    for (int j = 0; j < number_of_iterations; j += 1)
                    {
                        //increase our output color by the pixels in the area
                        const half2 offset = float2((k - number_of_iterations / 2) * tx_x,
                                                    (j - number_of_iterations / 2) * tx_y);
                        color_intensity_in_radius += tex2D(_MainTex, i.uv.xy + offset.xy).r;
                    }
                }

                if (color_intensity_in_radius > 0)
                {
                    return half4(1, 1, 1, 1);
                }

                return tex2D(_SceneTex, i.uv.xy);
            }
            ENDCG
        }
    }
}