using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Ink.Runtime;
using UnityEngine.EventSystems;
using System.Linq;


public interface INPC
{
    void ProcessTag(string tag);
}

public class DialogueManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Choices UI")]
    [SerializeField] private GameObject[] choices;
    private TextMeshProUGUI[] choicesText;

    [Header("Animations")]
    [SerializeField] private Animator characterAnimator1;
    
    private Story currentStory;
    public bool dialogueIsPlaying { get; private set; }
    private bool choiceToMake = false;

    private Dictionary<string, INPC> npcDictionary = new Dictionary<string, INPC>();

    private static DialogueManager instance;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogWarning("More than one Dialogue Manager in the scene");
        }
        instance = this;

        // Find all NPCs in the scene and store them in the dictionary
        INPC[] npcs = FindObjectsOfType<MonoBehaviour>().OfType<INPC>().ToArray();
        foreach (var npc in npcs)
        {
            string npcName = ((MonoBehaviour)npc).gameObject.name;
            if (!npcDictionary.ContainsKey(npcName))
            {
                npcDictionary.Add(npcName, npc);
            }
        }
    }

    public bool GetDialogueIsPlaying()
    {
        return dialogueIsPlaying;
    }

    public static DialogueManager GetInstance()
    {
        return instance;
    }

    private void Start()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);

        choicesText = new TextMeshProUGUI[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            choicesText[i] = choices[i].GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    private void Update()
    {
        if (!dialogueIsPlaying)
            return;

        if (Input.GetButtonDown("Interact") && !choiceToMake)
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset inkJSON)
    {
        currentStory = new Story(inkJSON.text);
        dialogueIsPlaying = true;
        dialoguePanel.SetActive(true);
    }

    private void ExitDialogueMode()
    {
        dialogueIsPlaying = false;
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
    }

    private void ContinueStory()
    {
        if (currentStory.canContinue)
        {
            dialogueText.text = currentStory.Continue();
            DisplayChoices();
            HandleTags(currentStory.currentTags);
        }
        else
        {
            ExitDialogueMode();
        }
    }

    private void HandleTags(List<string> currentTags)
    {
        foreach (string tag in currentTags)
        {
            string[] splitTag = tag.Split(':');
            if (splitTag.Length != 2)
            {
                Debug.LogError("Bad tag: " + tag);
                continue;
            }

            string tagKey = splitTag[0].Trim();
            string tagValue = splitTag[1].Trim();

            if (npcDictionary.TryGetValue(tagKey, out INPC npcScript))
            {
                npcScript.ProcessTag(tagValue);
            }
            else
            {
                Debug.LogError($"Can't find NPC: {tagKey}");
            }
        }
    }

    private void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        if (currentChoices.Count > choices.Length)
        {
            Debug.LogError("More choices than can be supported. Number of choices given: " + currentChoices.Count);
        }

        int index = 0;
        foreach (Choice choice in currentChoices)
        {
            choices[index].gameObject.SetActive(true);
            choicesText[index].text = choice.text;
            index++;
            
            if (index == 1)
            {
                choiceToMake = true;
            }
        }

        for (int i = index; i < choices.Length; i++)
        {
            choices[i].gameObject.SetActive(false);
        }
    }

    public void MakeChoice(int choiceIndex)
    {
        Debug.Log(choiceIndex);
        currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory();
        choiceToMake = false;
    }
}
