Shader "Diffuse Vertex Color" {
	Properties {
		_Color ("Main Color", Color) = (1,1,1,1)
		_Tint ("Tint", Color) = (1,1,1,1)
		_MainTex ("Base (RGB)", 2D) = "white" {}
	}
	SubShader {
	
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
			#pragma surface surf Lambert vertex:vert

			sampler2D _MainTex;
			fixed4 _Color;
			float4 _Tint;

			struct Input {
				float2 uv_MainTex;
				float4 color: COLOR;
			};

			void vert(inout appdata_full v) {
				//if(v.texcoord1.x>0) v.vertex.y += (v.texcoord1.x * _SinTime);
			}

			void surf (Input IN, inout SurfaceOutput o) {
				fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Albedo = IN.color * _Tint;
				o.Alpha = c.a;
			}
		ENDCG
	
	}

	Fallback "VertexLit"
}
