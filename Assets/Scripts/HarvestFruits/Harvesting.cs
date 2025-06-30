using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

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
        [SerializeField]
        private GameObject _messagePrefab;

        [SerializeField]
        private GameObject _itemBasePrefab;

        [SerializeField]
        private int minFruit;

        [SerializeField]
        private int maxFruit;

        [SerializeField]
        private string[] _messages;
        private Vector2 position;
        public PlayerMovement playerMovement;

        //AI
        public bool isAI = false;
        private TreeAgent aiAgent;
        void Start()
        {
            _animator = GetComponent<Animator>();
            position = transform.position;
            if (!isAI)
            {
                playerMovement = GameObject
                    .FindGameObjectWithTag("Player")
                    .GetComponent<PlayerMovement>();
            }
        }

        [System.Obsolete]
        public void Interact()
        {
            _isHarvested = true;
            _animator.SetTrigger("Harvested");

            //text
            var randomIndex = Random.Range(0, _messages.Length);
            var message = _messages[randomIndex];
            var msgObject = Instantiate(_messagePrefab, transform.position, Quaternion.identity);
            msgObject.GetComponentInChildren<TMP_Text>().SetText(message);
        }

        [System.Obsolete]
        public void InteractAsAI(TreeAgent agent)
        {
            isAI = true;
            aiAgent = agent;
            Interact();
        }


        //method activated by start of animation
        private void DisableControlsForSeconds()
        {
            if (!isAI)
            {
                // Player logic
                playerMovement.DisableMovement();
                player.GetComponent<Renderer>().enabled = false;
            }
            else if (aiAgent != null)
            {
                // AI logic
                aiAgent.FreezeAgent();
            }
        }

        //method activated by end of animation
        private void EnableControlsForSeconds()
        {
            if (!isAI)
            {
                // Player logic
                playerMovement.EnableMovement();
                player.GetComponent<Renderer>().enabled = true;
            }
            else if (aiAgent != null)
            {
                // AI logic
                aiAgent.UnfreezeAgent();
                isAI = false;
                aiAgent = null;
            }

            // Spawn cherries for both player and AI
            SpawnCherries();
        }

        private void SpawnCherries()
        {
            GameItemSpawner itemSpawner = GameObject.FindObjectOfType<GameItemSpawner>();
            int amountOfFruit = Random.Range(minFruit, maxFruit);
            if (itemSpawner != null)
            {
                for (int i = 0; i < amountOfFruit; i++)
                {
                    itemSpawner.SpawnFruit(position, 1, null);
                }
            }
        }

        public bool CanInteract()
        {
            return !_isHarvested;
        }
    }
}
