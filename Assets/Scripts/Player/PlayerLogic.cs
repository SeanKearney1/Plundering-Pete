using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{

    private int OldZone;
    private int CurZone;
    private GameObject Poseidon;
    private GameObject Camera;


    void Start()
    {
        Camera = GameObject.FindWithTag("MainCamera");
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
