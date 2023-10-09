using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void LoadGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Game Scene");
    }
}