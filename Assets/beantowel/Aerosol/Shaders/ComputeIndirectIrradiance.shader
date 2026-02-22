Shader "Aerosol/ComputeIndirectIrradiance"
{
    Properties
    {
    }

    SubShader
    {
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            Blend 0 Off
            Blend 1 One One
            HLSLPROGRAM
            #pragma vertex vertex
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "functions.hlsl"

            sampler3D single_rayleigh_scattering_texture;
            sampler3D single_mie_scattering_texture;
            sampler3D multiple_scattering_texture;
            int scattering_order;
            float4x4 luminance_from_radiance;

            struct VS_INPUT
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct VS_OUTPUT
            {
                float4 pos : SV_POSITION;
                float2 texcoords : TEXCOORD0;
            };

            struct PS_OUTPUT
            {
                float4 delta_irradiance : SV_Target0;
                float4 irradiance : SV_Target1;
            };

            VS_OUTPUT vertex(VS_INPUT v)
            {
                VS_OUTPUT output;
                output.pos = UnityObjectToClipPos(v.vertex);
                output.texcoords = v.uv * IRRADIANCE_TEXTURE_SIZE;
                return output;
            }

            PS_OUTPUT frag(VS_OUTPUT input)
            {
                PS_OUTPUT output;
                output.delta_irradiance = float4(ComputeIndirectIrradianceTexture(
                    ATMOSPHERE, single_rayleigh_scattering_texture,
                    single_mie_scattering_texture, multiple_scattering_texture,
                    input.texcoords, scattering_order), 1.0);
                output.irradiance = float4(mul((float3x3)luminance_from_radiance, output.delta_irradiance), 1.0);
                return output;
            }
            ENDHLSL
        }
    }
}
