using UnityEngine;

namespace InventorySystem
{
    [CreateAssetMenu(menuName = "Inventory/Item Definition", fileName = "New Item Definition")]
    public class ItemDefinition: ScriptableObject
    {
        [SerializeField]
        private string _name;
        [SerializeField]
        private bool _isStackable;
        [SerializeField]
        private int _maxStackSize;

        [SerializeField]
        private Sprite _inGameSprite;

        [SerializeField]
        private Sprite _uiSprite;

        public string Name => _name;
        public bool IsStackable => _isStackable;
        public int MaxStackSize => _maxStackSize;

        public Sprite InGameSprite => _inGameSprite;

        public Sprite UISprite => _uiSprite;


    }
}

