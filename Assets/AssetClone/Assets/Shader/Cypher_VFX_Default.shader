Shader "Cypher/VFX/Default" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		[ColorUsage(true, true)] [HDR] _Color ("Tint", Vector) = (1,1,1,1)
		_AlphaRange00 ("Alpha Range0 Start", Float) = 0
		_AlphaRange01 ("Alpha Range0 End", Float) = 1
		_AlphaRange10 ("Alpha Range1 Start", Float) = 0
		_AlphaRange11 ("Alpha Range1 End", Float) = 1
		_ColorMul0 ("Color Mul 0", Float) = 1
		_ColorMul1 ("Color Mul 1", Float) = 1
		_AlphaNoiseTex ("Alpha Noise Texture", 2D) = "white" {}
		_AlphaNoiseVelocity ("Alpha Noise Velocity", Vector) = (0,0,0,0)
		_AlphaNoiseRemap ("Alpha Noise Remap", Vector) = (0,1,0,1)
	}
	//DummyShaderTextExporter
	SubShader{
		Tags { "RenderType"="Opaque" }
		LOD 200

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float4x4 unity_ObjectToWorld;
			float4x4 unity_MatrixVP;
			float4 _MainTex_ST;

			struct Vertex_Stage_Input
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct Vertex_Stage_Output
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
			};

			Vertex_Stage_Output vert(Vertex_Stage_Input input)
			{
				Vertex_Stage_Output output;
				output.uv = (input.uv.xy * _MainTex_ST.xy) + _MainTex_ST.zw;
				output.pos = mul(unity_MatrixVP, mul(unity_ObjectToWorld, input.pos));
				return output;
			}

			Texture2D<float4> _MainTex;
			SamplerState sampler_MainTex;
			float4 _Color;

			struct Fragment_Stage_Input
			{
				float2 uv : TEXCOORD0;
			};

			float4 frag(Fragment_Stage_Input input) : SV_TARGET
			{
				return _MainTex.Sample(sampler_MainTex, input.uv.xy) * _Color;
			}

			ENDHLSL
		}
	}
}