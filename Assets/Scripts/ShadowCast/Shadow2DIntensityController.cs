using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Shadow2DIntensityController : MonoBehaviour
{
    [Range(0f, 1f)]
    public float shadowIntensity = 0.5f;
    
    [Tooltip("Light to control shadow intensity")]
    public Light2D targetLight;
    
    void Start()
    {
        if (targetLight == null)
        {
            // Try to find a global light in the scene
            Light2D[] lights = FindObjectsOfType<Light2D>();
            foreach (Light2D light in lights)
            {
                if (light.lightType == Light2D.LightType.Global)
                {
                    targetLight = light;
                    break;
                }
            }
        }
        
        UpdateShadowIntensity();
    }
    
    void OnValidate()
    {
        UpdateShadowIntensity();
    }
    
    public void UpdateShadowIntensity()
    {
        if (targetLight != null)
        {
            // Update shadow intensity through the light's shadow intensity property
            targetLight.shadowIntensity = shadowIntensity;
        }
    }
}