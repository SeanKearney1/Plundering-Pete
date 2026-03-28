using Unity.VisualScripting;
using UnityEngine;

public class HealthManager : MonoBehaviour
{

    public float Health;
    private float Cur_Health;

    private GameObject HealthBar;

    void Start()
    {
        Cur_Health = Health;
        HealthBar = transform.Find("HealthBar").gameObject;
    }

    void Update()
    {
        HealthBar.GetComponent<SpriteMask>().alphaCutoff = 1 - (Cur_Health / Health);
    }






    public void TookDamage(int damageType, GameObject attacker)
    {
        float damage_multiplier = 1f;
        BaseDamages baseDamages = new BaseDamages();

        if (!attacker.GetComponent<BotLogic>().IsUnityNull()) { damage_multiplier = attacker.GetComponent<BotLogic>().GetDamageMultiplier(); }


        if (damageType == 2) // Grenade
        {
            float distance = (transform.position-attacker.transform.position).magnitude;
            float max_distance = baseDamages.MaxGrenadeDistance;
            float damage = baseDamages.BaseDamage[damageType] * ( (max_distance-distance + baseDamages.MaxGrenadeDistance) / max_distance);

            if (damage > baseDamages.BaseDamage[damageType]) { damage = baseDamages.BaseDamage[damageType]; }

            Cur_Health -= damage;
        }
        else { Cur_Health -= baseDamages.BaseDamage[damageType] * damage_multiplier; }

        if (Cur_Health < 0) { Death(); }
    }


    private void Death()
    {
        if (!GetComponent<BotLogic>().IsUnityNull()) { GetComponent<BotLogic>().enabled = false; }
        if (!GetComponent<PlayerLogic>().IsUnityNull()) { GetComponent<PlayerLogic>().enabled = false; }
        if (!GetComponent<MovementLogic>().IsUnityNull()) { GetComponent<MovementLogic>().enabled = false; }
        if (!GetComponent<PlayerInput>().IsUnityNull()) { GetComponent<PlayerInput>().enabled = false; }
    }

}
