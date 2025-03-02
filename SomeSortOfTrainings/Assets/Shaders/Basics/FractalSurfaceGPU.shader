Shader "Custom/FractalSurfaceGPU"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface ConfigureSurface Standard fullforwardshadows addshadow
        #pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
        #pragma editor_sync_compilation

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 4.5

       #include "FractalGPU.hlsl"

        struct Input
        {
            float3 worldPos;
        };
        float _Smoothness;

        void ConfigureSurface (Input input, inout SurfaceOutputStandard surface) {
			surface.Albedo = GetFractalColor().rgb;
			surface.Smoothness = GetFractalColor().a;//Shader compiler recognizes and optimizes away the duplicated work!!!!!
		}
        ENDCG
    }
    FallBack "Diffuse"
}
