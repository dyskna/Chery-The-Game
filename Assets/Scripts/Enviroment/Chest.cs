using UnityEngine;
using System.Collections;

public class Chest : MonoBehaviour, IInteractable
{
    private Animator _animator;
    private bool _isOpened = false;
    private Vector2 position;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        position = transform.position;
    }

    public bool CanInteract()
    {
        return !_isOpened;
    }

    public void Interact()
    {
        if (!CanInteract()) return;

        _isOpened = true;
        _animator.SetBool("IsOpen", true);
    }
    
    public void ResetState()
    {
        StopAllCoroutines();
        _isOpened = false;
        if(_animator != null) _animator.SetBool("IsOpen", false);
    }

}