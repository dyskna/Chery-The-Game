using UnityEngine;
using System.Collections;

public interface IInteractable
{
    void Interact();
    bool CanInteract();
}
public class Harvesting : MonoBehaviour, IInteractable
{
    private Animator _animator;
    private bool _isHarvested = false;
    private float currentCooldown = 0f;
    private Vector2 position;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        position = transform.position;
    }

    public bool CanInteract()
    {
        return !_isHarvested;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        _isHarvested = true;
        _animator.SetBool("Harvested", true);

    }
    
    public void ResetState()
    {
        StopAllCoroutines();
        _isHarvested = false;
        currentCooldown = 0;
        if(_animator != null) _animator.SetBool("Harvested", false);
    }

}