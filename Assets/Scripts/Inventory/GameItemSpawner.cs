using UnityEditor;
using UnityEngine;
using UnityEngine.Timeline;

namespace InventorySystem
{
    public class GameItemSpawner : MonoBehaviour
    {
        [SerializeField]
        private GameObject _itemBasePrefab;

        public void SpawnItem(ItemStack itemStack, bool individually = false)
        {
            if (_itemBasePrefab == null) return;
            var item = PrefabUtility.InstantiatePrefab(_itemBasePrefab) as GameObject;
            item.transform.position = transform.position;
            var gameItemScript = item.GetComponent<GameItem>();
            if(individually) gameItemScript.SetStack(new ItemStack(itemStack.Item, 1)); 
            else gameItemScript.SetStack(new ItemStack(itemStack.Item, itemStack.NumberOfItems)); 
            int randomDirection = Random.value > 0.5f ? 1 : -1;
            gameItemScript.Throw(randomDirection); 
            
        }

        public void SpawnFruit(Vector2 position, int maxAmount)
        {
            if (_itemBasePrefab == null) return;
            var itemDef = _itemBasePrefab.GetComponent<GameItem>();
            var item = PrefabUtility.InstantiatePrefab(_itemBasePrefab) as GameObject;
            item.transform.position = position;
            var gameItemScript = item.GetComponent<GameItem>();
            gameItemScript.SetStack(new ItemStack(itemDef.Stack.Item, maxAmount)); 
            int randomDirection = Random.value > 0.5f ? 1 : -1;
            gameItemScript.Throw(randomDirection); 
        }

    }
}

