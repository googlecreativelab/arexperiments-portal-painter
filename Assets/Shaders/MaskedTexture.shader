/*
Copyright 2017 Google Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

   Shader "Transparent/UV2Mask" {
        Properties {
            _MainTex ("Base (RGB)", 2D) = "white" {}
            _MaskTex ("Mask (RGB)", 2D) = "white" {}
            _AlphaThreshold ("Alpha Threshold", Range(0,1)) = .5
		    _Distance("Distance", Range(0, 1)) = .5
		    _PortalEdgeColor ("Edge Color Tint", Color) = (0,0,0,.5)

		    _Offset("Sin/Cos Offset", Range(0, 30)) = 20
		    _Amplitude("Sin/Cos Amplitude", Range(0, .1)) = .02
        }
     
        SubShader {
            Tags {"Queue"="Transparent" }
            LOD 200
 
            CGPROGRAM
            #pragma surface surf Lambert alpha
			#pragma target 3.0
 
            sampler2D _MainTex;
            sampler2D _MaskTex;
			half _Distance;
			half _AlphaThreshold;

		    fixed4 _PortalEdgeColor;

		    half _Offset;
		    half _Amplitude;

            struct Input {
                float2 uv_MainTex : TEXCOORD0;
                float2 uv2_MaskTex : TEXCOORD1;
                half3 viewDir;
            };
            void surf (Input IN, inout SurfaceOutput surface) {
                half4 mask = tex2D (_MaskTex, IN.uv2_MaskTex.xy + float2(
                				sin(_Time.y + IN.uv2_MaskTex.y * _Offset) * _Amplitude, 
                				cos(_Time.x + IN.uv2_MaskTex.x * _Offset) * _Amplitude));

				half distance = (1 - _Distance);
				half coeff = cos(abs(normalize(IN.viewDir)));
				half3 offset = (IN.viewDir) * distance / 2 * (2 - coeff);
				half2 uv = half2(IN.uv_MainTex.x * _Distance + distance / 2 - offset.x, IN.uv_MainTex.y * _Distance + distance / 2 - offset.y);
                surface.Albedo = tex2D (_MainTex, uv).rgb;

                if (mask.r < _AlphaThreshold) {
	                surface.Alpha = 1-mask.r;
                } else if (mask.r >= _AlphaThreshold && mask.r < 1) {
                	surface.Albedo = _PortalEdgeColor;
                	surface.Alpha = 1-mask.r;
                } else {
                	surface.Alpha = 0;
                }
            }
            ENDCG
        }
    }