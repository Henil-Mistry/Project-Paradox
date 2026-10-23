Shader "Paradox/Illustrated Background"
{
    Properties { _MainTex ("Illustration", 2D) = "white" {} }
    SubShader
    {
        Pass
        {
            ZWrite Off ZTest Always Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _ViewAspect;
            struct v2f { float4 position : SV_POSITION; float2 uv : TEXCOORD0; };
            v2f vert(appdata_base v) { v2f o; o.position=float4(v.vertex.xy,0,1); o.uv=v.texcoord.xy; return o; }
            fixed4 frag(v2f i) : SV_Target
            {
                // Proportional fill: UVs stay within the illustration, avoiding clamped edge bands.
                float imageAspect=_MainTex_TexelSize.z/_MainTex_TexelSize.w;
                float viewAspect=_ViewAspect;
                float2 scale=float2(min(1,viewAspect/imageAspect),min(1,imageAspect/viewAspect));
                return tex2D(_MainTex,(i.uv-.5)*scale+.5);
            }
            ENDCG
        }
    }
}
