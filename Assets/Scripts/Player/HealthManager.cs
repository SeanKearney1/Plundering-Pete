using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class HealthManager : MonoBehaviour
{

    public float Health;
    private float Cur_Health;
    private float DamageTakenRecently = 0.0f;
    private float LastDamageTimeStamp = 0.0f;
    private bool PlayerDead = false;

    private GameObject HealthBar;
    private GameObject HealthBarBackground;
    private MovementLogic movementLogic;

    void Start()
    {
        Cur_Health = Health;
        HealthBar = transform.Find("HealthBar").gameObject;
        HealthBarBackground = transform.Find("HealthBarBackground").gameObject;
        movementLogic = GetComponent<MovementLogic>();
    }

    void Update()
    {
        HealthBar.GetComponent<SpriteMask>().alphaCutoff = 1 - (Cur_Health / Health);

        if (LastDamageTimeStamp + GeneralGameInfo.Const_DamageStackTime < Time.time) { DamageTakenRecently = 0.0f; }

    }






    public void TookDamage(int damageType, GameObject attacker)
    {
        float damage_multiplier = 1f;
        float damage;
        int anim_damage_type;

        LastDamageTimeStamp = Time.time;

        if (!attacker.GetComponent<BotLogic>().IsUnityNull()) { damage_multiplier = attacker.GetComponent<BotLogic>().GetDamageMultiplier(); }


        if (damageType == 2) // Grenade
        {
            float distance = (transform.position-attacker.transform.position).magnitude;
            float max_distance = GeneralGameInfo.Const_MaxGrenadeDistance;
            damage = GeneralGameInfo.Const_BaseDamage[damageType] * ( (max_distance-distance + GeneralGameInfo.Const_MaxGrenadeDistance) / max_distance);

            if (damage > GeneralGameInfo.Const_BaseDamage[damageType]) { damage = GeneralGameInfo.Const_BaseDamage[damageType]; }
        }
        else 
        {
            if (IsBlocking(attacker)) { damage = 0; }
            else { damage = GeneralGameInfo.Const_BaseDamage[damageType] * damage_multiplier; }
        }


        Cur_Health -= damage;
        DamageTakenRecently += damage;

        if (damage == 0) { anim_damage_type = -100; }
        else if (DamageTakenRecently < GeneralGameInfo.Const_MinDamageToKO) { anim_damage_type = GetDamageDirection(0, attacker); }
        else { anim_damage_type = GetDamageDirection(2, attacker); }



        if (Cur_Health <= 0) 
        {
            anim_damage_type = GetDamageDirection(4, attacker);
            Death();
        }

        movementLogic.SetDamageState(anim_damage_type);
    }





    private void Death()
    {
        PlayerDead = true;
        HealthBar.GetComponent<SpriteRenderer>().enabled = false;
        HealthBarBackground.GetComponent<SpriteRenderer>().enabled = false;
        if (!movementLogic.IsUnityNull()) { movementLogic.Death(); }
        if (!GetComponent<PlayerLogic>().IsUnityNull()) { GetComponent<PlayerLogic>().enabled = false; }
        if (!GetComponent<BotLogic>().IsUnityNull()) { GetComponent<BotLogic>().enabled = false; }
        if (!GetComponent<PlayerInput>().IsUnityNull()) { GetComponent<PlayerInput>().enabled = false; }
    }



    private bool IsBlocking(GameObject attacker)
    {
        int block_state = movementLogic.BlockingState();
        if (block_state == -1) { return false; }
        if (block_state == 0 && attacker.transform.position.x <= transform.position.x) { return true; }
        if (block_state == 1 && attacker.transform.position.x > transform.position.x)  { return true; }
        return false;
    }


    public bool IsPlayerDead() { return PlayerDead; }






    private int GetDamageDirection(int damage_type, GameObject attacker)
    { 
        Debug.Log("transform.eulerAngles.y = "+transform.eulerAngles.y+" "+transform.position.x+" > "+attacker.transform.position.x);
        if (transform.eulerAngles.y == 180)
        {
            Debug.Log("Turned Around!");
            if (transform.position.x < attacker.transform.position.x) { return damage_type + 1; }
        }
        else
        {
            if (transform.position.x > attacker.transform.position.x) { return damage_type + 1; }
        }

        return damage_type;
    }


}
