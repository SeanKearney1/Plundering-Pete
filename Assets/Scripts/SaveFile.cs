using System.Collections.Generic;
using UnityEngine;
using System.IO;

using System.Linq;



[System.Serializable]

public class SaveFile
{

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//   V  A  R  I  A  B  L  E  S
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


/////////////////////////////////////////
// G E N E R A L
/////////////////////////////////////////

/// Save Slot Related

    private bool EmptySave;
    private int SaveSlot;

/// Progress Related

    private bool GameCompleted; // 100% completed all levels, unlocks, achievements, etc.
    private int TotalDeaths;
    private float TotalPlayTime; // Counter goes up while in a level.
    private int CurrentUnlockedShips; //Bit Flag
    private int UnlockedCannonBalls; // Bit Flag
    private int UnlockedCannons; // Bit Flag

/// Level Related

    private int[] LevelStars; // Array holding how many stars the layer has earned on each level.
    private int LatestUnlockedLevel; // -1 if all levels are completed.

/// Player's Ship's Loadout Related 

    private int CurrentSelectedShipSize;
    private int[] SelectedCannons;
    private int[] SelectedCannonBalls;


/////////////////////////////////////////
// A C H I E V E M E N T S
/////////////////////////////////////////







////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  I  N  I  T  I  A  L  I  Z  E  R
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public SaveFile(int save_slot)
    {

    /// Save Slot related
        EmptySave = true;
        SaveSlot = save_slot;
    /// Progress Related
        GameCompleted = false;
        TotalDeaths = 0;
        TotalPlayTime = 0;
        CurrentUnlockedShips = 0;
        UnlockedCannonBalls = 0;
        UnlockedCannons = 0;

    /// Level Related
        LevelStars = new int[GeneralGameInfo.Const_TotalLevelCount];
        LatestUnlockedLevel = 1;

    /// Player's Ship's Loadout Related 
        CurrentSelectedShipSize = 0;
        SelectedCannons = new int[GeneralGameInfo.Const_CannonTypeCount];
        SelectedCannonBalls = new int[GeneralGameInfo.Const_CannonBallTypeCount];
    }


////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  F  I  L  E      F  U  N  C  T  I  O  N  S
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


    public void Save()
    {
        string str = FileToString();
        File.WriteAllText(GeneralGameInfo.Const_FileSavePath+SaveSlot,str);
    }




    public static void ResetSlot(int slot_idx)
    {
        SaveFile newsavefile = new SaveFile(slot_idx);
        newsavefile.Save();
    }










    public static SaveFile GetSaveSlot(int slot_idx)
    {
        string file_str;
        SaveFile newsaveFile = new SaveFile(slot_idx);
        if (File.Exists(GeneralGameInfo.Const_FileSavePath+slot_idx))
        {
            file_str = File.ReadAllText(GeneralGameInfo.Const_FileSavePath+slot_idx);
            newsaveFile = newsaveFile.StringToFile(file_str,slot_idx);
            newsaveFile.Debug__PrintSaveFile();
        }
        else
        {

            FileStream fstream = File.Create(GeneralGameInfo.Const_FileSavePath+slot_idx);
            fstream.Close();
            newsaveFile.Save();
        }
        return newsaveFile;
    }


    public SaveFile StringToFile(string file, int slot)
    {
        SaveFile new_file = new SaveFile(slot);

        string[] lines = file.Split('\n');

        new_file.SetEmptySave(SetFileBool(lines[0]));
        new_file.SetGameCompleted(SetFileBool(lines[1]));
        new_file.SetTotalDeaths(SetFileInt(lines[2]));
        new_file.SetTotalPlayTime(SetFileFloat(lines[3]));
        new_file.SetCurrentUnlockedShips(SetFileInt(lines[4]));
        new_file.SetUnlockedCannonBalls(SetFileInt(lines[5]));
        new_file.SetUnlockedCannons(SetFileInt(lines[6]));
        new_file.SetLevelStars(SetFileIntArr(lines[7]));
        new_file.SetLatestUnlockedLevel(SetFileInt(lines[8]));
        new_file.SetCurrentSelectedShipSize(SetFileInt(lines[9]));
        new_file.SetSelectedCannons(SetFileIntArr(lines[10]));
        new_file.SetSelectedCannonBalls(SetFileIntArr(lines[11]));

        return new_file;
    }

    public string FileToString()
    {
        string str = "";
        
        str += "EmptySave="+EmptySave;
        str += "\nGameCompleted="+GameCompleted;
        str += "\nTotalDeaths="+TotalDeaths;
        str += "\nTotalPlayTime="+TotalPlayTime;
        str += "\nCurrentUnlockedShips="+CurrentUnlockedShips;
        str += "\nUnlockedCannonBalls="+UnlockedCannonBalls;
        str += "\nUnlockedCannons="+UnlockedCannons;
        str += "\nLevelStars=";
        for (int i = 0; i < LevelStars.Length;i++) { str += LevelStars[i]+",";}
        str += "\nLatestUnlockedLevel="+LatestUnlockedLevel;
        str += "\nCurrentSelectedShipSize="+CurrentSelectedShipSize;
        str += "\nSelectedCannons=";
        for (int i = 0; i < SelectedCannons.Length;i++) { str += SelectedCannons[i]+",";}
        str += "\nSelectedCannonBalls=";
        for (int i = 0; i < SelectedCannonBalls.Length;i++) { str += SelectedCannonBalls[i]+",";}

        return str;
    }
    

    private bool SetFileBool(string value)
    {
        value = RemoveName(value);
        if (value == "True") { return true; }
        return false;
    }
    private float SetFileFloat(string value)
    {
        float x;
        value = RemoveName(value);
        float.TryParse(value, out x);
        return x;
    }
    private int SetFileInt(string value)
    {
        int x;
        value = RemoveName(value);
        int.TryParse(value, out x);
        return x;
    } 
    private int[] SetFileIntArr(string value)
    {
        List<int> list = new List<int>();
        int cur_int = 0;

        value = RemoveName(value);

        while (value.Length != 1)
        {
            if (value[0] == ',')
            {
                list.Add(cur_int);
                cur_int = 0;
            }
            else
            {
                cur_int *= 10;
                cur_int += value[0]-48;

            }
            value = value[1..];
        }
        list.Add(cur_int);

        int[] arr = new int[list.Count];

        for (int i = 0; i < list.Count; i++) { arr[i] = list[i]; }

        return arr;
    }


    private string RemoveName(string line)
    {
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '=') { return line.Substring(i+1); }
        }
        return line;
    }




////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//  G  E  T  T  E  R  S      A  N  D      S  E  T  T  E  R  S
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//////////////////////////////////////////////////////////////////////////////////
//  G  E  T  T  E  R  S
//////////////////////////////////////////////////////////////////////////////////

/// Save Slot related

    public bool GetEmptySave() { return EmptySave; }
    public int GetSaveSlot() { return SaveSlot; }

/// Progress Related
    public bool IsGameCompleted() { return GameCompleted; }
    public int GetTotalDeaths() { return TotalDeaths; }
    public float GetTotalPlayTime() { return TotalPlayTime; }
    public int GetCurrentUnlockedShips() { return CurrentUnlockedShips; }
    public int GetUnlockedCannonBalls() { return UnlockedCannonBalls; }
    public int GetUnlockedCannons() { return UnlockedCannons; }

/// Level Related

    public int[] GetLevelStars() { return LevelStars; }
    public int GetLatestUnlockedLevel() { return LatestUnlockedLevel; }

/// Player's Ship Loadout Related 

    public int GetCurrentShipType() { return CurrentSelectedShipSize; }
    public int[] GetSelectedCannons() { return SelectedCannons; }
    public int[] GetSelectedCannonBalls() { return SelectedCannonBalls; }





//////////////////////////////////////////////////////////////////////////////////
//  S  E  T  T  E  R  S
//////////////////////////////////////////////////////////////////////////////////

/// Save Slot related

    public void SetEmptySave(bool x) { EmptySave = x; }
    public void SetSaveSlot(int x) { SaveSlot = x; }

/// Progress Related
    public void SetGameCompleted(bool x) { GameCompleted = x; }
    public void SetTotalDeaths(int x) { TotalDeaths = x; }
    public void SetTotalPlayTime(float x) { TotalPlayTime = x; }
    public void SetCurrentUnlockedShips(int x) { CurrentUnlockedShips = x;; }
    public void SetUnlockedCannonBalls(int x) { UnlockedCannonBalls = x; }
    public void SetUnlockedCannons(int x) { UnlockedCannons = x; }

/// Level Related

    public void SetLevelStars(int[] x) { LevelStars = x; }
    public void SetLatestUnlockedLevel(int x) { LatestUnlockedLevel = x; }

/// Player's Ship Loadout Related 

    public void SetCurrentSelectedShipSize(int x) { CurrentSelectedShipSize = x; }
    public void SetSelectedCannons(int[] x) { SelectedCannons = x; }
    public void SetSelectedCannonBalls(int[] x) { SelectedCannonBalls = x; }


//////////////////////////////////////////////////////////////////////////////////
//  O  T  H  E  R  S
//////////////////////////////////////////////////////////////////////////////////

/// Progress Related
    public void AddDeath() { TotalDeaths++; }
    public void AddPlayTime(float x) { TotalPlayTime += x; } 

/// Level Related

    public int GetXLevelStars(int level) { return LevelStars[level]; }
    public void UpdateXLevelStars(int level, int stars) { LevelStars[level] = stars; }

/// Player's Ship Loadout Related






















//////////////////////////////////////////////////////////////////////////////////
//  D  E  B  U  G
//////////////////////////////////////////////////////////////////////////////////



    private void Debug__PrintSaveFile()
    {
        string str = "";
        
        str += "EmptySave="+EmptySave;
        str += "\nGameCompleted="+GameCompleted;
        str += "\nTotalDeaths="+TotalDeaths;
        str += "\nTotalPlayTime="+TotalPlayTime;
        str += "\nCurrentUnlockedShips="+CurrentUnlockedShips;
        str += "\nUnlockedCannonBalls="+UnlockedCannonBalls;
        str += "\nUnlockedCannons="+UnlockedCannons;
        str += "\nLevelStars=";
        for (int i = 0; i < LevelStars.Length;i++) { str += LevelStars[i]+",";}
        str += "\nLatestUnlockedLevel="+LatestUnlockedLevel;
        str += "\nCurrentSelectedShipSize="+CurrentSelectedShipSize;
        str += "\nSelectedCannons=";
        for (int i = 0; i < SelectedCannons.Length;i++) { str += SelectedCannons[i]+",";}
        str += "\nSelectedCannonBalls=";
        for (int i = 0; i < SelectedCannonBalls.Length;i++) { str += SelectedCannonBalls[i]+",";}

        Debug.Log("Read this File!!!!");
        Debug.Log(str);
    }






}


