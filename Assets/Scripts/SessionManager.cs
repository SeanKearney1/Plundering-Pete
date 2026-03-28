using System;
using UnityEngine;

public class SessionManager : MonoBehaviour
{
    public GameObject[] Ships;

    private SaveFile CurrentSaveFile;


    // Ship Types:
    //  0 - Rowboat
    //  1 - Piragua
    //  2 - Sloop
    //  3 - Galleon




    void Start()
    {
        DontDestroyOnLoad(gameObject);
        GeneralGameInfo.InitializeData(Ships);
    }

    void Update()
    {
        
    }


    public void SetCurrentSaveFile(SaveFile saveFile)
    {
        CurrentSaveFile = saveFile;
        CurrentSaveFile.SetEmptySave(false);
        CurrentSaveFile.Save();
    }

    public int GetShipType() { return CurrentSaveFile.GetCurrentShipType(); }

}
