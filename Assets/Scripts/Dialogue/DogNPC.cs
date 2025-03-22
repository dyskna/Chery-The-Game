using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace InventorySystem
{
public class DogAI : MonoBehaviour, INPC
{
    [Header("Dog Settings")]
    [SerializeField] private int pettingsNeededToFollow = 5;
    [SerializeField] private float fetchCooldown = 15f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private GameObject _itemBasePrefab;
    [SerializeField] private string[] _messages;
    [SerializeField] private GameObject _messagePrefab;
    [SerializeField] private int minFruit;
    [SerializeField] private int maxFruit;

    private Animator animator;
    private int pettingCount = 0;
    private bool isFollowing = false;
    private bool canFetchCoin = true;
    private Vector2 position;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        position = transform.position;
    }

    public void ProcessTag(string tag)
    {
        switch (tag)
        {
            case "Fetch":
                FetchCoin();
                break;

            default:
                Debug.Log("Unknown tag for dog: " + tag);
                break;
        }
    }

    private void FetchCoin()
    {
        if (!canFetchCoin)
        {
            Debug.Log("The dog is tired");
            var message = _messages[0];
            var msgObject = Instantiate(_messagePrefab, transform.position, Quaternion.identity);
            msgObject.GetComponentInChildren<TMP_Text>().SetText(message);
            return;
        }
        
        animator.SetTrigger("FetchT");
    }

    public void OnAnimFetchEnd()
    {
        GameItemSpawner itemSpawner = GameObject.FindObjectOfType<GameItemSpawner>();
        int amountOfFruit = Random.Range(minFruit,maxFruit);
        Debug.Log("Dog spawned coins "+ amountOfFruit);
        if (itemSpawner != null)
        {
            for(int i = 0 ; i<amountOfFruit; i++ )
                {itemSpawner.SpawnFruit(position, 1, _itemBasePrefab);}
        }
        canFetchCoin = false;
        StartCoroutine(FetchCooldown());
    }

    private IEnumerator FetchCooldown()
    {
        yield return new WaitForSeconds(fetchCooldown);
        canFetchCoin = true;
        Debug.Log("The dog is ready to bring another coin");
        
    }
}
}
