using UnityEngine;

public class PlayerLogic : MonoBehaviour
{

    private int OldZone;
    private int CurZone;

    private GameObject Poseidon;
    private GameObject Camera;
    private PlayerInput playerInput;
    private MovementLogic movementLogic;
    private Rigidbody2D rb;



    void Start()
    {
        Camera = GameObject.FindWithTag("MainCamera");
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdateCamera();

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


}
