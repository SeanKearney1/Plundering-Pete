using UnityEngine;

public class Grenade : MonoBehaviour
{
    public Vector2 Speed;
    public float FuseTime;
    public float ExplosionDuration;
    public Vector2 Velocity;
    public GameObject Smoke;
    private bool HasExploded = false;
    private float blast_time = 0.35f;
    private float BeginTimeStamp;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.angularVelocity = 360f;//Random.Range(-90.0f, 90.0f);
        BeginTimeStamp = Time.time;
        Velocity *= Speed;
        rb.linearVelocity = Velocity;


        GetComponent<Animator>().SetFloat("FuseTime",1/FuseTime);


    }

    void Update()
    {
        if (BeginTimeStamp + FuseTime <= Time.time)
        {
            if (BeginTimeStamp + blast_time + ExplosionDuration + FuseTime > Time.time)
            {
                if (!HasExploded) { Explode(); }
                ExplodeFade();
            }
            else
            {
                Destroy(gameObject);
            }
        }

    }



    private void Explode()
    {
        HasExploded = true;
        Instantiate(Smoke, transform.position, Quaternion.identity);

    }
    private void ExplodeFade()
    {
        
    }
}
