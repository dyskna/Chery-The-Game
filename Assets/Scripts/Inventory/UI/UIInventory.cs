using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InventorySystem.UI
{
    public class UIInventory : MonoBehaviour
    {
        [SerializeField]
        private GameObject _inventorySlotPrefab;

        [SerializeField]
        private Inventory _inventory;

        [SerializeField]
        private List<UIInventorySlot> _slots;

        public Inventory Inventory => _inventory;

        [ContextMenu("Initialize Inventory")]
        private void InitializeInventoryUI()
        {
            if (_inventory == null || _inventorySlotPrefab == null)
                return;

            _slots = new List<UIInventorySlot>(_inventory.Size);
            for (var i = 0; i < _inventory.Size; i++)
            {
                var uiSlot = PrefabUtility.InstantiatePrefab(_inventorySlotPrefab) as GameObject;
                uiSlot.transform.SetParent(transform, false);
                var uiSlotScript = uiSlot.GetComponent<UIInventorySlot>();
                uiSlotScript.AssignSlot(i);
                _slots.Add(uiSlotScript);
            }
        }
    }
}
