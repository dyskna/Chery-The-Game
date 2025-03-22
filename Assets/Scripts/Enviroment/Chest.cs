using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace InventorySystem
{
    public class chest : MonoBehaviour, IInteractable
{
    [SerializeField] private int minFruit;
    [SerializeField] private int maxFruit;
    private Animator _animator;
    private bool _isOpened;
    private Vector2 position;

    void Start()
    {
        _animator = GetComponent<Animator>();
        position = transform.position;
    }

        [System.Obsolete]
        public void Interact()
    {
        _isOpened = true;
        _animator.SetTrigger("open");
        GameItemSpawner itemSpawner = GameObject.FindObjectOfType<GameItemSpawner>();
        int amountOfFruit = Random.Range(minFruit,maxFruit);
        Debug.Log("Fruit on this tree: "+ amountOfFruit);
        if (itemSpawner != null)
        {
            for(int i = 0 ; i<amountOfFruit; i++ )
                {itemSpawner.SpawnFruit(position, 1, null);}
        }
    }
    public bool CanInteract()
    {
        return ! _isOpened;
    }

}

}
