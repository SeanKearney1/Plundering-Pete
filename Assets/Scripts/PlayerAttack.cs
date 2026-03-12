using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private bool ComboTimedAction1 = false;
    private bool IsUsingCutlass;
    private bool IsUsingPistol;
    private bool IsUsingGrenade;
    private bool IsUsingGrapple;
    private bool IsDucking;
    private bool IsJumping;
    private bool IsFalling;
    private bool IsWalking;
    private bool ComboDisableInputMovement = false;
    private bool can_jump = true;

    private float ComboBeginTimeStamp = 0;
    private float ComboDelay = 0.075f;
    private float ComboCooldown = 0.0f;
    private float ComboMovementCooldown = 0.0f;

    private int comboIndex = 0;


    private Rigidbody2D rb;
    private PlayerMovement playerMovement;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<PlayerMovement>();
        animator = GetComponent<Animator>();

    }


    void Update()
    {
        JumpValidator();
        getInputs();
        
        Countdowns();


        if (ValidateNextCombo()) { ComboStartLogic(); }
        ComboLogic();

        animator.SetInteger("ComboIdx",comboIndex);
        if (ComboCooldown != 0) 
        { 
            animator.SetBool("IsInCombo",true); 
            IsJumping = false;
            can_jump = false;
            }
        else 
        { 
            animator.SetBool("IsInCombo",false); 
            ComboTimedAction1 = true;
        }




    }


    private void JumpValidator()
    {
        if (rb.linearVelocity.y == 0) { can_jump = true; }
        if (Input.GetKey(KeyCode.UpArrow) && can_jump) 
        { 
            can_jump = false;
            IsJumping = true; 
        }
    }


    public bool GetAllowMovement()
    {
        return ComboDisableInputMovement;
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




    private void getInputs()
    {
        // Attacks
        IsUsingCutlass = Input.GetKey(KeyCode.A);
        IsUsingPistol = Input.GetKey(KeyCode.S);
        IsUsingGrenade = Input.GetKey(KeyCode.D);
        IsUsingGrapple = Input.GetKey(KeyCode.Space);

        // Movement
        if (rb.linearVelocityY != 0) { IsFalling = true; }
        else { IsFalling = false; }

        IsDucking = Input.GetKey(KeyCode.DownArrow);
        IsWalking = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow); // This is cool.


        //   Handles ComboBeginTimeStamp, this variable is used
        // for measuring when to start looking for combos.
        if (!(IsUsingCutlass || IsUsingPistol || IsUsingGrenade || IsUsingGrapple || IsWalking || IsJumping || IsFalling || IsDucking))
        {
            ComboBeginTimeStamp = Time.time;
        }
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
        if (IsDucking && IsWalking) 
        { 
            comboIndex = 1001;
            ComboCooldown = 1.25f;
            ComboMovementCooldown = 1.05f;
            ComboDisableInputMovement = true;
            Debug.Log("Somersault");
        }
        else if (IsJumping && IsWalking) 
        { 
            comboIndex = 1002;
            ComboCooldown = 1.35f;
            ComboMovementCooldown = 1.15f;
            ComboDisableInputMovement = true;
            rb.linearVelocityY = playerMovement.jumpHeight*1.25f;
            Debug.Log("Jump flip"); 
        }
        else if (IsJumping && !IsDucking && !IsWalking)
        {
            comboIndex = 1003;
            ComboCooldown = 0.5f;
            ComboMovementCooldown = 0f;
            ComboDisableInputMovement = false;
            rb.linearVelocityY += playerMovement.jumpHeight;
            Debug.Log("Jump"); 
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
            Debug.Log("Big Slash"); 
        }
        else if (IsWalking && !IsJumping) 
        {
            comboIndex = 2002;
            ComboCooldown = 1f;
            ComboMovementCooldown = 0f;
            Debug.Log("Slash");
        }
        else if ((IsJumping || IsFalling) && !IsWalking) 
        { 
            comboIndex = 2003;
            ComboCooldown = 0.85f; 
            ComboMovementCooldown = 0.85f;
            ComboDisableInputMovement = true;
            Debug.Log("Uppercut"); 
        }
        else if (IsDucking && !IsWalking) 
        { 
            comboIndex = 2004;
            ComboCooldown = 0.75f; 
            ComboMovementCooldown = 0.75f;
            ComboDisableInputMovement = true;
            Debug.Log("AOE Sweep the leg"); 
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
        comboIndex = 3000;
    }

    private void ComboStartGrenade()
    {
        comboIndex = 4000;
    }

    private void ComboStartGrapple()
    {
        comboIndex = 5000;
    }






////////////////////////////////////////////////////////////////////////////////////////////
// C O M B O   D U R A T I O N   F U N C T I O N S
////////////////////////////////////////////////////////////////////////////////////////////

    private void ComboLogic()
    {
        if (comboIndex > 1000 && comboIndex < 1999)      { ComboMovement(); }
        else if (comboIndex > 2000 && comboIndex < 2999) { ComboCutlass(); }
        else if (comboIndex > 3000 && comboIndex < 3999) { ComboPistol(); }
        else if (comboIndex > 4000 && comboIndex < 4999) { ComboGrenade(); }
        else if (comboIndex > 5000 && comboIndex < 5999) { ComboGrapple(); }
    }

    private void ComboMovement()
    {

    }

    private void ComboCutlass()
    {
        if (comboIndex == 2003) // Uppercut
        {
            if (ComboCooldown < 0.55 && ComboTimedAction1)
            {
                rb.linearVelocityY += playerMovement.jumpHeight*1.5f;
                ComboTimedAction1 = false;
            }
        }
    }
    private void ComboPistol()
    {
        
    }
    private void ComboGrenade()
    {
        
    }
    private void ComboGrapple()
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
