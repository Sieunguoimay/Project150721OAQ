Shader "Npu/Basic"{
    Properties {
        _MainTex ("Diffuse Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1.0,1.0,1.0,1.0)
		_Index ("Index", int) = 0
       
		[Header(Light)]
		_LightIntensity ("Light Intensity", Range(0, 1.5)) = 1
		_DiffuseStrength ("Diffuse Factor", Range(0, 1)) = 0.35
		
		[Header(Shadow)]
        _ShadowColor ("Shadow Color", Color) = (0,0,0,1.0)
        _ShadowStrength ("Shadow Factor", Range(0, 1)) = 0.5	
    }
    SubShader {
        Pass {
            Tags {"LightMode" = "ForwardBase"}
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma exclude_renderers flash
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight	
            #pragma multi_compile_instancing		

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"
            #include "AutoLight.cginc"
                        
            uniform sampler2D _MainTex;
            // uniform half4 _MainTex_ST;
            uniform half4 _Color;
            uniform half _DiffuseStrength;
            uniform half4 _ShadowColor;
            uniform half _ShadowStrength;
            uniform half _LightIntensity;
            // uniform int _Index;
            
            UNITY_INSTANCING_BUFFER_START(InstanceProperties)
            UNITY_DEFINE_INSTANCED_PROP(half4, _MainTex_ST)
            UNITY_DEFINE_INSTANCED_PROP(int, _Index)
            UNITY_INSTANCING_BUFFER_END(InstanceProperties)
            
            uniform half4 _GlobalTint; 
            uniform int _GlobalIndex; 

			struct vertexInput{
                half4 vertex : POSITION;
                half3 normal : NORMAL;
                half2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct vertexOutput{
                UNITY_VERTEX_INPUT_INSTANCE_ID
                half4 pos : SV_POSITION;
                half2 uv : TEXCOORD0;
                half3 worldPos : TEXCOORD1;
            	half nl : COLOR2;
                half4 diffuse : COLOR;
            	SHADOW_COORDS(2)
            };
          

            vertexOutput vert (vertexInput v)
			{
                vertexOutput o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                half4 st = UNITY_ACCESS_INSTANCED_PROP(InstanceProperties, _MainTex_ST);

            	half3 worldNormal = UnityObjectToWorldNormal(v.normal);

				o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv.xy * st.xy + st.zw;
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				o.nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diffuse = o.nl;
				o.diffuse.rgb += ShadeSH9(half4(worldNormal, 1));
				
				TRANSFER_SHADOW(o);

				return o;
			}
            
            half4 frag(vertexOutput i) : COLOR
            {
            	UNITY_SETUP_INSTANCE_ID(i);

                int index = UNITY_ACCESS_INSTANCED_PROP(InstanceProperties, _Index);
            	half4 tint = index < 0 || _GlobalIndex == index ? _Color : _Color * _GlobalTint;
				half4 color = tex2D(_MainTex, i.uv) * tint * _LightColor0 * _LightIntensity;
				color = lerp(color, color * i.diffuse, _DiffuseStrength);

            	half shadow = SHADOW_ATTENUATION(i);
				shadow = saturate(1 - lerp(shadow, 1, 1 - i.nl));
            	color.rgb = lerp(color.rgb, _ShadowColor, _ShadowStrength * shadow);

                return color;
            }
            
            ENDCG
            
        }

        UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"

    }
    //Fallback "Specular"
}