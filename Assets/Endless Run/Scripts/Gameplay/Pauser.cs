using UnityEngine;
using System.Collections;

public class Pauser : MonoBehaviour {
	private bool pauseEnabled = false;		
    public GameObject UI;
    public GameObject MMA;
    public GameObject PMA;

    void Start()
    {
        //MMA = GameObject.Find("BG");
        //PMA = GameObject.Find("UI Root");
        //PMA.audio.mute = true;
        //pauseEnabled = false;
        Time.timeScale = 1;
    }
    
    void  Update (){
        
        if(Input.GetButtonDown ("pauseButton")){
           
            if(pauseEnabled == true){
                pauseEnabled = false;
                UI.SetActive(false);
                Time.timeScale = 1;
                MMA.audio.mute = false;
                PMA.audio.mute = true;
              		
            }
            
            else if(pauseEnabled == false){
                pauseEnabled = true;
                UI.SetActive(true);
                Time.timeScale = 0; 
                MMA.audio.mute = true;
                PMA.audio.mute = false;
                
         
            }
        }
    }
}