using System;

namespace InventorySystem
{
    public enum InventoryOperation
    {
        Add,
        Remove
    }

    public class InventoryException : Exception
    {
        public InventoryOperation Operation { get; }
        public InventoryException(InventoryOperation operation, string message) : base($"{operation} Error: {message}")
        {
            Operation = operation;
        }
    }
}
