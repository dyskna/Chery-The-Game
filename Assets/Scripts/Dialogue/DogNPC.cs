using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

namespace InventorySystem
{
public class DogAI : MonoBehaviour, INPC
{
    [Header("Player")]
    public Transform playerPosition;

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
    private Rigidbody2D rb;
    private int pettingCount = 0;
    private bool isFollowing = false;
    private bool canFetchCoin = true;
    private Vector2 position;
    [SerializeField] public Vector3 offsetDog;
    [SerializeField] private float followDistance = 2f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (playerPosition != null && isFollowing)
        {
           Vector2 currentPosition = rb.position;
            Vector2 targetPosition = (Vector2)playerPosition.position - (Vector2)offsetDog;

            // Oblicz odległość do gracza
            float distanceToPlayer = Vector2.Distance(currentPosition, targetPosition);

            // Sprawdź, czy pies jest wystarczająco daleko, aby się poruszać
            if (distanceToPlayer > followDistance)
            {
                // Oblicz kierunek ruchu
                Vector2 moveDirection = (targetPosition - currentPosition).normalized;

                // Ustaw parametry animatora
                animator.SetFloat("Horizontal", moveDirection.x);
                animator.SetFloat("Vertical", moveDirection.y);
        
                // Oblicz prędkość
                float speed = Mathf.Clamp01(distanceToPlayer / followDistance);
                animator.SetFloat("Speed", speed);

                // Przesuń psa
                rb.velocity = moveDirection * moveSpeed;
            }
            else
            {
                // Zatrzymaj psa, gdy jest blisko gracza
                animator.SetFloat("Speed", 0);
                rb.velocity = Vector2.zero;
            }
        }
    }   

    public void ProcessTag(string tag)
    {
        switch (tag)
        {
            case "Fetch":
                FetchCoin();
                break;

            case "Pet":
                Pet();
                break;
            default:
                Debug.Log("Unknown tag for dog: " + tag);
                break;
        }
    }

    private void Pet()
    {
        animator.SetTrigger("Pet");
        pettingCount++;
        var message = _messages[1];
        var msgObject = Instantiate(_messagePrefab, transform.position, Quaternion.identity);
        msgObject.GetComponentInChildren<TMP_Text>().SetText(message);
        if(pettingCount == pettingsNeededToFollow)
        {
            
            isFollowing = true;
            //index 1 to pet
            
        }
    }
        

    private void FetchCoin()
    {
        if (!canFetchCoin)
        {
            Debug.Log("The dog is tired");
            //index 0 when tired
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
                {itemSpawner.SpawnFruit(transform.position, 1, _itemBasePrefab);}
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



