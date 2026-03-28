using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    LayerMask layerMask;
    public Vector2 Speed;
    public float FuseTime;
    public float ExplosionDuration;
    public Vector2 Velocity;
    public GameObject Smoke;
    private bool HasExploded = false;
    private float blast_time = 0.35f;
    private float BeginTimeStamp;
    private Rigidbody2D rb;
    private GameManager gameManager;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = 360f;//Random.Range(-90.0f, 90.0f);
        BeginTimeStamp = Time.time;
        Velocity *= Speed;
        rb.linearVelocity = Velocity;

        layerMask = LayerMask.GetMask("Hull");

        GetComponent<Animator>().SetFloat("FuseTime",1/FuseTime);


    }

    public void SetGameManager( GameManager new_gamemanager) { gameManager = new_gamemanager; }


    void Update()
    {
        if (BeginTimeStamp + FuseTime <= Time.time)
        {
            if (!HasExploded && BeginTimeStamp + blast_time + ExplosionDuration + FuseTime > Time.time)
            {
                HasExploded = true;
                Explode();
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }



    private void Explode()
    {
        Instantiate(Smoke, transform.position, Quaternion.identity);
        HurtVictims();


    }




    private void HurtVictims()
    {
        List<GameObject> victims = gameManager.GetEveryone();
        BaseDamages baseDamages = new BaseDamages();
        float cur_distance;

        for (int i = 0; i < victims.Count; i++) {

            cur_distance = (victims[i].transform.position - transform.position).magnitude;

            if (cur_distance <= baseDamages.MaxGrenadeDistance && !Physics2D.Linecast(transform.position, victims[i].transform.position, layerMask, 0, 0))
            {
                Debug.Log("victims["+i+"] = "+victims[i]);
                victims[i].GetComponent<HealthManager>().TookDamage(2, gameObject);
            }
        }


    }


}
