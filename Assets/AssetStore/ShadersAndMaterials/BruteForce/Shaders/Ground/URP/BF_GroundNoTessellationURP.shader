// MADE BY MATTHIEU HOULLIER
// Copyright 2023 BRUTE FORCE, all rights reserved.
// You are authorized to use this work if you have purchased the asset.
// Mail me at bruteforcegamesstudio@gmail.com if you have any questions or improvements you need.
Shader "BruteForce/URP/GroundNoTessellationURP" {

	Properties{

		[Header(IIIIIIII          Ground Textures          IIIIIIII)]
		[Space]
		_MainTex("Ground Albedo", 2D) = "white" {}
		_MainTexMult("Ground Albedo Saturation", Range(0,2)) = 0.11
		[MainColor][HDR]_Color("Ground Tint", Color) = (0.77,0.86,0.91,1)
		_OverallScale("Overall Scale", Float) = 1
		[Space]
		[NoScaleOffset]_GroundBumpMap("Ground Bumpmap", 2D) = "white" {}
		_NormalMultiplier("Ground Bumpmap Multiplier", Range(0,5)) = 0.4
		_GroundNormalScale("Ground Bumpmap Scale", Range(0,2)) = 1

		[Space(20)]
		[NoScaleOffset]_GroundSpecGlossMap("Ground Specular", 2D) = "black" {}
		_SpecMult("Spec Multiplier", Float) = 0.5
		[NoScaleOffset]_LittleSpec("Ground Little Spec", 2D) = "black" {}
		_LittleSpecForce("Little Spec Multiplier", Float) = 0.5
		_LittleSpecSize("Little Spec Size", Float) = 3

		[NoScaleOffset]_GroundHeight("Ground Displacement Texture", 2D) = "white" {}
		_HeightScale("Displacement Scale", Float) = 0.33
		_DisplacementStrength("Displacement Strength", Float) = 0.3
		_DisplacementOffset("Displacement Offset", Float) = 0.1
		_DisplacementColorMult("Displacement Color Multiplier", Float) = 0.95
		_DisplacementShadowMult("Displacement Shadow Multiplier",  Range(0,2)) = 0.56
		_UpVector("Up Vector", Float) = 1
		_NormalVector("Normal Vector", Float) = 0

		[Space(10)]
		[Header(IIIIIIII          Ground Values          IIIIIIII)]
		[Space(10)]
		_GroundScale("Ground Scale", float) = 1
		_BotColor("Dig Color", color) = (0.71,0.87,0.91,0)
		_NormalRTDepth("Normal Effect Depth", Range(0,3)) = 0.12
		_NormalRTStrength("Normal Effect Strength", Range(0,4)) = 2.2
		_RemoveGroundStrength("Dig Ground Strength", Range(0,3)) = 0.5

		[Space(10)]
		[Header(IIIIIIII          Tiling Textures          IIIIIIII)]
		[Space(10)]
		[NoScaleOffset]_TilingTex("Tiling Albedo", 2D) = "white" {}
		_TilingSaturation("Tiling Saturation", Float) = 1
		[HDR]_TilingTint("Tiling Texture Tint", Color) = (0.14,0.35,0.49,1)
		[Space]
		[NoScaleOffset]_NormalTex("Tiling Normal Texture", 2D) = "black" {}
		_NormalScale("Tiling Normal Scale", Range(0,3)) = 0.766
		[NoScaleOffset]_Roughness("Tiling Roughness Texture", 2D) = "black" {}
		[NoScaleOffset]_ParallaxMapTiling("Tiling Height map (R)", 2D) = "white" {}
		_HeightTileScale("Displacement Tile Scale", Float) = 0.33
		_DisplacementTileStrength("Displacement Tile Strength", Float) = 0.3
		_DisplacementTileOffset("Displacement Tile Offset", Float) = 0.1
		_ParallaxValue("Height scale", Range(0, 0.02)) = 0.005
		_ParallaxMinSamples("Parallax min samples", Range(2, 100)) = 4
		_ParallaxMaxSamples("Parallax max samples", Range(2, 100)) = 20

		[Space(10)]
		[Header(IIIIIIII          Tiling Values          IIIIIIII)]
		[Space(10)]
		_TilingScale("Tiling Scale", float) = 1
		_TilingTrail("Tiling Trail Color", Color) = (0.40,0.1,0.01,1)
		_AOValue("Ambient Occlusion", Range(0,1)) = 1
		_TransparencyValue("Tiling Transparency", Range(0,1)) = 1
		_TilingAngle("Tiling Angle", float) = 0

		[Space(10)]
		[Header(IIIIIIII          Mud Values          IIIIIIII)]
		[Space(10)]
		_MudScale("Mud Scale", float) = 1
		_MudWaveHeight("Mud Wave Height", float) = 1
		_MudHeight("Mud Water Height Offset", float) = 1
		[NoScaleOffset]_MudTex("Mud Albedo", 2D) = "white" {}
		_MudColor("Mud Color", color) = (0.71,0.87,0.91,0)
		[NoScaleOffset]_MudWater("Mud Water Albedo", 2D) = "white" {}
		[HDR]_MudWaterColor("Mud Water Color", color) = (0.71,0.87,0.91,0)
		_MudWaterSpecularColor("Mud Water Specular Color", color) = (0.71,0.87,0.91,0)
		[NoScaleOffset]_MudSpecular("Mud Specular", 2D) = "white" {}
		_MudSpecularMultiplier("Mud Specular Multiplier", float) = 1
		[NoScaleOffset]_MudWaterSpecular("Mud Water Specular", 2D) = "white" {}
		[NoScaleOffset]_MudNormal("Mud Normal", 2D) = "white" {}
		_MudNormalMultiplier("Mud Normal Multiplier", float) = 1
		[NoScaleOffset]_MudWaterNormal("Mud Water Normal", 2D) = "white" {}
		_MudWaterNormalMultiplier("Mud Water Normal Multiplier", float) = 1
		_WaterScale("Water Scale", float) = 1
		_WaterSpeed("Water Speed", float) = 1
		_WaterLevel("Water Level", Range(-1,1)) = 0

		[Space(10)]
		[Header(IIIIIIII          Custom Fog          IIIIIIII)]
		[Space(10)]
		[NoScaleOffset]_FogTex("Fog Texture", 2D) = "black" {}
		[NoScaleOffset]_FlowTex("Flow Texture", 2D) = "black" {}
		_FlowMultiplier("Flow Multiplier", Range(0,1)) = 0.3
		_FogIntensity("Fog Intensity", Range(0,1)) = 0.3
		_FogColor("Fog Color", Color) = (1.0,1.0,1.0,1.0)
		_FogScale("Fog Scale", float) = 1
		_FogDirection("Fog Direction", vector) = (1, 0.3, 2, 0)

		[Space(10)]
		[Header(IIIIIIII          Lighting          IIIIIIII)]
		[Space(10)]
		_ProjectedShadowColor("Projected Shadow Color",Color) = (0.17 ,0.56 ,0.1,1)
		_SpecColor("Specular Color", Color) = (1,1,1,1)
		_SpecForce("Ground Specular Force", Float) = 3
		_ShininessGround("Ground Shininess", Float) = 25
		[Space]
		_RoughnessStrength("Tiling Roughness Strength", Float) = 1.75
		_ShininessTiling("Tiling Shininess", Float) = 10
		[Space]
		_LightOffset("Light Offset", Range(0, 1)) = 0.2
		_LightHardness("Light Hardness", Range(0, 1)) = 0.686
		_RimColor("Rim Ground Color", Color) = (0.03,0.03,0.03,0)
		_LightIntensity("Additional Lights Intensity", Range(0.00, 4)) = 1


		[Header(Procedural Tiling)]
		[Space]
		[Toggle(USE_PR)] _UsePR("Use Procedural Tiling (Reduce performance)", Float) = 0
		_ProceduralDistance("Tile start distance", Float) = 5.5
		_ProceduralStrength("Tile Smoothness", Float) = 1.5
		[Space]

		[Space(10)]
		[Header(IIIIIIII          Pragmas          IIIIIIII)]
		[Space(10)]
		[Toggle(IS_TILING)] _ISTILING("Is Only Tiling", Float) = 0
		[Toggle(IS_GROUND)] _ISGROUND("Is Only Ground", Float) = 0
		[Toggle(IS_UNLIT)] _ISUNLIT("Is Unlit", Float) = 0
		[Toggle(USE_AL)] _UseAmbientLight("Use Ambient Light", Float) = 1
			[Toggle(USE_DECAL)] _USEDECAL("Use Decal", Float) = 0
		[Toggle(USE_RT)] _USERT("Use RT", Float) = 1
		[Toggle(IS_T)] _IST("Is Terrain", Float) = 0
		[Toggle(USE_VR)] _UseVR("Use For VR", Float) = 0
		[Toggle(USE_WT)] _USEWT("Use World Coordinates", Float) = 0
		[Toggle(USE_FOG)] _USEFOG("Use Custom Fog", Float) = 1
		[Toggle(USE_LOW)] _USELOW("Use Low End", Float) = 0
			[Toggle(USE_DS)] _USEDOUBLESPECULAR("Use Double Specular", Float) = 1


			// TERRAIN PROPERTIES //
			[HideInInspector] _TerrainHolesTexture("Holes Map (RGB)", 2D) = "white" {}
			[HideInInspector] _Control0("Control0 (RGBA)", 2D) = "white" {}
			[HideInInspector] _Control1("Control1 (RGBA)", 2D) = "white" {}
			// Textures
			[HideInInspector] _Splat0("Layer 0 (R)", 2D) = "white" {}
			[HideInInspector] _Splat1("Layer 1 (G)", 2D) = "white" {}
			[HideInInspector] _Splat2("Layer 2 (B)", 2D) = "white" {}
			[HideInInspector] _Splat3("Layer 3 (A)", 2D) = "white" {}
			[HideInInspector] _Splat4("Layer 4 (R)", 2D) = "white" {}
			[HideInInspector] _Splat5("Layer 5 (G)", 2D) = "white" {}
			[HideInInspector] _Splat6("Layer 6 (B)", 2D) = "white" {}
			[HideInInspector] _Splat7("Layer 7 (A)", 2D) = "white" {}

			// Normal Maps
			[HideInInspector] _Normal0("Normal 0 (R)", 2D) = "bump" {}
			[HideInInspector] _Normal1("Normal 1 (G)", 2D) = "bump" {}
			[HideInInspector] _Normal2("Normal 2 (B)", 2D) = "bump" {}
			[HideInInspector] _Normal3("Normal 3 (A)", 2D) = "bump" {}
			[HideInInspector] _Normal4("Normal 4 (R)", 2D) = "bump" {}
			[HideInInspector] _Normal5("Normal 5 (G)", 2D) = "bump" {}
			[HideInInspector] _Normal6("Normal 6 (B)", 2D) = "bump" {}
			[HideInInspector] _Normal7("Normal 7 (A)", 2D) = "bump" {}

			// Normal Scales
			[HideInInspector] _NormalScale0("Normal Scale 0 ", Float) = 1
			[HideInInspector] _NormalScale1("Normal Scale 1 ", Float) = 1
			[HideInInspector] _NormalScale2("Normal Scale 2 ", Float) = 1
			[HideInInspector] _NormalScale3("Normal Scale 3 ", Float) = 1
			[HideInInspector] _NormalScale4("Normal Scale 4 ", Float) = 1
			[HideInInspector] _NormalScale5("Normal Scale 5 ", Float) = 1
			[HideInInspector] _NormalScale6("Normal Scale 6 ", Float) = 1
			[HideInInspector] _NormalScale7("Normal Scale 7 ", Float) = 1

				// Mask Maps
				[HideInInspector] _Mask0("Mask 0 (R)", 2D) = "bump" {}
				[HideInInspector] _Mask1("Mask 1 (G)", 2D) = "bump" {}
				[HideInInspector] _Mask2("Mask 2 (B)", 2D) = "bump" {}
				[HideInInspector] _Mask3("Mask 3 (A)", 2D) = "bump" {}
				[HideInInspector] _Mask4("Mask 4 (R)", 2D) = "bump" {}
				[HideInInspector] _Mask5("Mask 5 (G)", 2D) = "bump" {}
				[HideInInspector] _Mask6("Mask 6 (B)", 2D) = "bump" {}
				[HideInInspector] _Mask7("Mask 7 (A)", 2D) = "bump" {}

				// specs color
				[HideInInspector] _Specular0("Specular 0 (R)", Color) = (1,1,1,1)
				[HideInInspector] _Specular1("Specular 1 (G)", Color) = (1,1,1,1)
				[HideInInspector] _Specular2("Specular 2 (B)", Color) = (1,1,1,1)
				[HideInInspector] _Specular3("Specular 3 (A)", Color) = (1,1,1,1)
				[HideInInspector] _Specular4("Specular 4 (R)", Color) = (1,1,1,1)
				[HideInInspector] _Specular5("Specular 5 (G)", Color) = (1,1,1,1)
				[HideInInspector] _Specular6("Specular 6 (B)", Color) = (1,1,1,1)
				[HideInInspector] _Specular7("Specular 7 (A)", Color) = (1,1,1,1)

					// Metallic
					[HideInInspector] _Metallic0("Metallic0", Float) = 0
					[HideInInspector] _Metallic1("Metallic1", Float) = 0
					[HideInInspector] _Metallic2("Metallic2", Float) = 0
					[HideInInspector] _Metallic3("Metallic3", Float) = 0
					[HideInInspector] _Metallic4("Metallic4", Float) = 0
					[HideInInspector] _Metallic5("Metallic5", Float) = 0
					[HideInInspector] _Metallic6("Metallic6", Float) = 0
					[HideInInspector] _Metallic7("Metallic7", Float) = 0

					[HideInInspector] _Splat0_ST("Size0", Vector) = (1,1,0)
					[HideInInspector] _Splat1_ST("Size1", Vector) = (1,1,0)
					[HideInInspector] _Splat2_ST("Size2", Vector) = (1,1,0)
					[HideInInspector] _Splat3_ST("Size3", Vector) = (1,1,0)
					[HideInInspector] _Splat4_STn("Size4", Vector) = (1,1,0)
					[HideInInspector] _Splat5_STn("Size5", Vector) = (1,1,0)
					[HideInInspector] _Splat6_STn("Size6", Vector) = (1,1,0)
					[HideInInspector] _Splat7_STn("Size7", Vector) = (1,1,0)

					[HideInInspector] _TerrainScale("Terrain Scale", Vector) = (1, 1 ,0)
					// TERRAIN PROPERTIES //
	}

		HLSLINCLUDE

#pragma shader_feature IS_T
#pragma shader_feature IS_TILING
#pragma shader_feature IS_GROUND
#pragma shader_feature USE_VR
#pragma shader_feature USE_COMPLEX_T
#pragma shader_feature USE_DECAL

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

#ifdef IS_T
				// TERRAIN DATA //
				sampler2D _Control0;
#ifdef USE_COMPLEX_T
				sampler2D _Control1;
#endif
				half4 _Specular0, _Specular1, _Specular2, _Specular3;
				float4 _Splat0_ST, _Splat1_ST, _Splat2_ST, _Splat3_ST;
				half _Metallic0, _Metallic1, _Metallic2, _Metallic3;
				half _NormalScale0, _NormalScale1, _NormalScale2, _NormalScale3;
				Texture2D _Splat0, _Splat1, _Splat2, _Splat3;
				Texture2D _Normal0, _Normal1, _Normal2, _Normal3;

				Texture2D _Mask0, _Mask1, _Mask2, _Mask3;

#ifdef USE_COMPLEX_T
				half4 _Specular4, _Specular5, _Specular6, _Specular7;
				float4 _Splat4_STn, _Splat5_STn, _Splat6_STn, _Splat7_STn;
				half _Metallic4, _Metallic5, _Metallic6, _Metallic7;
				half _NormalScale4, _NormalScale5, _NormalScale6, _NormalScale7;
				Texture2D _Splat4, _Splat5, _Splat6, _Splat7;
				Texture2D _Normal4, _Normal5, _Normal6, _Normal7;
				Texture2D _Mask4, _Mask5, _Mask6, _Mask7;
#endif

				float3 _TerrainScale;
				// TERRAIN DATA //
#endif
				SamplerState my_linear_repeat_sampler;
				SamplerState my_bilinear_repeat_sampler;
				SamplerState my_trilinear_repeat_sampler;
				SamplerState my_linear_clamp_sampler;

				void parallax_vert(
					float4 vertex,
					float3 normal,
					float4 tangent,
					out float4 eye
				) {
					float4x4 mW = unity_ObjectToWorld;
					float3 binormal = cross(normal, tangent.xyz) * tangent.w;
					float3 EyePosition = _WorldSpaceCameraPos;

					float4 localCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1));
					float3 eyeLocal = vertex - localCameraPos;
					float4 eyeGlobal = mul(float4(eyeLocal, 1), mW);
					float3 E = eyeGlobal.xyz;

					float3x3 tangentToWorldSpace;

					tangentToWorldSpace[0] = mul(normalize(tangent), mW);
					tangentToWorldSpace[1] = mul(normalize(binormal), mW);
					tangentToWorldSpace[2] = mul(normalize(normal), mW);

					float3x3 worldToTangentSpace = transpose(tangentToWorldSpace);

					eye.xyz = mul(E, worldToTangentSpace);
					eye.w = 1 - dot(normalize(E), -normal);
				}

				float2 parallax_offset(
					float fHeightMapScale,
					float4 eye,
					float2 texcoord,
					sampler2D heightMap,
					texture2D heightMapTex,
					int nMinSamples,
					int nMaxSamples,
					int index
				) { // 0 = Sampler2D - 1 = Texture2D //

					float fParallaxLimit = -length(eye.xy) / eye.z;
					fParallaxLimit *= fHeightMapScale;

					float2 vOffsetDir = normalize(eye.xy);
					float2 vMaxOffset = vOffsetDir * fParallaxLimit;

					int nNumSamples = (int)lerp(nMinSamples, nMaxSamples, saturate(eye.w));

					float fStepSize = 1.0 / (float)nNumSamples;

					float2 dx = ddx(texcoord);
					float2 dy = ddy(texcoord);

					float fCurrRayHeight = 1.0;
					float2 vCurrOffset = float2(0, 0);
					float2 vLastOffset = float2(0, 0);

					float fLastSampledHeight = 1;
					float fCurrSampledHeight = 1;

					int nCurrSample = 0;

					while (nCurrSample < nNumSamples)
					{
						if (index == 0)
						{
							fCurrSampledHeight = tex2Dgrad(heightMap, texcoord + vCurrOffset, dx, dy).r;
						}
						else
						{
							fCurrSampledHeight = heightMapTex.SampleGrad(my_linear_repeat_sampler, texcoord + vCurrOffset, dx, dy).r;
						}
						if (fCurrSampledHeight > fCurrRayHeight)
						{
							float delta1 = fCurrSampledHeight - fCurrRayHeight;
							float delta2 = (fCurrRayHeight + fStepSize) - fLastSampledHeight;

							float ratio = delta1 / (delta1 + delta2);

							vCurrOffset = (ratio)*vLastOffset + (1.0 - ratio) * vCurrOffset;

							nCurrSample = nNumSamples + 1;
						}
						else
						{
							nCurrSample++;

							fCurrRayHeight -= fStepSize;

							vLastOffset = vCurrOffset;
							vCurrOffset += fStepSize * vMaxOffset;

							fLastSampledHeight = fCurrSampledHeight;
						}
					}

					return vCurrOffset;
				}

				ENDHLSL

					SubShader{

						Pass {
							Tags {
							"RenderPipeline" = "UniversalRenderPipeline"
							}
								//Blend SrcAlpha OneMinusSrcAlpha

							HLSLPROGRAM
					// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
					#pragma exclude_renderers gles

												#pragma target 4.6


												#pragma multi_compile _ LOD_FADE_CROSSFADE

												#pragma multi_compile_fwdbase
												#pragma multi_compile_fog
												#pragma multi_compile _ LIGHTMAP_ON

												#pragma vertex vert
												#pragma fragment frag

												#define FORWARD_BASE_PASS
												#pragma shader_feature USE_AL
												#pragma shader_feature USE_RT
												#pragma shader_feature USE_WT
												#pragma shader_feature USE_LOW
												#pragma shader_feature USE_DS
												#pragma shader_feature IS_UNLIT
												#pragma shader_feature USE_PR
												#pragma shader_feature USE_FOG

												#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
												#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
												#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
												#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
												#pragma multi_compile _ _SHADOWS_SOFT
												#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE

										struct VertexData //appdata
										{
											float4 vertex : POSITION;
											float3 normal : NORMAL;
											float4 tangent : TANGENT;
											float2 uv : TEXCOORD0;
											float4 color : COLOR;

											float4 shadowCoord : TEXCOORD1;

											float fogCoord : TEXCOORD2;
					#ifdef USE_VR
											UNITY_VERTEX_INPUT_INSTANCE_ID
					#endif
											float4 eye: TEXCOORD5;
										};

										struct InterpolatorsVertex
										{
											float4 vertex : SV_POSITION;
											float3 normal : TEXCOORD1;
											float4 tangent : TANGENT;
											float4 uv : TEXCOORD0;
											float4 color : COLOR;
											float3 worldPos : TEXCOORD2;
											float3 viewDir: POSITION1;
											float3 normalDir: TEXCOORD3;

											float4 shadowCoord : TEXCOORD4;

											float fogCoord : TEXCOORD5;
					#ifdef USE_VR
												UNITY_VERTEX_OUTPUT_STEREO
					#endif

												float4 eye: TEXCOORD6;

					#ifdef LIGHTMAP_ON
												float4 shadowBaked : TEXCOORD7;
					#endif
										};

										sampler2D  _DetailTex;
										float4 _MainTex_ST, _DetailTex_ST;

										sampler2D _NormalMap;

										half4 _Color;

										sampler2D _TerrainHolesTexture;
										// Render Texture Effects //
										uniform sampler2D _GlobalEffectRT;
										uniform float3 _Position;
										uniform float _OrthographicCamSize;
										uniform sampler2D _GlobalEffectRTAdditional;
										uniform float3 _PositionAdd;
										uniform float _OrthographicCamSizeAdditional;

										sampler2D _MainTex;
										sampler2D _LittleSpec;
										sampler2D _GroundBumpMap;
										sampler2D _GroundSpecGlossMap;


										float _ParallaxValue;
										float _TilingAngle;
										uint _ParallaxMinSamples;
										uint _ParallaxMaxSamples;
										sampler2D _ParallaxMapTiling;
										Texture2D _MudTex;
										sampler2D _MudSpecular;
										Texture2D _MudNormal;

										half4 _BotColor;
										half4 _MudWaterSpecularColor;

										float _SpecForce, _SpecMult, _LittleSpecSize, _LittleSpecForce, _UpVector, _NormalVector, _TilingScale, _GroundScale, _WaterScale , _WaterSpeed, _WaterLevel, _TransparencyValue, _AOValue;
										float _NormalRTDepth, _NormalRTStrength, _RemoveGroundStrength, _DisplacementStrength, _DisplacementTileStrength, _NormalMultiplier;

										//Tiling Variables
										sampler2D _TilingTex;
										sampler2D _NormalTex;
										sampler2D _Roughness;
										sampler2D _GroundHeight;
										Texture2D _MudWater;
										Texture2D _MudWaterNormal;
										float _HeightScale;
										float _HeightTileScale;
										float _LightOffset;
										float _LightHardness;
										float _LightIntensity;
										float _TilingSaturation;
										float _MudScale;
										float _MudSpecularMultiplier;
										float _MudNormalMultiplier;
										float _MudWaterNormalMultiplier;
										float _MudHeight;
										float _MudWaveHeight;
										float _DisplacementColorMult, _DisplacementShadowMult;
										float _FogIntensity, _FogScale, _FlowMultiplier;
										float4 _FogColor;
										float4 _MudColor;
										sampler2D _MudWaterSpecular;
										float4 _MudWaterColor;

										Texture2D _FogTex;
										Texture2D _FlowTex;

										half _OffsetScale;
										half _OverallScale;
										half _RoughnessStrength;

										half _NormalScale, _DisplacementOffset, _DisplacementTileOffset, _GroundNormalScale, _MainTexMult;
										half4 _TilingTint;
										half4 _TilingTrail;

										float _ShininessTiling, _ShininessGround;
										float _HasRT;
										float4 _ProjectedShadowColor, _RimColor;
										float _ProceduralDistance, _ProceduralStrength;
										float3 _FogDirection;


										float3 calcNormal(float2 texcoord, sampler2D globalEffect)
										{
											const float3 off = float3(-0.0005 * _NormalRTDepth, 0, 0.0005 * _NormalRTDepth); // texture resolution to sample exact texels
											const float2 size = float2(0.002, 0.0); // size of a single texel in relation to world units

					#ifdef USE_LOW

											float sS = tex2Dlod(globalEffect, float4(texcoord.xy + 1 * off.xy * 10, 0, 0)).y;
											float s01 = sS * 0.245945946 * _NormalRTDepth;
											float s21 = sS * 0.216216216 * _NormalRTDepth;
											float s10 = sS * 0.540540541 * _NormalRTDepth;
											float s12 = sS * 0.162162162 * _NormalRTDepth;

											float gG = tex2Dlod(globalEffect, float4(texcoord.xy + 1 * off.xy, 0, 0)).z;
											float g01 = gG * 1.945945946 * _NormalRTDepth;
											float g21 = gG * 1.216216216 * _NormalRTDepth;
											float g10 = gG * 0.540540541 * _NormalRTDepth;
											float g12 = gG * 0.162162162 * _NormalRTDepth;

											float3 va = normalize(float3(size.xy, 0)) + normalize(float3(size.xy, g21 - g01));
											float3 vb = normalize(float3(size.yx, 0)) + normalize(float3(size.xy, g12 - g10));

											float3 vc = normalize(float3(size.xy, 0)) + normalize(float3(size.xy, s21 - s01));
											float3 vd = normalize(float3(size.yx, 0)) + normalize(float3(size.xy, s12 - s10));

											float3 calculatedNormal = normalize(cross(va, vb));
											calculatedNormal.y = normalize(cross(vc, vd)).x;
											return calculatedNormal;

					#else

											float s01 = tex2Dlod(globalEffect, float4(texcoord.xy + 4 * off.xy * 10, 0, 0)).y * 0.245945946 * _NormalRTDepth;
											float s21 = tex2Dlod(globalEffect, float4(texcoord.xy + 3 * off.zy * 10, 0, 0)).y * 0.216216216 * _NormalRTDepth;
											float s10 = tex2Dlod(globalEffect, float4(texcoord.xy + 2 * off.yx * 10, 0, 0)).y * 0.540540541 * _NormalRTDepth;
											float s12 = tex2Dlod(globalEffect, float4(texcoord.xy + 1 * off.yz * 10, 0, 0)).y * 0.162162162 * _NormalRTDepth;

											float g01 = tex2Dlod(globalEffect, float4(texcoord.xy + 4 * off.xy, 0, 0)).z * 1.945945946 * _NormalRTDepth;
											float g21 = tex2Dlod(globalEffect, float4(texcoord.xy + 3 * off.zy, 0, 0)).z * 1.216216216 * _NormalRTDepth;
											float g10 = tex2Dlod(globalEffect, float4(texcoord.xy + 2 * off.yx, 0, 0)).z * 0.540540541 * _NormalRTDepth;
											float g12 = tex2Dlod(globalEffect, float4(texcoord.xy + 1 * off.yz, 0, 0)).z * 0.162162162 * _NormalRTDepth;

											float3 va = normalize(float3(size.xy, 0)) + normalize(float3(size.xy, g21 - g01));
											float3 vb = normalize(float3(size.yx, 0)) + normalize(float3(size.xy, g12 - g10));

											float3 vc = normalize(float3(size.xy, 0)) + normalize(float3(size.xy, s21 - s01));
											float3 vd = normalize(float3(size.yx, 0)) + normalize(float3(size.xy, s12 - s10));

											float3 calculatedNormal = normalize(cross(va, vb));
											calculatedNormal.y = normalize(cross(vc, vd)).x;
											return calculatedNormal;
					#endif
										}

										float4 blendMultiply(float4 baseTex, float4 blendTex, float opacity)
										{
											float4 baseBlend = baseTex * blendTex;
											float4 ret = lerp(baseTex, baseBlend, opacity);
											return ret;
										}

										float2 hash2D2D(float2 s)
										{
											//magic numbers
											return frac(sin(s) * 4.5453);
										}

										//stochastic sampling
										float4 tex2DStochastic(sampler2D tex, float2 UV)
										{
											float4x3 BW_vx;
											float2 skewUV = mul(float2x2 (1.0, 0.0, -0.57735027, 1.15470054), UV * 3.464);

											//vertex IDs and barycentric coords
											float2 vxID = float2 (floor(skewUV));
											float3 barry = float3 (frac(skewUV), 0);
											barry.z = 1.0 - barry.x - barry.y;

											BW_vx = ((barry.z > 0) ?
												float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
												float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0 - barry.y, 1.0 - barry.x)));

											//calculate derivatives to avoid triangular grid artifacts
											float2 dx = ddx(UV);
											float2 dy = ddy(UV);

											float4 stochasticTex = mul(tex2D(tex, UV + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
												mul(tex2D(tex, UV + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
												mul(tex2D(tex, UV + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
											return stochasticTex;
										}
										//stochastic sampling
										float4 tex2DStochasticNormal(sampler2D tex, float2 UV)
										{
											float4x3 BW_vx;
											float2 skewUV = mul(float2x2 (1.0, 0.0, -0.57735027, 1.15470054), UV * 3.464);

											//vertex IDs and barycentric coords
											float2 vxID = float2 (floor(skewUV));
											float3 barry = float3 (frac(skewUV), 0);
											barry.z = 1.0 - barry.x - barry.y;

											BW_vx = ((barry.z > 0) ?
												float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
												float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0 - barry.y, 1.0 - barry.x)));

											//calculate derivatives to avoid triangular grid artifacts
											float2 dx = ddx(UV);
											float2 dy = ddy(UV);

											float4 stochasticTex = float4(mul(UnpackNormalScale(tex2D(tex, UV + hash2D2D(BW_vx[0].xy), dx, dy), _NormalScale), BW_vx[3].x) +
												mul(UnpackNormalScale(tex2D(tex, UV + hash2D2D(BW_vx[1].xy), dx, dy), _NormalScale), BW_vx[3].y) +
												mul(UnpackNormalScale(tex2D(tex, UV + hash2D2D(BW_vx[2].xy), dx, dy), _NormalScale), BW_vx[3].z),1);
											return stochasticTex;
										}

										float3 RGBToHSV(float3 c)
										{
											float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
											float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
											float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));
											float d = q.x - min(q.w, q.y);
											float e = 1.0e-10;
											return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
										}

										float3 HSVToRGB(float3 c)
										{
											float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
											float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
											return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
										}

										InterpolatorsVertex vert(VertexData v) {
											InterpolatorsVertex i;

					#ifdef USE_VR
											UNITY_SETUP_INSTANCE_ID(v);
											//UNITY_INITIALIZE_OUTPUT(InterpolatorsVertex, i);
											//UNITY_TRANSFER_INSTANCE_ID(InterpolatorsVertex, i);
											UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(i);
					#endif

											float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
											float3 originalPos = worldPos;


											float3 rippleMain = 0;
											float3 rippleMainAdditional = 0;

											float ripplesR = 0;
											float ripplesG = 0;
											float ripplesB = 0;

											float uvRTValue = 0;
											float3 mudPos = 0;


											float redValue = saturate(v.color.g + v.color.b);
											float greenValue = (v.color.r + v.color.b) / 2;
											float blueValue = saturate(v.color.r + v.color.g);

											_WaterScale = _WaterScale * 0.01;


					#ifdef USE_RT
											//RT Cam effects
											float2 uv = worldPos.xz - _Position.xz;
											uv = uv / (_OrthographicCamSize * 2);
											uv += 0.5;

											float2 uvAdd = worldPos.xz - _PositionAdd.xz;
											uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
											uvAdd += 0.5;

											if (_HasRT == 1)
											{
												// .b(lue) = Ground Dig / .r(ed) = Ground To Tiling / .g(reen) = Water Effect
												rippleMain = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
												rippleMainAdditional = tex2Dlod(_GlobalEffectRTAdditional, float4(uvAdd, 0, 0));
											}

											float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
											uvRTValue = saturate(uvGradient.x);

											ripplesR = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
											ripplesG = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
											ripplesB = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);

											v.color.g = saturate(v.color.g - ripplesR);

											redValue = saturate(v.color.g + v.color.b - ripplesR);
											greenValue = (v.color.r + v.color.b) / 2;
											blueValue = saturate(v.color.r + v.color.g);

											ripplesB = ripplesB + (1 - blueValue);
					#else

											ripplesB = ripplesB + (1 - blueValue);
					#endif


					#ifdef IS_T
											i.uv.xy = v.uv;

											float4 tilingUV = float4(i.uv.xy, 0, 0) * _TilingScale;

											float rotationAngle = _TilingAngle * 3.14156 / 180.0;
											float sina, cosa;
											sincos(rotationAngle, sina, cosa);
											float2x2 m = float2x2(cosa, -sina, sina, cosa);
											tilingUV = float4(mul(m, tilingUV), 0, 0);

											_GroundScale = _GroundScale * _OverallScale * 10;
											_TilingScale = _TilingScale * _OverallScale * 10;
											_DisplacementStrength = _DisplacementStrength * 0.1;
											_DisplacementTileStrength = _DisplacementTileStrength * 0.1;
					#else

					#ifdef USE_WT
											i.uv.xy = float2(worldPos.x + _MainTex_ST.z, worldPos.z + _MainTex_ST.w) * _OverallScale * 0.05;
					#else
											i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) * _OverallScale;
					#endif

											float4 tilingUV = float4(i.uv.xy, 0, 0) * _TilingScale;

											float rotationAngle = _TilingAngle * 3.14156 / 180.0;
											float sina, cosa;
											sincos(rotationAngle, sina, cosa);
											float2x2 m = float2x2(cosa, -sina, sina, cosa);
											tilingUV = float4(mul(m, tilingUV), 0, 0);
					#endif
											i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

											i.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject));

											float2 flowDirection = float2(-_Time.y * _WaterSpeed, 0);
											float3 bf = normalize(abs(i.normalDir));
											bf /= dot(bf, (float3)1);


											float2 tx = -worldPos.yx + float2(-_Time.y * _WaterSpeed, 0);
											float2 ty = -worldPos.yz + float2(-_Time.y * _WaterSpeed, 0);
											float2 tz = -worldPos.zx + float2(-_Time.y * _WaterSpeed * 0.5, 0);

					#ifndef USE_LOW
											float3 mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale, 0, 0)).r;
											mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale * 1.33 + 0.23, 0, 0)).g;
											mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale * 2.84 + 0.83, 0, 0)).b;

											float3 mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale, 0, 0)).r;
											mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale * 1.33 + 0.23, 0, 0)).g;
											mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale * 2.84 + 0.83, 0, 0)).b;

											float3 mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale, 0, 0)).r;
											mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale * 1.33 + 0.23, 0, 0)).g;
											mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale * 2.84 + 0.83, 0, 0)).b;

					#else
											float3 mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale, 0, 0)).rgb;

											float3 mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale, 0, 0)).rgb;

											float3 mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale, 0, 0)).rgb;
					#endif

											float3 mudSpecWater = lerp(mudWaterSpecB.rgb, mudWaterSpecA.rgb, saturate(pow(bf.z, 1)));
											mudSpecWater = lerp(mudSpecWater, mudWaterSpecC.rgb, pow(saturate(bf.y * 1), 3));

											float mudSpecular = lerp((tex2Dlod(_MudSpecular, float4(i.uv.xy * _MudScale, 0, 0))).r, 0 , _WaterLevel);

											float mudHeight = saturate(1 - mudSpecular) * saturate((mudSpecWater.r + mudSpecWater.g + mudSpecWater.b) / 3 * 1.55);
											float mudHeightUnchanged = saturate(1 - mudSpecular);

					#ifdef LIGHTMAP_ON
											i.lmap = v.uv.xy * unity_LightmapST.xy + unity_LightmapST.zw;
					#endif


											float slopeValue = 0;
										#ifdef IS_T
											half4 splat_control = tex2Dlod(_Control0, float4(i.uv.xy, 0,0));
					#ifdef USE_COMPLEX_T
											half4 splat_control1 = tex2Dlod(_Control1, float4(i.uv.xy, 0, 0));
					#endif

					#ifdef USE_COMPLEX_T
											float tilingValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
												- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7);
					#else
											float tilingValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3);
					#endif
											tilingValue = tilingValue - ripplesR;
											float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(pow(tilingValue, 3)));

											float2 terrainOffsetST0 = (_TerrainScale.xz / _Splat0_ST.xy);
											float2 terrainOffsetST1 = (_TerrainScale.xz / _Splat1_ST.xy);
											float2 terrainOffsetST2 = (_TerrainScale.xz / _Splat2_ST.xy);
											float2 terrainOffsetST3 = (_TerrainScale.xz / _Splat3_ST.xy);
					#ifdef USE_COMPLEX_T
											float2 terrainOffsetST4 = (_Splat4_ST.xy);
											float2 terrainOffsetST5 = (_Splat5_ST.xy);
											float2 terrainOffsetST6 = (_Splat6_ST.xy);
											float2 terrainOffsetST7 = (_Splat7_ST.xy);
					#endif

											float groundHeightNew = _Mask0.SampleLevel(my_linear_repeat_sampler, lerp(i.uv, tilingUV, _Metallic0) * terrainOffsetST0 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic0) , 0).r;
											groundHeightNew = lerp(groundHeightNew, _Mask1.SampleLevel(my_linear_repeat_sampler,  lerp(i.uv, tilingUV, _Metallic1) * terrainOffsetST1 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic1), 0).r, saturate(splat_control.g));
											groundHeightNew = lerp(groundHeightNew, _Mask2.SampleLevel(my_linear_repeat_sampler,  lerp(i.uv, tilingUV, _Metallic2) * terrainOffsetST2 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic2), 0).r, saturate(splat_control.b));
											groundHeightNew = lerp(groundHeightNew, _Mask3.SampleLevel(my_linear_repeat_sampler,  lerp(i.uv, tilingUV, _Metallic3) * terrainOffsetST3 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic3), 0).r, saturate(splat_control.a));
					#ifdef USE_COMPLEX_T
											groundHeightNew = lerp(groundHeightNew, _Mask4.SampleLevel(my_linear_repeat_sampler,  lerp(i.uv, tilingUV, _Metallic4) * terrainOffsetST4 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic4), 0).r, saturate(splat_control1.r));
											groundHeightNew = lerp(groundHeightNew, _Mask5.SampleLevel(my_linear_repeat_sampler,  lerp(i.uv, tilingUV, _Metallic5) * terrainOffsetST5 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic5), 0).r, saturate(splat_control1.g));
											groundHeightNew = lerp(groundHeightNew, _Mask6.SampleLevel(my_linear_repeat_sampler,  lerp(i.uv, tilingUV, _Metallic6) * terrainOffsetST6 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic6), 0).r, saturate(splat_control1.b));
											groundHeightNew = lerp(groundHeightNew, _Mask7.SampleLevel(my_linear_repeat_sampler,  lerp(i.uv, tilingUV, _Metallic7) * terrainOffsetST7 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic7), 0).r, saturate(splat_control1.a));
					#endif

											float groundHeight = groundHeightNew;
										#else
											float tilingValue = 0;

											tilingValue = redValue;

					#ifdef IS_GROUND
											tilingValue = 1;
					#endif
					#ifdef IS_TILING
											tilingValue = 0;
					#endif


											float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(1 - pow(tilingValue, 30)));

											float groundHeight = lerp(tex2Dlod(_GroundHeight, i.uv * heightTile * _GroundScale).r, tex2Dlod(_ParallaxMapTiling, tilingUV * heightTile).r, 1 - saturate(pow(tilingValue, 5)));

										#endif

										#ifdef USE_RT
											if (_HasRT == 1)
											{
												if (v.color.b > 0.95 && v.color.g < 0.05)
												{
													i.normal = normalize(v.normal);
												}
												else
												{
													i.normal = normalize(lerp(v.normal, calcNormal(uv, _GlobalEffectRT).rbb, saturate(tilingValue)));
												}
											}
											else
											{
												i.normal = normalize(v.normal);
											}
										#else
											i.normal = normalize(v.normal);
										#endif

											float3 newNormal = normalize(i.normalDir);

											float displacementStrength = lerp(_DisplacementStrength, _DisplacementTileStrength, saturate(1 - pow(tilingValue, 3)));
											float displacementValue = lerp(_DisplacementOffset, _DisplacementTileOffset, saturate(1 - tilingValue));

											worldPos += ((float4(0, -_RemoveGroundStrength, 0, 0) * _UpVector - newNormal * _RemoveGroundStrength * _NormalVector * displacementStrength) * ripplesB + (float4(0,  groundHeight, 0, 0) * _UpVector + newNormal * groundHeight * _NormalVector * displacementStrength) * saturate(1 - ripplesB));

											worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(1 - tilingValue));

											worldPos += (float4(0, 2 * displacementStrength * groundHeight, 0, 0) * _UpVector) + (newNormal * 2 * displacementStrength * groundHeight * _NormalVector) + newNormal * displacementValue * _NormalVector;

											float _Speed = 1.5;
											float _Frequency = 20;
											float _Amplitude = 0.2;

											mudSpecWater = lerp(0, mudSpecWater, saturate(1 - mudHeight));
											float mudSpecWaterValue = saturate((mudSpecWater.r + mudSpecWater.g + mudSpecWater.b) / 3 * 1.55);
											mudSpecWaterValue += ripplesG * 0.5;

											mudSpecWaterValue = lerp(0, mudSpecWaterValue, saturate(_WaterLevel + 1));

											float3 wNormal = mul(unity_ObjectToWorld, v.normal);

											mudPos.xyz = lerp(0, wNormal.xyz * mudSpecWaterValue, saturate(_WaterLevel + 1));

											worldPos.xyz += lerp(0, mudPos.xyz * _MudWaveHeight, saturate(pow(mudHeightUnchanged,5) * ripplesB + ripplesG * ripplesB * 0.25)) * tilingValue;

											worldPos.xyz += lerp(0, (wNormal.xyz * mudHeightUnchanged) , _MudHeight * ripplesB) * tilingValue;

					#ifdef IS_TILING
					#else
											worldPos = lerp(mul(unity_ObjectToWorld, v.vertex), worldPos,  saturate((v.color.r + v.color.b)));
					#endif

											v.vertex.xyz = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;

											i.vertex = GetVertexPositionInputs(v.vertex).positionCS;

											float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
											float3 viewDir = v.vertex.xyz - objCam.xyz;

										#ifdef IS_T
											float4 finalTangent = float4 (1.0, 0.0, 0.0, -1.0);
											finalTangent.xyz = finalTangent.xyz - v.normal * dot(v.normal, finalTangent.xyz); // Orthogonalize tangent to normal.

											float tangentSign = finalTangent.w * unity_WorldTransformParams.w;
											float3 bitangent = cross(v.normal.xyz, finalTangent.xyz) * tangentSign;

											i.viewDir = float3(
												dot(viewDir, finalTangent.xyz),
												dot(viewDir, bitangent.xyz),
												dot(viewDir, v.normal.xyz)
												);

											i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
											i.tangent = finalTangent;

										#else
											float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
											float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) * tangentSign;

											i.viewDir = float3(
												dot(viewDir, v.tangent.xyz),
												dot(viewDir, bitangent.xyz),
												dot(viewDir, v.normal.xyz)
												);

											i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
											i.tangent = v.tangent;
											float4 finalTangent = v.tangent;
										#endif

											i.color = v.color;
											i.color.a = lerp(1, 0, pow(-mudPos.y * pow(mudSpecWaterValue,5),5) * 10);

											VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);

					#ifdef LIGHTMAP_ON
											//i.shadowCoord.xy = v.shadowCoord.xy * unity_LightmapST.xy + unity_LightmapST.zw;
											i.shadowCoord = float4(v.shadowCoord.xy * unity_LightmapST.xy + unity_LightmapST.zw, 0, 0);
											i.shadowBaked = GetShadowCoord(vertexInput);
					#else
											i.shadowCoord = GetShadowCoord(vertexInput);
					#endif


					#ifndef USE_LOW
											parallax_vert(v.vertex, v.normal, finalTangent, i.eye);
					#endif

					#ifdef SHADER_API_MOBILE
											TRANSFER_VERTEX_TO_FRAGMENT(i);
					#endif

											i.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
											return i;
										}

										float4 frag(InterpolatorsVertex i) : SV_Target
										{
					#ifdef USE_VR
										UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)
					#endif
										 
										// Linear to Gamma //
										half gamma = 0.454545;
										_WaterScale = _WaterScale * 0.01;

										half4 lightColor = half4(1, 1, 1, 1);
										float4 bakedColorTex = 0;
										float shadowmap = 0;

										float4 shadowCoord;
										half3 lm = 1;
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
										shadowCoord = i.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
										shadowCoord = TransformWorldToShadowCoord(i.worldPos);
					#else
										shadowCoord = float4(0, 0, 0, 0);
					#endif

										float4 shadowCo = shadowCoord;
					#ifdef LIGHTMAP_ON
					#ifdef _MIXED_LIGHTING_SUBTRACTIVE
										shadowCo = i.shadowCustomCoord;
					#endif
					#endif
										Light mainLight = GetMainLight(shadowCo);


					#ifdef LIGHTMAP_ON
										float3 sampledLightMap = 1;
					#ifdef _MIXED_LIGHTING_SUBTRACTIVE
										sampledLightMap = SampleLightmap(i.shadowCoord.xy, normalize(i.normalDir)) * (mainLight.shadowAttenuation + 0.4);
					#else
										sampledLightMap = SampleLightmap(i.shadowCoord.xy, normalize(i.normalDir));
					#endif
										bakedColorTex = sampledLightMap;

										float sampleValue = (sampledLightMap.r + sampledLightMap.g + sampledLightMap.b) / 3;
										shadowmap = sampleValue;
										lightColor = half4(sampledLightMap, sampleValue);

										lightColor = lerp(_ProjectedShadowColor, 1.5, saturate(sampleValue));
					#else
										shadowmap = mainLight.shadowAttenuation;
										lightColor = half4(mainLight.color.rgb, (mainLight.color.r + mainLight.color.g + mainLight.color.b) / 3);
					#endif

					#if !UNITY_COLORSPACE_GAMMA
					_Color = pow(_Color, gamma) * 0.95;

					_Color.rgb = RGBToHSV(_Color.rgb);
					_Color.y *= 2;
					_Color.rgb = HSVToRGB(_Color.rgb);

					_MainTexMult = _MainTexMult * 0.85;
					_BotColor = pow(_BotColor, gamma);
					_TilingTint = pow(_TilingTint, gamma + 0.25) * 1.25;

					_TilingTint.rgb = RGBToHSV(_TilingTint.rgb);
					_TilingTint.y *= 1.1;
					_TilingTint.rgb = HSVToRGB(_TilingTint.rgb);

					_MudColor.rgb = RGBToHSV(_MudColor.rgb);
					_MudColor.y *= 0.6;
					_MudColor.rgb = HSVToRGB(_MudColor.rgb) * 2.25;

					_TilingTrail = pow(_TilingTrail, gamma);
					_ProjectedShadowColor = pow(_ProjectedShadowColor, gamma);
					_SpecColor = pow(_SpecColor, gamma);
					_RimColor = pow(_RimColor, gamma);
					lightColor = pow(lightColor, gamma);
					_LittleSpecForce = pow(_LittleSpecForce, gamma);
					_MainTexMult = pow(_MainTexMult, gamma);
					_TilingSaturation = _TilingSaturation * 1.8;

					#ifdef IS_T
										_Specular0 = pow(_Specular0, gamma);
										_Specular1 = pow(_Specular1, gamma);
										_Specular2 = pow(_Specular2, gamma);
										_Specular3 = pow(_Specular3, gamma);
					#ifdef USE_COMPLEX_T
										_Specular4 = pow(_Specular4, gamma);
										_Specular5 = pow(_Specular5, gamma);
										_Specular6 = pow(_Specular6, gamma);
										_Specular7 = pow(_Specular7, gamma);
					#endif
					#endif

					#endif

										// Terrain Setup //
					#ifdef IS_T
										int maxN = 4;

										Texture2D _MaskArray[8];
										_MaskArray[0] = _Mask0;
										_MaskArray[1] = _Mask1;
										_MaskArray[2] = _Mask2;
										_MaskArray[3] = _Mask3;

										half _MetallicArray[8];
										_MetallicArray[0] = _Metallic0;
										_MetallicArray[1] = _Metallic1;
										_MetallicArray[2] = _Metallic2;
										_MetallicArray[3] = _Metallic3;

										Texture2D _SplatArray[8];
										_SplatArray[0] = _Splat0;
										_SplatArray[1] = _Splat1;
										_SplatArray[2] = _Splat2;
										_SplatArray[3] = _Splat3;

										float4 _SplatSTArray[8];
										_SplatSTArray[0] = _Splat0_ST;
										_SplatSTArray[1] = _Splat1_ST;
										_SplatSTArray[2] = _Splat2_ST;
										_SplatSTArray[3] = _Splat3_ST;

										float4 _SpecularArray[8];
										_SpecularArray[0] = _Specular0;
										_SpecularArray[1] = _Specular1;
										_SpecularArray[2] = _Specular2;
										_SpecularArray[3] = _Specular3;

										Texture2D _NormalArray[8];
										_NormalArray[0] = _Normal0;
										_NormalArray[1] = _Normal1;
										_NormalArray[2] = _Normal2;
										_NormalArray[3] = _Normal3;

										half _NormalScaleArray[8];
										_NormalScaleArray[0] = _NormalScale0;
										_NormalScaleArray[1] = _NormalScale1;
										_NormalScaleArray[2] = _NormalScale2;
										_NormalScaleArray[3] = _NormalScale3;

					#ifdef USE_COMPLEX_T
										maxN = 8;

										_MaskArray[4] = _Mask4;
										_MaskArray[5] = _Mask5;
										_MaskArray[6] = _Mask6;
										_MaskArray[7] = _Mask7;

										_MetallicArray[4] = _Metallic4;
										_MetallicArray[5] = _Metallic5;
										_MetallicArray[6] = _Metallic6;
										_MetallicArray[7] = _Metallic7;

										_SplatArray[4] = _Splat4;
										_SplatArray[5] = _Splat5;
										_SplatArray[6] = _Splat6;
										_SplatArray[7] = _Splat7;

										_SplatSTArray[4] = _Splat4_STn;
										_SplatSTArray[5] = _Splat5_STn;
										_SplatSTArray[6] = _Splat6_STn;
										_SplatSTArray[7] = _Splat7_STn;

										_SpecularArray[4] = _Specular4;
										_SpecularArray[5] = _Specular5;
										_SpecularArray[6] = _Specular6;
										_SpecularArray[7] = _Specular7;

										_NormalArray[4] = _Normal4;
										_NormalArray[5] = _Normal5;
										_NormalArray[6] = _Normal6;
										_NormalArray[7] = _Normal7;

										_NormalScaleArray[4] = _NormalScale4;
										_NormalScaleArray[5] = _NormalScale5;
										_NormalScaleArray[6] = _NormalScale6;
										_NormalScaleArray[7] = _NormalScale7;
					#endif
					#endif
													float uvRTValue = 0;
													float2 uv = i.worldPos.xz - _Position.xz;
													uv = uv / (_OrthographicCamSize * 2);
													uv += 0.5;

													float2 uvAdd = i.worldPos.xz - _PositionAdd.xz;
													uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
													uvAdd += 0.5;

													float3 rippleMain = 0;
													float3 rippleMainAdditional = 0;
													float3 calculatedNormal = 0;
													float3 calculatedNormalAdd = 0;

													float ripplesR = 0;
													float ripplesG = 0;
													float ripplesB = 0;
													float tilingValue = 1;
													float oldTilingValue = 1;
													float groundHeight = 0;
													float groundHeightReal = 0;


													float2 tilingUV = i.uv * _TilingScale;

													float rotationAngle = _TilingAngle * 3.14156 / 180.0;
													float sina, cosa;
													sincos(rotationAngle, sina, cosa);
													float2x2 m = float2x2(cosa, -sina, sina, cosa);
													tilingUV = mul(m, tilingUV);

					#ifdef IS_TILING
													float redValue = 1;
													float greenValue = 1;
													float blueValue = 1;
					#else
					#ifdef USE_RT

													if (_HasRT == 1)
													{
														rippleMain = tex2D(_GlobalEffectRT, uv);
														rippleMainAdditional = tex2D(_GlobalEffectRTAdditional, uvAdd);

														float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - i.worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
														uvRTValue = saturate(uvGradient.x);
														ripplesR = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
														ripplesG = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
														ripplesB = saturate(lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue));
													}
					#endif

													float redValue = saturate(i.color.g + i.color.b - ripplesR);
													float greenValue = saturate(i.color.r + i.color.b);
													float blueValue = saturate(i.color.r + i.color.g);
					#endif


													ripplesB = saturate(ripplesB + (1 - blueValue) - saturate(ripplesR));
													tilingValue = 1;

					#ifdef IS_T

													_GroundScale = _GroundScale * _OverallScale * 10;
													_TilingScale = _TilingScale * _OverallScale * 10;
					#else
					#ifdef IS_GROUND
													tilingValue = 1;
					#else
													tilingValue = lerp(tilingValue, 0, saturate(pow(tex2D(_ParallaxMapTiling, tilingUV).r * 2 , 2) * (1 - saturate(i.color.g + i.color.b))));
													tilingValue = lerp(tilingValue, 0, saturate(ripplesR));
													oldTilingValue = tilingValue;

					#endif
					#endif

													float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(1 - pow(tilingValue, 3)));


					#ifdef USE_PR
													float dist = clamp(lerp(0, 1, (distance(_WorldSpaceCameraPos, i.worldPos) - _ProceduralDistance) / max(_ProceduralStrength, 0.05)), 0, 1);
					#endif

					#ifdef USE_PR
													groundHeightReal = lerp(tex2D(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r, tex2DStochastic(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r, dist);
					#else
													groundHeightReal = tex2D(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r;
					#endif

					#ifdef IS_T
					#else
					#ifndef USE_LOW
													// OCCLUSION PARALLAX //
													float2 offsetParallax = parallax_offset(_ParallaxValue, i.eye, tilingUV,
														_ParallaxMapTiling, _MudNormal, _ParallaxMinSamples, _ParallaxMaxSamples,0);

													tilingUV = tilingUV + offsetParallax;
					#endif
					#endif

										#ifdef IS_T
													half4 splat_control = tex2D(_Control0, i.uv);
					#ifdef USE_COMPLEX_T
													half4 splat_control1 = tex2D(_Control1, i.uv);
					#endif

													splat_control.r = lerp(splat_control.r, 0, saturate(ripplesR));
													splat_control.g = lerp(splat_control.g, 1, saturate(ripplesR));
					#ifdef USE_COMPLEX_T
													splat_control1.r = lerp(splat_control1.r, 0, saturate(ripplesR));
													splat_control1.g = lerp(splat_control1.g, 1, saturate(ripplesR));
					#endif

					#ifdef USE_COMPLEX_T
													float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
														- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7);
					#else								
													float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3);
					#endif

					#ifdef IS_GROUND
													tilingValue = 1;
					#else
													tilingValue = pow(splatOverall, 1 + clamp(abs((groundHeight - 0.5) * 20) * splatOverall, -1, 1));
					#endif

													float3 originalPos = i.worldPos;

													// Terrain Setup //
													float _splatControlArray[8];
													_splatControlArray[0] = splat_control.r;
													_splatControlArray[1] = splat_control.g;
													_splatControlArray[2] = splat_control.b;
													_splatControlArray[3] = splat_control.a;
					#ifdef USE_COMPLEX_T
													_splatControlArray[4] = splat_control1.r;
													_splatControlArray[5] = splat_control1.g;
													_splatControlArray[6] = splat_control1.b;
													_splatControlArray[7] = splat_control1.a;
					#endif


													// OFFSET COORDINATES //

													float2 terrainOffsetST[8];

													for (int n = 0; n < maxN; n++)
													{
														if (n <= 3)
														{
															terrainOffsetST[n] = (_TerrainScale.xz / _SplatSTArray[n].xy);
														}
														else
														{
															terrainOffsetST[n] = _SplatSTArray[n].xy;
														}
													}

					#ifndef USE_LOW
													// OCCLUSION PARALLAX //
													float2 offsetParallax = float2(0, 0);

													for (int n = 0; n < maxN; n++)
													{
														if (saturate(_splatControlArray[n] * (_MetallicArray[n])) > 0.5)
														{
															offsetParallax += parallax_offset(_ParallaxValue, i.eye, tilingUV * _TilingScale, _MainTex,
																_MaskArray[n], _ParallaxMinSamples, _ParallaxMaxSamples,1);
														}
													}

													i.uv.xy = i.uv.xy + offsetParallax;
					#endif

													// tiling Height Map //

													float3 tilingHeight = 1;
													for (int n = 0; n < maxN; n++)
													{
														tilingHeight = lerp(tilingHeight, _MaskArray[n].Sample(my_linear_repeat_sampler, lerp(i.uv * _GroundScale, tilingUV * _TilingScale, _MetallicArray[n]) * terrainOffsetST[n] * lerp(_GroundScale, _TilingScale, _MetallicArray[n])).r, saturate(_splatControlArray[n]));
													}

													// ground Splat //

													float3 groundSplatArray[8];
													for (int n = 0; n < maxN; n++)
													{
														groundSplatArray[n] = _SplatArray[n].Sample(my_linear_repeat_sampler, terrainOffsetST[n] * lerp(i.uv * _GroundScale, tilingUV * _TilingScale, _MetallicArray[n]));
													}

													// Ground NORMAL //
													float3 groundNormalArray[8];
													for (int n = 0; n < maxN; n++)
													{
														groundNormalArray[n] = UnpackNormalScale(_NormalArray[n].Sample(my_linear_repeat_sampler, terrainOffsetST[n] * lerp(i.uv * _GroundScale, tilingUV * _TilingScale, _MetallicArray[n])), _NormalScaleArray[n] * 2);
													}

													float3 normal = 0;
													for (int n = 0; n < maxN; n++)
													{
														normal = lerp(normal, groundNormalArray[n], saturate(_splatControlArray[n]));
													}

													// Tiling NORMAL //
													half3 normalTex = UnpackNormalScale(_NormalArray[0].Sample(my_linear_repeat_sampler, tilingUV), _NormalScale);
													for (int n = 0; n < maxN; n++)
													{
														normalTex = lerp(normalTex, groundNormalArray[n] * _NormalScaleArray[n], _splatControlArray[n]);
													}

													// groundHeightReal //
													float groundHeightNew = 0;
													for (int n = 0; n < maxN; n++)
													{
														groundHeightNew = lerp(groundHeightNew, _MaskArray[n].Sample(my_linear_repeat_sampler, heightTile * terrainOffsetST[n] * lerp(i.uv * _GroundScale, tilingUV * _TilingScale, _MetallicArray[n])).r, saturate(_splatControlArray[n]));
													}

													groundHeightReal = groundHeightNew;


										#else
					#ifdef USE_PR
													float3 normal = UnpackNormalScale(lerp(tex2D(_GroundBumpMap, (i.uv) * _GroundNormalScale * _GroundScale), tex2DStochastic(_GroundBumpMap, (i.uv) * _GroundNormalScale * _GroundScale), saturate(dist)), _NormalMultiplier * 2).rgb - i.normal;
													normal = lerp(normal, 0, 1 - pow(tilingValue, 3));
													half3 normalTex = -UnpackNormalScale(tex2D(_NormalTex, tilingUV), _NormalScale);
													normalTex = lerp(normalTex, tex2DStochasticNormal(_NormalTex, tilingUV), saturate(dist));
					#else
													float3 normal = UnpackNormalScale(tex2D(_GroundBumpMap, (i.uv) * _GroundNormalScale * _GroundScale), _NormalMultiplier * 2).rgb - i.normal;
													normal = lerp(normal,0, 1 - pow(tilingValue, 3));
													half3 normalTex = -UnpackNormalScale(tex2D(_NormalTex, tilingUV), _NormalScale);
					#endif		
										#endif

										#ifdef IS_T

													half4 c = _Specular0;

													for (int n = 0; n < maxN; n++)
													{
														float4 colorTerrain = _SpecularArray[n];
														if (colorTerrain.r == 0 && colorTerrain.g == 0 && colorTerrain.b == 0 && colorTerrain.a == 0)
														{
															colorTerrain = float4(1, 1, 1, 1);
														}
														c = lerp(c, colorTerrain, _splatControlArray[n] * (tilingValue) * (1 - _MetallicArray[n]));
													}
										#else
													half4 c = _Color;
										#endif

										#ifdef USE_RT

													if (_HasRT == 1)
													{

														calculatedNormal = calcNormal(uv, _GlobalEffectRT);
														calculatedNormal.g = 0;
														calculatedNormal.r = calculatedNormal.y;
														calculatedNormal.b = 0;

														calculatedNormal = lerp(calculatedNormal, 0, uvRTValue);
													}
										#endif	

													c = lerp(
														c,
														_BotColor,
														saturate(ripplesB * tilingValue));

													c = lerp(
														c,
														c,
														saturate(saturate(-ripplesB) * saturate(groundHeight + 0.5) * 1));

													c.rgb = lerp(c.rgb, c.rgb * _BotColor, clamp(ripplesB * saturate(calculatedNormal.r - 0.15) * _NormalRTStrength * 1, 0, 1));

													c.rgb = c.rgb * lightColor;
													c.rgb = lerp(c.rgb * _Color * _DisplacementColorMult, c.rgb, groundHeightReal);

													float3 normalEffect = i.normal;


										#ifdef IS_T
													// Ground LERP //

													float3 groundColor = groundSplatArray[0];

													for (int n = 0; n < maxN; n++)
													{
														groundColor = lerp(groundColor, groundSplatArray[n], saturate(_splatControlArray[n]));
													}

													c.rgb *= groundColor;
													c.rgb = pow(c.rgb, _MainTexMult);
										#else

					#ifdef USE_PR
													c *= lerp(1, saturate(pow(lerp(tex2D(_MainTex, i.uv * _GroundScale), tex2DStochastic(_MainTex, i.uv * _GroundScale), dist) + _MainTexMult * 0.225, 2)), _MainTexMult);
					#else
													c *= lerp(1, saturate(pow(tex2D(_MainTex, i.uv * _GroundScale) + _MainTexMult * 0.225, 2)), _MainTexMult);
					#endif


										#endif
					#ifdef USE_FOG
													half3 flowVal = (_FlowTex.Sample(my_bilinear_repeat_sampler, i.uv)) * _FlowMultiplier;

													float dif1 = frac(_Time.y * 0.15 + 0.5);
													float dif2 = frac(_Time.y * 0.15);

													half lerpVal = abs((0.5 - dif1) / 0.5);

													//_FogDirection
													half3 col1 = _FogTex.Sample(my_bilinear_repeat_sampler, i.uv * _FogScale - flowVal.xy * dif1 + (normalize(_FogDirection.xy) * _Time.y * -0.02 * _FogDirection.z));
													half3 col2 = _FogTex.Sample(my_bilinear_repeat_sampler, i.uv * _FogScale - flowVal.xy * dif2 + (normalize(_FogDirection.xy) * _Time.y * -0.02 * _FogDirection.z));

													half3 fogFlow = lerp(col1, col2, lerpVal);
													fogFlow = abs(pow(fogFlow, 5));
					#endif

													float3 viewDirTangent = i.viewDir;
										#ifdef IS_T
													// Tiling LERP //
													half4 TilingTex = half4(groundSplatArray[0],1);

													for (int n = 0; n < maxN; n++)
													{
														TilingTex.rgb = lerp(TilingTex, groundSplatArray[n], _splatControlArray[n]);
													}
										#else
					#ifdef USE_PR
													half4 TilingTex = half4(lerp(1, pow(lerp(tex2D(_TilingTex, tilingUV), tex2DStochastic(_TilingTex, tilingUV), dist), _TilingSaturation), _TilingTint.a) * _TilingTint.rgb * 2, 1);
					#else
													half4 TilingTex = half4(lerp(1, pow(pow(tex2D(_TilingTex, tilingUV),gamma), _TilingSaturation), _TilingTint.a) * _TilingTint.rgb * 2, 1);
					#endif

					#endif
													TilingTex.rgb = TilingTex.rgb * lightColor;

													float3 viewDirection = normalize(_WorldSpaceCameraPos - i.worldPos.xyz);
													float3 normalDirection = normalize(i.normalDir);
					#ifdef IS_T
													float3 normalDirectionWithNormal = normalize(i.normalDir) + normalize(i.normalDir) * lerp(normal , normalTex, saturate(1 - tilingValue));
					#else
													float3 normalDirectionWithNormal = normalize(i.normalDir) + normalize(i.normalDir) * abs(tex2D(_GroundBumpMap, i.uv * _GroundNormalScale * _GroundScale)) * _NormalMultiplier;
					#endif

													half fresnelValue = lerp(0, 1, saturate(dot(normalDirection, viewDirection)));
													_OffsetScale = max(0, _OffsetScale);

													float3 newRoughnessTex = tex2D(_Roughness, tilingUV).rgb;
					#ifdef USE_PR
													newRoughnessTex = lerp(newRoughnessTex, tex2DStochastic(_Roughness, tilingUV), saturate(dist));
					#endif

													float alphaTiling = 1;
													alphaTiling = saturate(1 - saturate((newRoughnessTex.r + newRoughnessTex.g + newRoughnessTex.b) / 3));

													half4 blended = 0;

													if (i.color.r > 0.95 && i.color.g < 0.05)
													{
														blended = TilingTex;
													}
													else
													{
														blended = TilingTex - saturate(_TilingTrail * ripplesB * _TilingTrail.a);
													}

													blended.rgb = lerp(blended.rgb + newRoughnessTex * 0.4, blended.rgb, alphaTiling);
					#ifdef IS_T
													blended.rgb = blended.rgb * saturate(tilingHeight);
					#else
													blended.rgb = lerp(blended.rgb , blended.rgb * saturate(pow(tex2D(_ParallaxMapTiling, tilingUV), 3.0)), _AOValue);
					#endif

													float3 albedo = 1;
										#ifdef	IS_TILING
													albedo = blended * 0.5;
										#else

													albedo = lerp(blended * 0.5, c.rgb, saturate(pow(oldTilingValue, 5)));
										#endif
													albedo.rgb = lerp(albedo.rgb * (1 - _AOValue),albedo.rgb,saturate(pow(tex2D(_ParallaxMapTiling, tilingUV).r, 3) + tilingValue));

													// Custom water flow //
													float2 flowDirection = float2(-_Time.y * _WaterSpeed, 0);

													float3 bf = normalize(abs(i.normalDir));
													bf /= dot(bf, (float3)1);

													float2 tx = -i.worldPos.yx + float2(-_Time.y * _WaterSpeed, 0);
													float2 ty = -i.worldPos.yz + float2(-_Time.y * _WaterSpeed, 0);
													float2 tz = -i.worldPos.zx + float2(-_Time.y * _WaterSpeed * 0.1, 0);
													float2 txBis = -i.worldPos.yx + float2(-_Time.y * _WaterSpeed * 0.56, 0);
													float2 tyBis = -i.worldPos.yz + float2(-_Time.y * _WaterSpeed * 0.76, 0);
													float2 tzBis = -i.worldPos.xz + float2(-_Time.y * _WaterSpeed * 0.2, 0);

					#ifndef USE_LOW
													float3 mudWaterA = _MudWater.Sample(my_linear_repeat_sampler, float2(tx * _WaterScale)).r;
													mudWaterA = _MudWater.Sample(my_linear_repeat_sampler, float2(tx * _WaterScale * 1.33 + 0.23)).g;
													mudWaterA = _MudWater.Sample(my_linear_repeat_sampler, float2(tx * _WaterScale * 2.84 + 0.83)).b;

													float3 mudWaterB = _MudWater.Sample(my_linear_repeat_sampler, float2(ty * _WaterScale)).r;
													mudWaterB = _MudWater.Sample(my_linear_repeat_sampler, float2(ty * _WaterScale * 1.33 + 0.23)).g;
													mudWaterB = _MudWater.Sample(my_linear_repeat_sampler, float2(ty * _WaterScale * 2.84 + 0.83)).b;

													float3 mudWaterC = _MudWater.Sample(my_linear_repeat_sampler, float2(tz * _WaterScale)).r;
													mudWaterC = _MudWater.Sample(my_linear_repeat_sampler, float2(tz * _WaterScale * 1.33 + 0.23)).g;
													mudWaterC = _MudWater.Sample(my_linear_repeat_sampler, float2(tz * _WaterScale * 2.84 + 0.83)).b;


													float3 mudWaterSpecA = tex2D(_MudWaterSpecular, float2(tx * _WaterScale)).r;
													mudWaterSpecA = tex2D(_MudWaterSpecular, float2(tx * _WaterScale * 1.33 + 0.23)).g;
													mudWaterSpecA = tex2D(_MudWaterSpecular, float2(tx * _WaterScale * 2.84 + 0.83)).b;

													float3 mudWaterSpecB = tex2D(_MudWaterSpecular, float2(ty * _WaterScale)).r;
													mudWaterSpecB = tex2D(_MudWaterSpecular, float2(ty * _WaterScale * 1.33 + 0.23)).g;
													mudWaterSpecB = tex2D(_MudWaterSpecular, float2(ty * _WaterScale * 2.84 + 0.83)).b;

													float3 mudWaterSpecC = tex2D(_MudWaterSpecular, float2(tz * _WaterScale)).r;
													mudWaterSpecC = tex2D(_MudWaterSpecular, float2(tz * _WaterScale * 1.33 + 0.23)).g;
													mudWaterSpecC = tex2D(_MudWaterSpecular, float2(tz * _WaterScale * 2.84 + 0.83)).b;

					#else

													float3 mudWaterA = _MudWater.Sample(my_linear_repeat_sampler, float2(tx * _WaterScale)).rgb;

													float3 mudWaterB = _MudWater.Sample(my_linear_repeat_sampler, float2(ty * _WaterScale)).rgb;

													float3 mudWaterC = _MudWater.Sample(my_linear_repeat_sampler, float2(tz * _WaterScale)).rgb;

													float3 mudWaterSpecA = tex2D(_MudWaterSpecular, float2(tx * _WaterScale)).rgb;

													float3 mudWaterSpecB = tex2D(_MudWaterSpecular, float2(ty * _WaterScale)).rgb;

													float3 mudWaterSpecC = tex2D(_MudWaterSpecular, float2(tz * _WaterScale)).rgb;
					#endif

													float3 mudWater = lerp(mudWaterA, mudWaterB, saturate(pow(bf.z, 1)));
													mudWater = lerp(mudWater, mudWaterC.rgb, pow(saturate(bf.y * 1), 3));

													float3 mudSpecWater = lerp(mudWaterSpecA.rgb, mudWaterSpecB.rgb, saturate(pow(bf.z,1)));
													mudSpecWater = lerp(mudSpecWater, mudWaterSpecC.rgb, pow(saturate(bf.y * 1), 3));
													mudSpecWater = 1 - mudSpecWater;


													// MUD RENDER //
													float3 mudTex = _MudTex.Sample(my_linear_repeat_sampler, i.uv * _MudScale).rgb;
													float mudSpecular = tex2D(_MudSpecular, i.uv * _MudScale);

													float mudWaterValue = saturate((mudWater.r + mudWater.g + mudWater.b) / 3);
													float mudSpecWaterValue = saturate((mudSpecWater.r + mudSpecWater.g + mudSpecWater.b) / 3);

													float mudHeight = pow(mudSpecular - clamp(_WaterLevel + 0.25, -1, 1), 1) * 2 * mudSpecWaterValue;
													float mudHeightUnchanged = pow(mudSpecular, 3) * 5;

													mudSpecWaterValue += ripplesG * 0.25;

													mudSpecWaterValue = lerp(0, mudSpecWaterValue, saturate(1 - mudHeight) * saturate(_WaterLevel + 0.75));

													albedo = lerp(albedo, mudTex * _MudColor + mudWaterValue * saturate(1 - mudHeight) * _MudWaterColor * saturate(_WaterLevel + 0.9), saturate(ripplesB * tilingValue));

													albedo += lerp(0,0.05, saturate(pow(i.color.a * 0.8,3) * ripplesB * pow(tilingValue,5) * saturate(_WaterLevel + 1))); // ecume render

													float3 lightDirection;
													float attenuation;
													half3 worldNormal;
					#if !UNITY_COLORSPACE_GAMMA
													shadowmap = pow(shadowmap, gamma);
					#endif
													// basic lighting from sun pos //
													float diff = 0;

													float3 N = normalize(normalDirection);
													float3 fragmentToLight = _MainLightPosition.xyz - i.worldPos.xyz;
													float3 L = normalize(fragmentToLight) * _MainLightPosition.w + normalize(_MainLightPosition.xyz) * (1 - _MainLightPosition.w);

													diff = dot(N, L);

													worldNormal.x = dot(normalDirection.x, lerp(normalTex, UnpackNormalScale(tex2D(_GroundBumpMap, i.uv * _GroundScale), _NormalMultiplier).rgb * 3 , tilingValue));
													worldNormal.y = dot(normalDirection.y, lerp(normalTex, UnpackNormalScale(tex2D(_GroundBumpMap, i.uv * _GroundScale), _NormalMultiplier).rgb * 3 , tilingValue));
													worldNormal.z = dot(normalDirection.z, lerp(normalTex, UnpackNormalScale(tex2D(_GroundBumpMap, i.uv * _GroundScale), _NormalMultiplier).rgb * 3 , tilingValue));

													_ShininessTiling = max(0.1, _ShininessTiling);
													_ShininessGround = max(0.1, _ShininessGround);

					#ifdef IS_UNLIT
													attenuation = 0.0; // no attenuation
													lightDirection = normalize(float3(0,1,0));
					#else

													if (0.0 == _MainLightPosition.w) // directional light
													{
														attenuation = 1.0; // no attenuation
														lightDirection = normalize(_MainLightPosition.xyz);
													}
													else // point or spot light
													{
														float3 vertexToLightSource =
															_MainLightPosition.xyz - i.worldPos.xyz;
														float distance = length(vertexToLightSource);
														attenuation = 1.0 / distance; // linear attenuation 
														lightDirection = normalize(vertexToLightSource);
													}
					#endif

					#ifdef LIGHTMAP_ON
													lightDirection = normalize(float3(0,1,0));
					#endif
													float3 lightDirectionInvert = float3(-lightDirection.x, lightDirection.y, -lightDirection.z);

													float3 diffuseReflection =
														attenuation * lightColor * _Color.rgb
														* max(0.0, dot(normalDirection, lightDirection));

													float3 specularReflection;
													if (dot(normalDirection, lightDirection) < 0.0)
														// light source on the wrong side
													{
														//specularReflection = float3(0.0, 0.0, 0.0);
														// no specular reflection
													}
													else // light source on the right side
													{
														specularReflection = attenuation * lightColor
															* _SpecColor.rgb * pow(max(0.0, dot(
																reflect(-lightDirection, normalDirection),
																viewDirection)), lerp(_ShininessTiling, _ShininessGround, tilingValue));
					#ifdef USE_DS
														specularReflection += attenuation * lightColor
															* _SpecColor.rgb * pow(max(0.0, dot(
																reflect(-lightDirectionInvert, normalDirection),
																viewDirection)), lerp(_ShininessTiling, _ShininessGround, tilingValue));
					#endif
													}

													float NdotL = 1;
					#ifdef LIGHTMAP_ON
													NdotL = 1;
					#else
													NdotL = 0.5 * (dot(_MainLightPosition.xyz, normalDirectionWithNormal)) + 0.5; // Lambert Normal adjustement

													float displacementStrength = lerp(_DisplacementStrength, _DisplacementTileStrength, saturate(1 - pow(tilingValue, 3)));
													NdotL = lerp(NdotL,NdotL + saturate(groundHeightReal) * 0.1 * displacementStrength - saturate(1 - groundHeightReal) * 0.1 * displacementStrength, tilingValue * _DisplacementShadowMult);
													NdotL = saturate(NdotL);
					#endif

													float lightIntensity = smoothstep(0.1 + _LightOffset * clamp((_LightHardness + 0.5) * 2,1,10), (0.101 + _LightOffset) * clamp((_LightHardness + 0.5) * 2, 1, 10), NdotL * shadowmap);
													_SpecForce = max(0.1, _SpecForce);

					#ifdef IS_UNLIT
													lightIntensity = 1;
					#endif

													half3 shadowmapColor = lerp(_ProjectedShadowColor, 1, saturate(lightIntensity));
													/*
					#ifndef LIGHTMAP_ON
													float zDist = dot(_WorldSpaceCameraPos - i.worldPos, UNITY_MATRIX_V[2].xyz);
													float fadeDist = saturate(1 - UnityComputeShadowFade(UnityComputeShadowFadeDistance(i.worldPos, zDist)) * diff);
													shadowmapColor = lerp(1, shadowmapColor, saturate(fadeDist));
					#endif
					*/


					// URP DECAL //
#ifdef USE_DECAL
													FETCH_DBUFFER(DBuffer, _DBufferTexture, int2(i.vertex.xy));

													DecalSurfaceData decalSurfaceData;
													DECODE_FROM_DBUFFER(DBuffer, decalSurfaceData);

													albedo.xyz = albedo.xyz * decalSurfaceData.baseColor.w + decalSurfaceData.baseColor.xyz;
#endif

													albedo.xyz = albedo.xyz * saturate(shadowmapColor);
										#ifdef IS_T
													float3 hole = tex2D(_TerrainHolesTexture, uv).rgb;
													clip(hole == 0.0f ? -1 : 1);

													float4 specGloss = pow(tex2D(_GroundSpecGlossMap, i.uv * 2 * (_TerrainScale.xz / _Splat0_ST.xy) * _GroundScale), _SpecForce) * _SpecMult;
													float4 littleSpec = tex2D(_LittleSpec, i.uv * (_TerrainScale.xz / _Splat0_ST.xy) * _LittleSpecSize * _GroundScale) * saturate(1 - ripplesB) * saturate(lightIntensity);
										#else
													float4 specGloss = pow(tex2D(_GroundSpecGlossMap, i.uv * _GroundScale), _SpecForce) * _SpecMult;
													float4 littleSpec = tex2D(_LittleSpec, i.uv * _GroundScale * _LittleSpecSize) * saturate(1 - ripplesB) * saturate(lightIntensity);

										#endif

										#ifdef	IS_TILING
													half rougnessTex = newRoughnessTex.r * 2 * _RoughnessStrength * saturate(1 - ripplesB) * 1;
										#else
													half rougnessTex = newRoughnessTex.r * 2 * _RoughnessStrength * saturate(1 - ripplesB) * (1 - tilingValue);
										#endif

					#ifdef IS_T
													rougnessTex = rougnessTex * saturate(normalTex + 0.25);
					#else
													rougnessTex = rougnessTex * saturate(normalTex + 0.25);
					#endif

										#ifdef	IS_TILING
													specGloss.r = specGloss.r * saturate(normal);
													float3 mudNormal = 1;
													float3 mudWaterNormal = 1;
										#else
													// ADD SPECULAR FOR MUD //
													float3 mudNormal = lerp(1, UnpackNormalScale(_MudNormal.Sample(my_bilinear_repeat_sampler, i.uv * _MudScale), 10), _MudNormalMultiplier);
													
													float3 mudWaterNormalA = UnpackNormalScale(_MudWaterNormal.Sample(my_bilinear_repeat_sampler, float2(tx * _WaterScale)), _MudWaterNormalMultiplier);
													float3 mudWaterNormalB = UnpackNormalScale(_MudWaterNormal.Sample(my_bilinear_repeat_sampler, float2(ty * _WaterScale)), _MudWaterNormalMultiplier);
													float3 mudWaterNormalC = UnpackNormalScale(_MudWaterNormal.Sample(my_bilinear_repeat_sampler, float2(tz * _WaterScale)), _MudWaterNormalMultiplier);

													float3 mudWaterNormalBisA = UnpackNormalScale(_MudWaterNormal.Sample(my_bilinear_repeat_sampler, float2(-txBis.x * _WaterScale * 0.77 + 0.26, tx.y * _WaterScale * 0.77 + 0.43)), _MudWaterNormalMultiplier);
													float3 mudWaterNormalBisB = UnpackNormalScale(_MudWaterNormal.Sample(my_bilinear_repeat_sampler, float2(-tyBis.x * _WaterScale * 0.77 + 0.67, ty.y * _WaterScale * 0.77 + 0.57)), _MudWaterNormalMultiplier);
													float3 mudWaterNormalBisC = UnpackNormalScale(_MudWaterNormal.Sample(my_bilinear_repeat_sampler, float2(-tzBis.x * _WaterScale * 0.77 + 0.33, tz.y * _WaterScale * 0.77 + 0.05)), _MudWaterNormalMultiplier);

													float3 mudWaterNormal = lerp(mudWaterNormalA, mudWaterNormalB, saturate(pow(bf.x, 1)));
													mudWaterNormal = lerp(mudWaterNormal, mudWaterNormalC.rgb, pow(saturate(bf.y * 1), 3));
													mudWaterNormal = mudSpecWater;
													float3 mudWaterNormalBis = lerp(mudWaterNormalBisA, mudWaterNormalBisB, saturate(pow(bf.x, 1)));
													mudWaterNormalBis = lerp(mudWaterNormalBis, mudWaterNormalBisC.rgb, pow(saturate(bf.y * 1), 3));
													mudWaterNormalBis = mudSpecWater;

													mudWaterNormal = pow(mudWaterNormal, 2) * 0.5;
													mudWaterNormal += ripplesG * 0.25;

													mudWaterNormalBis = saturate(mudWaterNormalBis);
													mudWaterNormal = saturate(mudWaterNormal + mudWaterNormalBis);

									#endif

													specGloss.r = specGloss.r * saturate(normal) + saturate(ripplesB * 30) * lerp(0, 1, saturate(saturate(1 - ripplesB * 5) * calculatedNormal.x * reflect(-lightDirection, normalDirection)).x * _NormalRTStrength * saturate(shadowmapColor * 2));
													specGloss.r = specGloss.r + lerp(0, 0.1, saturate(calculatedNormal.y * reflect(lightDirection, -normalDirection)).x * _NormalRTStrength * saturate(shadowmapColor * 2));

													mudSpecWaterValue = lerp(mudSpecWaterValue + pow(i.normal, 2) * 0.5, 0, saturate(mudHeight * 1.5)) + ripplesG * 0.15;

													specGloss.r += lerp(0, saturate(1 - ripplesG * 6) * saturate(mudHeightUnchanged) * saturate(mudNormal) * _MudSpecularMultiplier + diff * 0.05 + mudSpecWaterValue * saturate(mudWaterNormal) * saturate(_WaterLevel + 0.9), ripplesB) * 2;

													specGloss.r += i.normal * mudWaterNormal * ripplesB * saturate(1 - (i.normal * mudHeight * 1.5)) * saturate(_WaterLevel + 0.9);
													specularReflection *= lerp(1, _MudWaterSpecularColor, ripplesB * saturate(1 - (mudHeight * 1.5)) * saturate(_WaterLevel + 0.9));

													specGloss.r += lerp(0, 1, saturate(i.color.a * reflect(lightDirection, -normalDirection)).x * saturate(mudSpecWaterValue * ripplesB * 10));

													// SPECULAR REFLECTINON HAS A PROBLEM WITH unbaked light //
				#ifdef LIGHTMAP_ON
													specGloss.r = 0;
				#endif

												_LittleSpecForce = max(0, _LittleSpecForce);

									#ifdef	IS_TILING
									#else
												 specularReflection = lerp(specularReflection,  specularReflection * (specGloss.r + lerp(littleSpec.g * _LittleSpecForce * 0.2 , littleSpec.g * _LittleSpecForce, specularReflection)), saturate(tilingValue * 3)); // multiply the *3 for a better ground Tiling transition
									#endif


									#ifdef	IS_TILING
												specularReflection = diffuseReflection * 0.1 + specularReflection * rougnessTex;
									#else
									#ifdef USE_RT
												if (_HasRT == 1)
												{
													specularReflection = specularReflection - lerp(0, saturate(0.075), saturate(saturate(lightColor.a * lightIntensity + lightColor.a * 0.35) * saturate(1 - ripplesB * 4) * calculatedNormal.x * reflect(lightDirection, normalDirection)).x * _NormalRTStrength);
													specularReflection = specularReflection + lerp(0, saturate(0.125), saturate(saturate(lightColor.a * lightIntensity + lightColor.a * 0.35) * saturate(1 - ripplesB * 8) * calculatedNormal.x * reflect(-lightDirection, normalDirection)).x * _NormalRTStrength);
													specularReflection = specularReflection - lerp(0, saturate(1 - _ProjectedShadowColor) * 0.25, saturate(calculatedNormal.y * reflect(-lightDirection, normalDirection)).x * _NormalRTStrength);
													specularReflection = specularReflection - lerp(0, -0.1, saturate(calculatedNormal.y * -reflect(-lightDirection, normalDirection)).x * _NormalRTStrength);
													specularReflection = specularReflection + lerp(0, saturate(0.2), saturate(calculatedNormal.y * reflect(lightDirection, normalDirection)).x * _NormalRTStrength * 0.5);
												}
									#endif
												specularReflection = lerp(specularReflection, diffuseReflection * 0.1 + specularReflection * rougnessTex, saturate(pow(1 - tilingValue, 2) * 3));
									#endif


									#ifdef USE_AL

												half3 ambientColor = SampleSH(half4(lerp(normalDirection, normalDirection * normalEffect, saturate(ripplesB)), 1));

				#if !UNITY_COLORSPACE_GAMMA
												ambientColor = pow(ambientColor, gamma) * 1.0;
				#endif
												albedo.rgb = saturate(lerp((albedo.rgb + (ambientColor - 0.5) * 0.75), albedo.rgb + (ambientColor - 0.5) * 0.75, saturate(1 - ripplesB)));
									#endif
												half fresnelRefl = lerp(1, 0, saturate(dot(normalDirection, viewDirection) * 2.65 * _RimColor.a));

									#ifdef	IS_TILING

									#else
												albedo.rgb = lerp(albedo.rgb, albedo.rgb + _RimColor, saturate(tilingValue * (fresnelRefl + fresnelRefl * 0.2)));
									#endif
												albedo += float4(specularReflection.r, specularReflection.g, specularReflection.b, 1.0) * _SpecColor.rgb;

				#if !UNITY_COLORSPACE_GAMMA
												albedo = pow(albedo, 1 / gamma);
				#endif

				#ifdef USE_FOG
												// CUSTOM FOG RENDER //
												albedo.rgb = lerp(albedo.rgb, albedo.rgb + fogFlow * _FogColor, _FogIntensity);
				#endif
												albedo.rgb = albedo.rgb * lightColor.rgb;

												// Additional light pass in URP, thank you Unity for this //
												int additionalLightsCount = GetAdditionalLightsCount();
												half3 addLightColor = 0;
												for (int ii = 0; ii < additionalLightsCount; ii++)
												{
													Light light = GetAdditionalLight(ii, i.worldPos);
													addLightColor += (light.color * light.distanceAttenuation * _LightIntensity);
												}

												albedo.rgb += addLightColor;

												half transparency = 1;

												transparency = saturate(lerp(_TransparencyValue, 1, saturate(pow(tilingValue, 2))));


												albedo = max(0, albedo);
										albedo.xyz = MixFog(albedo, i.fogCoord);

										return float4(albedo, transparency);
									}

									ENDHLSL
									}

									Pass{
											Tags {
												"LightMode" = "ShadowCaster"
											}
												Blend SrcAlpha OneMinusSrcAlpha

											HLSLPROGRAM
										// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
										#pragma exclude_renderers gles

																	#pragma target 4.6


																	#pragma multi_compile _ LOD_FADE_CROSSFADE

																	#pragma multi_compile_fwdbase

																	#pragma vertex vert
																	#pragma fragment frag

																	#define FORWARD_BASE_PASS
																	#pragma shader_feature USE_AL
																	#pragma shader_feature USE_RT
																	#pragma shader_feature USE_WT
																	#pragma shader_feature USE_LOW
																	#pragma shader_feature USE_DS
																	#pragma shader_feature IS_UNLIT
																	#pragma shader_feature USE_PR

											#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
											#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
											#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
											float3 _LightDirection;
											float3 _LightPosition;


															struct VertexData //appdata
															{
																float4 vertex : POSITION;
																float3 normal : NORMAL;
																float4 tangent : TANGENT;
																float2 uv : TEXCOORD0;
																float4 color : COLOR;

																float4 shadowCoord : TEXCOORD1;
										#ifdef USE_VR
																UNITY_VERTEX_INPUT_INSTANCE_ID
										#endif
															};

															struct InterpolatorsVertex
															{
																float4 pos : SV_POSITION;
																float3 normal : TEXCOORD1;
																float4 tangent : TANGENT;
																float4 uv : TEXCOORD0;
																float4 color : COLOR;
																float3 worldPos : TEXCOORD2;
																float3 viewDir: POSITION1;
																float3 normalDir: TEXCOORD3;

																float4 shadowCoord : TEXCOORD4;
										#ifdef USE_VR
																	UNITY_VERTEX_OUTPUT_STEREO
										#endif
															};

															sampler2D  _DetailTex;
															float4 _MainTex_ST, _DetailTex_ST;

															sampler2D _NormalMap;

															half4 _Color;

															sampler2D _TerrainHolesTexture;
															// Render Texture Effects //
															uniform sampler2D _GlobalEffectRT;
															uniform float3 _Position;
															uniform float _OrthographicCamSize;
															uniform sampler2D _GlobalEffectRTAdditional;
															uniform float3 _PositionAdd;
															uniform float _OrthographicCamSizeAdditional;

															sampler2D _MainTex;
															sampler2D _LittleSpec;


															float _ParallaxValue;
															float _TilingAngle;
															uint _ParallaxMinSamples;
															uint _ParallaxMaxSamples;
															sampler2D _ParallaxMapTiling;
															Texture2D _TileMask;
															Texture2D _MudTex;
															sampler2D _MudSpecular;
															Texture2D _MudNormal;

															half4 _BotColor;
															half4 _MudWaterSpecularColor;

															float _SpecForce, _SpecMult, _LittleSpecSize, _LittleSpecForce, _UpVector, _NormalVector, _TilingScale, _GroundScale, _WaterScale , _WaterSpeed, _WaterLevel, _TransparencyValue, _AOValue;
															float _NormalRTDepth, _NormalRTStrength, _RemoveGroundStrength, _DisplacementStrength, _DisplacementTileStrength, _NormalMultiplier;

															//Tiling Variables
															sampler2D _TilingTex;
															sampler2D _NormalTex;
															sampler2D _Roughness;
															sampler2D _GroundHeight;
															Texture2D _MudWater;
															Texture2D _MudWaterNormal;
															float _HeightScale;
															float _HeightTileScale;
															float _LightOffset;
															float _LightHardness;
															float _LightIntensity;
															float _TilingSaturation;
															float _MudScale;
															float _MudSpecularMultiplier;
															float _MudNormalMultiplier;
															float _MudHeight;
															float _MudWaveHeight;
															float _DisplacementColorMult, _DisplacementShadowMult;
															float4 _MudColor;
															sampler2D _MudWaterSpecular;
															float4 _MudWaterColor;

															half _OffsetScale;
															half _OverallScale;
															half _RoughnessStrength;

															half _NormalScale, _DisplacementOffset, _DisplacementTileOffset, _GroundNormalScale, _MainTexMult;
															half4 _TilingTint;
															half4 _TilingTrail;

															float _ShininessTiling, _ShininessGround;
															float _HasRT;
															float4 _ProjectedShadowColor, _RimColor;
															float _ProceduralDistance, _ProceduralStrength;

															float4 blendMultiply(float4 baseTex, float4 blendTex, float opacity)
															{
																float4 baseBlend = baseTex * blendTex;
																float4 ret = lerp(baseTex, baseBlend, opacity);
																return ret;
															}

															float2 hash2D2D(float2 s)
															{
																//magic numbers
																return frac(sin(s) * 4.5453);
															}

															//stochastic sampling
															float4 tex2DStochastic(sampler2D tex, float2 UV)
															{
																float4x3 BW_vx;
																float2 skewUV = mul(float2x2 (1.0, 0.0, -0.57735027, 1.15470054), UV * 3.464);

																//vertex IDs and barycentric coords
																float2 vxID = float2 (floor(skewUV));
																float3 barry = float3 (frac(skewUV), 0);
																barry.z = 1.0 - barry.x - barry.y;

																BW_vx = ((barry.z > 0) ?
																	float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
																	float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0 - barry.y, 1.0 - barry.x)));

																//calculate derivatives to avoid triangular grid artifacts
																float2 dx = ddx(UV);
																float2 dy = ddy(UV);

																float4 stochasticTex = mul(tex2D(tex, UV + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
																	mul(tex2D(tex, UV + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
																	mul(tex2D(tex, UV + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
																return stochasticTex;
															}
															InterpolatorsVertex vert(VertexData v) {
																InterpolatorsVertex i;

				#ifdef USE_VR
																UNITY_SETUP_INSTANCE_ID(v);
																//UNITY_INITIALIZE_OUTPUT(InterpolatorsVertex, i);
																//UNITY_TRANSFER_INSTANCE_ID(InterpolatorsVertex, i);
																UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(i);
				#endif

																float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
																float3 originalPos = worldPos;


																float3 rippleMain = 0;
																float3 rippleMainAdditional = 0;

																float ripplesR = 0;
																float ripplesG = 0;
																float ripplesB = 0;

																float uvRTValue = 0;
																float3 mudPos = 0;


																float redValue = saturate(v.color.g + v.color.b);
																float greenValue = (v.color.r + v.color.b) / 2;
																float blueValue = saturate(v.color.r + v.color.g);

																_WaterScale = _WaterScale * 0.01;


				#ifdef USE_RT
																//RT Cam effects
																float2 uv = worldPos.xz - _Position.xz;
																uv = uv / (_OrthographicCamSize * 2);
																uv += 0.5;

																float2 uvAdd = worldPos.xz - _PositionAdd.xz;
																uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
																uvAdd += 0.5;

																if (_HasRT == 1)
																{
																	// .b(lue) = Ground Dig / .r(ed) = Ground To Tiling / .g(reen) = Water Effect
																	rippleMain = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
																	rippleMainAdditional = tex2Dlod(_GlobalEffectRTAdditional, float4(uvAdd, 0, 0));
																}

																float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
																uvRTValue = saturate(uvGradient.x);

																ripplesR = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
																ripplesG = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
																ripplesB = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);

																v.color.g = saturate(v.color.g - ripplesR);

																redValue = saturate(v.color.g + v.color.b - ripplesR);
																greenValue = (v.color.r + v.color.b) / 2;
																blueValue = saturate(v.color.r + v.color.g);

																ripplesB = ripplesB + (1 - blueValue);
				#else

																ripplesB = ripplesB + (1 - blueValue);
				#endif


				#ifdef IS_T
																i.uv.xy = v.uv;
																float4 tilingUV = float4(i.uv.xy, 0, 0) * _TilingScale;

																float rotationAngle = _TilingAngle * 3.14156 / 180.0;
																float sina, cosa;
																sincos(rotationAngle, sina, cosa);
																float2x2 m = float2x2(cosa, -sina, sina, cosa);
																tilingUV = float4(mul(m, tilingUV), 0, 0);
																_GroundScale = _GroundScale * _OverallScale * 10;
																_TilingScale = _TilingScale * _OverallScale * 10;
																_DisplacementStrength = _DisplacementStrength * 0.1;
																_DisplacementTileStrength = _DisplacementTileStrength * 0.1;
				#else

				#ifdef USE_WT
																i.uv.xy = float2(worldPos.x + _MainTex_ST.z, worldPos.z + _MainTex_ST.w) * _OverallScale * 0.05;
				#else
																i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) * _OverallScale;
				#endif

																float4 tilingUV = float4(i.uv.xy, 0, 0) * _TilingScale;

																float rotationAngle = _TilingAngle * 3.14156 / 180.0;
																float sina, cosa;
																sincos(rotationAngle, sina, cosa);
																float2x2 m = float2x2(cosa, -sina, sina, cosa);
																tilingUV = float4(mul(m, tilingUV), 0, 0);
				#endif
																i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

																i.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject));

																float2 flowDirection = float2(-_Time.y * _WaterSpeed, 0);
																float3 bf = normalize(abs(i.normalDir));
																bf /= dot(bf, (float3)1);


																float2 tx = -worldPos.yx + float2(-_Time.y * _WaterSpeed, 0);
																float2 ty = -worldPos.yz + float2(-_Time.y * _WaterSpeed, 0);
																float2 tz = -worldPos.zx + float2(-_Time.y * _WaterSpeed * 0.5, 0);

				#ifndef USE_LOW
																float3 mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale, 0, 0)).r;
																mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale * 1.33 + 0.23, 0, 0)).g;
																mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale * 2.84 + 0.83, 0, 0)).b;

																float3 mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale, 0, 0)).r;
																mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale * 1.33 + 0.23, 0, 0)).g;
																mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale * 2.84 + 0.83, 0, 0)).b;

																float3 mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale, 0, 0)).r;
																mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale * 1.33 + 0.23, 0, 0)).g;
																mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale * 2.84 + 0.83, 0, 0)).b;

				#else
																float3 mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale, 0, 0)).rgb;

																float3 mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale, 0, 0)).rgb;

																float3 mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale, 0, 0)).rgb;
				#endif

																float3 mudSpecWater = lerp(mudWaterSpecB.rgb, mudWaterSpecA.rgb, saturate(pow(bf.z, 1)));
																mudSpecWater = lerp(mudSpecWater, mudWaterSpecC.rgb, pow(saturate(bf.y * 1), 3));

																float mudSpecular = lerp((tex2Dlod(_MudSpecular, float4(i.uv.xy * _MudScale, 0, 0))).r, 0, _WaterLevel);

																float mudHeight = saturate(1 - mudSpecular) * saturate((mudSpecWater.r + mudSpecWater.g + mudSpecWater.b) / 3 * 1.55);
																float mudHeightUnchanged = saturate(1 - mudSpecular);



																float slopeValue = 0;
				#ifdef IS_T
																half4 splat_control = tex2Dlod(_Control0, float4(i.uv.xy, 0, 0));
				#ifdef USE_COMPLEX_T
																half4 splat_control1 = tex2Dlod(_Control1, float4(i.uv.xy, 0, 0));
				#endif

				#ifdef USE_COMPLEX_T
																float tilingValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
																	- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7);
				#else
																float tilingValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3);
				#endif
																float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(pow(tilingValue, 3)));

																float2 terrainOffsetST0 = (_TerrainScale.xz / _Splat0_ST.xy);
																float2 terrainOffsetST1 = (_TerrainScale.xz / _Splat1_ST.xy);
																float2 terrainOffsetST2 = (_TerrainScale.xz / _Splat2_ST.xy);
																float2 terrainOffsetST3 = (_TerrainScale.xz / _Splat3_ST.xy);
				#ifdef USE_COMPLEX_T
																float2 terrainOffsetST4 = (_Splat4_ST.xy);
																float2 terrainOffsetST5 = (_Splat5_ST.xy);
																float2 terrainOffsetST6 = (_Splat6_ST.xy);
																float2 terrainOffsetST7 = (_Splat7_ST.xy);
				#endif

																float groundHeightNew = _Mask0.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST0 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic0), 0).r;
																groundHeightNew = lerp(groundHeightNew, _Mask1.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST1 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic1), 0).r, saturate(splat_control.g));
																groundHeightNew = lerp(groundHeightNew, _Mask2.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST2 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic2), 0).r, saturate(splat_control.b));
																groundHeightNew = lerp(groundHeightNew, _Mask3.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST3 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic3), 0).r, saturate(splat_control.a));
				#ifdef USE_COMPLEX_T
																groundHeightNew = lerp(groundHeightNew, _Mask4.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST4 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic4), 0).r, saturate(splat_control1.r));
																groundHeightNew = lerp(groundHeightNew, _Mask5.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST5 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic5), 0).r, saturate(splat_control1.g));
																groundHeightNew = lerp(groundHeightNew, _Mask6.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST6 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic6), 0).r, saturate(splat_control1.b));
																groundHeightNew = lerp(groundHeightNew, _Mask7.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST7 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic7), 0).r, saturate(splat_control1.a));
				#endif

																float groundHeight = groundHeightNew;
				#else
																float tilingValue = 0;

																tilingValue = redValue;

				#ifdef IS_GROUND
																tilingValue = 1;
				#endif
				#ifdef IS_TILING
																tilingValue = 0;
				#endif


																float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(1 - pow(tilingValue, 30)));

																float groundHeight = lerp(tex2Dlod(_GroundHeight, float4(i.uv.xy, 0, 0) * heightTile * _GroundScale).r, tex2Dlod(_ParallaxMapTiling, tilingUV * heightTile).r, 1 - saturate(pow(tilingValue, 5)));

				#endif


																i.normal = normalize(v.normal);

																float3 newNormal = normalize(i.normalDir);

																float displacementStrength = lerp(_DisplacementStrength, _DisplacementTileStrength, saturate(1 - pow(tilingValue, 3)));
																float displacementValue = lerp(_DisplacementOffset, _DisplacementTileOffset, saturate(1 - tilingValue));

																worldPos += ((float4(0, -_RemoveGroundStrength, 0, 0) * _UpVector - newNormal * _RemoveGroundStrength * _NormalVector * displacementStrength) * ripplesB + (float4(0, groundHeight, 0, 0) * _UpVector + newNormal * groundHeight * _NormalVector * displacementStrength) * saturate(1 - ripplesB));

																worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(1 - tilingValue));

																worldPos += (float4(0, 2 * displacementStrength * groundHeight, 0, 0) * _UpVector) + (newNormal * 2 * displacementStrength * groundHeight * _NormalVector) + newNormal * displacementValue * _NormalVector;

																float _Speed = 1.5;
																float _Frequency = 20;
																float _Amplitude = 0.2;

																mudSpecWater = lerp(0, mudSpecWater, saturate(1 - mudHeight));
																float mudSpecWaterValue = saturate((mudSpecWater.r + mudSpecWater.g + mudSpecWater.b) / 3 * 1.55);
																mudSpecWaterValue += ripplesG * 0.5;

																mudSpecWaterValue = lerp(0, mudSpecWaterValue, saturate(_WaterLevel + 1));

																float3 wNormal = mul(unity_ObjectToWorld, v.normal);

																mudPos.xyz = lerp(0, wNormal.xyz * mudSpecWaterValue, saturate(_WaterLevel + 1));

																worldPos.xyz += lerp(0, mudPos.xyz * _MudWaveHeight, saturate(pow(mudHeightUnchanged, 5) * ripplesB + ripplesG * ripplesB * 0.25)) * tilingValue;

																worldPos.xyz += lerp(0, (wNormal.xyz * mudHeightUnchanged), _MudHeight * ripplesB) * tilingValue;

				#ifdef IS_TILING
				#else
																worldPos = lerp(mul(unity_ObjectToWorld, v.vertex), worldPos, saturate((v.color.r + v.color.b)));
				#endif

																v.vertex.xyz = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;

																//i.pos = GetVertexPositionInputs(v.vertex).positionCS;
																i.pos = TransformWorldToHClip(ApplyShadowBias(GetVertexPositionInputs(v.vertex).positionWS, GetVertexNormalInputs(v.normal).normalWS, _LightDirection));


																float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
																float3 viewDir = v.vertex.xyz - objCam.xyz;

				#ifdef IS_T
																float4 finalTangent = float4 (1.0, 0.0, 0.0, -1.0);
																finalTangent.xyz = finalTangent.xyz - v.normal * dot(v.normal, finalTangent.xyz); // Orthogonalize tangent to normal.

																float tangentSign = finalTangent.w * unity_WorldTransformParams.w;
																float3 bitangent = cross(v.normal.xyz, finalTangent.xyz) * tangentSign;

																i.viewDir = float3(
																	dot(viewDir, finalTangent.xyz),
																	dot(viewDir, bitangent.xyz),
																	dot(viewDir, v.normal.xyz)
																	);

																i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
																i.tangent = finalTangent;

				#else
																float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
																float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) * tangentSign;

																i.viewDir = float3(
																	dot(viewDir, v.tangent.xyz),
																	dot(viewDir, bitangent.xyz),
																	dot(viewDir, v.normal.xyz)
																	);

																i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
																i.tangent = v.tangent;
																float4 finalTangent = v.tangent;
				#endif

																i.color = v.color;
																i.color.a = lerp(1, 0, pow(-mudPos.y * pow(mudSpecWaterValue, 5), 5) * 10);



				#ifdef SHADER_API_MOBILE
																TRANSFER_VERTEX_TO_FRAGMENT(i);
				#endif
																//TRANSFER_SHADOW_CASTER_NORMALOFFSET(i)
																return i;
															}

															float4 frag(InterpolatorsVertex i) : SV_Target
															{
										#ifdef USE_VR
															UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)
										#endif

															_WaterScale = _WaterScale * 0.01;


																							float uvRTValue = 0;
																							float2 uv = i.worldPos.xz - _Position.xz;
																							uv = uv / (_OrthographicCamSize * 2);
																							uv += 0.5;

																							float2 uvAdd = i.worldPos.xz - _PositionAdd.xz;
																							uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
																							uvAdd += 0.5;

																							float3 rippleMain = 0;
																							float3 rippleMainAdditional = 0;
																							float3 calculatedNormal = 0;
																							float3 calculatedNormalAdd = 0;

																							float ripplesR = 0;
																							float ripplesG = 0;
																							float ripplesB = 0;
																							float tilingValue = 1;
																							float oldTilingValue = 1;
																							float groundHeight = 0;
																							float groundHeightReal = 0;

																							float2 tilingUV = i.uv * _TilingScale;

																							float rotationAngle = _TilingAngle * 3.14156 / 180.0;
																							float sina, cosa;
																							sincos(rotationAngle, sina, cosa);
																							float2x2 m = float2x2(cosa, -sina, sina, cosa);
																							tilingUV = mul(m, tilingUV);

															#ifdef IS_TILING
																							float redValue = 1;
																							float greenValue = 1;
																							float blueValue = 1;
															#else
															#ifdef USE_RT

																							if (_HasRT == 1)
																							{
																								rippleMain = tex2D(_GlobalEffectRT, uv);
																								rippleMainAdditional = tex2D(_GlobalEffectRTAdditional, uvAdd);

																								float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - i.worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
																								uvRTValue = saturate(uvGradient.x);
																								ripplesR = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
																								ripplesG = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
																								ripplesB = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);
																							}
															#endif

																							float redValue = saturate(i.color.g + i.color.b - ripplesR);
																							float greenValue = saturate(i.color.r + i.color.b);
																							float blueValue = saturate(i.color.r + i.color.g);
															#endif


																							ripplesB = saturate(ripplesB + (1 - blueValue) - saturate(ripplesR));
																							tilingValue = 1;

															#ifdef IS_T

																							_GroundScale = _GroundScale * _OverallScale * 10;
																							_TilingScale = _TilingScale * _OverallScale * 10;
															#else
															#ifdef IS_GROUND
																							tilingValue = 1;
															#else
																							tilingValue = lerp(tilingValue, 0, saturate(pow(tex2D(_ParallaxMapTiling, tilingUV).r * 2 , 2) * (1 - saturate(i.color.g + i.color.b))));
																							tilingValue = lerp(tilingValue, 0, saturate(ripplesR));
																							oldTilingValue = tilingValue;

															#endif
															#endif

																							float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(1 - pow(tilingValue, 3)));


															#ifdef USE_PR
																							float dist = clamp(lerp(0, 1, (distance(_WorldSpaceCameraPos, i.worldPos) - _ProceduralDistance) / max(_ProceduralStrength, 0.05)), 0, 1);
															#endif

															#ifdef USE_PR
																							groundHeightReal = lerp(tex2D(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r, tex2DStochastic(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r, dist);
															#else
																							groundHeightReal = tex2D(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r;
															#endif

															#ifdef IS_T
															#else
															#ifndef USE_LOW

															#endif
															#endif

																				#ifdef IS_T
																							half4 splat_control = tex2D(_Control0, i.uv);
															#ifdef USE_COMPLEX_T
																							half4 splat_control1 = tex2D(_Control1, i.uv);
															#endif
																							splat_control.r = lerp(splat_control.r, 0, saturate(ripplesR));
																							splat_control.g = lerp(splat_control.g, 1, saturate(ripplesR));
															#ifdef USE_COMPLEX_T
																							splat_control1.r = lerp(splat_control1.r, 0, saturate(ripplesR));
																							splat_control1.g = lerp(splat_control1.g, 1, saturate(ripplesR));
															#endif

															#ifdef USE_COMPLEX_T
																							float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
																								- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7);
															#else								
																							float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3);
															#endif

															#ifdef IS_GROUND
																							tilingValue = 1;
															#else
																							tilingValue = pow(splatOverall, 1 + clamp(abs((groundHeight - 0.5) * 20) * splatOverall, -1, 1));
															#endif
															#endif

																						half transparency = 1;

																						transparency = saturate(lerp(_TransparencyValue, 1, saturate(pow(tilingValue, 2))));

																						float3 albedo = 1;

																				return float4(albedo, transparency);
															}

																				ENDHLSL
									}


										// DepthNormals PASS
										Pass{
										Tags {
											"LightMode" = "DepthNormalsOnly"
										}

										HLSLPROGRAM

										#pragma target 4.6


												#pragma multi_compile _ LOD_FADE_CROSSFADE

												#pragma multi_compile_fwdbase

												#pragma vertex vert
												#pragma fragment frag

												#define FORWARD_BASE_PASS
												#pragma shader_feature USE_AL
												#pragma shader_feature USE_RT
												#pragma shader_feature IS_ADD
												#pragma shader_feature USE_INTER
												#pragma shader_feature USE_WC



																sampler2D  _DetailTex;
																float4 _MainTex_ST, _DetailTex_ST;

																sampler2D _NormalMap;

																half4 _Color;

																sampler2D _TerrainHolesTexture;
																// Render Texture Effects //
																uniform sampler2D _GlobalEffectRT;
																uniform float3 _Position;
																uniform float _OrthographicCamSize;
																uniform sampler2D _GlobalEffectRTAdditional;
																uniform float3 _PositionAdd;
																uniform float _OrthographicCamSizeAdditional;

																sampler2D _MainTex;
																sampler2D _LittleSpec;


																float _ParallaxValue;
																float _TilingAngle;
																uint _ParallaxMinSamples;
																uint _ParallaxMaxSamples;
																sampler2D _ParallaxMapTiling;
																Texture2D _TileMask;
																Texture2D _MudTex;
																sampler2D _MudSpecular;
																Texture2D _MudNormal;

																half4 _BotColor;
																half4 _MudWaterSpecularColor;

																float _SpecForce, _SpecMult, _LittleSpecSize, _LittleSpecForce, _UpVector, _NormalVector, _TilingScale, _GroundScale, _WaterScale , _WaterSpeed, _WaterLevel, _TransparencyValue, _AOValue;
																float _NormalRTDepth, _NormalRTStrength, _RemoveGroundStrength, _DisplacementStrength, _DisplacementTileStrength, _NormalMultiplier;

																//Tiling Variables
																sampler2D _TilingTex;
																sampler2D _NormalTex;
																sampler2D _Roughness;
																sampler2D _GroundHeight;
																Texture2D _MudWater;
																Texture2D _MudWaterNormal;
																float _HeightScale;
																float _HeightTileScale;
																float _LightOffset;
																float _LightHardness;
																float _LightIntensity;
																float _TilingSaturation;
																float _MudScale;
																float _MudSpecularMultiplier;
																float _MudNormalMultiplier;
																float _MudHeight;
																float _MudWaveHeight;
																float _DisplacementColorMult, _DisplacementShadowMult;
																float4 _MudColor;
																sampler2D _MudWaterSpecular;
																float4 _MudWaterColor;

																half _OffsetScale;
																half _OverallScale;
																half _RoughnessStrength;

																half _NormalScale, _DisplacementOffset, _DisplacementTileOffset, _GroundNormalScale, _MainTexMult;
																half4 _TilingTint;
																half4 _TilingTrail;

																float _ShininessTiling, _ShininessGround;
																float _HasRT;
																float4 _ProjectedShadowColor, _RimColor;
																float _ProceduralDistance, _ProceduralStrength;

										struct VertexData //appdata
										{
											float4 vertex : POSITION;
											float3 normal : NORMAL;
											float4 tangent : TANGENT;
											float2 uv : TEXCOORD0;
											float4 color : COLOR;
					#ifdef USE_VR
												UNITY_VERTEX_INPUT_INSTANCE_ID
					#endif

					#ifdef IS_ADD
					#ifdef USE_INTER
												float2 uv3 : TEXCOORD3;
											float2 uv4 : TEXCOORD4;
											float2 uv6 : TEXCOORD6;
											float2 uv7 : TEXCOORD7;
					#endif
					#endif
										};

										struct InterpolatorsVertex
										{
											float4 pos : SV_POSITION;
											float3 normal : TEXCOORD1;
											float4 tangent : TANGENT;
											float4 uv : TEXCOORD0;
											float4 color : COLOR;
											float3 worldPos : TEXCOORD2;
											float3 viewDir: POSITION1;
											float3 normalDir: TEXCOORD3;
					#ifdef USE_VR
												UNITY_VERTEX_OUTPUT_STEREO
					#endif
										};



										InterpolatorsVertex vert(VertexData v) {
											InterpolatorsVertex i;

					#ifdef USE_VR
											UNITY_SETUP_INSTANCE_ID(v);
											//UNITY_INITIALIZE_OUTPUT(InterpolatorsVertex, i);
											//UNITY_TRANSFER_INSTANCE_ID(InterpolatorsVertex, i);
											UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(i);
					#endif

											float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
											float3 originalPos = worldPos;


											float3 rippleMain = 0;
											float3 rippleMainAdditional = 0;

											float ripplesR = 0;
											float ripplesG = 0;
											float ripplesB = 0;

											float uvRTValue = 0;
											float3 mudPos = 0;


											float redValue = saturate(v.color.g + v.color.b);
											float greenValue = (v.color.r + v.color.b) / 2;
											float blueValue = saturate(v.color.r + v.color.g);

											_WaterScale = _WaterScale * 0.01;


					#ifdef USE_RT
											//RT Cam effects
											float2 uv = worldPos.xz - _Position.xz;
											uv = uv / (_OrthographicCamSize * 2);
											uv += 0.5;

											float2 uvAdd = worldPos.xz - _PositionAdd.xz;
											uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
											uvAdd += 0.5;

											if (_HasRT == 1)
											{
												// .b(lue) = Ground Dig / .r(ed) = Ground To Tiling / .g(reen) = Water Effect
												rippleMain = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
												rippleMainAdditional = tex2Dlod(_GlobalEffectRTAdditional, float4(uvAdd, 0, 0));
											}

											float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
											uvRTValue = saturate(uvGradient.x);

											ripplesR = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
											ripplesG = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
											ripplesB = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);

											v.color.g = saturate(v.color.g - ripplesR);

											redValue = saturate(v.color.g + v.color.b - ripplesR);
											greenValue = (v.color.r + v.color.b) / 2;
											blueValue = saturate(v.color.r + v.color.g);

											ripplesB = ripplesB + (1 - blueValue);
					#else

											ripplesB = ripplesB + (1 - blueValue);
					#endif


					#ifdef IS_T
											i.uv.xy = v.uv;
											float4 tilingUV = float4(i.uv.xy, 0, 0) * _TilingScale;

											float rotationAngle = _TilingAngle * 3.14156 / 180.0;
											float sina, cosa;
											sincos(rotationAngle, sina, cosa);
											float2x2 m = float2x2(cosa, -sina, sina, cosa);
											tilingUV = float4(mul(m, tilingUV), 0, 0);
											_GroundScale = _GroundScale * _OverallScale * 10;
											_TilingScale = _TilingScale * _OverallScale * 10;
											_DisplacementStrength = _DisplacementStrength * 0.1;
											_DisplacementTileStrength = _DisplacementTileStrength * 0.1;
					#else

					#ifdef USE_WT
											i.uv.xy = float2(worldPos.x + _MainTex_ST.z, worldPos.z + _MainTex_ST.w) * _OverallScale * 0.05;
					#else
											i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) * _OverallScale;
					#endif
											float4 tilingUV = float4(i.uv.xy, 0, 0) * _TilingScale;

											float rotationAngle = _TilingAngle * 3.14156 / 180.0;
											float sina, cosa;
											sincos(rotationAngle, sina, cosa);
											float2x2 m = float2x2(cosa, -sina, sina, cosa);
											tilingUV = float4(mul(m, tilingUV), 0, 0);
					#endif
											i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

											i.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject));

											float2 flowDirection = float2(-_Time.y * _WaterSpeed, 0);
											float3 bf = normalize(abs(i.normalDir));
											bf /= dot(bf, (float3)1);


											float2 tx = -worldPos.yx + float2(-_Time.y * _WaterSpeed, 0);
											float2 ty = -worldPos.yz + float2(-_Time.y * _WaterSpeed, 0);
											float2 tz = -worldPos.zx + float2(-_Time.y * _WaterSpeed * 0.5, 0);

					#ifndef USE_LOW
											float3 mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale, 0, 0)).r;
											mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale * 1.33 + 0.23, 0, 0)).g;
											mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale * 2.84 + 0.83, 0, 0)).b;

											float3 mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale, 0, 0)).r;
											mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale * 1.33 + 0.23, 0, 0)).g;
											mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale * 2.84 + 0.83, 0, 0)).b;

											float3 mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale, 0, 0)).r;
											mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale * 1.33 + 0.23, 0, 0)).g;
											mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale * 2.84 + 0.83, 0, 0)).b;

					#else
											float3 mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale, 0, 0)).rgb;

											float3 mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale, 0, 0)).rgb;

											float3 mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale, 0, 0)).rgb;
					#endif

											float3 mudSpecWater = lerp(mudWaterSpecB.rgb, mudWaterSpecA.rgb, saturate(pow(bf.z, 1)));
											mudSpecWater = lerp(mudSpecWater, mudWaterSpecC.rgb, pow(saturate(bf.y * 1), 3));

											float mudSpecular = lerp((tex2Dlod(_MudSpecular, float4(i.uv.xy * _MudScale, 0, 0))).r, 0, _WaterLevel);

											float mudHeight = saturate(1 - mudSpecular) * saturate((mudSpecWater.r + mudSpecWater.g + mudSpecWater.b) / 3 * 1.55);
											float mudHeightUnchanged = saturate(1 - mudSpecular);



											float slopeValue = 0;
					#ifdef IS_T
											half4 splat_control = tex2Dlod(_Control0, float4(i.uv.xy, 0, 0));
					#ifdef USE_COMPLEX_T
											half4 splat_control1 = tex2Dlod(_Control1, float4(i.uv.xy, 0, 0));
					#endif

					#ifdef USE_COMPLEX_T
											float tilingValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
												- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7);
					#else
											float tilingValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3);
					#endif
											float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(pow(tilingValue, 3)));

											float2 terrainOffsetST0 = (_TerrainScale.xz / _Splat0_ST.xy);
											float2 terrainOffsetST1 = (_TerrainScale.xz / _Splat1_ST.xy);
											float2 terrainOffsetST2 = (_TerrainScale.xz / _Splat2_ST.xy);
											float2 terrainOffsetST3 = (_TerrainScale.xz / _Splat3_ST.xy);
					#ifdef USE_COMPLEX_T
											float2 terrainOffsetST4 = (_Splat4_ST.xy);
											float2 terrainOffsetST5 = (_Splat5_ST.xy);
											float2 terrainOffsetST6 = (_Splat6_ST.xy);
											float2 terrainOffsetST7 = (_Splat7_ST.xy);
					#endif

											float groundHeightNew = _Mask0.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST0 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic0), 0).r;
											groundHeightNew = lerp(groundHeightNew, _Mask1.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST1 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic1), 0).r, saturate(splat_control.g));
											groundHeightNew = lerp(groundHeightNew, _Mask2.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST2 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic2), 0).r, saturate(splat_control.b));
											groundHeightNew = lerp(groundHeightNew, _Mask3.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST3 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic3), 0).r, saturate(splat_control.a));
					#ifdef USE_COMPLEX_T
											groundHeightNew = lerp(groundHeightNew, _Mask4.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST4 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic4), 0).r, saturate(splat_control1.r));
											groundHeightNew = lerp(groundHeightNew, _Mask5.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST5 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic5), 0).r, saturate(splat_control1.g));
											groundHeightNew = lerp(groundHeightNew, _Mask6.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST6 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic6), 0).r, saturate(splat_control1.b));
											groundHeightNew = lerp(groundHeightNew, _Mask7.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST7 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic7), 0).r, saturate(splat_control1.a));
					#endif

											float groundHeight = groundHeightNew;
					#else
											float tilingValue = 0;

											tilingValue = redValue;

					#ifdef IS_GROUND
											tilingValue = 1;
					#endif
					#ifdef IS_TILING
											tilingValue = 0;
					#endif


											float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(1 - pow(tilingValue, 30)));

											float groundHeight = lerp(tex2Dlod(_GroundHeight, float4(i.uv.xy, 0, 0) * heightTile * _GroundScale).r, tex2Dlod(_ParallaxMapTiling, tilingUV * heightTile).r, 1 - saturate(pow(tilingValue, 5)));

					#endif


											i.normal = normalize(v.normal);

											float3 newNormal = normalize(i.normalDir);

											float displacementStrength = lerp(_DisplacementStrength, _DisplacementTileStrength, saturate(1 - pow(tilingValue, 3)));
											float displacementValue = lerp(_DisplacementOffset, _DisplacementTileOffset, saturate(1 - tilingValue));

											worldPos += ((float4(0, -_RemoveGroundStrength, 0, 0) * _UpVector - newNormal * _RemoveGroundStrength * _NormalVector * displacementStrength) * ripplesB + (float4(0, groundHeight, 0, 0) * _UpVector + newNormal * groundHeight * _NormalVector * displacementStrength) * saturate(1 - ripplesB));

											worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(1 - tilingValue));

											worldPos += (float4(0, 2 * displacementStrength * groundHeight, 0, 0) * _UpVector) + (newNormal * 2 * displacementStrength * groundHeight * _NormalVector) + newNormal * displacementValue * _NormalVector;

											float _Speed = 1.5;
											float _Frequency = 20;
											float _Amplitude = 0.2;

											mudSpecWater = lerp(0, mudSpecWater, saturate(1 - mudHeight));
											float mudSpecWaterValue = saturate((mudSpecWater.r + mudSpecWater.g + mudSpecWater.b) / 3 * 1.55);
											mudSpecWaterValue += ripplesG * 0.5;

											mudSpecWaterValue = lerp(0, mudSpecWaterValue, saturate(_WaterLevel + 1));

											float3 wNormal = mul(unity_ObjectToWorld, v.normal);

											mudPos.xyz = lerp(0, wNormal.xyz * mudSpecWaterValue, saturate(_WaterLevel + 1));

											worldPos.xyz += lerp(0, mudPos.xyz * _MudWaveHeight, saturate(pow(mudHeightUnchanged, 5) * ripplesB + ripplesG * ripplesB * 0.25)) * tilingValue;

											worldPos.xyz += lerp(0, (wNormal.xyz * mudHeightUnchanged), _MudHeight * ripplesB) * tilingValue;

					#ifdef IS_TILING
					#else
											worldPos = lerp(mul(unity_ObjectToWorld, v.vertex), worldPos, saturate((v.color.r + v.color.b)));
					#endif

											v.vertex.xyz = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;

											i.pos = GetVertexPositionInputs(v.vertex).positionCS;


											float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
											float3 viewDir = v.vertex.xyz - objCam.xyz;

					#ifdef IS_T
											float4 finalTangent = float4 (1.0, 0.0, 0.0, -1.0);
											finalTangent.xyz = finalTangent.xyz - v.normal * dot(v.normal, finalTangent.xyz); // Orthogonalize tangent to normal.

											float tangentSign = finalTangent.w * unity_WorldTransformParams.w;
											float3 bitangent = cross(v.normal.xyz, finalTangent.xyz) * tangentSign;

											i.viewDir = float3(
												dot(viewDir, finalTangent.xyz),
												dot(viewDir, bitangent.xyz),
												dot(viewDir, v.normal.xyz)
												);

											i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
											i.tangent = finalTangent;

					#else
											float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
											float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) * tangentSign;

											i.viewDir = float3(
												dot(viewDir, v.tangent.xyz),
												dot(viewDir, bitangent.xyz),
												dot(viewDir, v.normal.xyz)
												);

											i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
											i.tangent = v.tangent;
											float4 finalTangent = v.tangent;
					#endif

											i.color = v.color;
											i.color.a = lerp(1, 0, pow(-mudPos.y * pow(mudSpecWaterValue, 5), 5) * 10);



					#ifdef SHADER_API_MOBILE
											TRANSFER_VERTEX_TO_FRAGMENT(i);
					#endif
											//TRANSFER_SHADOW_CASTER_NORMALOFFSET(i)
											return i;

										}

													float4 frag(InterpolatorsVertex i) : SV_Target
													{
											#ifdef USE_VR
																UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)
											#endif

																_WaterScale = _WaterScale * 0.01;


																								float uvRTValue = 0;
																								float2 uv = i.worldPos.xz - _Position.xz;
																								uv = uv / (_OrthographicCamSize * 2);
																								uv += 0.5;

																								float2 uvAdd = i.worldPos.xz - _PositionAdd.xz;
																								uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
																								uvAdd += 0.5;

																								float3 rippleMain = 0;
																								float3 rippleMainAdditional = 0;
																								float3 calculatedNormal = 0;
																								float3 calculatedNormalAdd = 0;

																								float ripplesR = 0;
																								float ripplesG = 0;
																								float ripplesB = 0;
																								float tilingValue = 1;
																								float oldTilingValue = 1;
																								float groundHeight = 0;
																								float groundHeightReal = 0;

																								float2 tilingUV = i.uv * _TilingScale;

																								float rotationAngle = _TilingAngle * 3.14156 / 180.0;
																								float sina, cosa;
																								sincos(rotationAngle, sina, cosa);
																								float2x2 m = float2x2(cosa, -sina, sina, cosa);
																								tilingUV = mul(m, tilingUV);

																#ifdef IS_TILING
																								float redValue = 1;
																								float greenValue = 1;
																								float blueValue = 1;
																#else
																#ifdef USE_RT

																								if (_HasRT == 1)
																								{
																									rippleMain = tex2D(_GlobalEffectRT, uv);
																									rippleMainAdditional = tex2D(_GlobalEffectRTAdditional, uvAdd);

																									float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - i.worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
																									uvRTValue = saturate(uvGradient.x);
																									ripplesR = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
																									ripplesG = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
																									ripplesB = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);
																								}
																#endif

																								float redValue = saturate(i.color.g + i.color.b - ripplesR);
																								float greenValue = saturate(i.color.r + i.color.b);
																								float blueValue = saturate(i.color.r + i.color.g);
																#endif


																								ripplesB = saturate(ripplesB + (1 - blueValue) - saturate(ripplesR));
																								tilingValue = 1;

																#ifdef IS_T

																								_GroundScale = _GroundScale * _OverallScale * 10;
																								_TilingScale = _TilingScale * _OverallScale * 10;
																#else
																#ifdef IS_GROUND
																								tilingValue = 1;
																#else
																								tilingValue = lerp(tilingValue, 0, saturate(pow(tex2D(_ParallaxMapTiling, tilingUV).r * 2 , 2) * (1 - saturate(i.color.g + i.color.b))));
																								tilingValue = lerp(tilingValue, 0, saturate(ripplesR));
																								oldTilingValue = tilingValue;

																#endif
																#endif

																								float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(1 - pow(tilingValue, 3)));


																#ifdef USE_PR
																								float dist = clamp(lerp(0, 1, (distance(_WorldSpaceCameraPos, i.worldPos) - _ProceduralDistance) / max(_ProceduralStrength, 0.05)), 0, 1);
																#endif

																#ifdef USE_PR
																								groundHeightReal = lerp(tex2D(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r, tex2DStochastic(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r, dist);
																#else
																								groundHeightReal = tex2D(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r;
																#endif

																#ifdef IS_T
																#else
																#ifndef USE_LOW

																#endif
																#endif

																					#ifdef IS_T
																								half4 splat_control = tex2D(_Control0, i.uv);
																#ifdef USE_COMPLEX_T
																								half4 splat_control1 = tex2D(_Control1, i.uv);
																#endif
																								splat_control.r = lerp(splat_control.r, 0, saturate(ripplesR));
																								splat_control.g = lerp(splat_control.g, 1, saturate(ripplesR));
																#ifdef USE_COMPLEX_T
																								splat_control1.r = lerp(splat_control1.r, 0, saturate(ripplesR));
																								splat_control1.g = lerp(splat_control1.g, 1, saturate(ripplesR));
																#endif

																#ifdef USE_COMPLEX_T
																								float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
																									- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7);
																#else								
																								float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3);
																#endif

																#ifdef IS_GROUND
																								tilingValue = 1;
																#else
																								tilingValue = pow(splatOverall, 1 + clamp(abs((groundHeight - 0.5) * 20) * splatOverall, -1, 1));
																#endif
																#endif

																							half transparency = 1;

																							transparency = saturate(lerp(_TransparencyValue, 1, saturate(pow(tilingValue, 2))));

																							float3 albedo = 1;

																					return float4(albedo, transparency);
													}


																				ENDHLSL
										}

																					// DepthNormals PASS
																					Pass{
																					Tags {
																						"LightMode" = "DepthNormals"
																					}

																					HLSLPROGRAM

																					#pragma target 4.6


																							#pragma multi_compile _ LOD_FADE_CROSSFADE

																							#pragma multi_compile_fwdbase

																							#pragma vertex vert
																							#pragma fragment frag

																							#define FORWARD_BASE_PASS
																							#pragma shader_feature USE_AL
																							#pragma shader_feature USE_RT
																							#pragma shader_feature IS_ADD
																							#pragma shader_feature USE_INTER
																							#pragma shader_feature USE_WC



																											sampler2D  _DetailTex;
																											float4 _MainTex_ST, _DetailTex_ST;

																											sampler2D _NormalMap;

																											half4 _Color;

																											sampler2D _TerrainHolesTexture;
																											// Render Texture Effects //
																											uniform sampler2D _GlobalEffectRT;
																											uniform float3 _Position;
																											uniform float _OrthographicCamSize;
																											uniform sampler2D _GlobalEffectRTAdditional;
																											uniform float3 _PositionAdd;
																											uniform float _OrthographicCamSizeAdditional;

																											sampler2D _MainTex;
																											sampler2D _LittleSpec;


																											float _ParallaxValue;
																											float _TilingAngle;
																											uint _ParallaxMinSamples;
																											uint _ParallaxMaxSamples;
																											sampler2D _ParallaxMapTiling;
																											Texture2D _TileMask;
																											Texture2D _MudTex;
																											sampler2D _MudSpecular;
																											Texture2D _MudNormal;

																											half4 _BotColor;
																											half4 _MudWaterSpecularColor;

																											float _SpecForce, _SpecMult, _LittleSpecSize, _LittleSpecForce, _UpVector, _NormalVector, _TilingScale, _GroundScale, _WaterScale , _WaterSpeed, _WaterLevel, _TransparencyValue, _AOValue;
																											float _NormalRTDepth, _NormalRTStrength, _RemoveGroundStrength, _DisplacementStrength, _DisplacementTileStrength, _NormalMultiplier;

																											//Tiling Variables
																											sampler2D _TilingTex;
																											sampler2D _NormalTex;
																											sampler2D _Roughness;
																											sampler2D _GroundHeight;
																											Texture2D _MudWater;
																											Texture2D _MudWaterNormal;
																											float _HeightScale;
																											float _HeightTileScale;
																											float _LightOffset;
																											float _LightHardness;
																											float _LightIntensity;
																											float _TilingSaturation;
																											float _MudScale;
																											float _MudSpecularMultiplier;
																											float _MudNormalMultiplier;
																											float _MudHeight;
																											float _MudWaveHeight;
																											float _DisplacementColorMult, _DisplacementShadowMult;
																											float4 _MudColor;
																											sampler2D _MudWaterSpecular;
																											float4 _MudWaterColor;

																											half _OffsetScale;
																											half _OverallScale;
																											half _RoughnessStrength;

																											half _NormalScale, _DisplacementOffset, _DisplacementTileOffset, _GroundNormalScale, _MainTexMult;
																											half4 _TilingTint;
																											half4 _TilingTrail;

																											float _ShininessTiling, _ShininessGround;
																											float _HasRT;
																											float4 _ProjectedShadowColor, _RimColor;
																											float _ProceduralDistance, _ProceduralStrength;

																					struct VertexData //appdata
																					{
																						float4 vertex : POSITION;
																						float3 normal : NORMAL;
																						float4 tangent : TANGENT;
																						float2 uv : TEXCOORD0;
																						float4 color : COLOR;
																#ifdef USE_VR
																							UNITY_VERTEX_INPUT_INSTANCE_ID
																#endif

																#ifdef IS_ADD
																#ifdef USE_INTER
																							float2 uv3 : TEXCOORD3;
																						float2 uv4 : TEXCOORD4;
																						float2 uv6 : TEXCOORD6;
																						float2 uv7 : TEXCOORD7;
																#endif
																#endif
																					};

																					struct InterpolatorsVertex
																					{
																						float4 pos : SV_POSITION;
																						float3 normal : TEXCOORD1;
																						float4 tangent : TANGENT;
																						float4 uv : TEXCOORD0;
																						float4 color : COLOR;
																						float3 worldPos : TEXCOORD2;
																						float3 viewDir: POSITION1;
																						float3 normalDir: TEXCOORD3;
																#ifdef USE_VR
																							UNITY_VERTEX_OUTPUT_STEREO
																#endif
																					};



																					InterpolatorsVertex vert(VertexData v) {
																						InterpolatorsVertex i;

																#ifdef USE_VR
																						UNITY_SETUP_INSTANCE_ID(v);
																						//UNITY_INITIALIZE_OUTPUT(InterpolatorsVertex, i);
																						//UNITY_TRANSFER_INSTANCE_ID(InterpolatorsVertex, i);
																						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(i);
																#endif

																						float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
																						float3 originalPos = worldPos;


																						float3 rippleMain = 0;
																						float3 rippleMainAdditional = 0;

																						float ripplesR = 0;
																						float ripplesG = 0;
																						float ripplesB = 0;

																						float uvRTValue = 0;
																						float3 mudPos = 0;


																						float redValue = saturate(v.color.g + v.color.b);
																						float greenValue = (v.color.r + v.color.b) / 2;
																						float blueValue = saturate(v.color.r + v.color.g);

																						_WaterScale = _WaterScale * 0.01;


																#ifdef USE_RT
																						//RT Cam effects
																						float2 uv = worldPos.xz - _Position.xz;
																						uv = uv / (_OrthographicCamSize * 2);
																						uv += 0.5;

																						float2 uvAdd = worldPos.xz - _PositionAdd.xz;
																						uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
																						uvAdd += 0.5;

																						if (_HasRT == 1)
																						{
																							// .b(lue) = Ground Dig / .r(ed) = Ground To Tiling / .g(reen) = Water Effect
																							rippleMain = tex2Dlod(_GlobalEffectRT, float4(uv, 0, 0));
																							rippleMainAdditional = tex2Dlod(_GlobalEffectRTAdditional, float4(uvAdd, 0, 0));
																						}

																						float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
																						uvRTValue = saturate(uvGradient.x);

																						ripplesR = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
																						ripplesG = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
																						ripplesB = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);

																						v.color.g = saturate(v.color.g - ripplesR);

																						redValue = saturate(v.color.g + v.color.b - ripplesR);
																						greenValue = (v.color.r + v.color.b) / 2;
																						blueValue = saturate(v.color.r + v.color.g);

																						ripplesB = ripplesB + (1 - blueValue);
																#else

																						ripplesB = ripplesB + (1 - blueValue);
																#endif


																#ifdef IS_T
																						i.uv.xy = v.uv;
																						float4 tilingUV = float4(i.uv.xy, 0, 0) * _TilingScale;

																						float rotationAngle = _TilingAngle * 3.14156 / 180.0;
																						float sina, cosa;
																						sincos(rotationAngle, sina, cosa);
																						float2x2 m = float2x2(cosa, -sina, sina, cosa);
																						tilingUV = float4(mul(m, tilingUV), 0, 0);
																						_GroundScale = _GroundScale * _OverallScale * 10;
																						_TilingScale = _TilingScale * _OverallScale * 10;
																						_DisplacementStrength = _DisplacementStrength * 0.1;
																						_DisplacementTileStrength = _DisplacementTileStrength * 0.1;
																#else

																#ifdef USE_WT
																						i.uv.xy = float2(worldPos.x + _MainTex_ST.z, worldPos.z + _MainTex_ST.w) * _OverallScale * 0.05;
																#else
																						i.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) * _OverallScale;
																#endif
																						float4 tilingUV = float4(i.uv.xy, 0, 0) * _TilingScale;

																						float rotationAngle = _TilingAngle * 3.14156 / 180.0;
																						float sina, cosa;
																						sincos(rotationAngle, sina, cosa);
																						float2x2 m = float2x2(cosa, -sina, sina, cosa);
																						tilingUV = float4(mul(m, tilingUV), 0, 0);
																#endif
																						i.uv.zw = TRANSFORM_TEX(v.uv, _DetailTex);

																						i.normalDir = normalize(mul(float4(v.normal, 0.0), unity_WorldToObject));

																						float2 flowDirection = float2(-_Time.y * _WaterSpeed, 0);
																						float3 bf = normalize(abs(i.normalDir));
																						bf /= dot(bf, (float3)1);


																						float2 tx = -worldPos.yx + float2(-_Time.y * _WaterSpeed, 0);
																						float2 ty = -worldPos.yz + float2(-_Time.y * _WaterSpeed, 0);
																						float2 tz = -worldPos.zx + float2(-_Time.y * _WaterSpeed * 0.5, 0);

																#ifndef USE_LOW
																						float3 mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale, 0, 0)).r;
																						mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale * 1.33 + 0.23, 0, 0)).g;
																						mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale * 2.84 + 0.83, 0, 0)).b;

																						float3 mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale, 0, 0)).r;
																						mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale * 1.33 + 0.23, 0, 0)).g;
																						mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale * 2.84 + 0.83, 0, 0)).b;

																						float3 mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale, 0, 0)).r;
																						mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale * 1.33 + 0.23, 0, 0)).g;
																						mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale * 2.84 + 0.83, 0, 0)).b;

																#else
																						float3 mudWaterSpecA = tex2Dlod(_MudWaterSpecular, float4(tx * _WaterScale, 0, 0)).rgb;

																						float3 mudWaterSpecB = tex2Dlod(_MudWaterSpecular, float4(ty * _WaterScale, 0, 0)).rgb;

																						float3 mudWaterSpecC = tex2Dlod(_MudWaterSpecular, float4(tz * _WaterScale, 0, 0)).rgb;
																#endif

																						float3 mudSpecWater = lerp(mudWaterSpecB.rgb, mudWaterSpecA.rgb, saturate(pow(bf.z, 1)));
																						mudSpecWater = lerp(mudSpecWater, mudWaterSpecC.rgb, pow(saturate(bf.y * 1), 3));

																						float mudSpecular = lerp((tex2Dlod(_MudSpecular, float4(i.uv.xy * _MudScale, 0, 0))).r, 0, _WaterLevel);

																						float mudHeight = saturate(1 - mudSpecular) * saturate((mudSpecWater.r + mudSpecWater.g + mudSpecWater.b) / 3 * 1.55);
																						float mudHeightUnchanged = saturate(1 - mudSpecular);



																						float slopeValue = 0;
																#ifdef IS_T
																						half4 splat_control = tex2Dlod(_Control0, float4(i.uv.xy, 0, 0));
																#ifdef USE_COMPLEX_T
																						half4 splat_control1 = tex2Dlod(_Control1, float4(i.uv.xy, 0, 0));
																#endif

																#ifdef USE_COMPLEX_T
																						float tilingValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
																							- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7);
																#else
																						float tilingValue = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3);
																#endif
																						float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(pow(tilingValue, 3)));

																						float2 terrainOffsetST0 = (_TerrainScale.xz / _Splat0_ST.xy);
																						float2 terrainOffsetST1 = (_TerrainScale.xz / _Splat1_ST.xy);
																						float2 terrainOffsetST2 = (_TerrainScale.xz / _Splat2_ST.xy);
																						float2 terrainOffsetST3 = (_TerrainScale.xz / _Splat3_ST.xy);
																#ifdef USE_COMPLEX_T
																						float2 terrainOffsetST4 = (_Splat4_ST.xy);
																						float2 terrainOffsetST5 = (_Splat5_ST.xy);
																						float2 terrainOffsetST6 = (_Splat6_ST.xy);
																						float2 terrainOffsetST7 = (_Splat7_ST.xy);
																#endif

																						float groundHeightNew = _Mask0.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST0 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic0), 0).r;
																						groundHeightNew = lerp(groundHeightNew, _Mask1.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST1 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic1), 0).r, saturate(splat_control.g));
																						groundHeightNew = lerp(groundHeightNew, _Mask2.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST2 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic2), 0).r, saturate(splat_control.b));
																						groundHeightNew = lerp(groundHeightNew, _Mask3.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST3 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic3), 0).r, saturate(splat_control.a));
																#ifdef USE_COMPLEX_T
																						groundHeightNew = lerp(groundHeightNew, _Mask4.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST4 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic4), 0).r, saturate(splat_control1.r));
																						groundHeightNew = lerp(groundHeightNew, _Mask5.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST5 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic5), 0).r, saturate(splat_control1.g));
																						groundHeightNew = lerp(groundHeightNew, _Mask6.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST6 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic6), 0).r, saturate(splat_control1.b));
																						groundHeightNew = lerp(groundHeightNew, _Mask7.SampleLevel(my_linear_repeat_sampler, float4(i.uv.xy, 0, 0) * terrainOffsetST7 * heightTile * lerp(_GroundScale, _TilingScale, _Metallic7), 0).r, saturate(splat_control1.a));
																#endif

																						float groundHeight = groundHeightNew;
																#else
																						float tilingValue = 0;

																						tilingValue = redValue;

																#ifdef IS_GROUND
																						tilingValue = 1;
																#endif
																#ifdef IS_TILING
																						tilingValue = 0;
																#endif


																						float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(1 - pow(tilingValue, 30)));

																						float groundHeight = lerp(tex2Dlod(_GroundHeight, float4(i.uv.xy, 0, 0) * heightTile * _GroundScale).r, tex2Dlod(_ParallaxMapTiling, tilingUV * heightTile).r, 1 - saturate(pow(tilingValue, 5)));

																#endif


																						i.normal = normalize(v.normal);

																						float3 newNormal = normalize(i.normalDir);

																						float displacementStrength = lerp(_DisplacementStrength, _DisplacementTileStrength, saturate(1 - pow(tilingValue, 3)));
																						float displacementValue = lerp(_DisplacementOffset, _DisplacementTileOffset, saturate(1 - tilingValue));

																						worldPos += ((float4(0, -_RemoveGroundStrength, 0, 0) * _UpVector - newNormal * _RemoveGroundStrength * _NormalVector * displacementStrength) * ripplesB + (float4(0, groundHeight, 0, 0) * _UpVector + newNormal * groundHeight * _NormalVector * displacementStrength) * saturate(1 - ripplesB));

																						worldPos = lerp(worldPos, mul(unity_ObjectToWorld, v.vertex), saturate(1 - tilingValue));

																						worldPos += (float4(0, 2 * displacementStrength * groundHeight, 0, 0) * _UpVector) + (newNormal * 2 * displacementStrength * groundHeight * _NormalVector) + newNormal * displacementValue * _NormalVector;

																						float _Speed = 1.5;
																						float _Frequency = 20;
																						float _Amplitude = 0.2;

																						mudSpecWater = lerp(0, mudSpecWater, saturate(1 - mudHeight));
																						float mudSpecWaterValue = saturate((mudSpecWater.r + mudSpecWater.g + mudSpecWater.b) / 3 * 1.55);
																						mudSpecWaterValue += ripplesG * 0.5;

																						mudSpecWaterValue = lerp(0, mudSpecWaterValue, saturate(_WaterLevel + 1));

																						float3 wNormal = mul(unity_ObjectToWorld, v.normal);

																						mudPos.xyz = lerp(0, wNormal.xyz * mudSpecWaterValue, saturate(_WaterLevel + 1));

																						worldPos.xyz += lerp(0, mudPos.xyz * _MudWaveHeight, saturate(pow(mudHeightUnchanged, 5) * ripplesB + ripplesG * ripplesB * 0.25)) * tilingValue;

																						worldPos.xyz += lerp(0, (wNormal.xyz * mudHeightUnchanged), _MudHeight * ripplesB) * tilingValue;

																#ifdef IS_TILING
																#else
																						worldPos = lerp(mul(unity_ObjectToWorld, v.vertex), worldPos, saturate((v.color.r + v.color.b)));
																#endif

																						v.vertex.xyz = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;

																						i.pos = GetVertexPositionInputs(v.vertex).positionCS;


																						float4 objCam = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0));
																						float3 viewDir = v.vertex.xyz - objCam.xyz;

																#ifdef IS_T
																						float4 finalTangent = float4 (1.0, 0.0, 0.0, -1.0);
																						finalTangent.xyz = finalTangent.xyz - v.normal * dot(v.normal, finalTangent.xyz); // Orthogonalize tangent to normal.

																						float tangentSign = finalTangent.w * unity_WorldTransformParams.w;
																						float3 bitangent = cross(v.normal.xyz, finalTangent.xyz) * tangentSign;

																						i.viewDir = float3(
																							dot(viewDir, finalTangent.xyz),
																							dot(viewDir, bitangent.xyz),
																							dot(viewDir, v.normal.xyz)
																							);

																						i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
																						i.tangent = finalTangent;

																#else
																						float tangentSign = v.tangent.w * unity_WorldTransformParams.w;
																						float3 bitangent = cross(v.normal.xyz, v.tangent.xyz) * tangentSign;

																						i.viewDir = float3(
																							dot(viewDir, v.tangent.xyz),
																							dot(viewDir, bitangent.xyz),
																							dot(viewDir, v.normal.xyz)
																							);

																						i.worldPos.xyz = mul(unity_ObjectToWorld, v.vertex);
																						i.tangent = v.tangent;
																						float4 finalTangent = v.tangent;
																#endif

																						i.color = v.color;
																						i.color.a = lerp(1, 0, pow(-mudPos.y * pow(mudSpecWaterValue, 5), 5) * 10);



																#ifdef SHADER_API_MOBILE
																						TRANSFER_VERTEX_TO_FRAGMENT(i);
																#endif
																						//TRANSFER_SHADOW_CASTER_NORMALOFFSET(i)
																						return i;

																					}

																								float4 frag(InterpolatorsVertex i) : SV_Target
																								{
																						#ifdef USE_VR
																											UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i)
																						#endif

																											_WaterScale = _WaterScale * 0.01;


																																			float uvRTValue = 0;
																																			float2 uv = i.worldPos.xz - _Position.xz;
																																			uv = uv / (_OrthographicCamSize * 2);
																																			uv += 0.5;

																																			float2 uvAdd = i.worldPos.xz - _PositionAdd.xz;
																																			uvAdd = uvAdd / (_OrthographicCamSizeAdditional * 2);
																																			uvAdd += 0.5;

																																			float3 rippleMain = 0;
																																			float3 rippleMainAdditional = 0;
																																			float3 calculatedNormal = 0;
																																			float3 calculatedNormalAdd = 0;

																																			float ripplesR = 0;
																																			float ripplesG = 0;
																																			float ripplesB = 0;
																																			float tilingValue = 1;
																																			float oldTilingValue = 1;
																																			float groundHeight = 0;
																																			float groundHeightReal = 0;

																																			float2 tilingUV = i.uv * _TilingScale;

																																			float rotationAngle = _TilingAngle * 3.14156 / 180.0;
																																			float sina, cosa;
																																			sincos(rotationAngle, sina, cosa);
																																			float2x2 m = float2x2(cosa, -sina, sina, cosa);
																																			tilingUV = mul(m, tilingUV);

																											#ifdef IS_TILING
																																			float redValue = 1;
																																			float greenValue = 1;
																																			float blueValue = 1;
																											#else
																											#ifdef USE_RT

																																			if (_HasRT == 1)
																																			{
																																				rippleMain = tex2D(_GlobalEffectRT, uv);
																																				rippleMainAdditional = tex2D(_GlobalEffectRTAdditional, uvAdd);

																																				float2 uvGradient = smoothstep(0, 5, length(max(abs(_Position.xz - i.worldPos.xz) - _OrthographicCamSize + 5, 0.0)));
																																				uvRTValue = saturate(uvGradient.x);
																																				ripplesR = lerp(rippleMain.x, rippleMainAdditional.x, uvRTValue);
																																				ripplesG = lerp(rippleMain.y, rippleMainAdditional.y, uvRTValue);
																																				ripplesB = lerp(rippleMain.z, rippleMainAdditional.z, uvRTValue);
																																			}
																											#endif

																																			float redValue = saturate(i.color.g + i.color.b - ripplesR);
																																			float greenValue = saturate(i.color.r + i.color.b);
																																			float blueValue = saturate(i.color.r + i.color.g);
																											#endif


																																			ripplesB = saturate(ripplesB + (1 - blueValue) - saturate(ripplesR));
																																			tilingValue = 1;

																											#ifdef IS_T

																																			_GroundScale = _GroundScale * _OverallScale * 10;
																																			_TilingScale = _TilingScale * _OverallScale * 10;
																											#else
																											#ifdef IS_GROUND
																																			tilingValue = 1;
																											#else
																																			tilingValue = lerp(tilingValue, 0, saturate(pow(tex2D(_ParallaxMapTiling, tilingUV).r * 2 , 2) * (1 - saturate(i.color.g + i.color.b))));
																																			tilingValue = lerp(tilingValue, 0, saturate(ripplesR));
																																			oldTilingValue = tilingValue;

																											#endif
																											#endif

																																			float heightTile = lerp(_HeightScale, _HeightTileScale, saturate(1 - pow(tilingValue, 3)));


																											#ifdef USE_PR
																																			float dist = clamp(lerp(0, 1, (distance(_WorldSpaceCameraPos, i.worldPos) - _ProceduralDistance) / max(_ProceduralStrength, 0.05)), 0, 1);
																											#endif

																											#ifdef USE_PR
																																			groundHeightReal = lerp(tex2D(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r, tex2DStochastic(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r, dist);
																											#else
																																			groundHeightReal = tex2D(_GroundHeight, i.uv.xy * heightTile * _GroundScale).r;
																											#endif

																											#ifdef IS_T
																											#else
																											#ifndef USE_LOW

																											#endif
																											#endif

																																#ifdef IS_T
																																			half4 splat_control = tex2D(_Control0, i.uv);
																											#ifdef USE_COMPLEX_T
																																			half4 splat_control1 = tex2D(_Control1, i.uv);
																											#endif
																																			splat_control.r = lerp(splat_control.r, 0, saturate(ripplesR));
																																			splat_control.g = lerp(splat_control.g, 1, saturate(ripplesR));
																											#ifdef USE_COMPLEX_T
																																			splat_control1.r = lerp(splat_control1.r, 0, saturate(ripplesR));
																																			splat_control1.g = lerp(splat_control1.g, 1, saturate(ripplesR));
																											#endif

																											#ifdef USE_COMPLEX_T
																																			float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3
																																				- splat_control1.r * _Metallic4 - splat_control1.g * _Metallic5 - splat_control1.b * _Metallic6 - splat_control1.a * _Metallic7);
																											#else								
																																			float splatOverall = saturate(1 - splat_control.r * _Metallic0 - splat_control.g * _Metallic1 - splat_control.b * _Metallic2 - splat_control.a * _Metallic3);
																											#endif

																											#ifdef IS_GROUND
																																			tilingValue = 1;
																											#else
																																			tilingValue = pow(splatOverall, 1 + clamp(abs((groundHeight - 0.5) * 20) * splatOverall, -1, 1));
																											#endif
																											#endif

																																		half transparency = 1;

																																		transparency = saturate(lerp(_TransparencyValue, 1, saturate(pow(tilingValue, 2))));

																																		float3 albedo = 1;

																																return float4(albedo, transparency);
																								}


																															ENDHLSL
																					}

				}
}