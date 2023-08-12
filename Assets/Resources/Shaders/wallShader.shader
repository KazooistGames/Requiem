

Shader "Custom/wallShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _BumpScale("Bump Scale", Range(0,5)) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}

        _FadeStart("Fade Start", Range(-0.5,0.5)) = 0.0
        _FadeEnd("Fade End", Range(-0.5,0.5)) = 0.5

    }
    SubShader
    {

        Tags { "RenderType" = "Opaque" }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha
        CGPROGRAM

        #pragma surface surf Standard fullforwardshadows // Physically based Standard lighting model, and enable shadows on all light types
        #pragma target 3.0 // Use shader model 3.0 target, to get nicer looking lighting
        sampler2D _MainTex;
        sampler2D _BumpMap;
        half _BumpScale;
        half _FadeStart;
        half _FadeEnd;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 color = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            if (IN.uv_MainTex.y >= _FadeEnd)
            {
                color = 0;
            }
            else if (IN.uv_MainTex.y > _FadeStart) {
                float range = _FadeEnd - _FadeStart;
                float scaled = IN.uv_MainTex.y - _FadeStart;
                float ratio = scaled / range;
                float scalar = 1 - ratio;
                color.r = color.r * scalar;
                color.g = color.g * scalar;
                color.b = color.b * scalar;
                color.a = scalar;
            }
            else
            {
                color.a = 1;
            }
            o.Albedo = color.rgb;
            o.Alpha = color.a;

            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_BumpMap), _BumpScale);
        }
        ENDCG      
    }
    FallBack "Diffuse"
}
