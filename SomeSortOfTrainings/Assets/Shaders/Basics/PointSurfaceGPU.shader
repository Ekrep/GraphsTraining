Shader "Custom/PointGPU"
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
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.5
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma editor_sync_compilation
        #include "PointGPU.hlsl"

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
        void ConfigureProcedural(Input input, inout SurfaceOutputStandard surface)
        {
            #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
            float3 position=_Positions[unity_InstanceID];
            unity_ObjectToWorld=0.0;
            unity_ObjectToWorld._m03_m13_m23_m33=float4(position,1.0);
            unity_ObjectToWorld._m00_m11_m22=_Step;
            #endif
        }

        void ConfigureSurface(Input input, inout SurfaceOutputStandard surface)
        {
            surface.Albedo.rg = input.worldPos.xy* 0.5+ 0.5;
            surface.Smoothness = _Smoothness;

        }
        ENDCG
    }
    FallBack "Diffuse"
}
