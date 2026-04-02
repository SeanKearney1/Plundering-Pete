using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{

    private int OldZone;
    private int CurZone;
    private bool PlayerDead = false;

    private GameObject Poseidon;
    private GameObject Camera;
    private PlayerInput playerInput;
    private MovementLogic movementLogic;
    private GrappleBeamLogic grappleBeamLogic;
    private Rigidbody2D rb;



    void Start()
    {
        grappleBeamLogic = transform.Find("GrappleBeam").GetComponent<GrappleBeamLogic>();;
        Camera = GameObject.FindWithTag("MainCamera");
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdateCamera();
        GrappleBeamLogic();
    }



    public void SetPoseidon(GameObject him)
    {
        Poseidon = him;
    }






    private void UpdateCamera()
    {
        CurZone = Poseidon.GetComponent<GameManager>().RetrievePlayerZone();

        if (CurZone != OldZone)
        {
            OldZone = CurZone;
            Camera.GetComponent<FollowPlayer>().SetZone( CurZone );
        }
    }





    private void GrappleBeamLogic()
    {
        List<Transform> grapple_points = Poseidon.GetComponent<GameManager>().GetGrapplePoints(transform.position);


        int cur_index = 0;
        float min_distance = 99999;

        for (int i = 0; i < grapple_points.Count; i++)
        {
            if ((transform.position - grapple_points[i].position).magnitude < min_distance)
            {
                min_distance = (transform.position - grapple_points[i].position).magnitude;
                cur_index = i;
            }
        }
        if (cur_index < grapple_points.Count) { grappleBeamLogic.SetEndPoint(grapple_points[cur_index].position); }
        






    }

}
