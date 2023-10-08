using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] RectTransform fadeImage;
    [SerializeField] RectTransform[] hearts;

    public GameObject pauseMenu;
    PlayerController playerController;
    
    void Start()
    {
        playerController = GameObject.FindObjectOfType<PlayerController>();
        fadeImage.gameObject.SetActive(false);

        // PlayRespawnAnimation();
    }

    void Update()
    {
        
    }

    public void PlayRespawnAnimation()
    {
        StartCoroutine(RespawnAnimation());
    }

    IEnumerator RespawnAnimation()
    {
        LeanTween.alpha(fadeImage, 0, 0);
        fadeImage.gameObject.SetActive(true);

        LeanTween.alpha(fadeImage, 1, 0.5f);
        PlayHeartDecreaseAnimation();
        yield return new WaitForSeconds(1f);

        playerController.GetComponent<CapsuleCollider>().enabled = true;
        playerController.Respawn();
        playerController.anim.SetTrigger("dizzy");

        LeanTween.alpha(fadeImage, 0, 0.5f);
        yield return new WaitForSeconds(1f);

        fadeImage.gameObject.SetActive(false);

    }

    public void PlayGameOverAnimation()
    {
        StartCoroutine(GameOverAnimation());
    }

    IEnumerator GameOverAnimation()
    {
        LeanTween.alpha(fadeImage, 0, 0);
        fadeImage.gameObject.SetActive(true);

        LeanTween.alpha(fadeImage, 1, 0.5f);
        PlayHeartDecreaseAnimation();

        yield return new WaitForSeconds(1.5f);
        playerController.GetComponent<CapsuleCollider>().enabled = true;
        playerController.Respawn();

        // Game Over
        SceneManager.LoadScene("Game Over Screen");

    }

    public void PlayHeartDecreaseAnimation()
    {
        StartCoroutine(HeartDecreaseAnimation());
    }

    IEnumerator HeartDecreaseAnimation()
    {
        LeanTween.scale(hearts[playerController.lives], Vector3.zero, 1f);
        yield return new WaitForSeconds(1f);
    }

    public void ReplenishHearts()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            LeanTween.scale(hearts[i], Vector3.one, 1f);
        }
    }
}
