Shader "Custom/TreeFrontLighting"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _LightPos("Light Position", Vector) = (0, 0, 0, 0)
        _LightIntensity("Light Intensity", Range(0, 2)) = 1.0
        _AmbientLight("Ambient Light", Range(0, 1)) = 0.3
        _FadeDistance("Fade Distance", Float) = 3.0
        [Toggle] _DebugMode("Debug Mode", Float) = 0
        _LightAngle("Light Angle (0-360°)", Range(0, 360)) = 180
        _LightAngleRange("Light Angle Range (0-180°)", Range(0, 180)) = 90
        _TransitionSmoothness("Transition Smoothness", Range(0.1, 45)) = 15.0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _LightPos;
            float _LightIntensity;
            float _AmbientLight;
            float _FadeDistance;
            float _DebugMode;
            float _LightAngle;
            float _LightAngleRange;
            float _TransitionSmoothness;
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float4 color : COLOR;
                float4 objPos : TEXCOORD2;
            };
            
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.objPos = v.vertex;
                o.color = v.color;
                return o;
            }
            
            float GetAngleTo360(float rad)
            {
                float deg = degrees(rad);
                return (deg < 0) ? (deg + 360.0) : deg;
            }
            
            float GetAngleDifference(float a, float b)
            {
                float diff = abs(a - b);
                return (diff > 180.0) ? (360.0 - diff) : diff;
            }
            
            // Smooth transition function
            float SmoothTransition(float angle, float threshold, float smoothness)
            {
                // Calculate smooth transition from 1 to 0
                return smoothstep(threshold, threshold + smoothness, angle);
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                
                // Get tree origin (bottom center of the sprite)
                float3 treePos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Vector from light to tree
                float2 lightToTree = float2(treePos.x - _LightPos.x, treePos.y - _LightPos.y);
                
                // Calculate angle from light to tree (in degrees, 0-360 range)
                float angleRad = atan2(lightToTree.y, lightToTree.x);
                float angleDeg = GetAngleTo360(angleRad);
                
                // Get angle difference in the shortest direction around the circle
                float angleDiff = GetAngleDifference(angleDeg, _LightAngle);
                
                // Calculate smooth transition factor based on angle difference
                float halfRange = _LightAngleRange * 0.5;
                float transitionFactor = 1.0 - SmoothTransition(angleDiff, halfRange, _TransitionSmoothness);
                
                // Calculate distance and falloff
                float distance = length(lightToTree);
                float distanceFactor = 1.0 - saturate(distance / _FadeDistance);
                
                // Calculate final illumination with smooth transition
                float illumination = transitionFactor * distanceFactor * _LightIntensity + _AmbientLight;
                illumination = saturate(illumination);
                
                // Debug visualization if enabled
                if (_DebugMode > 0.5) 
                {
                    // Show gradient from red (front) to blue (back) based on transition factor
                    return fixed4(transitionFactor, 0, 1.0 - transitionFactor, col.a);
                }
                
                // Apply illumination to color
                col.rgb *= illumination;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}