using Unity.VisualScripting;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed;
    public float jumpHeight;
    private Rigidbody2D rb;
    private CapsuleCollider2D hitBox;
    private Vector2 capsule_collider_size;
    private Animator animator;
    private PlayerAttack playerAttackScript;
    private bool can_jump = true;

    private bool isDucking = false;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        hitBox = transform.Find("HitBox").GetComponent<CapsuleCollider2D>();
        animator = GetComponent<Animator>();
        playerAttackScript = GetComponent<PlayerAttack>();
        capsule_collider_size = hitBox.size;
    }

    void Update()
    {
        // Animation Stuff...
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow)) { animator.SetBool("IsWalking",true); }
        else            { animator.SetBool("IsWalking",false); }

        if (isDucking) { animator.SetBool("IsDucking",true); }
        else           { animator.SetBool("IsDucking",false); }

        if (rb.linearVelocity.y != 0) { animator.SetBool("IsJumping",true); }
        else                          { animator.SetBool("IsJumping",false); }
    }

    void FixedUpdate()
    {
        if (!playerAttackScript.GetAllowMovement())
        {
            MoveLogic();
        }
        
    }




    private void MoveLogic()
    {
        // Gets Player Input.
        float LookX = 0;
        float moveX = 0;
        float moveY = rb.linearVelocity.y;

        if (Input.GetKey(KeyCode.LeftArrow))  { LookX -= 1;}
        if (Input.GetKey(KeyCode.RightArrow)) { LookX += 1;}

        moveX = LookX;

        // Duck Logic.
        if (Input.GetKey(KeyCode.DownArrow)) { 
            hitBox.size = new Vector2(capsule_collider_size.x,capsule_collider_size.x);
            hitBox.transform.localPosition = new Vector3(0.0f,-0.475f,0.0f);
            isDucking = true;
            moveX = 0;
        }
        else { 
            hitBox.size = capsule_collider_size;
            hitBox.transform.localPosition = new Vector3(0.0f,0.15f,0.0f);
            isDucking = false; 
        }


        flipSprite(LookX);
        rb.linearVelocity = new Vector2(moveX*moveSpeed,moveY);
    }









    private void flipSprite(float moveX)
    {
        if (moveX < 0.0)      { 
            transform.eulerAngles = new Vector3(0, 0, 0); 
            Debug.Log("Looking Left!!!");
            }
        else if (moveX > 0.0) { 
            transform.eulerAngles = new Vector3(0,180,0); 
            Debug.Log("Looking Right!!!");
            }
    }





    public bool getIsDucking() { return isDucking; }




    /*private bool ValidateJump()
    {


        // Uppercut
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.A)) { return true; }
        // Throw
        if (Input.GetKey(KeyCode.UpArrow) && (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))) { return true; }

        return false;
    }*/

}
