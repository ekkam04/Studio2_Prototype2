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

            playerController.lives -= 1;
            if (playerController.lives >= 1) {
                print("You have " + playerController.lives + " live(s) left. Respawning...");
                
                playerController.rb.velocity = Vector3.zero;
                playerController.GetComponent<CapsuleCollider>().enabled = false;
                playerController.audioSource.PlayOneShot(playerController.failSound);
                uiManager.PlayRespawnAnimation();
            }
            else
            {
                print("You have no lives left. Game over.");

                playerController.rb.velocity = Vector3.zero;
                playerController.GetComponent<CapsuleCollider>().enabled = false;
                playerController.audioSource.PlayOneShot(playerController.failSound);
                uiManager.PlayGameOverAnimation();
            }
        }
    }
}
