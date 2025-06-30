using UnityEngine;

namespace InventorySystem
{
    public class CherryCollector : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Cherry"))
        {
            // Notify agent if it's an AI
            TreeAgent agent = GetComponent<TreeAgent>();
            if (agent != null) agent.OnCherryCollected();

            Destroy(other.gameObject);
        }
    }
}
}
