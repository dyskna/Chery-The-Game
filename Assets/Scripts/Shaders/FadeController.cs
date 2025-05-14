using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FadeController : MonoBehaviour
{
    public Transform playerTransform;
    private Material treeMaterial;
    
    [Header("Light Settings")]
    public float lightIntensity = 0.5f;
    public float ambientLight = 0.5f;
    public float fadeDistance = 0.3f;
    
    [Header("Front Detection Settings")]
    public bool debugMode = false;
    
    [Tooltip("The angle at which the tree will be fully lit (in degrees)")]
    [Range(0, 360)]
    public float lightAngle = 90f;
    
    [Tooltip("The angular range where the tree will be lit (degrees)")]
    [Range(0, 180)]
    public float lightAngleRange = 180f;
    
    [Header("Global Light")]
    [Tooltip("The reference to world light script - leave empty to find automatically")]
    public WorldTime.WorldLight worldLightScript;
    private Light2D globalLight;
    
    [Tooltip("If true, uses global light color and intensity")]
    public bool useGlobalLight = true;
    
    // Store original light settings
    private Color originalLightColor = Color.white;
    private float originalLightIntensity = 1.0f;
    
    // For debugging
    [SerializeField, ReadOnly]
    private Color currentLightColor = Color.white;
    [SerializeField, ReadOnly]
    private float currentLightIntensity = 1.0f;
    
    // Flag to track if player is close enough to affect lighting
    [SerializeField, ReadOnly]
    private bool playerInRange = false;
    
    // Default lighting values - will be used when player is out of range
    private Color defaultLightColor = Color.white;
    private float defaultLightIntensity = 0f;  // No extra lighting when player is out of range
    private float defaultAmbientLight = 0.5f;  // Default ambient light when player is out of range
    private bool materialInitialized = false;
    
    // Higher base ambient light to match the global scene appearance
    [Range(0, 1)]
    public float baseAmbientLight = 0.7f;
    
    void Start()
    {
        // Find player if not assigned
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }
        
        // Get the SpriteRenderer component
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on this GameObject");
            return;
        }
        
        // Create a unique material instance for this object
        treeMaterial = new Material(spriteRenderer.sharedMaterial);
        spriteRenderer.material = treeMaterial;
        Debug.Log($"Created unique material instance for {gameObject.name}");
        
        // Find world light script if not assigned
        FindAndSetupWorldLight();
        
        // Store the default ambient light value from the inspector
        defaultAmbientLight = ambientLight;
        
        // Apply initial settings
        InitializeMaterial();
    }
    
    private void FindAndSetupWorldLight()
    {
        if (worldLightScript == null)
        {
            worldLightScript = FindObjectOfType<WorldTime.WorldLight>();
            
            if (worldLightScript == null)
            {
                Debug.LogWarning("No WorldLight script found. Global lighting will not work.");
                useGlobalLight = false;
                return;
            }
        }
        
        // Get the Light2D component from the WorldLight script
        globalLight = worldLightScript.GetComponent<Light2D>();
        
        if (globalLight == null)
        {
            Debug.LogWarning("Light2D component not found on WorldLight. Global lighting will not work.");
            useGlobalLight = false;
            return;
        }
        
        // Store initial light values
        originalLightColor = globalLight.color;
        originalLightIntensity = globalLight.intensity;
        
        // Set default light color from global light
        defaultLightColor = originalLightColor;
        
        Debug.Log($"Successfully connected to world light: {worldLightScript.name}");
        Debug.Log($"Initial light color: {originalLightColor}, intensity: {originalLightIntensity}");
    }
    
    // Initialize material with default values (called once at start)
    void InitializeMaterial()
    {
        if (treeMaterial == null) return;
        
        // Set up default parameters that won't change
        treeMaterial.SetFloat("_FadeDistance", fadeDistance);
        treeMaterial.SetFloat("_DebugMode", debugMode ? 1f : 0f);
        treeMaterial.SetFloat("_LightAngleRange", lightAngleRange);
        treeMaterial.SetFloat("_TransitionSmoothness", 15.0f); // Default value
        
        // Set up default lighting parameters
        treeMaterial.SetVector("_LightPos", transform.position); // Default position (own position)
        treeMaterial.SetFloat("_LightAngle", lightAngle);
        treeMaterial.SetColor("_LightColor", defaultLightColor);
        treeMaterial.SetFloat("_LightIntensity", defaultLightIntensity);
        
        // Start with a higher ambient light to match the global scene
        treeMaterial.SetFloat("_AmbientLight", baseAmbientLight);
        
        materialInitialized = true;
    }
    
    void Update()
    {
        if (treeMaterial == null || !materialInitialized) return;
        
        // Cache the current values from global light for debugging
        if (useGlobalLight && globalLight != null)
        {
            currentLightColor = globalLight.color;
            currentLightIntensity = globalLight.intensity;
            
            // Update default light color from global light
            defaultLightColor = currentLightColor;
        }
        
        // Always update the global ambient lighting regardless of player position
        UpdateGlobalLighting();
        
        // Check if player is close enough to affect lighting
        playerInRange = IsPlayerInRange();
        
        // Only update directional lighting if player is in range
        if (playerInRange)
        {
            UpdatePlayerDirectionalLighting();
        }
    }
    
    // Check if player is close enough to affect lighting
    bool IsPlayerInRange()
    {
        if (playerTransform == null) return false;
        
        float distance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(playerTransform.position.x, playerTransform.position.y)
        );
        
        return distance <= fadeDistance;
    }
    
    // Update global lighting (always applied regardless of player position)
    void UpdateGlobalLighting()
    {
        if (useGlobalLight && globalLight != null)
        {
            // Calculate ambient light based on global light intensity
            // Apply higher base value to match the scene's global lighting
            float adjustedAmbient = baseAmbientLight * Mathf.Max(0.5f, currentLightIntensity);
            treeMaterial.SetFloat("_AmbientLight", adjustedAmbient);
            
            // Apply global light color to material
            treeMaterial.SetColor("_LightColor", currentLightColor);
        }
        else
        {
            // Use default values if not using global light
            treeMaterial.SetFloat("_AmbientLight", baseAmbientLight);
            treeMaterial.SetColor("_LightColor", Color.white);
        }
    }
    
    // Update directional lighting when player is in range
    void UpdatePlayerDirectionalLighting()
    {
        // Use position from player for lighting angle
        treeMaterial.SetVector("_LightPos", playerTransform.position);
        
        if (useGlobalLight && globalLight != null)
        {
            // Scale the directional light intensity based on global light
            float scaledIntensity = lightIntensity * currentLightIntensity;
            treeMaterial.SetFloat("_LightIntensity", scaledIntensity);
        }
        else
        {
            // Use default values if not using global light
            treeMaterial.SetFloat("_LightIntensity", lightIntensity);
        }
    }
    
    // Editor-only attribute for debugging properties
    public class ReadOnlyAttribute : PropertyAttribute {}
}