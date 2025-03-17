using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
   public float moveSpeed = 5f;
   private bool canMove = true;

   public Rigidbody2D rb;
   public Animator animator;

   Vector2 movement;

    // Update is called once per frame
    void Update()
    {
        //Input
        if (DialogueManager.GetInstance().dialogueIsPlaying || !canMove)
        {
            return;
        }


        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");

        animator.SetFloat("Horizontal", movement.x);
        animator.SetFloat("Vertical", movement.y);
        animator.SetFloat("Speed", movement.sqrMagnitude);
    }

    void FixedUpdate()
    {
        if (canMove) 
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    public void DisableMovement()
    {
        canMove = false;
        movement = Vector2.zero; // Stop movement immediately
        animator.SetFloat("Speed", 0); // Set animation to idle
    }

    public void EnableMovement()
    {
        canMove = true;
    }
}
