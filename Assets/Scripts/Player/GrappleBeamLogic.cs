using UnityEngine;

public class GrappleBeamLogic : MonoBehaviour
{
    private float scrollSpeed = 0.5f;
    SpriteRenderer spriteRenderer;
    Renderer the_renderer;
    MovementLogic movementLogic;
    Vector2 endPoint = new Vector2(0,0);
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        the_renderer = GetComponent<Renderer>();
        movementLogic = transform.parent.GetComponent<MovementLogic>();
        spriteRenderer.enabled = false;
    }


    void Update()
    {
        //the_renderer.material.mainTextureOffset = new Vector2(Time.time * scrollSpeed, 0);

        float angle = Mathf.Rad2Deg * Mathf.Atan2(endPoint.y - transform.position.y, endPoint.x - transform.position.x);

        if (!movementLogic.LookingLeft())
        {
            angle *= -1;
            transform.localScale = new Vector3(-1,1,1);
        }
        else { transform.localScale = new Vector3(1,1,1); }

        spriteRenderer.size = new Vector2((new Vector2(transform.position.x,transform.position.y)-endPoint).magnitude,spriteRenderer.size.y);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y,angle);
    }


    public void SetEndPoint(Vector2 new_endpoint) { endPoint = new_endpoint; }
}