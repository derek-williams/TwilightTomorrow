/// <summary>
/// Game controller.
/// This script use for control game loading and spawn character when load complete
/// </summary>

using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class GameController : MonoBehaviour {
	
	public PatternSystem patSysm; //pattern system
	public CameraFollow cameraFol;	//camera
	public float speedAddEveryDistance = 300;
	public float speedAdd = 0.5f;
	public float speedMax = 20;
	public int selectPlayer;
	public GameObject[] playerPref;
	public Vector3 posStart;
	public bool previewProgressBar;
	public bool useShowPercent;
	public Texture2D textureProgressBar_Frame, textureProgressBar_Color;
	public Rect rect_progressbar, rect_percent_text;

	private float percentCount;
	private float distanceCheck;
	[HideInInspector]
	public int countAddSpeed;
	private CalOnGUI calOnGUI;

	// scene change parameters
	private float sceneDistanceCheck = 0.0f; // counter for scene change
	public float changeSceneEverySteps = 20; // number of steps before a scene change
	public PatternSystem[] patternSystemManagers;
	private float[] sceneSpawnPlayerZPosition; // the z position of the player at the time of a scene change stored in order of currentSceneNumber
	public bool inEndlessMode; // flags endless or story mode
	public bool recycleScenes; // true reuses scenes, false leaves story mode in the last scene indefinitely
	public int startInSceneNumber; // number of scene to start in (0-4 usually)
	public int currentSceneNumber=1; // the current scene number so the relevant pattern manager can be enabled
	private int previousSceneNumber=0; // the previous scene number
	public int maxSceneNumber; // this functions as an END scene for story mode
	[HideInInspector]
	public bool sceneReady = true;
	private float playerSTARTzPos = 16; // hard coded beginning of the first scene's player's z position

	public static GameController instace;

	void Awake(){
		// ADD SHOP AND TITLE SCREEN CONTROL STUFF HERE -- LIKE CHOOSING START SCENE, ENDLESS MODE, AND ANY OTHER OPTIONS....

		// deactivate all psm's so the scene will run correctly...
        foreach (PatternSystem temp in patternSystemManagers) {
            temp.gameObject.SetActive(false);
        }

	}

	void Start(){
		// in case there's no end scene set, use the default of the last one in the list of Pattern Systems
		if (maxSceneNumber < 0 || maxSceneNumber > (int)(patternSystemManagers.Length -1f)) maxSceneNumber = (int)(patternSystemManagers.Length -1f);

		currentSceneNumber = startInSceneNumber;

		// set endless or story
		determineMode ();
		// initiate things and load scenes/pattern systems
		if(Application.isPlaying == true){
			selectPlayer = PlayerPrefs.GetInt("SelectPlayer");
			instace = this;
			calOnGUI = GetComponent<CalOnGUI>();
			StartCoroutine(WaitLoading());
			sceneSpawnPlayerZPosition = new float[patternSystemManagers.Length];
			sceneSpawnPlayerZPosition[0] = playerSTARTzPos;
		}
	}

	void determineMode(){
		// set the only scene to be scene zero if in endless mode -- leave this out and you can set which scene to start in
		if (inEndlessMode)startInSceneNumber = 0;

		// clear all pattern systems by loading and deactivating them
		foreach (PatternSystem patSys in patternSystemManagers){
			//patSys.gameObject.SetActive(true);
			//loadScene(patSys);
			patSys.gameObject.SetActive(false);
		}
		// activate the appropriate start pattern system
		patSysm = patternSystemManagers [startInSceneNumber];
		patSysm.gameObject.SetActive (true);
		//previousSceneNumber = currentSceneNumber;
		//currentSceneNumber = startInSceneNumber;

	}

	// *******************************************************************************
	// THESE ARE NOT USED AT ALL RIGHT NOW....
	void loadScene(PatternSystem temp){
		// activate each patternsystem in succession to load level
		temp.gameObject.SetActive (true);
		StartCoroutine (WaitLoadingPatternSystemsBackground ());
	}

	IEnumerator WaitLoadingPatternSystemsBackground(){
		while(patSysm.loadingComplete == false){
			yield return 0;	
		}
	}
	// *******************************************************************************


	void OnGUI(){
		if(Application.isPlaying == true){
			if(patSysm.loadingComplete == false){
				percentCount = Mathf.Lerp(percentCount, patSysm.loadingPercent, 5 * Time.deltaTime);
				GUI.BeginGroup(new Rect(calOnGUI.SetGUI_Left(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width*(percentCount/100), rect_progressbar.height)));
				if(textureProgressBar_Color == null){
					GUI.Box(new Rect(0,0, calOnGUI.SetGUI_Left(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height).width, calOnGUI.SetGUI(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height).height),"");
					
				}else{
					GUI.DrawTexture(new Rect(0,0, calOnGUI.SetGUI_Left(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height).width, calOnGUI.SetGUI(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height).height),textureProgressBar_Color);
					
				}
				GUI.EndGroup();
				if(textureProgressBar_Frame != null){
					GUI.DrawTexture(new Rect(calOnGUI.SetGUI_Left(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height)), textureProgressBar_Frame);	
				}
				if(useShowPercent)
				GUI.Label(new Rect(calOnGUI.SetGUI(rect_percent_text.x, rect_percent_text.y, rect_percent_text.width , rect_percent_text.height)),percentCount.ToString("0")+"%");
			}
		}else{
			if(previewProgressBar == true){
				if(calOnGUI == null){
					calOnGUI = new CalOnGUI();
				}
				GUI.BeginGroup(new Rect(calOnGUI.SetGUI_Left(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width , rect_progressbar.height)));
				if(textureProgressBar_Color == null){
					GUI.Box(new Rect(0,0, calOnGUI.SetGUI_Left(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height).width, calOnGUI.SetGUI(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height).height),"");
			
				}else{
					GUI.DrawTexture(new Rect(0,0, calOnGUI.SetGUI_Left(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height).width, calOnGUI.SetGUI(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height).height),textureProgressBar_Color);
					
				}
				GUI.EndGroup();
				if(textureProgressBar_Frame != null){
					GUI.DrawTexture(new Rect(calOnGUI.SetGUI_Left(rect_progressbar.x, rect_progressbar.y, rect_progressbar.width, rect_progressbar.height)), textureProgressBar_Frame);	
				}
				if(useShowPercent)
				GUI.Label(new Rect(calOnGUI.SetGUI(rect_percent_text.x, rect_percent_text.y, rect_percent_text.width , rect_percent_text.height)),"100%");
			}
		}
	}
	
	//Loading method
	IEnumerator WaitLoading(){
		while(patSysm.loadingComplete == false){
			yield return 0;	
		}
		StartCoroutine(InitPlayer());
	}
	
	//Spawn player method
	IEnumerator InitPlayer(){
		Debug.Log ("NEW PLAYER!!!");
		GameObject go = (GameObject)Instantiate(playerPref[selectPlayer], posStart, Quaternion.identity);
		cameraFol.target = go.transform;

		yield return 0;
		StartCoroutine(UpdatePerDistance());
	}
	
	//update distance score
	IEnumerator UpdatePerDistance(){
		while(true){
			if(PatternSystem.instance.loadingComplete){
				if(GameAttribute.gameAttribute.pause == false
					&& GameAttribute.gameAttribute.isPlaying == true
					&& GameAttribute.gameAttribute.life > 0){
					if(Controller.instace.transform.position.z > 0){
						GameAttribute.gameAttribute.distance += GameAttribute.gameAttribute.speed * Time.deltaTime;
						distanceCheck += GameAttribute.gameAttribute.speed * Time.deltaTime;
						if(distanceCheck >= speedAddEveryDistance){
							Item.difficulty = Item.difficulty + Item.diffIncrPerSteps;
							if (Item.difficulty > 100) Item.difficulty = 100;
							GameAttribute.gameAttribute.speed += speedAdd;
							if(GameAttribute.gameAttribute.speed >= speedMax){
								GameAttribute.gameAttribute.speed = speedMax;	
							}
							countAddSpeed++;
							distanceCheck = 0;
						}
						sceneDistanceCheck += GameAttribute.gameAttribute.speed * Time.deltaTime;

						// check to see if the scene needs to change
						if(!inEndlessMode && (sceneDistanceCheck > changeSceneEverySteps)){
						//if(!inEndlessMode && (sceneDistanceCheck >= changeSceneEverySteps) && currentSceneNumber < maxSceneNumber){
							//sceneReady = false;
							switchScene();
						}else{
							//if(GameAttribute.gameAttribute.currentSceneNumber > 2) Debug.Log(patSysm.GetInstanceID() );
						}
					}
				}
			}
			yield return 0;
		}
	}

	void switchScene(){
		// change scenes if we're in story mode == activate the next patternSystemManager
		//patSysm.transform.parent.gameObject.SetActive(false);
		previousSceneNumber = currentSceneNumber;
		currentSceneNumber += 1;
		// recycle scenes -- (this only happens if you change the test in UpdatePerDistance() to allow csn>msn)
		if (currentSceneNumber > maxSceneNumber) {
				if (recycleScenes){
					currentSceneNumber = startInSceneNumber;
					reOrientPlayer();
			} else{
				currentSceneNumber = maxSceneNumber;
				previousSceneNumber = currentSceneNumber; // endless mode now...
				return; // no scene switch necessary since we are in endless-nonRecycle mode... just keep going.
				}
			}

		Debug.Log("CHANGE SCENE from "+previousSceneNumber.ToString()+" to "+currentSceneNumber.ToString());

		// capture current z position of player in case we need to respawn in this area
		GameObject temp2 = GameObject.FindGameObjectWithTag("Player");
		sceneSpawnPlayerZPosition[currentSceneNumber] = temp2.transform.position.z;

		popRepopScene ();

		// reset counter
		sceneDistanceCheck = 0.0f;
	}

	void reOrientPlayer(){
		// re orient the player to the current scene position so that when the patternManger gets reactivated, the player is at the beginning of it
		// This is only going to be necessary if you are going back to a scene that has already been loaded. New scenes just load themselves (for now)

		// reset z position of player to match the current scene generation/reactivation
		GameObject temp = GameObject.FindGameObjectWithTag("Player");
		Debug.Log (temp.transform.position);
		temp.transform.position = new Vector3(temp.transform.position.x,temp.transform.position.y,sceneSpawnPlayerZPosition[currentSceneNumber]);
		Debug.Log (" moving "+(temp.transform).ToString()+" from " + sceneSpawnPlayerZPosition [previousSceneNumber].ToString() + " to " + sceneSpawnPlayerZPosition [currentSceneNumber].ToString ());
		Debug.Log (temp.transform.position);

		// reset camera to continue following character
		cameraFol.posCamera.z = temp.transform.position.z + cameraFol.distance;
	}

	void popRepopScene(){
		// in case the scenes aren't being recycled, this avoids needless repopulating and populating
		if (currentSceneNumber == previousSceneNumber) return;

		sceneReady = false;

		// change scene
		patSysm = (patternSystemManagers[currentSceneNumber]);
		patSysm.gameObject.SetActive(true);

		// make the objects appear from current scene and activate the pattern manager
		foreach (GameObject bldg in patSysm.item_Obj) {
			bldg.gameObject.SetActive(true);
		}
		foreach (GameObject bldg in patSysm.building_Obj) {
			bldg.gameObject.SetActive(true);
		}
		//foreach (GameObject bldg in patSysm.floor_Obj) {
		//	bldg.gameObject.SetActive(true);
		//}

		// turn off the old pattSys
		patSysm = (patternSystemManagers[previousSceneNumber]);
		patSysm.gameObject.SetActive(false);

		// make the objects disappear from last scene and deactivate the pattern manager
		foreach (GameObject bldg in patSysm.item_Obj) {
			bldg.gameObject.SetActive(false);
		}
		foreach (GameObject bldg in patSysm.building_Obj) {
			bldg.gameObject.SetActive(false);
		}
		//foreach (GameObject bldg in patSysm.floor_Obj) {
		//	bldg.gameObject.SetActive(false);
		//}
		
		sceneReady = true;
	}

	//reset game
	public IEnumerator ResetGame(){
		// this resets the game after death and is called from Controller


		GameAttribute.gameAttribute.isPlaying = false;
		GUIManager.instance.showSumGUI = true;
		int oldCoind = GameData.LoadCoin ();
		GameData.SaveCoin((int)GameAttribute.gameAttribute.coin+oldCoind);
		distanceCheck = 0;
		countAddSpeed = 0;
		sceneDistanceCheck = 0.0f;


		Weapon.instance.Reset ();
		//StopAllCoroutines();
		//determineMode ();


		yield return 0;	
	}

	public void ResetSceneCount(){
		// this is primarily accessed from GameAttribute only
		// for restarting the game from the pause menu

		determineMode ();
		sceneDistanceCheck = 0.0f;
	}

	void OnApplicationQuit(){
		// deactivate all patternSystem managers to make restarting easier (starting with any active crashes it)
		foreach (PatternSystem temp in patternSystemManagers) {
			temp.gameObject.SetActive(false);
		}
	}
} // end of GameController : MonoBehaviour: class
