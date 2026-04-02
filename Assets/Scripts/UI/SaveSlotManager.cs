using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSlotManager : MonoBehaviour
{
    public GameObject Warning;
    private SessionManager davyjones;
    private int wiping_save_slot;

    void Start()
    {
        Warning.SetActive(false);
        davyjones = GameObject.Find("DavyJones").gameObject.GetComponent<SessionManager>();
        UpdateInfo();
    }



    private void UpdateInfo()
    {
        SaveFile[] saveFiles = new SaveFile[GeneralGameInfo.Const_SaveSlotCount];
        for (int i = 0; i < saveFiles.Length; i++) { saveFiles[i] = SaveFile.GetSaveSlot(i+1); }
        FillSaveSlotStats(saveFiles); 
    }



    public void SelectedSaveSlot(int save_slot)
    {
        Debug.Log("Selected Save Slot "+save_slot+"!");

        davyjones.SetCurrentSaveFile(SaveFile.GetSaveSlot(save_slot));


        SceneManager.LoadScene("SaveSlotHomeMenu");
    }

    public void AskWipeSaveSlot(int save_slot)
    {
        Warning.SetActive(true);
        wiping_save_slot = save_slot;
    }

    public void WipeSaveSlot()
    {
        SaveFile.ResetSlot(wiping_save_slot);
        WarningDisable();
        UpdateInfo();
    }

    public void WarningDisable()
    {
        Warning.SetActive(false);
    }



    private void FillSaveSlotStats(SaveFile[] saveFiles)
    {
        for (int i = 0; i < transform.childCount;i++)
        {
            FillSaveSlotInfo(transform.GetChild(i).gameObject, saveFiles[i], i);
        }
    }

    private void FillSaveSlotInfo(GameObject slot, SaveFile saveFile, int slot_num)
    {
        int total_stars = 0;
        TimeSpan total_time;
        TextMeshProUGUI title = slot.transform.Find("Title").GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI save_button = slot.transform.Find("Save Button").GetComponentInChildren<TextMeshProUGUI>();
        GameObject inputs = slot.transform.Find("Inputs").gameObject;
        TextMeshProUGUI levels = inputs.transform.Find("Completed Levels").GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI awards = inputs.transform.Find("Awards").GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI stars = inputs.transform.Find("Stars").GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI time = inputs.transform.Find("Total Time").GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI deaths = inputs.transform.Find("Total Deaths").GetComponentInChildren<TextMeshProUGUI>();


        if (saveFile.GetEmptySave())
        {
            title.text = "Empty #"+slot_num;
            save_button.text = "New Save";
            levels.text = "NA";
            awards.text = "NA";
            stars.text = "NA";
            time.text = "NA";
            deaths.text = "NA";
        }
        else
        {
            title.text = "Save #"+slot_num;
            save_button.text = "Load Save";
            levels.text = saveFile.GetLatestUnlockedLevel()-1 + " / " + GeneralGameInfo.Const_TotalLevelCount; 
            awards.text = "67 / 69";

            for (int i = 0; i < saveFile.GetLevelStars().Length; i++) { total_stars += saveFile.GetXLevelStars(i); }
            stars.text = total_stars + " / " + GeneralGameInfo.Const_TotalLevelCount*3;

            total_time = TimeSpan.FromSeconds(saveFile.GetTotalPlayTime());
            time.text = total_time.ToString("hh':'mm':'ss");

            deaths.text = ""+saveFile.GetTotalDeaths();
        }

    }



}
