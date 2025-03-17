using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InventorySystem
{
    public class InventoryInputHandler : MonoBehaviour
    {
        private Inventory _inventory;

        private void Awake()
        {
            _inventory = GetComponent<Inventory>();
        }

        private void Update()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll > 0f || Input.GetButtonDown("PreviousItem"))
            {
                OnPreviousItem();
            }
            else if (scroll < 0f || Input.GetButtonDown("NextItem")) 
            {
                OnNextItem();
            }

            if (Input.GetButtonDown("ThrowItem"))
            {
                OnThrowOneItem();
            }
            
            if (Input.GetButtonDown("ThrowItem") && Input.GetKey(KeyCode.LeftControl))
            {
                OnThrowItem();
            }
        }


        private void OnThrowOneItem()
        {
            if (_inventory.GetActiveSlot().HasItem)
            {
            _inventory.RemoveOneItem(_inventory.ActiveSlotIndex, true);
            }
        }

        private void OnThrowItem()
        {
            if (_inventory.GetActiveSlot().HasItem)
            {
            _inventory.RemoveItem(_inventory.ActiveSlotIndex, true);
            }
        }

        private void OnNextItem()
        {
            _inventory.ActivateSlot(_inventory.ActiveSlotIndex + 1);
        }

        private void OnPreviousItem()
        {
            _inventory.ActivateSlot(_inventory.ActiveSlotIndex - 1);
        }

    }
}

