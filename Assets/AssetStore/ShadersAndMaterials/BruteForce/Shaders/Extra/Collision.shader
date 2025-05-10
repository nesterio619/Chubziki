Shader "Unlit/Toon"
{
    Properties
    {
        _Color(" Color (additive only)", Color) = (1,1,1,1)
        _OverallLight("OverallLight", Float) = 1
    }
    SubShader
    {
        Tags {"RenderType" = "Transparent" "LightMode" = "ForwardBase" "DisableBatching" = "True" }
        ZWrite Off
        Cull Front

        Blend OneMinusDstColor One // Soft additive
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float4 hitNormal : TEXCOORD0;
                float4 hitPos : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float4 hitNormal : TEXCOORD0;
                float4 hitPos : TEXCOORD1;
                float3 worldPos: TEXCOORD4;
            };

            float4 _Color;
            float _OverallLight;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.hitNormal = v.hitNormal;
                o.hitPos = v.hitPos;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = _Color;

                col.rgb *= _OverallLight * i.color.a;
                col.a = 1;

                float dotProduct = dot(i.hitNormal.xyz, i.worldPos.xyz - i.hitPos.xyz);
                col.rgb = lerp(col.rgb, float3(0,0,0), saturate(pow(dotProduct*5,5)));

                return col;
            }
            ENDCG
        }
    }
}
