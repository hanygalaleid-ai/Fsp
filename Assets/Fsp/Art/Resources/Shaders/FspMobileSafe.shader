Shader "Fsp/MobileSafeLit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float3 worldNormal : TEXCOORD1; float3 viewDir : TEXCOORD2; UNITY_FOG_COORDS(3) float4 vertex : SV_POSITION; };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.viewDir = WorldSpaceViewDir(v.vertex);
                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 normal = normalize(i.worldNormal);
                fixed diffuse = saturate(dot(normal, normalize(_WorldSpaceLightPos0.xyz))) * 0.5 + 0.5;
                fixed3 ambient = ShadeSH9(float4(normal, 1.0));
                fixed rim = pow(1.0 - saturate(dot(normal, normalize(i.viewDir))), 3.0);
                fixed3 lighting = max(ambient, fixed3(0.58, 0.60, 0.64)) + _LightColor0.rgb * diffuse * 0.58 + rim * fixed3(0.12, 0.14, 0.17);
                fixed4 baseColor = tex2D(_MainTex, i.uv) * _Color;
                fixed4 result = fixed4(baseColor.rgb * lighting, baseColor.a);
                UNITY_APPLY_FOG(i.fogCoord, result);
                return result;
            }
            ENDCG
        }
    }
    Fallback Off
}
