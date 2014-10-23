using UnityEngine;
using System.Collections;

public class DeathScreen : MonoBehaviour
{
    private bool pauseEnabled = false;
    public GameObject UI;
    public GameObject MMA;
    public GameObject PMA;
    private GameObject hero;

    void Start()
    {
        hero = GameObject.FindGameObjectWithTag("Player");
        PMA.audio.mute = true;
        pauseEnabled = false;
        Time.timeScale = 1;
        Screen.showCursor = false;
    }

    void Update()
    {

        if (hero.activeSelf == false)
        {
            pauseEnabled = true;
            UI.SetActive(true);
            Time.timeScale = 0;
            MMA.audio.mute = true;
            PMA.audio.mute = false;

        }
    }
}
