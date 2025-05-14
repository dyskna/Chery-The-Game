using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer))]
public class FadeController : MonoBehaviour
{
    public Transform playerTransform;

    [Header("Light Settings")]
    public float lightIntensity = 0.5f;
    public float ambientLight = 0.5f;
    public float fadeDistance = 0.3f;

    [Header("Front Detection")]
    public float lightAngle = 90f;
    public float lightAngleRange = 180f;

    [Header("Global Light Settings")]
    public bool useGlobalLight = true;
    public WorldTime.WorldLight worldLightScript;
    public float baseAmbientLight = 0.7f;

    [Header("Debug")]
    public bool debugMode = false;

    private Light2D globalLight;
    private Material treeMaterial;
    private bool materialInitialized = false;

    void Start()
    {
        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                playerTransform = player.transform;
        }

        var spriteRenderer = GetComponent<SpriteRenderer>();
        treeMaterial = new Material(spriteRenderer.sharedMaterial);
        spriteRenderer.material = treeMaterial;

        FindGlobalLight();
        InitializeMaterial();
    }

    void FindGlobalLight()
    {
        if (!useGlobalLight) return;

        if (worldLightScript == null)
            worldLightScript = FindObjectOfType<WorldTime.WorldLight>();

        if (worldLightScript != null)
            globalLight = worldLightScript.GetComponent<Light2D>();
        else
            useGlobalLight = false;
    }

    void InitializeMaterial()
    {
        if (treeMaterial == null) return;

        treeMaterial.SetFloat("_FadeDistance", fadeDistance);
        treeMaterial.SetFloat("_DebugMode", debugMode ? 1f : 0f);
        treeMaterial.SetFloat("_LightAngle", lightAngle);
        treeMaterial.SetFloat("_LightAngleRange", lightAngleRange);
        treeMaterial.SetFloat("_TransitionSmoothness", 15f);
        treeMaterial.SetFloat("_LightIntensity", 0f);
        treeMaterial.SetFloat("_AmbientLight", baseAmbientLight);
        treeMaterial.SetColor("_LightColor", Color.white);
        treeMaterial.SetColor("_GlobalLightColor", Color.white);

        materialInitialized = true;
    }

    void Update()
    {
        if (!materialInitialized) return;

        UpdateGlobalLighting();

        if (IsPlayerInRange())
            UpdatePlayerLighting();
    }

    bool IsPlayerInRange()
    {
        if (playerTransform == null) return false;
        return Vector2.Distance(transform.position, playerTransform.position) <= fadeDistance;
    }

    void UpdateGlobalLighting()
    {
        Color globalColor = useGlobalLight && globalLight != null ? globalLight.color : Color.white;
        float globalIntensity = useGlobalLight && globalLight != null ? globalLight.intensity : 1f;

        treeMaterial.SetColor("_GlobalLightColor", globalColor);
        treeMaterial.SetColor("_LightColor", globalColor);

        float adjustedAmbient = baseAmbientLight * Mathf.Max(0.5f, globalIntensity);
        treeMaterial.SetFloat("_AmbientLight", adjustedAmbient);
    }

    void UpdatePlayerLighting()
    {
        treeMaterial.SetVector("_LightPos", playerTransform.position);

        float scaledIntensity = lightIntensity;
        if (useGlobalLight && globalLight != null)
            scaledIntensity *= globalLight.intensity;

        treeMaterial.SetFloat("_LightIntensity", scaledIntensity);
    }
}
