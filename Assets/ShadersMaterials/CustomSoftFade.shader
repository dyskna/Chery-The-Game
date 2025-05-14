Shader "Custom/CustomSoftFade"
{
    Properties
    {
        _MainTex("Sprite Texture", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _LightPos("Light Position", Vector) = (0, 0, 0, 0)
        _LightColor("Light Color", Color) = (1,1,1,1)
        _LightIntensity("Light Intensity", Range(0, 2)) = 1.0
        _AmbientLight("Ambient Light", Range(0, 1)) = 0.5
        _FadeDistance("Fade Distance", Float) = 3.0
        [Toggle] _DebugMode("Debug Mode", Float) = 0
        _LightAngle("Light Angle (0-360°)", Range(0, 360)) = 180
        _LightAngleRange("Light Angle Range (0-180°)", Range(0, 180)) = 90
        _TransitionSmoothness("Transition Smoothness", Range(0.1, 45)) = 15.0
	_GlobalLightColor("Global Light Color", Color) = (1, 1, 1, 1)

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
            float4 _LightColor;
            float _LightIntensity;
            float _AmbientLight;
            float _FadeDistance;
            float _DebugMode;
            float _LightAngle;
            float _LightAngleRange;
            float _TransitionSmoothness;
	    float4 _GlobalLightColor;
            
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
            
            float SmoothTransition(float angle, float threshold, float smoothness)
            {
                return smoothstep(threshold, threshold + smoothness, angle);
            }
            
            fixed4 frag(v2f i) : SV_Target
            {
                // Base texture color and tint
                fixed4 col = tex2D(_MainTex, i.uv) * _Color * i.color;
                
                // Object position in world space (pivot point)
                float3 treePos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;
                
                // Vector from the light (player) to the object
                float2 lightToTree = float2(treePos.x - _LightPos.x, treePos.y - _LightPos.y);
                
                // Calculate the angle between the light and the object
                float angleRad = atan2(lightToTree.y, lightToTree.x);
                float angleDeg = GetAngleTo360(angleRad);
                float angleDiff = GetAngleDifference(angleDeg, _LightAngle);
                
                // Check if the light is in front of the object (within the angular range)
                float halfRange = _LightAngleRange * 0.5;
                float inFrontFactor = 1.0 - SmoothTransition(angleDiff, halfRange, _TransitionSmoothness);
                
                // Distance factor – fades light based on how far the player is
                float distance = length(lightToTree);
                float distanceFactor = 1.0 - saturate(distance / _FadeDistance);
                
                // Final multiplier for directional lighting: only when player is in front and close
                float lightMultiplier = inFrontFactor * distanceFactor;
                
                // Base ambient lighting – always present regardless of player position
                float3 ambient = col.rgb * _AmbientLight * _GlobalLightColor.rgb;

                
                // Additional directional light from the player (only from the front)
                float3 directionalLight = col.rgb * _LightColor.rgb * _LightIntensity * lightMultiplier;
                
		// Debug visualization: output factors instead of final color
                if (_DebugMode > 0.5) 
                {
                    return fixed4(inFrontFactor, distanceFactor, lightMultiplier, col.a);
                }
                
                // Combine ambient and directional lighting
    		// Ambient is always applied
    		// Directional is added only if the player is in front and close
                col.rgb = ambient + directionalLight;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
}