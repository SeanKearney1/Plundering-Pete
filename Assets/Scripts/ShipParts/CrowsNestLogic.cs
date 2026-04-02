using UnityEngine;

public class CrowsNestLogic : MonoBehaviour
{
    private Transform GrappleLeft;
    private Transform GrappleRight;

    void Start()
    {
       GrappleLeft = transform.Find("GrapplePointLeft");
       GrappleRight = transform.Find("GrapplePointRight");
    }

    void Update()
    {
        
    }



    public Transform GetNearestGrapplePoint(Vector3 grappler)
    {
        float distance1 = (grappler-GrappleLeft.position).magnitude;
        float distance2 = (grappler-GrappleRight.position).magnitude;
        if (distance1 <= distance2) { return GrappleLeft; }
        return GrappleRight;
    }



    public Transform[] GetGrapplePoints() { return new Transform[] { GrappleLeft, GrappleRight }; }
}
