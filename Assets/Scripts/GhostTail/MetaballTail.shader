Shader "Unlit/MetaballTail"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _k ("Constant K", Range(0.0, 0.1)) = 0.04
        _ShadowThreshold ("Shadow Threshold", Range(0, 1)) = 0.45
        _ShadowStrength ("Shadow Strength", Range(0, 1)) = 0.75
        _ShadowColor ("Shadow Color", Color) = (1, 1, 1, 1)
        _EdgeStrength ("Edge Strength", Range(0, 1)) = 0.6
        _EdgeWidth ("Edge Width", Float) = 4
        _EdgeColor ("Edge Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="TransParent" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _P0;
            float4 _P1;
            float4 _P2;
            float4 _P3;
            float4 _P4;
            float4 _P5;
            float4 _P6;
            float4 _P7;
            float4 _P8;
            float4 _P9;
            float4 _P10;
            float4 _P11;
            float4 _P12;
            float4 _P13;
            float4 _P14;
            float4 _P15;
            float4 _Color;
            float _ShadowThreshold;
            float _ShadowStrength;
            float4 _ShadowColor;
            float _EdgeStrength;
            float _EdgeWidth;
            float4 _EdgeColor;
            
            int _NumPoints;
            float _k;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            // dist to this circle
            half circle(half3 p, half3 c, half r)
            {
                return length(p-c) - r;
            }

            // dist to the shape union
            float circles(half2 p, float4 _Points[16])
            {
                float sum = 0;
                for (int i = 0; i < _NumPoints; i++)
                {
                    float d = circle(half3(p, 0), float3(_Points[i].xy, 0), _Points[i].w);
                    sum += exp(-_k * d);
                }
                float dist = -log(sum) / _k;
                return dist;
            }

            float2 circlesNormal(half2 p, float4 pts[16])
            {
                const float eps = 0.002f;
                const half2 v1 = half2(1, 1);
                const half2 v2 = half2(-1, -1);
                const half2 v3 = half2(1, -1);
                const half2 v4 = half2(-1, 1);
                return normalize(
                    v1 * circles(p + v1*eps, pts) +
                    v2 * circles(p + v2*eps, pts) +
                    v3 * circles(p + v3*eps, pts) +
                    v4 * circles(p + v4*eps, pts)
                    );
            }

            float4 mmul(float4x4 m, float4 pt)
            {
                float4 pos = float4(pt.xy, 0, 1);
                pos = mul(m, pos);
                pos.y *= -1;
                pos.xy = (pos.xy * 0.5) + 0.5;
                pos.xy *= _ScreenParams.xy;
                pos.zw = pt.zw;
                return pos;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4x4 m = UNITY_MATRIX_VP;
                // sample the texture
                float4 ScreenPoints[16] = {
                    #if 1
                    mmul(m, _P0),
                    mmul(m, _P1),
                    mmul(m, _P2),
                    mmul(m, _P3),
                    mmul(m, _P4),
                    mmul(m, _P5),
                    mmul(m, _P6),
                    mmul(m, _P7),
                    mmul(m, _P8),
                    mmul(m, _P9),
                    mmul(m, _P10),
                    mmul(m, _P11),
                    mmul(m, _P12),
                    mmul(m, _P13),
                    mmul(m, _P14),
                    mmul(m, _P15)
                    #else
                    _P0, _P1, _P2, _P3,
                    _P4, _P5, _P6, _P7,
                    _P8, _P9, _P10, _P11,
                    _P12, _P13, _P14, _P15
                    #endif
                };
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                float2 uv = i.screenPos.xy / i.screenPos.w;
                int2 xyCentered = (uv * _ScreenParams.xy);
                xyCentered = xyCentered / 4 * 4;
                float ccl = circles(xyCentered, ScreenPoints);
                if (ccl > 0) clip(-1);
                if (ccl > - _EdgeWidth)
                {
                    col.rgb = _EdgeColor.rgb;
                    return col;
                }
                
                float2 norm = circlesNormal(xyCentered, ScreenPoints);

                float z = pow(saturate(-ccl * 0.04), 0.5);
                float3 norm3 = normalize(float3(norm, z * 3));

                if (dot(norm3, normalize(float3(0.2, -1, 0))) > _ShadowThreshold)
                {
                    col.rgb = _ShadowColor.rgb;
                } 
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col;
            }
            ENDCG
        }
    }
}
