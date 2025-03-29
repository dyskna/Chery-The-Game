using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class SceneShift : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayableDirector timeline;
    private bool canInteract = true;
    private WorldTime.WorldTime worldTime;
    private TimeSpan returnTime;
    private TimeSpan finalTime;

    void Start()
    {
        worldTime = FindObjectOfType<WorldTime.WorldTime>();
        if (worldTime == null)
        {
            Debug.LogError("WorldTime object not found in the scene!");
            return; // Zapobiega błędom, jeśli `worldTime` nie istnieje
        }

        returnTime = TimeSpan.FromMinutes(960); // 16:00
        finalTime = TimeSpan.FromMinutes(1140); // 19:00
    }

    void FixedUpdate()
    {
        if (worldTime != null && worldTime._currentTime >= returnTime)
        {
            canInteract = true; // Opcjonalnie: pozwala na interakcję po 16:00
        }
    }

    public void Interact()
    {
        if (canInteract && worldTime != null && worldTime._currentTime >= returnTime)
        {
            canInteract = false;
            timeline.stopped += OnTimelineEnd;
            timeline.Play();
        }
    }

    private void OnTimelineEnd(PlayableDirector director)
    {
        SceneManager.LoadScene("Nightmare");
        timeline.stopped -= OnTimelineEnd;
    }

    public bool CanInteract()
    {
        return canInteract;
    }
}
