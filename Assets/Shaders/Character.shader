Shader "Alex/Lambert"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _ShadowIntensity ("Shadowed", Float) = 1.35
    }
    SubShader
    {
        Tags { 
			"Queue" = "Transparent" "IgnoreProjector" = "True" "PreviewType" = "Plane" "RenderType" = "Transparent"
		}       
        
        Pass 
        {
            ZWrite On
            Cull Back
            Lighting On
            /*
            Stencil {
                Ref 0
                Comp GEqual
                Pass Replace 
                Fail Keep 
            }*/
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            half4 _Color;
    uniform half4 _MainTex_ST;
    uniform half4 _LightColor0;
            half _ShadowIntensity;

            struct v2f {
                half4 pos : SV_POSITION;
                half3 uv : TEXCOORD0;
            }; 
            
            struct a2v_simple {
                float4 vertex : POSITION;
                float4 texcoord : TEXCOORD0;
                float4 normal : NORMAL;
            };

            v2f vert(a2v_simple v)
            {
                v2f o;
                
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
                
                o.uv.z = dot(normalize(v.normal), ObjSpaceLightDir(v.vertex));
                return o;
            }
            
            half4 frag(v2f i) : COLOR 
            {                        
                half4 c = tex2D(_MainTex, i.uv.xy);
                c.rgb *= _Color.rgb * i.uv.z * _ShadowIntensity + 1;
                
                c.rgb *= UNITY_LIGHTMODEL_AMBIENT.rgb * _LightColor0.rgb;

                c.a = 1;
                return c;
            }
            ENDCG
        }
    }
    FallBack "Mobile/Diffuse"
}
