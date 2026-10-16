Shader "Duskline/AwakeningGlow" {
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Off ZWrite Off Blend SrcAlpha One
 Pass { CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct input {float4 vertex:POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
 struct output {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
 output vert(input v){output o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
 fixed4 frag(output i):SV_Target {fixed4 c=i.color;c.a*=pow(saturate(1-abs(i.uv.y*2-1)),1.5);return c;}
 ENDCG }
 }
}
