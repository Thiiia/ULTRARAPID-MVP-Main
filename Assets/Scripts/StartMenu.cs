using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Slideshow");
    }

    public void LoadTestScene()
    {
        SceneManager.LoadScene("Test");
    }

    public void QuitGame()
    {
        // Only works in standalone builds, not WebGL
        Application.Quit();
    }
}
