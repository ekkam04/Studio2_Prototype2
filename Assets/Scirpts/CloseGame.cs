using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseGame : MonoBehaviour
{
    public void EndGame()
    {
        Time.timeScale = 1;
        Application.Quit();
    }
}