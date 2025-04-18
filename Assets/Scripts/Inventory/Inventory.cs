using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InventorySystem
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField]
        private int _size = 8;

        [SerializeField]
        private List<InventorySlot> _slots;

        private int _activeSlotIndex;

        public int Size => _size;
        public List<InventorySlot> Slots => _slots;

        public int ActiveSlotIndex
        {
            get => _activeSlotIndex;
            private set
            {
                _slots[_activeSlotIndex].Active = false; //desactivate previous slot'
                _activeSlotIndex = value < 0 ? _size - 1 : value % Size;
                _slots[_activeSlotIndex].Active = true;
            }
        }

        private void Awake()
        {
            if (_size > 0)
                _slots[0].Active = true;
        }

        private void OnValidate()
        {
            AdjustSize();
        }

        private void AdjustSize()
        {
            if (_slots == null)
                _slots = new List<InventorySlot>();
            if (_slots.Count > _size)
                _slots.RemoveRange(_size, _slots.Count - _size);
            if (_slots.Count < _size)
                _slots.AddRange(new InventorySlot[_size - _slots.Count]);
        }

        public bool IsFull()
        {
            return _slots.Count(slot => slot.HasItem) >= _size;
        }

        public bool CanAcceptItem(ItemStack itemStack)
        {
            var slotWithStackableItem = FindSlot(itemStack.Item, true);
            return !IsFull() || slotWithStackableItem != null;
        }

        private InventorySlot FindSlot(ItemDefinition item, bool onlyStackable = false)
        {
            return _slots.FirstOrDefault(_slots =>
                _slots.Item == item && item.IsStackable || !onlyStackable
            );
        }

        public bool HasItem(ItemStack itemStack, bool checkNumberOfItems = false)
        {
            var itemSlot = FindSlot(itemStack.Item);
            if (itemSlot == null)
                return false;
            if (checkNumberOfItems)
            {
                if (itemStack.Item.IsStackable)
                {
                    return itemSlot.NumberOfItems >= itemStack.NumberOfItems;
                }

                return _slots.Count(slot => slot.Item == itemStack.Item) >= itemStack.NumberOfItems;
            }
            return true;
        }

        public ItemStack RemoveItem(int atIndex, bool spawn = false)
        {
            if (!_slots[atIndex].HasItem)
                throw new InventoryException(InventoryOperation.Remove, "Slot is Empty");

            if (spawn && TryGetComponent<GameItemSpawner>(out var itemSpawner))
            {
                itemSpawner.SpawnItem(_slots[atIndex].State);
            }

            ClearSlot(atIndex);
            return new ItemStack();
        }

        public ItemStack RemoveItem(ItemStack itemStack)
        {
            var itemSlot = FindSlot(itemStack.Item);
            if (itemSlot == null)
            {
                throw new InventoryException(InventoryOperation.Remove, "No Item in the Inventory");
            }
            if (itemSlot.Item.IsStackable && itemSlot.NumberOfItems < itemStack.NumberOfItems)
            {
                throw new InventoryException(InventoryOperation.Remove, "Not enough Items");
            }

            itemSlot.NumberOfItems -= itemStack.NumberOfItems;
            if (itemSlot.Item.IsStackable && itemSlot.NumberOfItems > 0)
            {
                return itemSlot.State;
            }

            itemSlot.Clear(); //no stackable, delete whole item
            return new ItemStack();
        }

        public ItemStack AddItem(ItemStack itemStack)
        {
            InventorySlot relevantSlot = null;
            int remainingItems = itemStack.NumberOfItems;

            // Dodajemy do istniejących slotów z tym samym przedmiotem
            if (itemStack.Item.IsStackable)
            {
                foreach (var slot in _slots.Where(s => s.HasItem && s.Item == itemStack.Item))
                {
                    int canAdd = itemStack.Item.MaxStackSize - slot.NumberOfItems;
                    if (canAdd <= 0)
                        continue;

                    int added = Mathf.Min(remainingItems, canAdd);
                    slot.NumberOfItems += added;
                    remainingItems -= added;
                    relevantSlot = slot;

                    if (remainingItems <= 0)
                        return relevantSlot.State;
                }
            }

            // Dodajemy do pustych slotów
            while (remainingItems > 0)
            {
                var emptySlot = _slots.FirstOrDefault(slot => !slot.HasItem);
                if (emptySlot == null)
                    throw new InventoryException(InventoryOperation.Add, "Inventory is full");

                int toAdd = itemStack.Item.IsStackable
                    ? Mathf.Min(remainingItems, itemStack.Item.MaxStackSize)
                    : 1;

                emptySlot.State = new ItemStack(itemStack.Item, toAdd);
                remainingItems -= toAdd;
                relevantSlot = emptySlot;

                if (!itemStack.Item.IsStackable || remainingItems <= 0)
                    break;
            }

            return relevantSlot.State;
        }

        public void ClearSlot(int atIndex)
        {
            _slots[atIndex].Clear();
        }

        public ItemStack RemoveOneItem(int atIndex, bool spawn = false)
        {
            if (!_slots[atIndex].HasItem)
                throw new InventoryException(InventoryOperation.Remove, "Slot is Empty");

            if (spawn)
            {
                if (TryGetComponent<GameItemSpawner>(out var itemSpawner))
                {
                    itemSpawner.SpawnItem(_slots[atIndex].State, true);
                    if (!_slots[atIndex].Item.IsStackable)
                    {
                        ClearSlot(atIndex);
                        return new ItemStack();
                    }
                }
            }

            _slots[atIndex].NumberOfItems--;
            if (_slots[atIndex].NumberOfItems == 0)
            {
                ClearSlot(atIndex);
                return new ItemStack();
            }
            return _slots[atIndex].State;
        }

        public void ActivateSlot(int atIndex)
        {
            ActiveSlotIndex = atIndex;
        }

        public InventorySlot GetActiveSlot()
        {
            return _slots[_activeSlotIndex];
        }
    }
}
