using UnityEngine;

[CreateAssetMenu(fileName = "BaseDamages", menuName = "Scriptable Objects/BaseDamages")]
public class BaseDamages : ScriptableObject
{
    public float[] BaseDamage = {
        7, // Cutlass
        18, // Pistol
        50, // Grenade
        40, // Grapple
    };

    public float MaxGrenadeDistance = 3.5f;
    public float MinGrenadeDistance = 1.75f;
}
