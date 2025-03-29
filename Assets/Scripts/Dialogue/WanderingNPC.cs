using UnityEngine;
using System.Collections;

public class NPCWandering : MonoBehaviour
{
    [Header("Wandering Behavior")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float minWanderWaitTime = 2f;
    [SerializeField] private float maxWanderWaitTime = 5f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("State Control")]
    [SerializeField] private bool isWandering = true;
    [SerializeField] private bool isFollowing = false;

    [Header("Optional References")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;

    private Vector2 startPosition;


    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        startPosition = rb ? rb.position : (Vector2)transform.position;
        StartCoroutine(WanderRoutine());
    }

    private IEnumerator WanderRoutine()
    {
        Debug.Log(DialogueManager.GetInstance().GetDialogueIsPlaying());
        Debug.Log("Wandering"+isWandering);
        while (isWandering)
        {
            
            if (!isFollowing && 
                (DialogueManager.GetInstance().GetDialogueIsPlaying() == false) && 
                rb != null && animator != null)
            {
                yield return StartCoroutine(Wander());
            }
            
            float waitTime = Random.Range(minWanderWaitTime, maxWanderWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator Wander()
    {
        Vector2 wanderTarget = startPosition + Random.insideUnitCircle * wanderRadius;
        float elapsedTime = 0f;

        while (elapsedTime < 3f)
        {
            if (DialogueManager.GetInstance().GetDialogueIsPlaying() == true)
            {
                if (animator != null)
                    animator.SetFloat("Speed", 0);
                yield break;
            }

            Vector2 direction = (wanderTarget - (Vector2)transform.position).normalized;
            float distanceToTarget = Vector2.Distance(transform.position, wanderTarget);

            if (distanceToTarget > 0.1f)
            {
                if (animator != null)
                {
                    animator.SetFloat("Speed", moveSpeed);
                    animator.SetFloat("Horizontal", direction.x);
                    animator.SetFloat("Vertical", direction.y);
                }
                rb.MovePosition(
                    (Vector2)transform.position + direction * moveSpeed * Time.fixedDeltaTime
                );
            }
            else
            {
                if (animator != null)
                    animator.SetFloat("Speed", 0);
                break;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (animator != null)
            animator.SetFloat("Speed", 0);
    }

    public void SetFollowing()
    {
        isFollowing = true;
        isWandering = false;
        StopAllCoroutines();
    }

    // Metoda do włączania/wyłączania wędrowania
    public void SetWandering()
    {
        isWandering = true;
        isFollowing = false;
        StopAllCoroutines(); 
        StartCoroutine(WanderRoutine());
    }
}