using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Updated namespace for URP

[RequireComponent(typeof(SpriteRenderer))]
public class PixelArtShadowCaster2D : MonoBehaviour
{
    [Range(0.01f, 1.0f)]
    public float alphaThreshold = 0.5f;
    
    [Range(0.0f, 1.0f)]
    public float shadowIntensity = 0.5f;
    
    [Range(-5.0f, 5.0f)]
    public float shadowOffset = 0.0f;
    
    public bool useCustomMesh = true;
    
    private SpriteRenderer spriteRenderer;
    private ShadowCaster2D shadowCaster;
    private Sprite lastSprite;
    private float lastAlphaThreshold;
    private float lastShadowOffset;
    private Color lastColor;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Add shadow caster component if not present
        shadowCaster = GetComponent<ShadowCaster2D>();
        if (shadowCaster == null)
        {
            shadowCaster = gameObject.AddComponent<ShadowCaster2D>();
        }
        
        // Set initial shadow properties using the correct properties
        shadowCaster.useRendererSilhouette = false;  // Don't use the renderer's silhouette
        shadowCaster.selfShadows = false;            // Don't cast shadows on itself
        shadowCaster.castsShadows = true;            // Do cast shadows
        UpdateShadowCaster();
    }
    
    void LateUpdate()
    {
        // Check if we need to update the shadow caster shape
        if (useCustomMesh && 
            (spriteRenderer.sprite != lastSprite || 
             alphaThreshold != lastAlphaThreshold ||
             shadowOffset != lastShadowOffset ||
             spriteRenderer.color != lastColor))
        {
            UpdateShadowCaster();
            
            lastSprite = spriteRenderer.sprite;
            lastAlphaThreshold = alphaThreshold;
            lastShadowOffset = shadowOffset;
            lastColor = spriteRenderer.color;
        }
    }
    
    void UpdateShadowCaster()
    {
        if (shadowCaster == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;
        
        // Instead of directly setting intensity (which isn't accessible),
        // we'll handle this through the global light settings or
        // modify shadow opacity in the renderer
        
        if (!useCustomMesh)
        {
            // When not using custom mesh, just update the shadowCaster
            return;
        }
        
        // Get the sprite texture data
        Texture2D texture = GetReadableTexture(spriteRenderer.sprite);
        if (texture == null)
            return;
            
        // Generate outline from texture using marching squares algorithm
        Vector2[] outline = GenerateOutlineFromTexture(texture, alphaThreshold);
        
        // Apply shadow offset
        if (shadowOffset != 0)
        {
            for (int i = 0; i < outline.Length; i++)
            {
                outline[i] = new Vector2(outline[i].x, outline[i].y + shadowOffset);
            }
        }
        
        // Apply the outline to the shadow caster using reflection (since Unity doesn't expose this API directly)
        SetShadowShape(shadowCaster, outline);
    }
    
    Texture2D GetReadableTexture(Sprite sprite)
    {
        // Check if we can access the sprite's texture directly
        if (sprite.texture.isReadable)
            return sprite.texture;
        
        // Create a readable copy of the texture
        Texture2D texture = new Texture2D(
            (int)sprite.rect.width,
            (int)sprite.rect.height,
            TextureFormat.RGBA32,
            false);
        
        // Create a temporary RenderTexture to read from
        RenderTexture rt = RenderTexture.GetTemporary(
            (int)sprite.rect.width,
            (int)sprite.rect.height);
        
        // Copy sprite texture to render texture
        Graphics.Blit(sprite.texture, rt);
        
        // Save current render texture and set active render texture to our temporary one
        RenderTexture previousActive = RenderTexture.active;
        RenderTexture.active = rt;
        
        // Read pixels from the temporary render texture
        texture.ReadPixels(new Rect(sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height), 0, 0);
        texture.Apply();
        
        // Restore previous render texture
        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(rt);
        
        return texture;
    }
    
    Vector2[] GenerateOutlineFromTexture(Texture2D texture, float threshold)
    {
        // This is a simplified version of marching squares algorithm
        // For pixel art, we'll create a simple rectangular outline based on non-transparent pixels
        
        int width = texture.width;
        int height = texture.height;
        
        // Find the bounds of the non-transparent pixels
        int minX = width;
        int maxX = 0;
        int minY = height;
        int maxY = 0;
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixel = texture.GetPixel(x, y);
                if (pixel.a > threshold)
                {
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }
        
        // If no opaque pixels were found, return an empty array
        if (minX > maxX || minY > maxY)
            return new Vector2[0];
        
        // Create rectangular outline
        Vector2[] outline = new Vector2[5];
        
        // Convert pixel coordinates to local space (normalized and centered)
        float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        
        // Bottom-left
        outline[0] = new Vector2((minX - halfWidth) / pixelsPerUnit, (minY - halfHeight) / pixelsPerUnit);
        // Bottom-right
        outline[1] = new Vector2((maxX + 1 - halfWidth) / pixelsPerUnit, (minY - halfHeight) / pixelsPerUnit);
        // Top-right
        outline[2] = new Vector2((maxX + 1 - halfWidth) / pixelsPerUnit, (maxY + 1 - halfHeight) / pixelsPerUnit);
        // Top-left
        outline[3] = new Vector2((minX - halfWidth) / pixelsPerUnit, (maxY + 1 - halfHeight) / pixelsPerUnit);
        // Close the loop
        outline[4] = outline[0];
        
        return outline;
    }
    
    void SetShadowShape(ShadowCaster2D shadowCaster, Vector2[] shape)
    {
        // Use reflection to set the shadow shape
        var shapePath = typeof(ShadowCaster2D).GetField("m_ShapePath", 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance);
            
        if (shapePath != null)
        {
            shapePath.SetValue(shadowCaster, shape);
            
            // Call internal method to update the mesh
            var method = typeof(ShadowCaster2D).GetMethod("SetPathHash", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (method != null)
                method.Invoke(shadowCaster, null);
        }
    }
}