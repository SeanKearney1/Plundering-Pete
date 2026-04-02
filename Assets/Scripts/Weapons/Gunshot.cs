using System;
using Unity.VisualScripting;
using UnityEngine;

public class Gunshot : MonoBehaviour
{
    public GameObject Smoke;
    public Vector2 Direction;
    public float FlashDuration;
    private float BeginTimeStamp;
    private GameManager gameManager;
    private GameObject Owner;

    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject new_smoke;

        spriteRenderer = GetComponent<SpriteRenderer>();

        Smoke.SetActive(false);

        BeginTimeStamp = Time.time;

        new_smoke = Instantiate(Smoke, transform.position, Quaternion.identity);
        new_smoke.GetComponent<SmokeEffect>().Direction = Direction;
        new_smoke.SetActive(true);
        FireBullet();
    }

    public void SetGameManager( GameManager new_gamemanager) { gameManager = new_gamemanager; }
    public void SetOwner( GameObject new_owner) { Owner = new_owner; }

    // Update is called once per frame
    void Update()
    {
        if (BeginTimeStamp + FlashDuration >= Time.time)
        {
            Color color = spriteRenderer.color;

            if (BeginTimeStamp + (FlashDuration/2) >= Time.time) { color.a = (float)Math.Sqrt(2*(Time.time - BeginTimeStamp) / FlashDuration); }
            else  { color.a = (float)Math.Sqrt(2-(2*(Time.time - BeginTimeStamp) / FlashDuration)); }

            
            spriteRenderer.color = color;
        }
        else
        {
            Destroy(gameObject);
        }
    }



        private void FireBullet()
    {
        ContactFilter2D contactFilter = new ContactFilter2D();
        RaycastHit2D[] results = new RaycastHit2D[2];

        contactFilter.useTriggers = true;
        contactFilter.useLayerMask = true;
        contactFilter.layerMask = GeneralGameInfo.Const_PistolMask;

        Physics2D.Linecast(GetStartBulletPos(transform.position), GetEndBulletPos(transform.position), contactFilter, results); 


        
        Debug.DrawLine(GetStartBulletPos(transform.position), GetEndBulletPos(transform.position), Color.darkOrchid, 100f);

        for (int i = 0; i < results.Length; i++) {
            if (!results[i].collider.IsUnityNull() && results[i].collider.gameObject.tag == "HitBox")
            {

                Debug.Log("target.name = "+results[i].collider.name);
                if (results[i].collider.transform.parent.GetComponent<HealthManager>()) 
                {
                    results[i].collider.transform.parent.GetComponent<HealthManager>().TookDamage(1,Owner);
                }            
            }
        }
    }

    private Vector2 GetStartBulletPos(Vector2 start_pos)
    {
        if (Direction.x > 0) { start_pos.x -= GeneralGameInfo.Const_BulletOffset; }
        else { start_pos.x += GeneralGameInfo.Const_BulletOffset; }
        return start_pos;
    }

    private Vector2 GetEndBulletPos(Vector2 start_pos)
    {
        if (Direction.x > 0) { start_pos.x += GeneralGameInfo.Const_BulletDistance; }
        else { start_pos.x -= GeneralGameInfo.Const_BulletDistance; }
        return start_pos;
    }
}
