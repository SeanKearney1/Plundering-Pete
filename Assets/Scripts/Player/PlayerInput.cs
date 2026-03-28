using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    private bool IsPaused = false;
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
        movementLogic.Set_IsUsingMovmentCombo(Input.GetKey(KeyCode.Q));
        movementLogic.Set_IsUsingCutlass(Input.GetKey(KeyCode.A));
        movementLogic.Set_IsUsingPistol(Input.GetKey(KeyCode.S));
        movementLogic.Set_IsUsingGrenade(Input.GetKey(KeyCode.D));
        movementLogic.Set_IsUsingGrapple(Input.GetKey(KeyCode.Space));
        movementLogic.Set_IsDucking(Input.GetKey(KeyCode.DownArrow));
        movementLogic.Set_IsTryingToJump(Input.GetKey(KeyCode.UpArrow));
        movementLogic.Set_IsWalking(Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow));// This is cool.
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
