Shader "Custom/SeeThroughMask"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags 
        { 
            "RenderType"="Geometry"
            "Queue"="Transparent"
        }

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
            Fail Keep
            ZFail Keep
        }
        ColorMask 0 // Don't write to the color buffer

        Pass
        {
            ZWrite On
            ZTest Always
        }
    }
    FallBack "Diffuse"
}
