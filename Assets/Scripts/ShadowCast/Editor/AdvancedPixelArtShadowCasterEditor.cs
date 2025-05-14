using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(AdvancedPixelArtShadowCaster2D))]
public class AdvancedPixelArtShadowCasterEditor : Editor
{
    SerializedProperty alphaThreshold;
    SerializedProperty shadowIntensity;
    SerializedProperty shadowOffset;
    SerializedProperty simplificationLevel;
    SerializedProperty outlineMethod;
    
    GUIStyle headerStyle;
    
    void OnEnable()
    {
        alphaThreshold = serializedObject.FindProperty("alphaThreshold");
        shadowIntensity = serializedObject.FindProperty("shadowIntensity");
        shadowOffset = serializedObject.FindProperty("shadowOffset");
        simplificationLevel = serializedObject.FindProperty("simplificationLevel");
        outlineMethod = serializedObject.FindProperty("outlineMethod");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        if (headerStyle == null)
        {
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.fontSize = 14;
        }
        
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Pixel Art Shadow Settings", headerStyle);
        EditorGUILayout.Space(5);
        
        // Main section - Shadow Properties
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Shadow Properties", EditorStyles.boldLabel);
        
        EditorGUILayout.Slider(shadowIntensity, 0, 1, "Shadow Intensity");
        EditorGUILayout.Slider(shadowOffset, -5, 5, "Shadow Offset");
        
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.Space(10);
        
        // Shape Settings Section
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Shape Detection", EditorStyles.boldLabel);
        
        EditorGUILayout.PropertyField(outlineMethod, new GUIContent("Outline Method"));
        EditorGUILayout.Slider(alphaThreshold, 0.01f, 1f, "Alpha Threshold");
        
        if (outlineMethod.enumValueIndex == (int)AdvancedPixelArtShadowCaster2D.OutlineMethod.AccuratePolygon)
        {
            EditorGUILayout.PropertyField(simplificationLevel, new GUIContent("Simplification Level"));
            EditorGUILayout.HelpBox("Higher values mean simpler shapes with fewer vertices. Lower values create more accurate but complex shadow shapes.", MessageType.Info);
        }
        
        EditorGUILayout.EndVertical();
        
        // Button to update shadow shape
        EditorGUILayout.Space(10);
        if (GUILayout.Button("Update Shadow Shape", GUILayout.Height(30)))
        {
            AdvancedPixelArtShadowCaster2D caster = (AdvancedPixelArtShadowCaster2D)target;
            
            // Force shadow update
            var updateMethod = typeof(AdvancedPixelArtShadowCaster2D).GetMethod("UpdateShadowCaster", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (updateMethod != null)
                updateMethod.Invoke(caster, null);
                
            EditorUtility.SetDirty(target);
        }
        
        // Help info
        EditorGUILayout.Space(5);
        EditorGUILayout.HelpBox(
            "Simple Rectangle: Creates a rectangular shadow based on sprite bounds.\n\n" +
            "Accurate Polygon: Creates a polygon that follows the sprite's shape more closely.",
            MessageType.Info);
        
        serializedObject.ApplyModifiedProperties();
    }
}
#endif