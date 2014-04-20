Shader "Custom/Colours_Only" 
{
	Properties 
	{
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma surface surf Lambert

		struct Input
		{
			float4 color : COLOR;
		};

		void surf( Input input, inout SurfaceOutput output ) 
		{
			output.Albedo = input.color.xyz;
			output.Alpha = input.color.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
