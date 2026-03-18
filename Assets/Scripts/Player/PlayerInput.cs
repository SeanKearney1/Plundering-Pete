using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    private bool IsUsingMovmentCombo;
    private bool IsUsingCutlass;
    private bool IsUsingPistol;
    private bool IsUsingGrenade;
    private bool IsUsingGrapple;
    private bool IsDucking;
    private bool IsTryingToJump;
    private bool IsWalking;



    private MovementLogic movementLogic;
    void Start()
    {
        movementLogic = GetComponent<MovementLogic>();
    }

    void Update()
    {
        SetInputs();
        SetLook();
    }


    private void SetInputs()
    {
        // Attacks
        IsUsingMovmentCombo = Input.GetKey(KeyCode.Q);
        IsUsingCutlass = Input.GetKey(KeyCode.A);
        IsUsingPistol = Input.GetKey(KeyCode.S);
        IsUsingGrenade = Input.GetKey(KeyCode.D);
        IsUsingGrapple = Input.GetKey(KeyCode.Space);

        // Movement
        IsDucking = Input.GetKey(KeyCode.DownArrow);
        IsTryingToJump = Input.GetKey(KeyCode.UpArrow);
        IsWalking = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow); // This is cool.


        movementLogic.Set_IsUsingMovmentCombo(IsUsingMovmentCombo);
        movementLogic.Set_IsUsingCutlass(IsUsingCutlass);
        movementLogic.Set_IsUsingPistol(IsUsingPistol);
        movementLogic.Set_IsUsingGrenade(IsUsingGrenade);
        movementLogic.Set_IsUsingGrapple(IsUsingGrapple);
        movementLogic.Set_IsDucking(IsDucking);
        movementLogic.Set_IsWalking(IsWalking);
        movementLogic.Set_IsTryingToJump(IsTryingToJump);
    }




    private void SetLook()
    {
        Vector2 Look = new Vector2(0,0);
        if (Input.GetKey(KeyCode.LeftArrow))  { Look.x -= 1;}
        if (Input.GetKey(KeyCode.RightArrow)) { Look.x += 1;}
        if (Input.GetKey(KeyCode.UpArrow))  { Look.y += 1;}
        if (Input.GetKey(KeyCode.DownArrow)) { Look.y -= 1;}

        movementLogic.Set_Look(Look);
        
    }


}
