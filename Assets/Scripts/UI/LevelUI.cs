using System;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelUI : MonoBehaviour
{
    private bool DidPlayerWin;
    private int StartedPreGameMenu = 0;
    private float GameFinishTimeStamp = -69;
    private float OGScreenX;
    private GameObject PreGameMenu;
    private GameObject PauseMenu;
    private GameObject GameLoseScreen;
    private GameObject GameWinScreen;

    private bool InitializedFades = false;
    private List<UnityEngine.UI.Image> FadeImages = new List<UnityEngine.UI.Image>();
    private List<TextMeshProUGUI> FadeTexts = new List<TextMeshProUGUI>();
    private List<float> FadeImagesMaxs = new List<float>();
    private List<float> FadeTextsMaxs = new List<float>();

    void Start()
    {
        PreGameMenu = transform.Find("PreGameMenu").gameObject;
        PauseMenu = transform.Find("PauseMenu").gameObject;
        GameLoseScreen = transform.Find("GameLoseScreen").gameObject;
        GameWinScreen = transform.Find("GameWinScreen").gameObject;

        Debug.Log(GameLoseScreen);

        PauseMenu.SetActive(false);
        GameLoseScreen.SetActive(false);
        GameWinScreen.SetActive(false);
     
    }


    void Update()
    {
        // PreGame menu, put here so background has time to render.
        if (StartedPreGameMenu == 1) { Time.timeScale = 0.0f; }
        if (StartedPreGameMenu < 2)  { StartedPreGameMenu++; }


        if (GameFinishTimeStamp + GeneralGameInfo.Const_GameOverScreenFadeInTime > Time.time) { GameOver(); }
    }


    public void StartGame()
    {
        PreGameMenu.SetActive(false);
        Time.timeScale = 1.0f;
    }



    public void UnPause()
    {
        PauseMenu.SetActive(false);
        Time.timeScale = 1.0f;
    }


    public void PauseLogic()
    {
        Debug.Log("Attempting to Pause!!!");
        if (!PauseMenu.activeSelf)
        {
            PauseMenu.SetActive(true);
            Time.timeScale = 0.0f;
        }
        else
        {
            PauseMenu.SetActive(false);
            Time.timeScale = 1.0f;
        }
    }


    public void ToHomeMenu()
    {
        Debug.Log("Trying to Go Home!!!");
        SceneManager.LoadScene("SaveSlotHomeMenu");
        Time.timeScale = 1.0f;
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1.0f;
    }

    public void NextLevel()
    {
        SceneManager.LoadScene(GetNextLevel());
    }



    private string GetNextLevel()
    {
        string level_name = SceneManager.GetActiveScene().name;
        int cur_level;

        // Level_  = 5;

        Debug.Log("Trying to Parse "+level_name.Substring(6));

        int.TryParse(level_name.Substring(6), out cur_level);

        cur_level++;

        if (cur_level > GeneralGameInfo.Const_TotalLevelCount)
        {
            return "SaveSlotHomeMenu";
        }

        return "Level_"+cur_level;
    }






    public void EnableGameOverUI(bool did_player_win)
    {
        DidPlayerWin = did_player_win;
        GameFinishTimeStamp = Time.time;
        if (DidPlayerWin) 
        {
            OGScreenX = GameWinScreen.transform.position.x;
            GameWinScreen.SetActive(true);
        }
        else
        {
            OGScreenX = GameLoseScreen.transform.position.x;
            GameLoseScreen.SetActive(true);
        }
    }

    private void GameOver()
    {
        if (DidPlayerWin) { SlideInScreen(GameWinScreen); }
        else { FadeInScreen(GameLoseScreen); }
    }


    private void SlideInScreen(GameObject screen)
    {
        float slide_progress = -(OGScreenX*2) * (1-((Time.time - GameFinishTimeStamp) / GeneralGameInfo.Const_GameOverScreenFadeInTime));
        screen.transform.position = new Vector3(slide_progress + OGScreenX, screen.transform.position.y, screen.transform.position.z);
    }



    private void FadeInScreen(GameObject screen)
    {
        float alpha_progress = (Time.time - GameFinishTimeStamp) / GeneralGameInfo.Const_GameOverScreenFadeInTime;
        
        if (!InitializedFades)
        {
            InitializedFades = true;
            foreach (UnityEngine.UI.Image image in screen.GetComponentsInChildren<UnityEngine.UI.Image>())
            {
                FadeImages.Add(image);
                FadeImagesMaxs.Add(image.color.a);
            }
            foreach (TextMeshProUGUI text in screen.GetComponentsInChildren<TextMeshProUGUI>())
            {
                FadeTexts.Add(text);
                FadeTextsMaxs.Add(text.color.a);
            }
        }

        for (int i = 0; i < FadeImages.Count; i++)
        {
            if (alpha_progress > FadeImagesMaxs[i])
            { FadeImages[i].color = new Color(FadeImages[i].color.r,FadeImages[i].color.g,FadeImages[i].color.b,FadeImagesMaxs[i]); }
            else
            { FadeImages[i].color = new Color(FadeImages[i].color.r,FadeImages[i].color.g,FadeImages[i].color.b,alpha_progress); }
        }

        for (int i = 0; i < FadeTexts.Count; i++)
        {
            if (alpha_progress > FadeTextsMaxs[i])
            { FadeTexts[i].color = new Color(FadeTexts[i].color.r,FadeTexts[i].color.g,FadeTexts[i].color.b,FadeTextsMaxs[i]); }
            else
            { FadeTexts[i].color = new Color(FadeTexts[i].color.r,FadeTexts[i].color.g,FadeTexts[i].color.b,alpha_progress); }
        }            
        

    }


}
