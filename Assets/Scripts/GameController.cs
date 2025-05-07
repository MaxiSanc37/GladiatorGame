using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public GameOver gameOver;

    public void GameOver()
    {
        gameOver.Setup();
        Time.timeScale = 0;
    }
}
