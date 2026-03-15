using System;
using Unity.VisualScripting;
using UnityEngine;

public class SmokeEffect : MonoBehaviour
{
    public float SmokeSpeed;
    public Vector2 Direction;
    public float SmokeFadeOutTime;
    public float MaxSize;
    public float fadinTime;
    public float InitialAnimationHoldTime;
    //public bool KeepOrigin;


    private float BeginTimeStamp = 0f;
    private bool FrozeAnimation = false;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    void Start()
    {
        BeginTimeStamp = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        if (Direction.x > 0) { spriteRenderer.flipX = true; }
        Direction *= SmokeSpeed;
    }

    void Update()
    {    
        float cur_size = 1;

        if (SmokeSpeed != 0)
        { 
            rb.linearVelocity = Direction * (float)(1 - Math.Sqrt(Math.Sqrt((Time.time - BeginTimeStamp) / SmokeFadeOutTime)));
        }


        if (BeginTimeStamp + InitialAnimationHoldTime < Time.time && !FrozeAnimation) 
        { 
            if (!animator.IsUnityNull()) { animator.enabled = false; }
            FrozeAnimation = true;
        }
        

        if (BeginTimeStamp + fadinTime >= Time.time)
        {
            Color color = spriteRenderer.color;
            color.a = (Time.time - BeginTimeStamp) / fadinTime;
            spriteRenderer.color = color;

            cur_size = (float)Math.Sqrt(Math.Sqrt((Time.time - BeginTimeStamp) / fadinTime));
            transform.localScale = new Vector2(cur_size,1);  
        }

        else if (BeginTimeStamp + fadinTime + SmokeFadeOutTime >= Time.time)
        {
            Color color = spriteRenderer.color;
            color.a = (float)(1 - Math.Sqrt((Time.time - fadinTime - BeginTimeStamp) / SmokeFadeOutTime));
            spriteRenderer.color = color;

            cur_size += MaxSize*((Time.time - fadinTime - BeginTimeStamp) / SmokeFadeOutTime);
            transform.localScale = new Vector2(cur_size,cur_size);  
        }
        else { Destroy(gameObject); }
    }
}