using Unity.VisualScripting;
using UnityEngine;

public class DefensiveShipPartInfo : MonoBehaviour
{
    private GameObject Owner;

    public void SetOwner(GameObject new_owner) { Owner = new_owner; }

    public bool GetIsNotBeingUsed()
    {
        if (Owner.IsUnityNull()) { return true; }
        return false;
    }



}
