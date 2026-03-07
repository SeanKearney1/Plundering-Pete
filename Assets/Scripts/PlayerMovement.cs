using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public float jumpHeight;


    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Vector2 capsule_collider_size;
    private Animator animator;
    private bool can_jump = true;

    private bool isDucking = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponentInChildren<CapsuleCollider2D>();
        animator = GetComponentInChildren<Animator>();
        capsule_collider_size = capsuleCollider.size;
    }

    void Update()
    {
        
    }



    void FixedUpdate()
    {
        // Gets Player Input.
        float moveX = 0;
        float moveY = rb.linearVelocity.y;

        if (Input.GetKey(KeyCode.LeftArrow))  { moveX -= 1;}
        if (Input.GetKey(KeyCode.RightArrow)) { moveX += 1;}


        // Checks to see if player can jump again.
        if (rb.linearVelocity.y == 0) { can_jump = true; }


        // Jump logic.
        if (Input.GetKey(KeyCode.UpArrow) && can_jump) { 
            moveY += jumpHeight; 
            can_jump = false;
        }




        // Duck Logic.
        if (Input.GetKey(KeyCode.DownArrow)) { 
            capsuleCollider.size = new Vector2(capsule_collider_size.x,capsule_collider_size.x);
            isDucking = true;
            moveX = 0;
        }
        else { 
            capsuleCollider.size = capsule_collider_size;
            isDucking = false; 
        }

        // Animation Stuff...
        if (moveX != 0) { animator.SetBool("IsWalking",true); }
        else            { animator.SetBool("IsWalking",false); }

        if (isDucking) { animator.SetBool("IsDucking",true); }
        else           { animator.SetBool("IsDucking",false); }

        if (rb.linearVelocity.y != 0) { animator.SetBool("IsJumping",true); }
        else                          { animator.SetBool("IsJumping",false); }
        

        flipSprite(moveX);
        rb.linearVelocity = new Vector2(moveX*moveSpeed,moveY);
    }










    private void flipSprite(float moveX)
    {
        if (moveX < 0.0)      { transform.eulerAngles = new Vector3(0, 0, 0); }
        else if (moveX > 0.0) { transform.eulerAngles = new Vector3(0,180,0); }
    }





    public bool getIsDucking() { return isDucking; }



}
