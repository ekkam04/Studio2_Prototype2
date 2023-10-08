using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] RectTransform fadeImage;
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
        // yield return new WaitForSeconds(1f);

        // LeanTween.scale(fadeImage, Vector3.zero, 0);
        // fadeImage.gameObject.SetActive(true);
        // LeanTween.scale(fadeImage, new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeInOutQuad);

        // yield return new WaitForSeconds(1f);
        // playerController.Respawn();

        // LeanTween.scale(fadeImage, Vector3.zero, 1f).setEase(LeanTweenType.easeInOutQuad);

        LeanTween.alpha(fadeImage, 0, 0);
        // yield return new WaitForSeconds(0.1f);
        fadeImage.gameObject.SetActive(true);

        LeanTween.alpha(fadeImage, 1, 0.5f);
        yield return new WaitForSeconds(1f);

        playerController.Respawn();
        playerController.anim.SetTrigger("dizzy");

        LeanTween.alpha(fadeImage, 0, 0.5f);
        yield return new WaitForSeconds(1f);

        fadeImage.gameObject.SetActive(false);

    }
}
