using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



public class MovementLogic : MonoBehaviour
{
    private bool ScriptEnabled = true;

    private bool IsUsingMovmentCombo;
    private bool IsUsingCutlass;
    private bool IsUsingPistol;
    private bool IsUsingGrenade;
    private bool IsUsingGrapple;
    private bool WasUsingGrapple = false;
    private bool IsCurrentlyGrappling = false;
    private Vector2 GrapplePoint;
    private bool IsDucking;
    private bool IsJumping;
    private bool IsTryingToJump;
    private bool IsFalling;
    private bool IsWalking;
    private bool IsHurting = false;
    private List<HealthManager> MeleeVictims = new List<HealthManager>();
    private int CauseOfDamage;
    private float DamageCooldown = 0;
    private bool ComboDisableInputMovement = false;
    private bool can_jump = true;
    private Vector2 Look = new Vector2(0,0);

    private float ComboBeginTimeStamp = 0;
    private float ComboDelay = 0.075f;
    private float ComboCooldown = 0.0f;
    private float ComboMovementCooldown = 0.0f;
    private int comboIndex = 0;


    private GameObject CurOceanLadder;
    private bool OnOceanLadder = false;
    private bool InitialedOceanLadderProcess = true;
    private List<Vector2> OceanLadderCheckoints;
    private float OceanLadderMovementSpeed = 5;


    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer SecondaryWeapon;
    private CapsuleCollider2D hitBox;
    private CapsuleCollider2D playerCollision;
    private Vector2 hitboxColliderSize;
    private GameObject CurCollidedLadder;
    private GameObject LadderBeingUsed;
    private bool SnappedPlayerToLadder = false;
    private GameObject Poseidon;
    private GameObject healthBarMask;
    private GameObject healthBarBackground;
    private MeleeLogic meleeLogic;
    private GrappleBeamLogic grappleBeamLogic;

    private Transform OGParent;

    public float moveSpeed;
    public float jumpHeight;
    public Sprite PistolSprite;
    public Sprite GrenadeSprite;
    public Sprite GrappleSprite;
    public GameObject Bullet;
    public GameObject Grenade;


    void Start()
    {
        // Fetching Components.
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        healthBarMask = transform.Find("HealthBar").gameObject;
        healthBarBackground = transform.Find("HealthBarBackground").gameObject;
        hitBox = transform.Find("HitBox").GetComponent<CapsuleCollider2D>();
        playerCollision = transform.Find("CollisionBox").GetComponent<CapsuleCollider2D>();
        SecondaryWeapon = transform.Find("PlayerSprite/Torso1/Torso2/Torso3/ArmRight1/ArmRight2/HandRight/SecondaryWeaponSprite").GetComponent<SpriteRenderer>();


        if (transform.Find("GrappleBeam"))
        {
            grappleBeamLogic = transform.Find("GrappleBeam").GetComponent<GrappleBeamLogic>();
        }


        // Saving the OG hitbox collider size
        hitboxColliderSize = hitBox.size;

        Bullet.SetActive(false);
    }


    void Update()
    {
        if (ScriptEnabled) {
            if (!(OnOceanLadder || IsCurrentlyGrappling)) { NormalMovementLogic(); }
            else if (OnOceanLadder) { OceanLadderLogic(); }
            else if (IsCurrentlyGrappling) { GrappleMovement(); }
        }
    }

    void FixedUpdate()
    { 
        if (ScriptEnabled && !(ComboDisableInputMovement || OnOceanLadder || IsCurrentlyGrappling))
        {
            if (LadderBeingUsed.IsUnityNull()) {
                MoveLogic();
                if (!CurCollidedLadder.IsUnityNull()) { StartCoroutine(OneWayPlatformLogic()); }
            }
            else { LadderLogic(); }
        }
    }

























    private void NormalMovementLogic()
    {
        if (LadderBeingUsed.IsUnityNull()) { JumpValidator(); }
        else { IsJumping = false; }

        if (Math.Abs(rb.linearVelocityY) > 1) { IsFalling = true; }
        else { IsFalling = false; }
        


        if (!(IsUsingCutlass || IsUsingPistol || IsUsingGrenade || IsUsingGrapple || WasUsingGrapple || IsWalking || IsJumping || IsFalling || IsDucking))
        {
            UpdateComboBeginTimeStamp();
        }

        if ((comboIndex > 9000 && comboIndex < 9999) || DamageCooldown != 0) { animator.SetBool("IsInHurt",true); }
        else { animator.SetBool("IsInHurt",false); }

        if (ValidateNextCombo()) { ComboStartLogic(); }

        GrappleBeamLogic();

        UpdateAnimator();


        if (comboIndex > 3000 && comboIndex < 4000) { SecondaryWeapon.sprite = PistolSprite; }
        else if (comboIndex > 4000 && comboIndex < 5000) { SecondaryWeapon.sprite = GrenadeSprite; }
        else if (comboIndex > 5000 && comboIndex < 6000) { SecondaryWeapon.sprite = GrappleSprite; }

        Countdowns();
    }



//////////////////////////////////////////////////////////////////////////////////////////////
//  G E T T E R S   A N D   S E T T E R S
//////////////////////////////////////////////////////////////////////////////////////////////

    public void Enable(bool enabler) { ScriptEnabled = enabler; }

    public void UpdateComboBeginTimeStamp() { ComboBeginTimeStamp = Time.time; }

    public void SetPoseidon(GameObject new_gamemanager) { Poseidon = new_gamemanager; }
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

    public void SetComboDelay(float new_delay) { ComboDelay = new_delay; }

    public void SetMeleeScript(MeleeLogic new_melee_script) { meleeLogic = new_melee_script; }

    public void SetDamageState(int damage_type)
    {
        Debug.Log("Took Damage!!!!!");
        CauseOfDamage = damage_type;
        IsHurting = true;
    }



    public float Get_ComboCooldown() { return ComboCooldown; }
    public float Get_ComboMovementCooldown() { return ComboMovementCooldown; }

    public bool Get_OnOceanLadder() { return OnOceanLadder; }


    public bool LookingLeft()
    {
        if (transform.eulerAngles.y == 0) { return true; }
        else { return false; }
    }

    public void Death()
    {
        Look = new Vector2(0,0);
        rb.linearVelocityX = 0;
    }













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

        if (Duck(false, false)) { moveX = 0; }

        flipSprite(Look.x);
        rb.linearVelocity = new Vector2(moveX*moveSpeed,moveY);
    }


    public void flipSprite(float the_look)
    {
        if (the_look < 0.0)
        { 
            transform.eulerAngles = new Vector3(0, 0, 0);
            healthBarMask.transform.localPosition = new Vector3(-0.75f,healthBarMask.transform.localPosition.y, healthBarMask.transform.localPosition.z);
            healthBarMask.transform.localScale = new Vector3(0.5f, healthBarMask.transform.localScale.y, healthBarMask.transform.localScale.z);
            healthBarBackground.transform.localScale = new Vector3(-0.5f, healthBarBackground.transform.localScale.y, healthBarBackground.transform.localScale.z);
        }
        else if (the_look > 0.0)
        {
            transform.eulerAngles = new Vector3(0,180,0);
            healthBarMask.transform.localPosition = new Vector3(0.75f,healthBarMask.transform.localPosition.y, healthBarMask.transform.localPosition.z);
            healthBarMask.transform.localScale = new Vector3(-0.5f, healthBarMask.transform.localScale.y, healthBarMask.transform.localScale.z);
            healthBarBackground.transform.localScale = new Vector3(-0.5f, healthBarBackground.transform.localScale.y, healthBarBackground.transform.localScale.z);
        }
    }




    private bool Duck(bool ForceDuck, bool DuckState)
    {

        if ((!ForceDuck && IsDucking) || (ForceDuck && DuckState)) { 
            hitBox.size = new Vector2(hitboxColliderSize.x,hitboxColliderSize.x);
            hitBox.transform.localPosition = new Vector3(0.0f,-0.475f,0.0f);
            return true;
        }
        hitBox.size = hitboxColliderSize;
        hitBox.transform.localPosition = new Vector3(0.0f,0.15f,0.0f); 
        return false;
    }




////////////////////////////////////////////////////////////////////////////////////////
//  L  A  D  D  E  R      L  O  G  I  C
////////////////////////////////////////////////////////////////////////////////////////

    public GameObject IsOnLadder() { return LadderBeingUsed; }


    private void LadderLogic()
    {
        if (!SnappedPlayerToLadder)
        {
            SnappedPlayerToLadder = true;
            rb.gravityScale = 0;
        }
        LadderMovement();
    }


    private void LadderMovement()
    {
        rb.linearVelocity = new Vector2(Look.x*moveSpeed,Look.y*moveSpeed);

        flipSprite(Look.x);

        if (!rb.IsTouching(LadderBeingUsed.GetComponent<BoxCollider2D>())) { ExitLadder(); }
    }

    private void ExitLadder()
    {
        SnappedPlayerToLadder = false;
        LadderBeingUsed = null;
        rb.gravityScale = 1;
    }






    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ShipTrigger_Ladder"))
        { 
            CurCollidedLadder = collision.gameObject;
            if (!LadderBeingUsed.IsUnityNull()) { LadderBeingUsed = CurCollidedLadder; }
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ShipTrigger_Ladder")) { CurCollidedLadder = null; }
    }




    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ShipTrigger_LadderOceanBottom"))
        {
            SetOceanLadder(collision.transform.parent.gameObject);
        }
    }





    private IEnumerator OneWayPlatformLogic()
    {
        if (IsDucking && transform.position.y > CurCollidedLadder.transform.position.y)
        {
            BoxCollider2D PlatformCollision = CurCollidedLadder.GetComponent<BoxCollider2D>();
            Physics2D.IgnoreCollision(playerCollision,PlatformCollision);
            yield return new WaitForSeconds(0.5f);
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
        ComboCooldown = CountdownLogic(ComboCooldown);
        ComboMovementCooldown = CountdownLogic(ComboMovementCooldown);
        DamageCooldown = CountdownLogic(DamageCooldown);

        if (ComboCooldown == 0) { comboIndex = 0; }
        if (ComboMovementCooldown == 0) { ComboDisableInputMovement = false; }
    }

    private float CountdownLogic(float timer)
    {
        if (timer > 0) { timer -= Time.deltaTime; }
        if (timer < 0) { timer = 0;}
        return timer;
    }


    private bool ValidateNextCombo()
    {
        if (IsHurting && !(comboIndex > 9000 && comboIndex < 9999)) { return true; }
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

        if (IsHurting) { ComboStartDamaged(); }
        else if (IsUsingCutlass) { ComboStartCutlass(); }
        else if (IsUsingPistol)  { ComboStartPistol();  }
        else if (IsUsingGrenade) { ComboStartGrenade(); }
        else if (IsUsingGrapple || WasUsingGrapple) { ComboStartGrapple(); }
        else if (IsDucking || IsJumping || IsFalling || IsWalking) { ComboStartMovement(); }
    }


    private void ComboStartMovement()
    {
        if (IsDucking && IsWalking) // IsUsingMovmentCombo &&
        { 
            comboIndex = 1001;
            ComboCooldown = 1.25f;
            ComboMovementCooldown = 1.05f;
            ComboDisableInputMovement = true;
            Duck(true, true);
            Debug.Log("Somersault");
        }
        else if (IsJumping && IsWalking) // IsUsingMovmentCombo &&
        { 
            comboIndex = 1002;
            ComboCooldown = 1.35f;
            ComboMovementCooldown = 1.15f;
            ComboDisableInputMovement = true;
            Duck(true, true);
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
            ComboCooldown = 1f;
            ComboMovementCooldown = 1f;
            //ComboCooldown = 1.15f;
            //ComboMovementCooldown = 1.15f;
            ComboDisableInputMovement = true;
            Debug.Log("Big Slash"); 
        }
        else if (IsWalking && !IsJumping) 
        {
            comboIndex = 2002;
            ComboCooldown = 1f;
            ComboMovementCooldown = 0f;
            Debug.Log("Slash");
        }
        else if ((IsJumping || IsFalling) && !IsWalking && !IsDucking) 
        { 
            comboIndex = 2003;
            ComboCooldown = 0.85f; 
            ComboMovementCooldown = 0.85f;
            ComboDisableInputMovement = true;
            Debug.Log("Uppercut"); 
        }
        else if (IsDucking && !IsWalking && CurCollidedLadder.IsUnityNull()) 
        { 
            comboIndex = 2004;
            ComboCooldown = 0.75f; 
            ComboMovementCooldown = 0.75f;
            ComboDisableInputMovement = true;
            Debug.Log("Block"); 
        }
        else if (IsJumping && IsWalking) 
        { 
            comboIndex = 2005;
            ComboCooldown = 1.6f; 
            ComboMovementCooldown = 1.6f;
            ComboDisableInputMovement = true;
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

        if (IsUsingGrapple) {
            if (!IsDucking && !IsWalking && !IsJumping) // Grapple Idle
            {
                comboIndex = 5001;
            }
            else if (!IsDucking && IsWalking && !IsJumping) // Grapple Walk
            {
                comboIndex = 5002;
            }
            else if (IsDucking)  // Grapple Crouch
            {
                comboIndex = 5003;
            }
            else if (IsJumping)  // Grapple Jump
            {
                comboIndex = 5004;
            }
        }
        else //Grapple
        {
            rb.gravityScale = 0f;
            IsCurrentlyGrappling = true;
            animator.animatePhysics = false;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (IsUsingGrapple) { WasUsingGrapple = true; }
        else { WasUsingGrapple = false; }

    }



    private void ComboStartDamaged()
    {
        // Stumble
        if (CauseOfDamage == 0) // Stumble Damage From Back
        {
            comboIndex = 9001;
            ComboCooldown = 0.5f; 
            ComboMovementCooldown = 0.5f;
            ComboDisableInputMovement = true;
        }
        else if (CauseOfDamage == 1) // Stumble Damage From Front
        {
            comboIndex = 9002;
            ComboCooldown = 0.5f; 
            ComboMovementCooldown = 0.5f;
            ComboDisableInputMovement = true;
        }


        // Fall
        else if (CauseOfDamage == 2) // Fall Damage From Back
        {
            comboIndex = 9003;
            ComboCooldown = 2f; 
            ComboMovementCooldown = 2.25f;
            ComboDisableInputMovement = true;
        }
        else if (CauseOfDamage == 3) // Fall Damage From Front
        {
            comboIndex = 9004;
            ComboCooldown = 2f; 
            ComboMovementCooldown = 2.25f;
            ComboDisableInputMovement = true;
        }

        // Die
        else if (CauseOfDamage == 4) // Die Damage From Back
        {
            comboIndex = 9005;
            ComboCooldown =1f; 
            ComboMovementCooldown = 1f;
            ComboDisableInputMovement = true;
        }
        else if (CauseOfDamage == 5) // Die Damage From Front
        {
            comboIndex = 9006;
            ComboCooldown = 1f; 
            ComboMovementCooldown = 1f;
            ComboDisableInputMovement = true;
        }
        DamageCooldown = ComboCooldown + GeneralGameInfo.Const_DamageCooldown;
        CauseOfDamage = -1;
        IsHurting = false;
        DisableMelee();
    }



////////////////////////////////////////////////////////////////////////////////////////////
//  G R A P P L E   F U N C T I O N S
////////////////////////////////////////////////////////////////////////////////////////////



    private void GrappleBeamLogic()
    {
        List<Transform> grapple_points = Poseidon.GetComponent<GameManager>().GetGrapplePoints(transform.position);


        int cur_index = -1;
        float min_distance = 99999;

        for (int i = 0; i < grapple_points.Count; i++)
        {
            if ((transform.position - grapple_points[i].position).magnitude < min_distance)
            {
                min_distance = (transform.position - grapple_points[i].position).magnitude;
                cur_index = i;
            }
        }
        if (cur_index > -1) { GrapplePoint = grapple_points[cur_index].position; }
        else { GrapplePoint = new Vector2(); }


        if (cur_index < grapple_points.Count && !grappleBeamLogic.IsUnityNull())
        {
            grappleBeamLogic.SetEndPoint(GrapplePoint);
        }

        if (!grappleBeamLogic.IsUnityNull()) { GrappleBeamSwitch(); }
    }







    private void GrappleBeamSwitch()
    {
        if (comboIndex > 5000 && comboIndex <= 5004)
        {
            grappleBeamLogic.gameObject.GetComponent<SpriteRenderer>().enabled = true;
            grappleBeamLogic.gameObject.GetComponent<GrappleBeamLogic>().enabled = true;
        }
        else
        {
            grappleBeamLogic.gameObject.GetComponent<SpriteRenderer>().enabled = false;
            grappleBeamLogic.gameObject.GetComponent<GrappleBeamLogic>().enabled = false;
        }
    }



    private void GrappleMovement()
    {
        Vector2 grapple_direction = (GrapplePoint - new Vector2(transform.position.x,transform.position.y)).normalized;

        if ((GrapplePoint - new Vector2(transform.position.x,transform.position.y) ).magnitude <= GeneralGameInfo.Const_MinDistanceToGrapplePoint)
        {
            rb.gravityScale = 1f;
            IsCurrentlyGrappling = false;
            animator.animatePhysics = false;
            rb.linearVelocityY += jumpHeight;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
        else
        {
            rb.linearVelocity = grapple_direction * GeneralGameInfo.Const_GrappleSpeed;
        }
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



    public void AddMeleeVictim(HealthManager new_victim)
    {
        MeleeVictims.Add(new_victim);
    }

    public void EnableMelee()
    {
        meleeLogic.IsMeleeActive(true);
    }
    public void DisableMelee()
    {
        AttackMelee();
        meleeLogic.IsMeleeActive(false);
        MeleeVictims = new List<HealthManager>();
    }

    private void AttackMelee()
    {
        for (int i = 0; i < MeleeVictims.Count; i++) { MeleeVictims[i].TookDamage(0,gameObject); }
    }



    public void AttackPistol()
    {
        GameObject new_bullet;
        Vector2 direction;
        Vector3 bullet_pos = SecondaryWeapon.transform.Find("Muzzle").position;

        if (transform.eulerAngles.y == 0) { direction = new Vector2(-1,0); }
        else { direction = new Vector2(1,0); }
        
        new_bullet = Instantiate(Bullet, bullet_pos, Quaternion.identity);
        new_bullet.GetComponent<Gunshot>().Direction = direction;
        new_bullet.GetComponent<Gunshot>().SetGameManager(Poseidon.GetComponent<GameManager>());
        new_bullet.GetComponent<Gunshot>().SetOwner(gameObject);
        if (direction.x > 0) { new_bullet.GetComponent<SpriteRenderer>().flipX = true; }
        new_bullet.SetActive(true);
    }






    public void AttackGrenade()
    {
        GameObject new_grenade;
        Vector3 grenade_pos = SecondaryWeapon.transform.position;
        Vector2 grenade_velocity;

        if (transform.eulerAngles.y == 0) { grenade_velocity = new Vector2(-1,1); }
        else { grenade_velocity = new Vector2(1,1); }

        new_grenade = Instantiate(Grenade, grenade_pos, Quaternion.identity);
        new_grenade.GetComponent<Grenade>().Velocity = grenade_velocity;
        new_grenade.GetComponent<Grenade>().SetGameManager(Poseidon.GetComponent<GameManager>());
        new_grenade.SetActive(true);
    }

    public void AttackGrapple()
    {
        
    }







////////////////////////////////////////////////////////////////////////
// O C E A N   L A D D E R   L O G I C
////////////////////////////////////////////////////////////////////////


    public void SetOceanLadder(GameObject Cur_Ocean_Ladder)
    {
        List<Vector2> cps = Cur_Ocean_Ladder.GetComponent<OceanLadder>().LadderPoints;
        CurOceanLadder = Cur_Ocean_Ladder;
        OceanLadderCheckoints = new List<Vector2>();
        // Prevents script from modifing original.
        for (int i = 0; i < cps.Count; i++)
        {
            OceanLadderCheckoints.Add(new Vector2(cps[i].x,cps[i].y));
            Debug.Log(OceanLadderCheckoints.Count-1);
        }

        OnOceanLadder = true;

        
    }


    private void OceanLadderLogic()
    {

        

        if (InitialedOceanLadderProcess)
        {
            if (CurOceanLadder.GetComponent<OceanLadder>().FacingWest) { flipSprite(1); }
            else { flipSprite(-1); }

            animator.SetInteger("ComboIdx",-69420);
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(0,0);
            OGParent = transform.parent;
            transform.parent = CurOceanLadder.transform;
            transform.localPosition = new Vector3(OceanLadderCheckoints[0].x,OceanLadderCheckoints[0].y,transform.localPosition.z);
            InitialedOceanLadderProcess = false;
            playerCollision.enabled = false;
        }

        if (OceanLadderCheckoints.Count > 0)
        {


            Vector2 direction = (OceanLadderCheckoints[0] - new Vector2(transform.localPosition.x, transform.localPosition.y)).normalized;
            transform.localPosition += new Vector3(direction.x,direction.y,transform.localPosition.z) * OceanLadderMovementSpeed/2 * Time.deltaTime;
            rb.linearVelocity = direction * OceanLadderMovementSpeed/2;

            if (Math.Abs(transform.localPosition.y - OceanLadderCheckoints[0].y) < 0.1) 
            {
                OceanLadderCheckoints.RemoveAt(0);
            }
        }
        else
        {
            rb.gravityScale = 1f;
            transform.parent = OGParent;
            OnOceanLadder = false;
            InitialedOceanLadderProcess = true;
            playerCollision.enabled = true;
        }
    }













////////////////////////////////////////////////////////////////////////
// D E B U G
////////////////////////////////////////////////////////////////////////

    private void debug__PrintAnimStats(string name)
    {
        Debug.Log(name+", comboIndex: "+comboIndex+", ComboCooldown: "+ComboCooldown);
    }


}
