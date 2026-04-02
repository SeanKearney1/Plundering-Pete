using UnityEngine;
using UnityEngine.SceneManagement;

public class UIButtons : MonoBehaviour
{

    public void Play()
    {
        SceneManager.LoadScene("LoadSaveFileScene");
    }
    public void Settings()
    {
        SceneManager.LoadScene("SettingsScene");
    }
    public void MainMenu()
    {
        SceneManager.LoadScene("TitleScene");
    }
    public void SaveFileMenu()
    {
        SceneManager.LoadScene("LoadSaveFileScene");
    }
    public void PlayLevel(int level)
    {
        SceneManager.LoadScene("Level_"+level);
    }
    public void Exit() 
    {
        Application.Quit(); 
    }
}
