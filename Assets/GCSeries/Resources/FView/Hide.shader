// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Hide" {
SubShader
    {
        Tags {"Queue" = "Geometry-1998"}
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        ZWrite On
        ColorMask 0
        ZTest Always
        Pass
        {
            Color(0,0,0,0)
        }
    }
}
