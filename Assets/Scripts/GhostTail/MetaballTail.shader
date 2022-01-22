Shader "Unlit/MetaballTail"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _P1 ("P1", Vector) = (0, 0, 0, 0)
        _P2 ("P2", Vector) = (0, 0, 0, 0)
        _P3 ("P3", Vector) = (0, 0, 0, 0)
        _P4 ("P4", Vector) = (0, 0, 0, 0)
        _k ("Constant K", Range(0.0, 1)) = 0.4
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
            float _ShadowThreshold;
            
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
            float circles(half2 p)
            {
                float sum = 0;
                float4 _Points[16] = {
                    _P0, _P1, _P2, _P3,
                    _P4, _P5, _P6, _P7,
                    _P8, _P9, _P10, _P11,
                    _P12, _P13, _P14, _P15,
                };
                for (int i = 0; i < _NumPoints; i++)
                {
                    float d = circle(half3(p, 0), float3(_Points[i].xy, 0), _Points[i].w);
                    sum += exp(-_k * d);
                }
                float dist = -log(sum) / _k;
                return dist;
            }

            float2 circlesNormal(half2 p)
            {
                const float eps = 0.002f;
                const half2 v1 = half2(1, 1);
                const half2 v2 = half2(-1, -1);
                const half2 v3 = half2(1, -1);
                const half2 v4 = half2(-1, 1);
                return normalize(
                    v1 * circles(p + v1*eps) +
                    v2 * circles(p + v2*eps) +
                    v3 * circles(p + v3*eps) +
                    v4 * circles(p + v4*eps)
                    );
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                float2 uv = i.screenPos.xy / i.screenPos.w;
                int2 xyCentered = (uv * _ScreenParams.xy);
                xyCentered = xyCentered / 4 * 4;
                float ccl = circles(xyCentered);
                if (ccl > 0) clip(-1);
                
                float2 norm = circlesNormal(xyCentered);

                float z = pow(saturate(-ccl * 0.04), 0.5);
                float3 norm3 = normalize(float3(norm, z * 3));

                if (dot(norm3, normalize(float3(0.2, -1, 0))) > _ShadowThreshold)
                {
                    col.rgb *= 0.5;
                }
                
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
