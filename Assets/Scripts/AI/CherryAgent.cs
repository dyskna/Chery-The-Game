using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class CompetitiveCherryAgent : Agent
{

    [Header("Ustawienia Agenta")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float interactionDistance;

    [Header("Nagrody i Kary")]
    [SerializeField] private float treeRewardValue;
    [SerializeField] private float chestRewardValue;
    [SerializeField] private float wallCollisionPenalty;
    [SerializeField] private float winReward;
    [SerializeField] private float losePenalty;


    [SerializeField] private float opponentPenalty = 0.5f;
    
    [Header("Ustawienia Przeciwników")]
    [Tooltip("Przeciągnij tu drugiego agenta (do treningu AI vs AI)")]
    [SerializeField] public CompetitiveCherryAgent opponent;
    [Tooltip("Przeciągnij tu obiekt gracza z komponentem Collector (do gry AI vs Człowiek)")]

    private Rigidbody2D rb;
    private List<IInteractable> allTargets = new List<IInteractable>();
    //private Vector3 startPosition;
    private int score = 0;
    private Transform arenaTransform;

private Vector3 startPosition;  

public override void Initialize()
{
    rb = GetComponent<Rigidbody2D>();
    startPosition = transform.position; 

    if (arenaTransform == null)
    {
        arenaTransform = transform.parent;
    }

    allTargets.AddRange(arenaTransform.GetComponentsInChildren<IInteractable>());
}

public override void OnEpisodeBegin()
{
    // Reset przez Rigidbody2D
    if (rb != null)
    {
        rb.position = startPosition;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }
    
    score = 0;

    foreach (var target in allTargets)
    {
        (target as Harvesting)?.ResetState();
        (target as Chest)?.ResetState();
    }
}

public override void CollectObservations(VectorSensor sensor)
{
    // Stan wlasny agenta (5 wymiarow)
    sensor.AddObservation(transform.localPosition / 25f);    // pozycja (2D)
    sensor.AddObservation(rb.linearVelocity / moveSpeed);    // predkosc (2D)
    sensor.AddObservation(score / 30f);                      // wynik (1D)

    // Stan przeciwnika (3 wymiary)
    if (opponent != null)
    {
        sensor.AddObservation(opponent.transform.localPosition / 25f);
        sensor.AddObservation(opponent.GetScore() / 30f);
    }
    else
    {
        sensor.AddObservation(Vector3.zero);
        sensor.AddObservation(0f);
    }

    // Wszystkie zasoby na arenie (13 x 5 = 65 wymiarow)
    foreach (var target in allTargets)
    {
        if (target != null)
        {
            var targetMono = target as MonoBehaviour;
            
            // Pozycja wzgledna zasobu
            Vector3 relativePos = (targetMono.transform.localPosition 
                                   - transform.localPosition) / 25f;
            sensor.AddObservation(relativePos);  // 3 floats
            
            // Status dostepnosci (0 = zebrane, 1 = dostepne)
            sensor.AddObservation(target.CanInteract() ? 1f : 0f);
            
            // Typ zasobu (0 = drzewo, 1 = skrzynia)
            sensor.AddObservation(target is Chest ? 1f : 0f);
        }
    }
}
// Calkowity rozmiar: 5 + 3 + 65 = 73 wymiary


public override void OnActionReceived(ActionBuffers actions)
{
    float moveX = actions.ContinuousActions[0];
    float moveY = actions.ContinuousActions[1];
    rb.linearVelocity = new Vector2(moveX, moveY) * moveSpeed;

    TryInteractWithClosestTarget();

    if (StepCount >= MaxStep)
    {
        EndEpisodeByWinner();
    }
}

    
    public int GetScore()
    {
        return score;
    }

    private void TryInteractWithClosestTarget()
    {
    IInteractable closestTarget = FindClosestAvailableTarget();
    if (closestTarget == null) return;
    
    var closestTargetTransform = (closestTarget as MonoBehaviour).transform;
    float distance = Vector2.Distance(transform.position, closestTargetTransform.position);

    if (distance <= interactionDistance)
    {
        closestTarget.Interact();

        int rewardValue = 0;
        if (closestTarget is Harvesting)
        {
            rewardValue = (int)treeRewardValue;
        }
        else if (closestTarget is Chest)
        {
            rewardValue = (int)chestRewardValue;
        }


        this.score += rewardValue;
        
        // Nagroda dla agenta 
        AddReward(rewardValue);
        
        // Kara dla przeciwnika 
        if (opponent != null) 
            opponent.AddReward(-opponentPenalty * rewardValue);

        
    }

    
}


private void EndEpisodeByWinner()
{
    if (opponent != null)
    {
        int scoreDiff = score - opponent.GetScore();
        
        if (scoreDiff > 0)
        {
            AddReward(winReward + scoreDiff * 0.5f);
        }
        else if (scoreDiff < 0)
        {
            SetReward(losePenalty + scoreDiff * 0.5f);
        }
        else 
        {
            if (score == 0)
            {
    
                AddReward(losePenalty * 2.0f); 
            }
            else
            {
                // Mniejsza kara za remis z punktami
                SetReward(losePenalty/2.0f);
            }
        }
    }
    EndEpisode();
}

    private IInteractable FindClosestAvailableTarget()
    {
        IInteractable closest = null;
        float minDistance = float.MaxValue;

        foreach (var target in allTargets)
        {
            var targetMono = target as MonoBehaviour;
            if (targetMono != null && target.CanInteract()) // tylko nie zebrane
            {
                float distance = Vector2.Distance(transform.position, targetMono.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = target;
                }
            }
        }

        return closest;
    }

    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(wallCollisionPenalty);
        }
    }
    
}