using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController instance;
    public SpriteRenderer background;
    public float playerScore = 0;
    public float playerCombo = 0;
    public float comboBonus = 0;
    public float playerLives = 10;
    public GameObject winImage;
    public GameObject loseImage;
    public bool lost = false;

    public GameObject player;

    public AudioSource audioSource;
    public List<AudioClip> songs;

    private void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
        else
        {
            instance = this;
        }
        ChangeSong(1);
    }
    public void EndGame(bool win)
    {
        if (win)
        {
            winImage.GetComponent<Animator>().SetTrigger("Results");
            StartCoroutine(ResetGame(4));
        }
        else
        {
            lost = true;
            loseImage.GetComponent<Animator>().SetTrigger("Results");
            StartCoroutine(ResetGame(2));
        }
    }
    public IEnumerator ResetGame(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene("MainMenu");
    }
    public void ChangeSong(int level)
    {
        audioSource.Stop();
        audioSource.PlayOneShot(songs[level]);
    }
}
