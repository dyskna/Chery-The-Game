using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;

public interface IInteractable
{
    void Interact();
    bool CanInteract();
}

namespace InventorySystem
{
    public class Harvesting : MonoBehaviour, IInteractable
    {
    //animka
    public Animator _animator;
    public bool _isHarvested;
    public GameObject player;
    //text
    [SerializeField] private GameObject _messagePrefab;
    [SerializeField] private GameObject _itemBasePrefab;
    [SerializeField] private int minFruit;
    [SerializeField] private int maxFruit;

    [SerializeField] private string[] _messages;
    private Vector2 position;
    public PlayerMovement playerMovement;

    

    void Start()
    {
        _animator = GetComponent<Animator>();
        position = transform.position;
        playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
    }

        [System.Obsolete]
        public void Interact()
    {

        //odpowiada za animke i znikanie postaci na 5 sec
        _isHarvested = true;
        _animator.SetTrigger("Harvested");
        DisableControlsTemporarily(5f);

        //text
        var randomIndex = Random.Range(0, _messages.Length);
        var message = _messages[randomIndex];
        var msgObject = Instantiate(_messagePrefab, transform.position, Quaternion.identity);
        msgObject.GetComponentInChildren<TMP_Text>().SetText(message);

        //zbieranie
        GameItemSpawner itemSpawner = GameObject.FindObjectOfType<GameItemSpawner>();
        int amountOfFruit = Random.Range(minFruit,maxFruit);
        Debug.Log("Fruit on this tree: "+ amountOfFruit);
        if (itemSpawner != null)
        {
            for(int i = 0 ; i<amountOfFruit; i++ )
                {itemSpawner.SpawnFruit(position, 1, null);}
        }
    
        
    }

    public void DisableControlsTemporarily(float duration)
    {
        StartCoroutine(DisableControlsForSeconds(duration));
    }

    private IEnumerator DisableControlsForSeconds(float seconds)
    {
        
        playerMovement.DisableMovement(); 
        player.GetComponent<Renderer>().enabled = false;

        yield return new WaitForSeconds(seconds);

        playerMovement.EnableMovement(); 
        player.GetComponent<Renderer>().enabled = true;
        
    }

    public bool CanInteract()
    {
        return ! _isHarvested;
    }

}

}
