Shader "Unlit/MeshTest"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}

		_Test01Mapping_Texture("_Test01Mapping_Texture", Range(0,1)) = 0
		_Test02Mapping_Texture("_Test02Mapping_Texture", Range(0,1)) = 0

		[Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
    }
    SubShader
    {
        Tags 
		{ 
			"RenderType"="Opaque" 
		}
        LOD 100

		Cull [_Cull]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
				float2 uv1 : TEXCOORD1;
				float2 uv2 : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

			half4 _Color;

            sampler2D _MainTex;
            float4 _MainTex_ST;

			float _Test01Mapping_Texture;
			float _Test02Mapping_Texture;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv1 = TRANSFORM_TEX(v.uv1, _MainTex);
				o.uv2 = TRANSFORM_TEX(v.uv2, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				float2 uv = i.uv;
				if(_Test01Mapping_Texture > 0.5)
				{
					// using frac to support unity 2017
					uv.x = frac(i.uv1.x);
					uv.y = frac(i.uv1.y);
				}

				if(_Test02Mapping_Texture > 0.5)
				{
					// using frac to support unity 2017
					uv.x = frac(i.uv2.x);
					uv.y = frac(i.uv2.y);
				}

                fixed4 col = tex2D(_MainTex, uv);
				col.rgb *= _Color;
				col.a *= _Color.a;

                return col;
            }
            ENDCG
        }
    }
}
