using System;
using UnityEngine;

public class Gunshot : MonoBehaviour
{
    public GameObject Smoke;
    public Vector2 Direction;
    public float FlashDuration;
    private float BeginTimeStamp;
    private GameManager gameManager;

    private SpriteRenderer spriteRenderer;
    LayerMask layerMask;

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
        Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward));
    }
}
