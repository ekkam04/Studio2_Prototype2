using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnManager : MonoBehaviour
{
    PlayerController playerController;
    UIManager uiManager;
    
    void Start()
    {
        playerController = GameObject.FindObjectOfType<PlayerController>();
        uiManager = GameObject.FindObjectOfType<UIManager>();
    }

    void OnTriggerEnter(Collider col) {
        if (col.gameObject.tag == "Player") {

            if (playerController.lives > 0) {
                playerController.lives -= 1;
                print("You have " + playerController.lives + " lives left. Respawning...");
                // playerController.Respawn();
                uiManager.PlayRespawnAnimation();
            }
            else
            {
                // Game Over
                print("Game Over");
            }
        }
    }
}
