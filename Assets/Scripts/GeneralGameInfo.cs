using UnityEngine;

[CreateAssetMenu(fileName = "GeneralGameInfo", menuName = "Scriptable Objects/GeneralGameInfo")]
public class GeneralGameInfo : ScriptableObject
{
    public static string Const_FileSavePath = Application.persistentDataPath+"/saveSlot";
    public static int Const_TotalLevelCount = 1;
    public static int Const_SaveSlotCount = 3;

    public static int Const_CannonTypeCount = 4;
    public static int Const_CannonBallTypeCount = 4;
    public static int Const_GameOverScreenFadeInTime = 3;
    public static float Const_BulletDistance = 50f;
    public static float Const_BulletOffset = 1f;

    public static float[] Const_BaseDamage = {
        10, // Cutlass
        25, // Pistol
        50, // Grenade
        33, // Grapple
    };

    public static float Const_DamageCooldown = 0.5f;
    public static float Const_DamageStackTime = 1f;
    public static float Const_MinDamageToKO = 25f;
    public static float Const_MaxGrenadeDistance = 3.5f;
    public static float Const_MinGrenadeDistance = 1.75f;
    public static float Const_MaxGrappleDistance = 100f;


    public static LayerMask Const_HullMask;
    public static LayerMask Const_PistolMask;

    public static GameObject[] Ships;


    public static void InitializeData(GameObject[] new_ships)
    {
        Ships = new_ships;
        Const_HullMask = LayerMask.GetMask("Hull");
        Const_PistolMask = LayerMask.GetMask("Hull","Player_Bot");
    }




///////////////////////////////////////////////////////////////////////////////////
//  L  E  V  E  L  S
///////////////////////////////////////////////////////////////////////////////////
//
////////////////////////////////////////////
//  P A G E   B L U E P R I N T
////////////////////////////////////////////
//
//  Level 1: Starter - New Enemies, mechanics, etc.
//  Level 2: Harder  - Exands upon level 1.
//  Level 3: Fleet   - Several to alot of ships.
//  Level 4: Boss    - Boss fight or difficuly level.
//
//////////////////////////
//  P A G E    0 1
//////////////////////////
//
//  Level 1:
//
//  Level 2:
//
//  Level 3:
//
//  Level 4:
//
//////////////////////////
//  P A G E    0 2
//////////////////////////
//
//
//
//
//
//
//
//
//



}
