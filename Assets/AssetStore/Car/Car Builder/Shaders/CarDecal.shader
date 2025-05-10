Shader "CarBuilder/Decal"
{
    Properties
    {
		_Color("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
		[KeywordEnum(Fit, Envelope, Strech)] _FittingMode("Fitting mode", Float) = 0
		[KeywordEnum(None, Min, Middle, Max)] _AnchorMode("Anchor mode", Float) = 0
		[KeywordEnum(Zero, One, Two)] _UVChannel("UV Channel", Float) = 0

    }
    SubShader
    {
        Tags 
		{ 
			"RenderType"="Opaque"
			"Queue" = "Transparent"
		}
        LOD 100

		Blend One OneMinusSrcAlpha
		ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
			half4 _Color;
			float _FittingMode;
			float _AnchorMode;
			float _UVChannel;
			
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv = v.uv;
				#pragma multi_compile _UVCHANNEL_ZERO _UVCHANNEL_ONE _UVCHANNEL_TWO
				#if defined(_UVCHANNEL_ONE)
					o.uv = v.uv1;	
				#elif defined(_UVCHANNEL_TWO)
					o.uv = v.uv2;
				#endif

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				// assume the provided texture is 1:1
				float2 uv = i.uv.xy;
				float aspectRatio = i.uv.z;

				#pragma multi_compile _FITTINGMODE_FIT _FITTINGMODE_STRECH _FITTINGMODE_ENVELOPE
				#pragma multi_compile _ANCHORMODE_NONE _ANCHORMODE_MIDDLE _ANCHORMODE_MAX _ANCHORMODE_MIN
				

				#if defined(_FITTINGMODE_FIT)
				
					if(aspectRatio > 1)
					{
						float range = aspectRatio;
						uv.x *= aspectRatio;

						#if defined(_ANCHORMODE_MIDDLE)
							uv.x -= range / 2 - 0.5;
						#elif defined(_ANCHORMODE_MAX)
							uv.x += 1 - range;
						#endif
					}
					else
					{
						float range = 1/aspectRatio;
						uv.y *= range;

						#if defined(_ANCHORMODE_MIDDLE)
							uv.y -= range / 2 - 0.5;
						#elif defined(_ANCHORMODE_MAX)
							uv.y -= range - 1;
						#endif
					}


				#elif defined(_FITTINGMODE_STRECH)

					// nothing changes 
				
				#elif defined(_FITTINGMODE_ENVELOPE)

					if(aspectRatio > 1)
					{
						float range = 1/aspectRatio;
						uv.y *= range;
						uv.y -= range / 2 - 0.5;
					}
					else
					{
						float range = aspectRatio;
						uv.x *= range;
						uv.x -= range / 2 - 0.5;
					}

				#endif

				// keep in 0 - 1
				uv = saturate(uv);

                fixed4 col = tex2D(_MainTex, uv);
				col.rgb *= _Color.rgb;
				
				col.a *= _Color.a;
				col.rgb *= col.a;
			
			
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
