using UnityEngine;

[CreateAssetMenu(fileName = "GeneralGameInfo", menuName = "Scriptable Objects/GeneralGameInfo")]
public class GeneralGameInfo : ScriptableObject
{
    public static string Const_FileSavePath = Application.persistentDataPath+"/saveSlot";
    public static int Const_TotalLevelCount = 1;
    public static int Const_SaveSlotCount = 3;

    public static int Const_CannonTypeCount = 4;
    public static int Const_CannonBallTypeCount = 4;

    public static GameObject[] Ships;


    public static void InitializeData(GameObject[] new_ships)
    {
        Ships = new_ships;
    }



}
