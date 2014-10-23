using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public Light beam;
	public float batteryLife;
	private float batteryLifeCounter;
	public float batteryDead;
	public float coolDownTime;
	public float coolDownCounter;
	public float radius; // 
	public int fullOn = 8; // seems to be a spotlight restriction
	public int fullOff = 0;
	public float batteryDecrease = 1f; // amount per second the batteryLife and coolDown decreases while on
	public float weaponRange; // range of weapon back into scene
	private float minWeaponRange = 5f; // minimum functioning distance for weapon
	public bool isOn = false; // whether or not the weapon is on
	public float isSputtering = 0f; // increments while sputtering -- 0 = not sputtering
	private float sputterLength = 48f; // how many frames it should sputter for
	private float sputterInc; // the increment counter of the sputtering effect (light on/off every sputterInc frames)
	private float origSputterInc = 12f;
	public GameObject[] weaponPref;
	public int prefWeaponNum = 1; //  this is the preferred weapon which should be set by the store/PlayerPrefs however it needs to be

	public int tunnelLayer = 11; // hard coded number of the layer for the tunnels -- necessary to avoid grouping tunnels with other obstacles for detection

	public static Weapon instance;

	// Use this for initialization
	public void Start () {
		if (weaponRange < minWeaponRange)weaponRange = minWeaponRange;
		// deactivate all weapon choices
		for (int i = 0; i < weaponPref.Length; i++){
			weaponPref [i].SetActive (false);
		} 
		// set chosen weapon to active
		// needs implementation...
		// enable weaponPref[PlayerPrefs.GetInt ("SelectWeapon")];
		weaponPref [prefWeaponNum].SetActive (true);

		Reset (); // setup all flags and such to start state (off, not sputtering, full battery, and fully cooled down
	}

	void onEnable(){
		Debug.Log ("HELLO WORLD, weapon enabled."); // if anyone knows why this never gets called, please let me know!
		Reset ();
	}

	public void Reset(){

		sputterInc=origSputterInc;
		instance = this;
		beam.intensity = fullOff;
		isOn = false;
		batteryLife = GameAttribute.gameAttribute.origBatteryLife;
		batteryLifeCounter = batteryLife;
		batteryDead = GameAttribute.gameAttribute.origBatteryDead;
		coolDownTime = GameAttribute.gameAttribute.origCoolDownTime;
		coolDownCounter = coolDownTime;
		radius = GameAttribute.gameAttribute.origWeaponRadius;
		isSputtering = 0f;

	}

	// Update is called once per frame
	void Update () {
		//Debug.Log (isSputtering);
		//Debug.Log(coolDownCounter);
		// check to make sure battery is still on
		// if battery is off, cool down period increments

		// if it is sputtering, toggle every frame for sputterLength/sputterInc frames.
		if (isSputtering > 0) sputter();
		// otherwise, keep it on and check to see if it hit anything
		else {
				if (isOn) {
					if(Time.frameCount%72 == 0) Debug.Log("discharging..." + batteryLifeCounter + "/" + batteryLife);
					stayOn();
					// cast rays for firing
					fireWeapon ();
				}
				else{	
					if(Time.frameCount%72 == 0 && coolDownCounter < coolDownTime) Debug.Log("recharging..." +coolDownCounter + "/" + coolDownTime);
					coolDown();
				}
			} // end if sputtering/else block


	}

	public void turnOn(){
		isOn = true;
		beam.intensity = fullOn;
		}
	public void turnOff(){
		isOn = false;
		beam.intensity = fullOff;
	}
	public void toggle(){
		// turn on if recharged, turn off otherwise
		if (isOn) {
						turnOff ();
						Debug.Log ("OFF....... ----"+coolDownCounter);

				} else {
						if (batteryLifeCounter >= batteryDead || isSputtering > 0){
							Debug.Log (".......ON ----"+coolDownCounter);
							turnOn ();
						}
						else {
							// start sputtering if it isn't already, continue if it is
							if (isSputtering == 0) isSputtering = 1f;
						}
				}
	}
	public void sputter(){
		// turn on and off for a second and sputter
		isSputtering += 1f; // timer for overall sputtering effect
		sputterInc -= 1f; // timer for intermittant sputtering
		if (sputterInc <= 0){
			sputterInc = origSputterInc; // reset intermittant sputtering timer
			toggle ();
		}
		if (isSputtering > sputterLength) {
			// stop sputtering in the off position
			turnOff ();
			isSputtering = 0f;
			sputterInc = origSputterInc;
		}

	}

	public void coolDown(){
		// recharge if off, and cool down if off
		coolDownCounter += Time.deltaTime * batteryDecrease;
		if (coolDownCounter >= coolDownTime) {
			batteryLifeCounter = batteryLife;
			coolDownCounter = coolDownTime;
		}
	}

	public void stayOn(){
		// increase need for weapon to cool down
		coolDownCounter -= Time.deltaTime * batteryDecrease;
		if (coolDownCounter <= 0.0f)
			coolDownCounter = 0.0f;
		// decrease battery life
		batteryLifeCounter -= Time.deltaTime * batteryDecrease;
		if (batteryLifeCounter <= batteryDead) {
			// turn off weapon and initiate coolDown if dead battery
			isOn = false;
			turnOff ();
		}

	}
	private void fireWeapon(){
		// check to see if the beam hit anything and destroy it
		// setup rays
		//Debug.Log ("FIRE!!!");

		// find center of player
		Renderer tempTrans = transform.GetComponentInChildren <Renderer> ();

		Bounds b = tempTrans.renderer.bounds;
		Vector3 c = new Vector3 (b.center.x, b.max.y, b.center.z);
		// draw a ray from the center of the player
		Vector3 p = Vector3.Lerp(transform.position,c,.8f); // position of the firedRay (half way between head and center)
		Ray weaponRay = new Ray (p, Vector3.forward);
		Debug.DrawRay (weaponRay.origin, weaponRay.direction);
		RaycastHit hit;

		// see if you hit anything killable
		if (Physics.Raycast (weaponRay, out hit, weaponRange)) {

			GameObject temp = hit.collider.gameObject;
			//Debug.Log(temp);
			// search up the heirarchy to see if you hit an Item
			while(temp.transform.parent != null){
				GameObject target = temp.transform.gameObject; // was: target = temp.transform.parent.gameObject;
				// disable (kill) it if it is an Item of tag, item, ground, or obstacle
				if (target.GetComponent<Item>()) {
					Item tempItem = target.GetComponent<Item>();
					// check and see if it's killable *************************  ADD other TARGETS here.
					// || tempItem.typeItem == Item.TypeItem.Obstacle_Roll || (tempItem.typeItem == Item.TypeItem.Obstacle && tempItem.gameObject.layer != tunnelLayer)
					if (tempItem.typeItem == Item.TypeItem.Moving_Obstacle){
							// destroying doesn't work because the pattern system manager (among others) references these objects later on upon restart
							//Destroy(target.gameObject);
							target.gameObject.GetComponent<Item>().Reset();// target.gameObject.SetActive(false);
					}
					break; // exit the while loop and do nothing if you hit an Item, but it's not killable (e.g., ramps, tunnels, etc.)
				}
				//Debug.Log( temp.transform.parent);
				temp = (GameObject) temp.transform.parent.transform.gameObject; // iterate up the heirarchy
			}
			// tidy up


			}
			else {}//Debug.Log("&&&&&&&&&&&&&&, "+temp);


			//Debug.Log("***********************************************Hit, "+temp);
	
		// cast back into z axis
		// if hit something, destroy it
		}



} // end of class
