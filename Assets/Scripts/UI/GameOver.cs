using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject gameOverScreen;

    public GameObject gameplayUI;

    public void Setup()
    {
        gameOverScreen.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
    }

    public void RestartButton()
    {
        SceneManager.LoadScene("Game");
        Time.timeScale = 1.0f;
    }
}
