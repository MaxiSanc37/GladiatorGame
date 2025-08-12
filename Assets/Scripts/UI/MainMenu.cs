using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Game Closed"); // For viewing in the editor
    }
}
