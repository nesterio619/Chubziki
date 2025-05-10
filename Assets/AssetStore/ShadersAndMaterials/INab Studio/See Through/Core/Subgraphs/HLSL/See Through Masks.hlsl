#ifndef SEETHROUGHMASKS_INCLUDED
#define SEETHROUGHMASKS_INCLUDED

// Common defines
#define SeeThroughArrayMax 6

// Common
float4 Positions[SeeThroughArrayMax];
float Scales[SeeThroughArrayMax];

float2 WorldToScreenPos(float3 worldPos, float4x4 ViewProjectionMatrix, float ZBufferSign, float AspectRatio)
{
    float4 pos = mul(ViewProjectionMatrix, float4(worldPos, 1.0));

    // Check if the position is behind the camera. For a left-handed coordinate system:
    // If z is greater than w, the position is behind the camera.
    // Adjust this condition if using a right-handed system.
    if (pos.z > pos.w)
    {
        return float2(-1.0, -1.0); // Indicate an invalid position.
    }

    float4 posHalf = pos * 0.5;
    float2 screenPos = float2(posHalf.x, posHalf.y * ZBufferSign) + posHalf.w;
    screenPos /= pos.w;
    
    // Adjust screen position based on aspect ratio.
    return float2(screenPos.x, screenPos.y / AspectRatio);
}

void SeeThroughMasks_float(float2 screenSpacePosition, float4x4 ViewProjectionMatrix, float ZBufferSign, float AspectRatio, float pixelDepth, float _DepthOffset, float _OffsetSmoothness, int _UseDepthOffset, out float Out, out float OutScale)
{
    int NumberOfMasks = 0;

#if defined(_MASKSNUMBER__1)
	NumberOfMasks = 1;
#elif _MASKSNUMBER__2
	NumberOfMasks = 2;
#elif _MASKSNUMBER__3 
	NumberOfMasks = 3;
#elif _MASKSNUMBER__4 
	NumberOfMasks = 4;
#elif _MASKSNUMBER__5 
	NumberOfMasks = 5;
#elif _MASKSNUMBER__6 
	NumberOfMasks = 6;
#endif

    float sum = 99999;
    float outScale = 0;
    
	[unroll]
    for (int i = 0; i < NumberOfMasks; i++)
    {
#if defined(_HDRP)
        float3 MaskScaleWorldPos = GetCameraRelativePositionWS(Positions[i].xyz);
#else     
        float3 MaskScaleWorldPos = Positions[i].xyz;
#endif
        
        float2 MaskOriginScreenPos = WorldToScreenPos(MaskScaleWorldPos, ViewProjectionMatrix, ZBufferSign, AspectRatio);

        //  + 0.0001 makes sure that there is no cutoff dot when radius is 0
        float screenDistance = distance(screenSpacePosition, MaskOriginScreenPos) + 0.0001;
        
        float scale;
        
        float valueAdd = 0;
        float4 MaskScaleViewProjectionPos;
        
        
        MaskScaleViewProjectionPos = mul(ViewProjectionMatrix, float4(MaskScaleWorldPos, 1.0));
        outScale = max(MaskScaleViewProjectionPos.w, outScale);
        
#if defined(_USESTABLESCREENRADIUS)
        scale = Scales[i] * _ScreenRadiusMultiplier;
        
        //if (_UseDepthOffset == 1) 
        //{
        //    MaskScaleViewProjectionPos = mul(ViewProjectionMatrix, float4(MaskScaleWorldPos, 1.0));
        //}
#else
        float2 originScreenPos = WorldToScreenPos(MaskScaleWorldPos, ViewProjectionMatrix, ZBufferSign, AspectRatio);
        
        // This is a hack to obtain the scale of the mask in screen space. It introduces some artifacts and thus the radius is not stable when changing angles.
        
        //float3 MaskScaleWorldPosX = MaskScaleWorldPos + viewDirWorld.x * float3(Scales[i], 0, 0);
        //float3 MaskScaleWorldPosY = MaskScaleWorldPos + viewDirWorld.y * float3(0, Scales[i], 0);
        //float3 MaskScaleWorldPosZ = MaskScaleWorldPos + viewDirWorld.z * float3(0, 0, Scales[i]);
        //
        //float2 scaledScreenPosX = WorldToScreenPos(MaskScaleWorldPosX, ViewProjectionMatrix, ZBufferSign, AspectRatio);
        //float2 scaledScreenPosY = WorldToScreenPos(MaskScaleWorldPosY, ViewProjectionMatrix, ZBufferSign, AspectRatio);
        //float2 scaledScreenPosZ = WorldToScreenPos(MaskScaleWorldPosZ, ViewProjectionMatrix, ZBufferSign, AspectRatio);
        //
        //float scaleDistanceX = distance(originScreenPos, scaledScreenPosX);
        //float scaleDistanceY = distance(originScreenPos, scaledScreenPosY);
        //float scaleDistanceZ = distance(originScreenPos, scaledScreenPosZ);
        //
        //float maxDistance = saturate(max(scaleDistanceX, max(scaleDistanceY, scaleDistanceZ)));
        //scale = maxDistance;
        
        // Implementation of the scale based on the distance to the camera
        // This gives stable results since it uses view projection matrix to calcualtre the mask position in view space
        
        scale = (Scales[i] / MaskScaleViewProjectionPos.w);
#endif
        if (_UseDepthOffset == 1)
        {
            float distanceToCamera = MaskScaleViewProjectionPos.w;
            float depthDifference = pixelDepth - distanceToCamera;
      
            if (pixelDepth + _DepthOffset > distanceToCamera)
            {
                valueAdd = lerp(0, .1, abs((pixelDepth + _DepthOffset - distanceToCamera) / _OffsetSmoothness));
            }
        }
        
        float screenMaskOut = screenDistance - scale + valueAdd;
        
        sum = min(screenMaskOut, sum);
    }

    OutScale = outScale;
	Out = sum;
    //Out = 1;
}

#endif