/// <summary>
/// Game attribute.
/// this script use for set all attribute in game(ex speedgame,character life)
/// </summary>

using UnityEngine;
using System.Collections;

public class GameAttribute : MonoBehaviour {
	// scene changing parameters

	public float starterSpeed = 5; //Speed Character
	public float starterLife = 1; //Life character
	
	[HideInInspector]
	public float distance;
	[HideInInspector]
	public float coin;
	[HideInInspector]
	public int level = 0;
	[HideInInspector]
	public bool isPlaying;
	[HideInInspector]
	public bool pause = false;
	[HideInInspector]
	public bool ageless = false;
	[HideInInspector]
	public bool deleyDetect = false;
	[HideInInspector]
	public float multiplyValue;
	
	//[HideInInspector]
	public float speed = 5;
	[HideInInspector]
	public float life = 3;

	// item/enemy attributes
	public int origKillPlayer = 1; // original kill factor (0-100), 0=disables enemy aggression
	public int origDiffEnemy = 0; // original difficulty (moving items' aggression level 0-100)
	public int origKillPlayerThreshold = 75;
	public int origKillPlayerFactor = 1;
	public float origMaxDistSeePlayer = 20f;
	public int origDiffIncrPerSteps = 5; // amount to increase the difficulty every GameController.speedAddEveryDistance steps

	// weapon attributes
	public float origBatteryLife = 10;
	public float origBatteryDead = 0;
	public float origCoolDownTime = 5;
	public float origWeaponRadius = 1.8f; // number of lanes 


	public static GameAttribute gameAttribute;
	
	void Start(){
		//Setup all attribute
		gameAttribute = this;
		DontDestroyOnLoad(this);
		speed = starterSpeed;
		distance = 0;
		coin = 0;
		life = starterLife;
		level = 0;
		pause = false;
		deleyDetect = false;
		ageless = false;
		isPlaying = true;
	}
	
	public void CountDistance(float amountCount){
		distance += amountCount * Time.smoothDeltaTime;	
	}
	
	public void ActiveShakeCamera(){
		CameraFollow.instace.ActiveShake();	
	}
	
	public void Pause(bool isPause){
		//pause varible
		pause = isPause;
	}
	
	public void Resume(){
		//resume
		pause = false;
	}
	
	public void Reset(){
		//Reset all attribute when character die
		speed = starterSpeed;
		distance = 0;
		coin = 0;
		life = starterLife;
		level = 0;
		pause = false;
		deleyDetect = false;
		ageless = false;
		isPlaying = true;
		Building.instance.Reset();
		Item.instance.Reset();
		//PatternSystem.instance.Reseted();
		CameraFollow.instace.Reset();
		Controller.instace.Reset();
		Controller.instace.timeJump = 0;
		Controller.instace.timeMagnet = 0;
		Controller.instace.timeMultiply = 0;
		Controller.instace.timeSprint = 0;
		GUIManager.instance.Reset();
		Weapon.instance.Reset ();

		// this may need to reset the whole gameController -- this only resets the scene number
		// This may do it (currently hangs the game on reset from pause menu)----- 
		//GameController.instace.StartCoroutine(GameController.instace.ResetGame());
		GameController.instace.ResetSceneCount();

		// this is a hack... sorry... :(
		Application.LoadLevel ("GamePlay");
	}
}
