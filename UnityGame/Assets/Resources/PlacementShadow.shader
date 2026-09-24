Shader "BaggingOrder/PlacementShadow" {
 Properties { _MainTex ("Silhouette",2D)="white" {} _Color ("Tint",Color)=(.2,.3,.15,.6) }
 SubShader { Tags { "Queue"="Overlay" "RenderType"="Transparent" }
 Blend SrcAlpha OneMinusSrcAlpha
 ZWrite Off
 ZTest Always
 Cull Off
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 sampler2D _MainTex;float4 _MainTex_TexelSize;fixed4 _Color;
 struct input {float4 vertex:POSITION;float2 uv:TEXCOORD0;};
 struct output {float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
 output vert(input v){output o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
 fixed4 frag(output i):SV_Target {
 float alpha=0,total=0;
 for(int x=-2;x<=2;x++)for(int y=-2;y<=2;y++){
 float weight=(3-abs(x))*(3-abs(y));
 alpha+=tex2D(_MainTex,i.uv+float2(x,y)*_MainTex_TexelSize.xy*1.8).a*weight;total+=weight;}
 return fixed4(_Color.rgb,_Color.a*alpha/total);
 }
 ENDCG
 } }
}
