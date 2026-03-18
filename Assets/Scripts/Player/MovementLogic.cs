using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;



public class MovementLogic : MonoBehaviour
{
    private bool IsUsingMovmentCombo;
    private bool IsUsingCutlass;
    private bool IsUsingPistol;
    private bool IsUsingGrenade;
    private bool IsUsingGrapple;
    private bool IsDucking;
    private bool IsJumping;
    private bool IsTryingToJump;
    private bool IsFalling;
    private bool IsWalking;
    private bool ComboDisableInputMovement = false;
    private bool can_jump = true;
    private Vector2 Look = new Vector2(0,0);

    private float ComboBeginTimeStamp = 0;
    private float ComboDelay = 0.075f;
    private float ComboCooldown = 0.0f;
    private float ComboMovementCooldown = 0.0f;
    private int comboIndex = 0;


    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer SecondaryWeapon;
    private CapsuleCollider2D hitBox;
    private CapsuleCollider2D playerCollision;
    private Vector2 hitboxColliderSize;
    [SerializeField] BoxCollider2D PlatformCollision;
    private GameObject CurCollidedLadder;
    private GameObject LadderBeingUsed;
    private bool SnappedPlayerToLadder = false;

    public float moveSpeed;
    public float jumpHeight;
    public Sprite PistolSprite;
    public Sprite GrenadeSprite;
    public GameObject Bullet;
    public GameObject Grenade;

    void Start()
    {
        // Fetching Components.
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        hitBox = transform.Find("HitBox").GetComponent<CapsuleCollider2D>();
        playerCollision = transform.Find("CollisionBox").GetComponent<CapsuleCollider2D>();
        SecondaryWeapon = transform.Find("PlayerSprite/Torso1/Torso2/Torso3/ArmRight1/ArmRight2/HandRight/SecondaryWeaponSprite").GetComponent<SpriteRenderer>();

        // Saving the OG hitbox collider size
        hitboxColliderSize = hitBox.size;

        Bullet.SetActive(false);
    }


    void Update()
    {
        JumpValidator();

        if (rb.linearVelocityY != 0) { IsFalling = true; }
        else { IsFalling = false; }
        

        if (!(IsUsingCutlass || IsUsingPistol || IsUsingGrenade || IsUsingGrapple || IsWalking || IsJumping || IsFalling || IsDucking))
        {
            UpdateComboBeginTimeStamp();
        }

        UpdateAnimator();

        Countdowns();


        if (ValidateNextCombo()) { ComboStartLogic(); }




        if (comboIndex > 3000 && comboIndex < 4000) { SecondaryWeapon.sprite = PistolSprite; }
        else if (comboIndex > 4000 && comboIndex < 5000) { SecondaryWeapon.sprite = GrenadeSprite; }



    }

    void FixedUpdate()
    {
        if (!ComboDisableInputMovement)
        {
            if (LadderBeingUsed.IsUnityNull()) {
                MoveLogic();
                if (!CurCollidedLadder.IsUnityNull()) { StartCoroutine(OneWayPlatformLogic()); }
            }
            else { LadderLogic(); }
        }
        
    }





//////////////////////////////////////////////////////////////////////////////////////////////
//  M  O  V  E  M  E  N  T
//////////////////////////////////////////////////////////////////////////////////////////////

    public void UpdateComboBeginTimeStamp() { ComboBeginTimeStamp = Time.time; }

    // Get inputs:
    public void Set_IsUsingMovmentCombo(bool var) { IsUsingMovmentCombo = var; }
    public void Set_IsUsingCutlass(bool var) { IsUsingCutlass = var; }
    public void Set_IsUsingPistol(bool var) { IsUsingPistol = var; }
    public void Set_IsUsingGrenade(bool var) { IsUsingGrenade = var; }
    public void Set_IsUsingGrapple(bool var) { IsUsingGrapple = var; }
    public void Set_IsDucking(bool var) { IsDucking = var; }
    public void Set_IsWalking(bool var) { IsWalking = var; }
    public void Set_IsTryingToJump(bool var) { IsTryingToJump = var; }

    public void Set_Look(Vector2 new_look) { Look = new_look; }







    private void UpdateAnimator()
    {
        if (ComboCooldown != 0) 
        {  
            IsJumping = false;
            can_jump = false;
        }

        animator.SetBool("IsWalking",IsWalking);
        animator.SetBool("IsDucking",IsDucking);
        animator.SetBool("IsJumping",IsFalling);
        animator.SetInteger("ComboIdx",comboIndex);
    }



    private void MoveLogic()
    {
        // Gets Player Input.
        float moveX = Look.x;
        float moveY = rb.linearVelocity.y;


        // Duck Logic.
        if (IsDucking) { 
            hitBox.size = new Vector2(hitboxColliderSize.x,hitboxColliderSize.x);
            hitBox.transform.localPosition = new Vector3(0.0f,-0.475f,0.0f);
            moveX = 0;
        }
        else { 
            hitBox.size = hitboxColliderSize;
            hitBox.transform.localPosition = new Vector3(0.0f,0.15f,0.0f); 
        }


        flipSprite();
        rb.linearVelocity = new Vector2(moveX*moveSpeed,moveY);
    }


    private void flipSprite()
    {
        if (Look.x < 0.0)      { transform.eulerAngles = new Vector3(0, 0, 0); }
        else if (Look.x > 0.0) { transform.eulerAngles = new Vector3(0,180,0); }
    }





////////////////////////////////////////////////////////////////////////////////////////
//  L  A  D  D  E  R      L  O  G  I  C
////////////////////////////////////////////////////////////////////////////////////////

    public GameObject GetCurrentCollidedLadder() { return CurCollidedLadder; }


    private void LadderLogic()
    {
        if (!SnappedPlayerToLadder)
        {
            //transform.parent = LadderBeingUsed.transform;  
            SnappedPlayerToLadder = true;
            rb.gravityScale = 0;
        }
        LadderMovement();
    }


    private void LadderMovement()
    {
        rb.linearVelocity = new Vector2(Look.x*moveSpeed,Look.y*moveSpeed);

        flipSprite();

        if (!rb.IsTouching(LadderBeingUsed.GetComponent<BoxCollider2D>())) { ExitLadder(); }
    }

    private void ExitLadder()
    {
        //transform.parent = null;
        SnappedPlayerToLadder = false;
        LadderBeingUsed = null;
        rb.gravityScale = 1;
    }






    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ShipTrigger_Ladder")) { CurCollidedLadder = collision.gameObject; }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ShipTrigger_Ladder")) { CurCollidedLadder = null; }
    }

    private IEnumerator OneWayPlatformLogic()
    {
        if (IsDucking && transform.position.y > CurCollidedLadder.transform.position.y)
        {
            PlatformCollision = CurCollidedLadder.GetComponent<BoxCollider2D>();
            Physics2D.IgnoreCollision(playerCollision,PlatformCollision);
            yield return new WaitForSeconds(0.25f);
            Physics2D.IgnoreCollision(playerCollision,PlatformCollision,false);
        }
    }


//////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    private void JumpValidator()
    {
        if (rb.linearVelocity.y == 0) { can_jump = true; }
        if (IsTryingToJump && can_jump) 
        { 
            can_jump = false;
            IsJumping = true; 
        }
    }


    private void Countdowns()
    {
        if (ComboCooldown > 0) { ComboCooldown -= Time.deltaTime; }
        if (ComboMovementCooldown > 0) { ComboMovementCooldown -= Time.deltaTime; }

        if (ComboCooldown == 0) { comboIndex = 0; }

        if (ComboCooldown < 0) { ComboCooldown = 0; }
        if (ComboMovementCooldown < 0) { ComboMovementCooldown = 0; }


        if (ComboMovementCooldown == 0) { ComboDisableInputMovement = false; }
    }


    private bool ValidateNextCombo()
    {

        if (ComboBeginTimeStamp + ComboDelay > Time.time) { return false; }
        if (ComboCooldown != 0) { return false; }

        return true;
    }







/*
	Combos:
		(Down + Left or Right)	- (Somersault.)
		(Up + Left or Right)	- (Jump flip.)
		(A) Big Slash           - (AOE attack.)
		(A + Left or Right)	- (Slash.)
		(A + Up)		- (Uppercut.)
		(A + Down)		- (AOE Sweep the leg.)
		(A + Up + Left or Right)- (Throw.)
		(Grapple into an enemy) - (Some sort of attack.)


        COMBO LOGIC INDEXES:

        Movement: 1,000 - 1,999
        Cutlass:  2,000 - 2,999
        Pistol:   3,000 - 3,999
        Grenade:  4,000 - 4,999
        Grapple:  5,000 - 5,999
    */




////////////////////////////////////////////////////////////////////////////////////////////
// C O M B O   S T A R T   F U N C T I O N S
////////////////////////////////////////////////////////////////////////////////////////////


    private void ComboStartLogic()
    {
        if (!(IsUsingCutlass || IsUsingPistol || IsUsingGrenade || IsUsingGrapple) && (IsDucking || IsJumping || IsFalling || IsWalking)) { ComboStartMovement(); }
        else if (IsUsingCutlass) { ComboStartCutlass(); }
        else if (IsUsingPistol)  { ComboStartPistol();  }
        else if (IsUsingGrenade) { ComboStartGrenade(); }
        else if (IsUsingGrapple) { ComboStartGrapple(); }
    }


    private void ComboStartMovement()
    {
        if (IsDucking && IsWalking) // IsUsingMovmentCombo &&
        { 
            comboIndex = 1001;
            ComboCooldown = 1.25f;
            ComboMovementCooldown = 1.05f;
            ComboDisableInputMovement = true;
            Debug.Log("Somersault");
        }
        else if (IsJumping && IsWalking) // IsUsingMovmentCombo &&
        { 
            comboIndex = 1002;
            ComboCooldown = 1.35f;
            ComboMovementCooldown = 1.15f;
            ComboDisableInputMovement = true;
            Debug.Log("Jump flip"); 
        }
        else if (IsJumping && !IsDucking && CurCollidedLadder.IsUnityNull())
        {
            comboIndex = 1003;
            ComboCooldown = 0.5f;
            ComboMovementCooldown = 0f;
            ComboDisableInputMovement = false;
            rb.linearVelocityY += jumpHeight;
            Debug.Log("Jump"); 
        }
        else if (IsJumping && !IsDucking && !CurCollidedLadder.IsUnityNull())
        {
            LadderBeingUsed = CurCollidedLadder;
            Debug.Log("Grabbed Ladder"); 
        }
    }






    private void ComboStartCutlass()
    {
        if (!IsWalking && !(IsFalling || IsJumping) && !IsDucking) 
        { 
            comboIndex = 2001;
            ComboCooldown = 1.15f;
            ComboMovementCooldown = 1.15f;
            ComboDisableInputMovement = true;
            AttackCutlass();
            Debug.Log("Big Slash"); 
        }
        else if (IsWalking && !IsJumping) 
        {
            comboIndex = 2002;
            ComboCooldown = 1f;
            ComboMovementCooldown = 0f;
            AttackCutlass();
            Debug.Log("Slash");
        }
        else if ((IsJumping || IsFalling) && !IsWalking && !IsDucking) 
        { 
            comboIndex = 2003;
            ComboCooldown = 0.85f; 
            ComboMovementCooldown = 0.85f;
            ComboDisableInputMovement = true;
            AttackCutlass();
            Debug.Log("Uppercut"); 
        }
        else if (IsDucking && !IsWalking && CurCollidedLadder.IsUnityNull()) 
        { 
            comboIndex = 2004;
            ComboCooldown = 0.75f; 
            ComboMovementCooldown = 0.75f;
            ComboDisableInputMovement = true;
            AttackCutlass();
            Debug.Log("Block"); 
        }
        else if (IsJumping && IsWalking) 
        { 
            comboIndex = 2005;
            ComboCooldown = 1.6f; 
            ComboMovementCooldown = 1.6f;
            ComboDisableInputMovement = true;
            AttackCutlass();
            Debug.Log("Throw"); 
        }
    }

    private void ComboStartPistol()
    {
        

        if (!(IsFalling || IsJumping))
        {
            comboIndex = 3001;
            ComboCooldown = 3.25f; 
            ComboMovementCooldown = 3.25f;
            ComboDisableInputMovement = true;
            Debug.Log("Pistol"); 
        }
    }

    private void ComboStartGrenade()
    {
        if (!IsDucking) {
            comboIndex = 4001;
            ComboCooldown = 2.4f; 
            ComboMovementCooldown = 2.4f;
            ComboDisableInputMovement = true;
            Debug.Log("Grenade!!!"); 
        }
        else if (IsDucking) {
            comboIndex = 4002;
            ComboCooldown = 1f; 
            ComboMovementCooldown = 1f;
            ComboDisableInputMovement = true;
            Debug.Log("Grenade Plant!!!"); 
        }
    }

    private void ComboStartGrapple()
    {
        comboIndex = 5000;
    }






////////////////////////////////////////////////////////////////////////////////////////////
// C O M B O   D U R A T I O N   F U N C T I O N S
////////////////////////////////////////////////////////////////////////////////////////////


    private void AnimEvent_JumpFlip()
    {
        rb.linearVelocityY = jumpHeight*1.25f;
    }

    private void AnimEvent_Uppercut()
    {
        rb.linearVelocityY += jumpHeight*1.5f;
    }






////////////////////////////////////////////////////////////////////////
// W E A P O N   L O G I C
////////////////////////////////////////////////////////////////////////

    private void AttackCutlass()
    {
        
    }

    private void AttackPistol()
    {
        GameObject new_bullet;
        Vector2 direction;
        Vector3 bullet_pos = SecondaryWeapon.transform.Find("Muzzle").position;

        if (transform.eulerAngles.y == 0) { direction = new Vector2(-1,0); }
        else { direction = new Vector2(1,0); }
        
        new_bullet = Instantiate(Bullet, bullet_pos, Quaternion.identity);
        new_bullet.GetComponent<Gunshot>().Direction = direction;
        if (direction.x > 0) { new_bullet.GetComponent<SpriteRenderer>().flipX = true; }
        new_bullet.SetActive(true);
    }

    private void AttackGrenade()
    {
        GameObject new_grenade;
        Vector3 grenade_pos = SecondaryWeapon.transform.position;
        Vector2 grenade_velocity;

        if (transform.eulerAngles.y == 0) { grenade_velocity = new Vector2(-1,1); }
        else { grenade_velocity = new Vector2(1,1); }

        new_grenade = Instantiate(Grenade, grenade_pos, Quaternion.identity);
        new_grenade.GetComponent<Grenade>().Velocity = grenade_velocity;
        new_grenade.SetActive(true);
    }

    private void AttackGrapple()
    {
        
    }







////////////////////////////////////////////////////////////////////////
// D E B U G
////////////////////////////////////////////////////////////////////////

    private void debug__PrintAnimStats(string name)
    {
        Debug.Log(name+", comboIndex: "+comboIndex+", ComboCooldown: "+ComboCooldown);
    }


}
