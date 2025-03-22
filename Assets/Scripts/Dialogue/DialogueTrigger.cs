using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private bool multipleInteract = false;

    [Header("Visual Cue")]
     [SerializeField] private GameObject visualCue;

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;

    private bool _isRead;

    private void Awake()
    {   
        visualCue.SetActive(true);
    }


    public void Interact()
    {
        DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
        if(!multipleInteract)
        {
            _isRead = true;
        }

        visualCue.SetActive(false);
    }

    public bool CanInteract()
    {
        if (DialogueManager.GetInstance().GetDialogueIsPlaying()) 
        {
            return false;
        }

        if (multipleInteract && _isRead) 
        {
            _isRead = false;
            visualCue.SetActive(true);
        }

        return !_isRead;
    }
    
}
