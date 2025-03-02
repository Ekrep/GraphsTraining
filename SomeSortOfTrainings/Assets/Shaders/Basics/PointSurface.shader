Shader "Custom/PointSurface"
{
    Properties
    {
        _Smoothness("Smoothness",Range(0,1)) = 0.5
    }
    Fallback "Diffuse"
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
       

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface ConfigureSurface Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        float _Smoothness;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface)
        {
            surface.Albedo.rg = input.worldPos.xy* 0.5+ 0.5;
            surface.Smoothness = _Smoothness;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
