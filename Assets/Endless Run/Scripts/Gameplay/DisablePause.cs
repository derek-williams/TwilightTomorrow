using UnityEngine;
using System.Collections;

public class DisablePause : MonoBehaviour {

    public GameObject PauseMenu;
    public GameObject MMA;
    public GameObject PMA;

	// Use this for initialization
	void OnClick () {

        Time.timeScale = 1;
        PauseMenu.SetActive(false);
        MMA.audio.mute = false;
        PMA.audio.mute = true;
	
	}

}
