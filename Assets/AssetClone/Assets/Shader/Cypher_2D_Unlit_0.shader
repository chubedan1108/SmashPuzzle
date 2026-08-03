Shader "Cypher/2D/Unlit" {
	Properties {
		_MainTex ("Main Texture", 2D) = "white" {}
		[ColorUsage(true, true)] [HDR] _Color ("Tint", Vector) = (1,1,1,1)
		[Toggle] _HasAdditionalTransform ("Has Additional Transform", Float) = 0
		_AdditionalTransform0 ("Additional Transform 0", Vector) = (1,0,0,0)
		_AdditionalTransform1 ("Additional Transform 1", Vector) = (0,1,0,0)
		_AdditionalTransform2 ("Additional Transform 2", Vector) = (0,0,1,0)
		_AdditionalTransform3 ("Additional Transform 3", Vector) = (0,0,0,1)
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrc ("Blend Src", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendDst ("Blend Dst", Float) = 10
		[Enum(UnityEngine.Rendering.BlendOp)] _BlendOp ("Blend Operation", Float) = 0
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _ColorMask ("Color Mask", Float) = 15
		[Enum(UnityEngine.Rendering.CullMode)] _CullV2 ("Cull Mode", Float) = 2
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