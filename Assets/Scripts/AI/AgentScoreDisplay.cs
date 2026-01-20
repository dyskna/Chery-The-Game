// ZAPISZ W: Assets/Scripts/AgentScoreDisplay.cs
// (NIE w folderze Editor!)

using UnityEngine;
using TMPro;

public class AgentScoreDisplay : MonoBehaviour
{
    [Header("Referencje")]
    [SerializeField] private CompetitiveCherryAgent agent;
    [SerializeField] private TextMeshPro scoreText;
    
    [Header("Ustawienia pozycji")]
    [SerializeField] private Vector3 textOffset = new Vector3(0, 0.8f, 0);
    [SerializeField] private bool followAgent = true;
    
    [Header("Ustawienia wyglądu")]
    [SerializeField] private string agentName = "AI";
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private float fontSize = 4f;
    [SerializeField] private bool showName = false;
    [SerializeField] private bool showScore = true;
    
    [Header("Efekty")]
    [SerializeField] private bool flashOnScoreChange = true;
    [SerializeField] private Color flashColor = Color.yellow;
    [SerializeField] private float flashDuration = 0.2f;
    
    private int lastScore = 0;
    private float flashTimer = 0f;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        
        // Jeśli nie przypisano agenta, znajdź w tym samym obiekcie
        if (agent == null)
        {
            agent = GetComponent<CompetitiveCherryAgent>();
        }

        // Jeśli nie ma TextMeshPro, stwórz automatycznie
        if (scoreText == null)
        {
            CreateScoreText();
        }
        else
        {
            ConfigureText();
        }
    }

    private void CreateScoreText()
    {
        GameObject textObj = new GameObject("ScoreText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = textOffset;
        textObj.transform.localRotation = Quaternion.identity;
        
        scoreText = textObj.AddComponent<TextMeshPro>();
        ConfigureText();
    }

    private void ConfigureText()
    {
        if (scoreText == null) return;
        
        scoreText.fontSize = fontSize;
        scoreText.color = textColor;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.fontStyle = FontStyles.Bold;
        
        // Sortowanie - zawsze na wierzchu
        scoreText.sortingOrder = 100;
        
        // Ustaw wielkość
        RectTransform rect = scoreText.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.sizeDelta = new Vector2(2, 0.5f);
        }
    }

    private void Update()
    {
        if (agent == null || scoreText == null) return;

        // Aktualizuj pozycję
        if (followAgent)
        {
            scoreText.transform.position = agent.transform.position + textOffset;
        }

        // Zawsze patrz na kamerę
        if (mainCamera != null)
        {
            scoreText.transform.rotation = mainCamera.transform.rotation;
        }

        // Aktualizuj tekst
        UpdateScoreText();
        
        // Efekt błysku
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            float t = flashTimer / flashDuration;
            scoreText.color = Color.Lerp(textColor, flashColor, t);
        }
    }

    private void UpdateScoreText()
    {
        int currentScore = agent.GetScore();
        
        // Sprawdź czy wynik się zmienił
        if (flashOnScoreChange && currentScore != lastScore && lastScore != 0)
        {
            TriggerFlash();
        }
        lastScore = currentScore;

        // Buduj tekst
        string displayText = "";
        
        if (showName)
        {
            displayText += $"{agentName}\n";
        }
        
        if (showScore)
        {
            displayText += $"<size={fontSize * 1.5f}>{currentScore}</size>";
        }

        scoreText.text = displayText;
    }

    private void TriggerFlash()
    {
        flashTimer = flashDuration;
    }

    // Wywołaj to z zewnątrz aby zmienić kolor (np. czerwony dla AI, niebieski dla gracza)
    public void SetColor(Color color)
    {
        textColor = color;
        if (scoreText != null)
        {
            scoreText.color = color;
        }
    }

    public void SetAgentName(string name)
    {
        agentName = name;
    }
}