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
    [SerializeField] private float fetchAnimationDuration = 1f; 
    [SerializeField] private float fetchAnimationDistance = 0.5f; 
    [SerializeField] private GameObject _itemBasePrefab;
    [SerializeField] private string[] _messages;
    [SerializeField] private GameObject _messagePrefab;
    [SerializeField] private int minFruit;
    [SerializeField] private int maxFruit;

    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    private int pettingCount = 0;
    private bool isFollowing = false;
    private bool canFetchCoin = true;
    private bool isFetchAnimating = false;
    private Vector2 previousPosition;
    private const float stopThreshold = 0.0001f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
{
    if (playerPosition != null && isFollowing)
    {
        Vector2 currentPosition = rb.position;
        
        // Oblicz kierunek od psa do gracza
        Vector2 directionToPlayer = ((Vector2)playerPosition.position - currentPosition).normalized;
        
        // Ustaw target position z offsetem
        float desiredDistance = 0.12f; // Pożądany dystans od gracza
        Vector2 targetPosition = (Vector2)playerPosition.position - directionToPlayer * desiredDistance;

        Vector2 movement = targetPosition - currentPosition;

        if (Vector2.Distance(currentPosition, previousPosition) > stopThreshold)
        {
            animator.SetFloat("Speed", moveSpeed);
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }

        rb.MovePosition(currentPosition + movement * moveSpeed * Time.fixedDeltaTime);
        previousPosition = currentPosition;
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
        
        if (!isFetchAnimating)
        {
            StartCoroutine(FetchAnimation());
        }
    }

    private IEnumerator FetchAnimation()
    {
        isFetchAnimating = true;

        Vector3 startPosition = rb.position;
        Vector3 fetchDirection = Vector2.right.normalized; 
        Vector3 fetchEndPosition = startPosition + fetchDirection * fetchAnimationDistance;

        float elapsedTime = 0;
        while (elapsedTime < fetchAnimationDuration/2)
        {
            float t = elapsedTime / (fetchAnimationDuration/2);
            transform.position = Vector2.Lerp(startPosition, fetchEndPosition, t);
            
            // Ustawienie kierunku animacji
            Vector2 moveDirection = (fetchEndPosition - startPosition).normalized;
            animator.SetFloat("Speed", moveSpeed);
            animator.SetFloat("Horizontal", moveDirection.x);
            animator.SetFloat("Vertical", moveDirection.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = fetchEndPosition;

        elapsedTime = 0;
        while (elapsedTime < fetchAnimationDuration/2)
        {
            float t = elapsedTime / (fetchAnimationDuration/2);
            transform.position = Vector2.Lerp(fetchEndPosition, playerPosition.position, t);
            
            // Ustawienie kierunku animacji powrotu
            Vector2 returnDirection = (playerPosition.position - fetchEndPosition).normalized;
            //animator.SetFloat("Speed", moveSpeed);
            animator.SetBool("Fetch", true);
            animator.SetFloat("Horizontal", returnDirection.x);
            animator.SetFloat("Vertical", returnDirection.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = playerPosition.position;
        animator.SetBool("Fetch", false);
        animator.SetFloat("Speed", 0);
        OnAnimFetchEnd();
        isFetchAnimating = false;
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



