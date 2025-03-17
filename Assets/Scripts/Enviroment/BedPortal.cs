using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;



public class BedPortal : MonoBehaviour, IInteractable
{
    public Animator _animator;
    public GameObject player;
    public bool _isSlept;

    private WorldTime.WorldTime worldTime;
    

    void Start()
    {
        worldTime = FindObjectOfType<WorldTime.WorldTime>();
        if (worldTime == null)
        {
            Debug.LogError("WorldTime object not found in the scene!");
        }
        _animator = GetComponent<Animator>();

    }
    public void Interact()
    {
        TimeSpan sleepTime = TimeSpan.FromMinutes(1140); // Odpowiada godzinie 19:00
        
        if (worldTime != null && worldTime._currentTime >= sleepTime)
        {
            Debug.Log("Możesz iść spać!");

            _isSlept = true;
            player.SetActive(false); // Ukrycie gracza
            _animator.SetTrigger("Slept"); // Uruchomienie animacji

            // Wywołanie po 5 sekundach metody SetFalse
            Invoke("SetFalse", 5.0f);
        }
        
    }

    void SetFalse()
    {
        //player.SetActive(true);
        SceneManager.LoadScene("Nightmare");
        _isSlept = false;
    }
    public bool CanInteract()
    {
        return ! _isSlept;
    }
    
}
