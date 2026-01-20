// Umieść w folderze Assets/Editor/

using UnityEngine;
using UnityEditor;
using TMPro;

public class SetupAgentScoreDisplay : EditorWindow
{
    private Color agent1Color = new Color(1f, 0.3f, 0.3f); // Czerwony
    private Color agent2Color = new Color(0.3f, 0.5f, 1f); // Niebieski
    private string agent1Name = "AI Red";
    private string agent2Name = "AI Blue";
    private float fontSize = 3f;
    private Vector3 textOffset = new Vector3(0, 0.8f, 0);

    [MenuItem("Tools/Setup Agent Score Display")]
    public static void ShowWindow()
    {
        GetWindow<SetupAgentScoreDisplay>("Agent Score Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Konfiguracja wyświetlania punktów nad głowami", EditorStyles.boldLabel);
        GUILayout.Space(10);

        EditorGUILayout.HelpBox(
            "Automatycznie doda wyświetlanie punktów nad wszystkimi agentami we wszystkich arenach.",
            MessageType.Info
        );

        GUILayout.Space(10);

        // Ustawienia
        agent1Name = EditorGUILayout.TextField("Nazwa Agent 1:", agent1Name);
        agent1Color = EditorGUILayout.ColorField("Kolor Agent 1:", agent1Color);
        
        GUILayout.Space(5);
        
        agent2Name = EditorGUILayout.TextField("Nazwa Agent 2:", agent2Name);
        agent2Color = EditorGUILayout.ColorField("Kolor Agent 2:", agent2Color);
        
        GUILayout.Space(5);
        
        fontSize = EditorGUILayout.FloatField("Rozmiar czcionki:", fontSize);
        textOffset = EditorGUILayout.Vector3Field("Offset nad głową:", textOffset);

        GUILayout.Space(20);

        if (GUILayout.Button("Dodaj wyświetlanie do WSZYSTKICH agentów", GUILayout.Height(40)))
        {
            SetupAllAgents();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Usuń wszystkie wyświetlacze", GUILayout.Height(30)))
        {
            if (EditorUtility.DisplayDialog("Potwierdzenie", 
                "Czy na pewno chcesz usunąć wszystkie komponenty AgentScoreDisplay?", 
                "Tak", "Anuluj"))
            {
                RemoveAllDisplays();
            }
        }
    }

    private void SetupAllAgents()
    {
        CompetitiveCherryAgent[] agents = FindObjectsOfType<CompetitiveCherryAgent>();
        
        if (agents.Length == 0)
        {
            EditorUtility.DisplayDialog("Błąd", "Nie znaleziono żadnych agentów!", "OK");
            return;
        }

        int setupCount = 0;

        // Grupuj agentów po arenach
        foreach (CompetitiveCherryAgent agent in agents)
        {
            // Sprawdź czy już ma display
            AgentScoreDisplay existingDisplay = agent.GetComponent<AgentScoreDisplay>();
            if (existingDisplay != null)
            {
                Debug.Log($"Agent {agent.name} już ma wyświetlacz - aktualizuję");
                DestroyImmediate(existingDisplay);
            }

            // Określ który to agent w arenie (pierwszy czy drugi)
            Transform arena = agent.transform.parent;
            CompetitiveCherryAgent[] agentsInArena = arena.GetComponentsInChildren<CompetitiveCherryAgent>();
            
            bool isFirstAgent = (agentsInArena.Length > 0 && agentsInArena[0] == agent);
            
            // Dodaj display
            AgentScoreDisplay display = agent.gameObject.AddComponent<AgentScoreDisplay>();
            
            // Stwórz TextMeshPro
            GameObject textObj = new GameObject("ScoreText");
            textObj.transform.SetParent(agent.transform);
            textObj.transform.localPosition = textOffset;
            textObj.transform.localRotation = Quaternion.identity;
            textObj.transform.localScale = Vector3.one;
            
            TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.sortingOrder = 100;
            
            RectTransform rect = tmp.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.sizeDelta = new Vector2(2, 0.5f);
            }

            // Ustaw kolory i nazwy
            Color agentColor = isFirstAgent ? agent1Color : agent2Color;
            string agentName = isFirstAgent ? agent1Name : agent2Name;
            
            tmp.color = agentColor;

            // Przypisz przez SerializedObject
            SerializedObject so = new SerializedObject(display);
            so.FindProperty("agent").objectReferenceValue = agent;
            so.FindProperty("scoreText").objectReferenceValue = tmp;
            so.FindProperty("textOffset").vector3Value = textOffset;
            so.FindProperty("agentName").stringValue = agentName;
            so.FindProperty("textColor").colorValue = agentColor;
            so.FindProperty("fontSize").floatValue = fontSize;
            so.ApplyModifiedProperties();

            setupCount++;
            
            Debug.Log($"Skonfigurowano wyświetlacz dla: {agent.name} ({agentName})");
        }

        EditorUtility.DisplayDialog("Gotowe!", 
            $"Skonfigurowano wyświetlacze dla {setupCount} agentów!", "OK");
    }

    private void RemoveAllDisplays()
    {
        AgentScoreDisplay[] displays = FindObjectsOfType<AgentScoreDisplay>();
        
        int removed = 0;
        foreach (AgentScoreDisplay display in displays)
        {
            // Usuń również TextMeshPro
            Transform scoreText = display.transform.Find("ScoreText");
            if (scoreText != null)
            {
                DestroyImmediate(scoreText.gameObject);
            }
            
            DestroyImmediate(display);
            removed++;
        }

        EditorUtility.DisplayDialog("Usunięto", 
            $"Usunięto {removed} wyświetlaczy.", "OK");
    }
}