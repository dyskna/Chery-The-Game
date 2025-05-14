using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // Updated namespace for URP
using System.Collections.Generic;
using System.Reflection;

[RequireComponent(typeof(SpriteRenderer))]
public class AdvancedPixelArtShadowCaster2D : MonoBehaviour
{
    [Range(0.01f, 1.0f)]
    public float alphaThreshold = 0.5f;
    
    [Range(0.0f, 1.0f)]
    public float shadowIntensity = 0.5f;
    
    [Range(-5.0f, 5.0f)]
    public float shadowOffset = 0.0f;
    
    [Range(1, 10)]
    public int simplificationLevel = 3;
    
    public enum OutlineMethod
    {
        SimpleRectangle,
        AccuratePolygon
    }
    
    public OutlineMethod outlineMethod = OutlineMethod.AccuratePolygon;
    
    private SpriteRenderer spriteRenderer;
    private ShadowCaster2D shadowCaster;
    private Sprite lastSprite;
    private float lastAlphaThreshold;
    private float lastShadowOffset;
    private OutlineMethod lastOutlineMethod;
    private int lastSimplificationLevel;
    
    // Cached reflection fields and methods
    private FieldInfo shapePathField;
    private MethodInfo setPathHashMethod;
    private MethodInfo tryUpdateShapeParametersMethod;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Add shadow caster component if not present
        shadowCaster = GetComponent<ShadowCaster2D>();
        if (shadowCaster == null)
        {
            shadowCaster = gameObject.AddComponent<ShadowCaster2D>();
        }
        
        // Cache reflection fields and methods
        CacheReflectionMembers();
        
        // Set initial shadow properties using the correct properties
        shadowCaster.useRendererSilhouette = false;  // Don't use the renderer's silhouette
        shadowCaster.selfShadows = false;            // Don't cast shadows on itself
        shadowCaster.castsShadows = true;            // Do cast shadows
        UpdateShadowCaster();
    }
    
    void CacheReflectionMembers()
    {
        // Try different possible field names for Unity 6
        string[] possibleShapePathFieldNames = { 
            "m_ShapePath",              // Older Unity versions
            "m_Shape.m_Paths",          // Potential nested structure
            "m_Shape.paths",            // Another potential structure
            "m_ShapePathCache",         // Potential Unity 6 name
            "m_Path"                    // Another potential name
        };
        
        foreach (string fieldName in possibleShapePathFieldNames)
        {
            shapePathField = typeof(ShadowCaster2D).GetField(fieldName, 
                BindingFlags.NonPublic | 
                BindingFlags.Instance);
                
            if (shapePathField != null)
                break;
        }
        
        // Try to find the methods needed
        setPathHashMethod = typeof(ShadowCaster2D).GetMethod("SetPathHash", 
            BindingFlags.NonPublic | 
            BindingFlags.Instance);
        
        // Try different method names that might be used in Unity 6
        string[] possibleUpdateMethodNames = {
            "TryUpdateShapeParameters",
            "UpdateShape",
            "UpdateShadowShape",
            "OnShapeUpdated",
            "UpdateMesh"
        };
        
        foreach (string methodName in possibleUpdateMethodNames)
        {
            tryUpdateShapeParametersMethod = typeof(ShadowCaster2D).GetMethod(methodName, 
                BindingFlags.NonPublic | 
                BindingFlags.Instance);
                
            if (tryUpdateShapeParametersMethod != null)
                break;
        }
        
        // Log warning if we couldn't find necessary reflection members
        if (shapePathField == null)
        {
            Debug.LogWarning("AdvancedPixelArtShadowCaster2D: Could not find shape path field via reflection. " +
                "This component may not work correctly with Unity 6.");
            
            // Try to list available fields for debugging
            FieldInfo[] fields = typeof(ShadowCaster2D).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            string availableFields = "Available fields: ";
            foreach (var field in fields)
            {
                availableFields += field.Name + ", ";
            }
            Debug.Log(availableFields);
            
            // Also log methods for debugging
            MethodInfo[] methods = typeof(ShadowCaster2D).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance);
            string availableMethods = "Available methods: ";
            foreach (var method in methods)
            {
                availableMethods += method.Name + ", ";
            }
            Debug.Log(availableMethods);
        }
    }
    
    void LateUpdate()
    {
        // Check if we need to update the shadow caster shape
        if (spriteRenderer.sprite != lastSprite || 
            alphaThreshold != lastAlphaThreshold ||
            shadowOffset != lastShadowOffset ||
            outlineMethod != lastOutlineMethod ||
            simplificationLevel != lastSimplificationLevel)
        {
            UpdateShadowCaster();
            
            lastSprite = spriteRenderer.sprite;
            lastAlphaThreshold = alphaThreshold;
            lastShadowOffset = shadowOffset;
            lastOutlineMethod = outlineMethod;
            lastSimplificationLevel = simplificationLevel;
        }
    }
    
    void UpdateShadowCaster()
    {
        if (shadowCaster == null || spriteRenderer == null || spriteRenderer.sprite == null)
            return;
        
        // Instead of directly setting intensity (which isn't accessible),
        // we'll use the global light intensity settings of URP
        // The shadowIntensity value is kept for compatibility and potential future use
        
        // Get the sprite texture data
        Texture2D texture = GetReadableTexture(spriteRenderer.sprite);
        if (texture == null)
            return;
            
        // Generate outline from texture based on chosen method
        Vector2[] outline;
        
        if (outlineMethod == OutlineMethod.SimpleRectangle)
        {
            outline = GenerateRectangularOutline(texture, alphaThreshold);
        }
        else
        {
            outline = GeneratePolygonOutline(texture, alphaThreshold);
        }
        
        // Apply shadow offset
        if (shadowOffset != 0)
        {
            for (int i = 0; i < outline.Length; i++)
            {
                outline[i] = new Vector2(outline[i].x, outline[i].y + shadowOffset);
            }
        }
        
        // Apply the outline to the shadow caster using reflection
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
        
        // Read pixels from the render texture
        texture.ReadPixels(new Rect(0, 0, sprite.rect.width, sprite.rect.height), 0, 0);
        texture.Apply();
        
        // Restore previous render texture
        RenderTexture.active = previousActive;
        RenderTexture.ReleaseTemporary(rt);
        
        return texture;
    }
    
    Vector2[] GenerateRectangularOutline(Texture2D texture, float threshold)
    {
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
    
    Vector2[] GeneratePolygonOutline(Texture2D texture, float threshold)
    {
        int width = texture.width;
        int height = texture.height;
        
        // Create a binary image based on alpha values
        bool[,] binaryImage = new bool[width, height];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                binaryImage[x, y] = texture.GetPixel(x, y).a > threshold;
            }
        }
        
        // Find the contour using marching squares
        List<Vector2> contour = MarchingSquares(binaryImage, width, height);
        
        // Simplify the contour with Douglas-Peucker algorithm
        contour = SimplifyContour(contour, 1.0f / simplificationLevel);
        
        // If no contour was found, return an empty array
        if (contour.Count == 0)
            return new Vector2[0];
        
        // Convert to local space coordinates
        float pixelsPerUnit = spriteRenderer.sprite.pixelsPerUnit;
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        
        for (int i = 0; i < contour.Count; i++)
        {
            contour[i] = new Vector2(
                (contour[i].x - halfWidth) / pixelsPerUnit,
                (contour[i].y - halfHeight) / pixelsPerUnit
            );
        }
        
        // Ensure the contour is closed
        if (contour[0] != contour[contour.Count - 1])
        {
            contour.Add(contour[0]);
        }
        
        return contour.ToArray();
    }
    
    List<Vector2> MarchingSquares(bool[,] binaryImage, int width, int height)
    {
        List<Vector2> contour = new List<Vector2>();
        bool[,] visited = new bool[width + 1, height + 1];
        
        // Find the first point on the contour
        bool foundStart = false;
        int startX = 0, startY = 0;
        
        for (int y = 0; y < height && !foundStart; y++)
        {
            for (int x = 0; x < width && !foundStart; x++)
            {
                if (binaryImage[x, y])
                {
                    startX = x;
                    startY = y;
                    foundStart = true;
                }
            }
        }
        
        if (!foundStart)
            return contour;
            
        // Initial direction is right
        int dx = 1;
        int dy = 0;
        int x2 = startX;
        int y2 = startY;
        
        // Trace the boundary
        do {
            // Add current point to contour
            contour.Add(new Vector2(x2, y2));
            visited[x2, y2] = true;
            
            // Check in current direction
            bool frontEmpty = (x2 + dx < 0 || x2 + dx >= width || 
                               y2 + dy < 0 || y2 + dy >= height || 
                               !binaryImage[x2 + dx, y2 + dy]);
            
            if (frontEmpty) {
                // Turn right
                int tempDx = dx;
                dx = -dy;
                dy = tempDx;
            } else {
                // Move forward
                x2 += dx;
                y2 += dy;
                
                // Turn left
                int tempDx = dx;
                dx = dy;
                dy = -tempDx;
            }
            
            // Exit condition - back to start
            if (x2 == startX && y2 == startY)
                break;
                
            // Prevent infinite loops
            if (contour.Count > width * height)
                break;
                
        } while (true);
        
        return contour;
    }
    
    List<Vector2> SimplifyContour(List<Vector2> points, float epsilon)
    {
        if (points.Count < 3)
            return points;
            
        List<Vector2> result = new List<Vector2>();
        DouglasPeuckerReduction(points, 0, points.Count - 1, epsilon, result);
        return result;
    }
    
    void DouglasPeuckerReduction(List<Vector2> points, int startIndex, int endIndex, float epsilon, List<Vector2> resultPoints)
    {
        if (endIndex <= startIndex + 1) {
            // Add the start point to result
            if (!resultPoints.Contains(points[startIndex]))
                resultPoints.Add(points[startIndex]);
            
            // Add the end point to result if different from start
            if (endIndex > startIndex && !resultPoints.Contains(points[endIndex]))
                resultPoints.Add(points[endIndex]);
                
            return;
        }
        
        // Find the point with the maximum distance
        float dmax = 0;
        int index = startIndex;
        
        for (int i = startIndex + 1; i < endIndex; i++) {
            float d = PerpendicularDistance(points[i], points[startIndex], points[endIndex]);
            if (d > dmax) {
                index = i;
                dmax = d;
            }
        }
        
        // If maximum distance is greater than epsilon, recursively simplify
        if (dmax > epsilon) {
            DouglasPeuckerReduction(points, startIndex, index, epsilon, resultPoints);
            DouglasPeuckerReduction(points, index, endIndex, epsilon, resultPoints);
        } else {
            // Add only the start and end points
            if (!resultPoints.Contains(points[startIndex]))
                resultPoints.Add(points[startIndex]);
                
            if (!resultPoints.Contains(points[endIndex]))
                resultPoints.Add(points[endIndex]);
        }
    }
    
    float PerpendicularDistance(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
    {
        // Calculate the perpendicular distance from point to line
        float area = Mathf.Abs(
            (lineStart.y - lineEnd.y) * point.x +
            (lineEnd.x - lineStart.x) * point.y +
            (lineStart.x * lineEnd.y - lineEnd.x * lineStart.y)
        );
        
        float bottom = Mathf.Sqrt(
            Mathf.Pow(lineEnd.y - lineStart.y, 2) +
            Mathf.Pow(lineEnd.x - lineStart.x, 2)
        );
        
        return area / bottom;
    }
    
    void SetShadowShape(ShadowCaster2D shadowCaster, Vector2[] shape)
    {
        // In Unity 6, ShadowCaster2D uses Vector3[] for shapePath instead of Vector2[]
        // Convert the Vector2[] to Vector3[] (setting z=0)
        Vector3[] shape3D = new Vector3[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            shape3D[i] = new Vector3(shape[i].x, shape[i].y, 0);
        }
        
        // Check if we have cached the reflection members
        if (shapePathField == null)
        {
            CacheReflectionMembers();
            
            // If still null, try direct API approach for Unity 6
            if (shapePathField == null)
            {
                // Try new Unity 6 API approach if available
                // Check if the ShadowCaster2D has a public API we can use
                var shapeField = typeof(ShadowCaster2D).GetProperty("shapePath", 
                    BindingFlags.Public | 
                    BindingFlags.Instance);
                    
                if (shapeField != null)
                {
                    // Use the public API to set the shape
                    shapeField.SetValue(shadowCaster, shape3D);
                }
                else
                {
                    Debug.LogError("AdvancedPixelArtShadowCaster2D: Could not set shadow shape. " +
                        "This component is not compatible with this version of Unity.");
                }
                
                return;
            }
        }
            
        try
        {
            // Try to set value using reflection with Vector3[] array
            shapePathField.SetValue(shadowCaster, shape3D);
            
            // Call internal method to update the mesh if available
            if (setPathHashMethod != null)
                setPathHashMethod.Invoke(shadowCaster, null);
            
            // For Unity 2D Light upgrade 
            if (tryUpdateShapeParametersMethod != null)
                tryUpdateShapeParametersMethod.Invoke(shadowCaster, null);
                
            // If no method was found, try to force a mesh update
            if (setPathHashMethod == null && tryUpdateShapeParametersMethod == null)
            {
                // Try to find any method that might update the shape
                var allMethods = typeof(ShadowCaster2D).GetMethods(
                    BindingFlags.NonPublic | 
                    BindingFlags.Instance);
                    
                foreach (var method in allMethods)
                {
                    if (method.Name.Contains("Update") && method.Name.Contains("Shape") && 
                        method.GetParameters().Length == 0)
                    {
                        method.Invoke(shadowCaster, null);
                        break;
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("AdvancedPixelArtShadowCaster2D: Error setting shadow shape: " + e.Message);
        }
    }
}