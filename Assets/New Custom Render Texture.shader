Shader "UI/Blur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BlurSize ("Blur Size", Range(0, 30)) = 10
        _Brightness ("Brightness", Range(0, 1)) = 0.75
        _Saturation ("Saturation", Range(0, 1)) = 0.8
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        LOD 100
        
        GrabPass { "_GrabTexture" }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 grabPos : TEXCOORD0;
            };
            
            sampler2D _GrabTexture;
            float4 _GrabTexture_TexelSize;
            float _BlurSize;
            float _Brightness;
            float _Saturation;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                return o;
            }
            
            // Apply Gaussian blur to a given texture coordinate
            half4 blur(sampler2D image, float2 uv, float2 resolution, float radius) 
            {
                half4 color = half4(0, 0, 0, 0);
                float2 texelSize = 1.0 / resolution;
                
                // Gaussian kernel weights (approximated)
                float kernel[5];
                kernel[0] = 0.227027;
                kernel[1] = 0.1945946;
                kernel[2] = 0.1216216;
                kernel[3] = 0.054054;
                kernel[4] = 0.016216;
                
                // Horizontal blur
                for (int i = -4; i <= 4; i++) 
                {
                    float2 offset = float2(texelSize.x * i * radius, 0);
                    color += tex2D(image, uv + offset) * kernel[abs(i)];
                }
                
                // Vertical blur
                half4 blurColor = half4(0, 0, 0, 0);
                for (int j = -4; j <= 4; j++) 
                {
                    float2 offset = float2(0, texelSize.y * j * radius);
                    blurColor += tex2D(image, uv + offset) * kernel[abs(j)];
                }
                
                // Combine horizontal and vertical blur
                color = (color + blurColor) * 0.5;
                
                return color;
            }
            
            // Adjust brightness and saturation
            half4 adjustColor(half4 color, float brightness, float saturation) 
            {
                // Convert to grayscale
                float gray = dot(color.rgb, float3(0.299, 0.587, 0.114));
                
                // Adjust saturation
                color.rgb = lerp(float3(gray, gray, gray), color.rgb, saturation);
                
                // Adjust brightness
                color.rgb *= brightness;
                
                return color;
            }

            half4 frag (v2f i) : SV_Target
            {
                float2 grabTexCoord = i.grabPos.xy / i.grabPos.w;
                half4 blurredCol = blur(_GrabTexture, grabTexCoord, _GrabTexture_TexelSize.zw, _BlurSize);
                
                // Apply brightness and saturation adjustments
                blurredCol = adjustColor(blurredCol, _Brightness, _Saturation);
                
                // Add slight darkening
                blurredCol.rgb *= 0.95;
                
                return blurredCol;
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}