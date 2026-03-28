using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public void UnPause()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1.0f;
    }

    public void ToMainMenu()
    {
        SceneManager.LoadScene("TitleScene");
        Time.timeScale = 1.0f;
    }

}
