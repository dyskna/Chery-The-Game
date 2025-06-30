using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

namespace InventorySystem
{
    public class TreeAgent : Agent
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 1.5f;

    private Rigidbody2D rb;
    private SpriteRenderer agentRenderer;
    private bool isFrozen = false;

        public override void Initialize()
        {
            rb = GetComponent<Rigidbody2D>();
            agentRenderer = GetComponent<SpriteRenderer>();

            // Set rigidbody properties
            rb.gravityScale = 0;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        
            // Add action space validation
            var actionSpec = GetComponent<Unity.MLAgents.Policies.BehaviorParameters>().BrainParameters.ActionSpec;
            Debug.Log($"Continuous Actions: {actionSpec.NumContinuousActions}, " +
             $"Discrete Branches: {string.Join(",", actionSpec.BranchSizes)}"); 
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Agent's frozen state
        sensor.AddObservation(isFrozen);

        // 2. Nearest tree information
        Vector2 treeDirection = Vector2.zero;
        float treeDistance = float.MaxValue;
        bool canInteract = false;

        GameObject[] trees = GameObject.FindGameObjectsWithTag("Tree");
        foreach (var tree in trees)
        {
            float dist = Vector2.Distance(transform.position, tree.transform.position);
            if (dist < treeDistance)
            {
                treeDistance = dist;
                treeDirection = (tree.transform.position - transform.position).normalized;

                var interactable = tree.GetComponent<IInteractable>();
                canInteract = (interactable != null && interactable.CanInteract());
            }
        }

        sensor.AddObservation(treeDirection);
        sensor.AddObservation(treeDistance);
        sensor.AddObservation(canInteract);

        // 3. Cherries information
        GameObject[] cherries = GameObject.FindGameObjectsWithTag("Cherry");
        sensor.AddObservation(cherries.Length);

        if (cherries.Length > 0)
        {
            Vector2 closestCherryDir = (cherries[0].transform.position - transform.position).normalized;
            sensor.AddObservation(closestCherryDir);
        }
        else
        {
            sensor.AddObservation(Vector2.zero);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Skip actions if frozen (during interaction animation)
        if (isFrozen)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Movement
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];
        rb.linearVelocity = new Vector2(moveX, moveY) * moveSpeed;

        // Interaction
        int shouldInteract = actions.DiscreteActions[0];
        if (shouldInteract == 1)
        {
            TryInteractWithTree();
        }
    }

    private void TryInteractWithTree()
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, interactionRadius);
        foreach (var collider in nearbyObjects)
        {
            if (collider.CompareTag("Tree"))
            {
                var tree = collider.GetComponent<Harvesting>();
                if (tree != null && tree.CanInteract())
                {
                    tree.InteractAsAI(this);
                    AddReward(0.5f); // Reward for starting interaction
                    break;
                }
            }
        }
    }

    // Called by Harvesting script during animation event
    public void FreezeAgent()
    {
        isFrozen = true;
        agentRenderer.enabled = false;
        rb.linearVelocity = Vector2.zero;
    }

    // Called by Harvesting script during animation event
    public void UnfreezeAgent()
    {
        isFrozen = false;
        agentRenderer.enabled = true;
    }

    // Call this when the agent collects a cherry
    public void OnCherryCollected()
    {
        AddReward(1.0f); // Reward for collecting cherry
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");

        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.E) ? 1 : 0;
    }

    // Visualize interaction radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
}
