using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private bool IsUsingCutlass;
    private bool IsUsingPistol;
    private bool IsUsingGrenade;
    private bool IsUsingGrapple;
    private bool IsDucking;
    private bool IsJumping;
    private bool IsWalking;

    private float ComboBeginTimeStamp = 0;
    private float ComboDelay = 0.25f;
    private float ComboCooldown = 0.0f;

    void Start()
    {
        
    }


    void Update()
    {
        getInputs();
        
        if (ComboBeginTimeStamp+ComboDelay <= Time.time) { ComboLogic(); }
    }




    private void getInputs()
    {
        // Attacks
        IsUsingCutlass = Input.GetKey(KeyCode.A);
        IsUsingPistol = Input.GetKey(KeyCode.S);
        IsUsingGrenade = Input.GetKey(KeyCode.D);
        IsUsingGrapple = Input.GetKey(KeyCode.Space);

        // Movement
        IsDucking = Input.GetKey(KeyCode.DownArrow);
        IsJumping = Input.GetKey(KeyCode.UpArrow);
        IsWalking = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow); // This is cool.


        //   Handles ComboBeginTimeStamp, this variable is used
        // for measuring when to start looking for combos.
        if (!(IsUsingCutlass || IsUsingPistol || IsUsingGrenade || IsUsingGrapple || IsWalking || IsJumping || IsDucking))
        {
            ComboBeginTimeStamp = Time.time;
        }
    }




/*
	Combos:
		(Down + Left or Right)	- (Somersault.)
		(Up + Left or Right)	- (Jump flip.)
		(A) Roundhouse Kick     - (AOE attack.)
		(A + Left or Right)	- (Kick.)
		(A + Up)		- (Uppercut.)
		(A + Down)		- (AOE Sweep the leg.)
		(A + Up + Left or Right)- (Throw.)
		(Grapple into an enemy) - (Some sort of attack.)

*/

    private void ComboLogic()
    {

        if (!(IsUsingCutlass || IsUsingPistol || IsUsingGrenade || IsUsingGrapple) && (IsDucking || IsJumping || IsWalking)) { ComboMovement(); }
        else if (IsUsingCutlass) { ComboCutlass(); }
        else if (IsUsingPistol)  { ComboPistol();  }
        else if (IsUsingGrenade) { ComboGrenade(); }
        else if (IsUsingGrapple) { ComboGrapple(); }
    }


    private void ComboMovement()
    {
        if (IsDucking && IsWalking) { Debug.Log("Somersault"); }
        else if (IsJumping && IsWalking) { Debug.Log("Jump flip"); }
    }

    private void ComboCutlass()
    {
        if (!IsWalking && !IsJumping && !IsDucking) { Debug.Log("Roundhouse Kick"); }
        else if (IsWalking) { Debug.Log("Kick"); }
        else if (IsJumping && !IsWalking) { Debug.Log("Uppercut"); }
        else if (IsDucking && !IsWalking) { Debug.Log("AOE Sweep the leg"); }
        else if (IsJumping && IsWalking) { Debug.Log("Throw"); }
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

}
