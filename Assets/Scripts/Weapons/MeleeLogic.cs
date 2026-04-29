using UnityEditor;
using UnityEngine;

public class MeleeLogic : MonoBehaviour
{
    private bool MeleeActive = false;
    private MovementLogic movementLogic;
    void Start()
    {
        Transform the_parent = transform;
        for (int i = 0; i < 9; i++) { 
            the_parent = the_parent.transform.parent; 
            Debug.Log("the_parent = "+the_parent.name);
            }
        movementLogic = the_parent.GetComponent<MovementLogic>();
        movementLogic.SetMeleeScript(this);
    }


    public void IsMeleeActive(bool x) { MeleeActive = x; }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (ValidateVictim(collision))
        {
            movementLogic.AddMeleeVictim(collision.transform.parent.gameObject.GetComponent<HealthManager>());
        }
    }





    private bool ValidateVictim(Collider2D collision)
    {
        if (!(collision.gameObject.CompareTag("HitBox") && MeleeActive)) { return false; }
        if (collision.transform.parent.gameObject.GetComponent<BotLogic>() && movementLogic.gameObject.GetComponent<BotLogic>()) { return false; }
        if (!collision.transform.parent.gameObject.GetComponent<BotLogic>() && !movementLogic.gameObject.GetComponent<BotLogic>()) { return false; }
        return true;
    }
}
