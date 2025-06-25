using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections.Generic;
using InventorySystem;


public class CherryAgent : Agent
{
    public float speed = 5f;
    public List<Harvesting> trees;
    private Harvesting currentTarget;

    public override void OnEpisodeBegin()
    {
        currentTarget = null;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var tree in trees)
        {
            sensor.AddObservation(tree.transform.position);
            sensor.AddObservation(tree._isHarvested ? 1 : 0);
        }

        sensor.AddObservation(transform.position);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int targetIndex = Mathf.FloorToInt(actions.DiscreteActions[0]);

        if (targetIndex < trees.Count)
        {
            currentTarget = trees[targetIndex];
            MoveToTarget();
        }
    }

    private void MoveToTarget()
    {
        if (currentTarget == null || currentTarget._isHarvested) return;

        Vector2 direction = (currentTarget.transform.position - transform.position).normalized;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, currentTarget.transform.position) < 0.5f && currentTarget.CanInteract())
        {
            currentTarget.Interact(); // Add a check to ensure this is AI-safe
            CherryGameManager manager = FindObjectOfType<CherryGameManager>();
            if (manager != null)
                manager.AddScore(false, UnityEngine.Random.Range(1, 3)); // Simulate reward
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // For debugging AI decisions
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = UnityEngine.Random.Range(0, trees.Count);
    }
}
