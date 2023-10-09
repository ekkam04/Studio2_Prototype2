using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void LoadGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Main Menu");
    }
}