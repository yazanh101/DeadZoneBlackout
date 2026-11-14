
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MilitaryBase");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
