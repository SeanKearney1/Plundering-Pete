using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtons : MonoBehaviour
{

    public void Play()
    {
        
    }
    public void Levels() 
    {
        SceneManager.LoadScene("LevelMenuScene");
    }
    public void Settings()
    {

    }
    public void MainMenu()
    {
        SceneManager.LoadScene("TitleScene");
    }
    public void Exit() 
    {
        Application.Quit(); 
    }
}
