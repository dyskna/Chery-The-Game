using UnityEngine;

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
    
    void Start()
    {
        // GameObject playerController = FindObjectsOfType<Player>();
        // if (playerController != null)
        // {
        //     playerTransform = playerController.transform;
        // }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        // Get the material from the sprite renderer
        treeMaterial = GetComponent<SpriteRenderer>().material;
        
        // Apply initial settings
        UpdateMaterialSettings();
    }
    
    void Update()
    {
        // Update the light position to match the player position
        if (treeMaterial != null && playerTransform != null)
        {
            treeMaterial.SetVector("_LightPos", playerTransform.position);
            UpdateMaterialSettings();
        }
    }
    
    void UpdateMaterialSettings()
    {
        if (treeMaterial != null)
        {
            treeMaterial.SetFloat("_LightIntensity", lightIntensity);
            treeMaterial.SetFloat("_AmbientLight", ambientLight);
            treeMaterial.SetFloat("_FadeDistance", fadeDistance);
            treeMaterial.SetFloat("_DebugMode", debugMode ? 1f : 0f);
            treeMaterial.SetFloat("_LightAngle", lightAngle);
            treeMaterial.SetFloat("_LightAngleRange", lightAngleRange);
        }
    }
    
    // Helper method to visualize the light direction in the scene view
    // void OnDrawGizmos()
    // {
    //     if (playerTransform != null)
    //     {
    //         Vector3 treePos = transform.position;
    //         Vector3 playerPos = playerTransform.position;
            
    //         // Draw line from player to tree
    //         Gizmos.color = Color.yellow;
    //         Gizmos.DrawLine(playerPos, treePos);
            
    //         // Draw "front" direction
    //         float angleRad = lightAngle * Mathf.Deg2Rad;
    //         Vector3 lightDir = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0) * 1.5f;
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawLine(treePos, treePos + lightDir);
            
    //         // Draw light angle range
    //         float halfRange = lightAngleRange * 0.5f * Mathf.Deg2Rad;
    //         Vector3 lightDirMin = new Vector3(
    //             Mathf.Cos(angleRad - halfRange), 
    //             Mathf.Sin(angleRad - halfRange), 
    //             0
    //         ) * 1.2f;
            
    //         Vector3 lightDirMax = new Vector3(
    //             Mathf.Cos(angleRad + halfRange), 
    //             Mathf.Sin(angleRad + halfRange), 
    //             0
    //         ) * 1.2f;
            
    //         Gizmos.color = Color.cyan;
    //         Gizmos.DrawLine(treePos, treePos + lightDirMin);
    //         Gizmos.DrawLine(treePos, treePos + lightDirMax);
    //     }
    // }
}
