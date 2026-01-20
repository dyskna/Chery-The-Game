using UnityEngine;
using UnityEngine.Tilemaps;

public class ArenaSizeChecker : MonoBehaviour
{
    public Tilemap targetTilemap; // Przypisz np. warstwę "Borders" lub "Tilemap" (podłoga)

    void Start()
    {
        // Pobiera granice w koordynatach komórek (liczbę kafelków)
        BoundsInt bounds = targetTilemap.cellBounds;
        
        // Oblicza rzeczywisty rozmiar w jednostkach Unity (Unity Units)
        Vector3 sizeInUnits = targetTilemap.localBounds.size;

        Debug.Log($"Rozmiar w kafelkach: {bounds.size.x} x {bounds.size.y}");
        Debug.Log($"Rozmiar w jednostkach Unity: {sizeInUnits.x} x {sizeInUnits.y}");
        
        // Aby uzyskać środek areny:
        Debug.Log($"Środek areny: {targetTilemap.localBounds.center}");
    }
}