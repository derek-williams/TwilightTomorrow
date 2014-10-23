/// <summary>
/// Item
/// this script use for control effect item(ex. duration item,effect item)
/// </summary>

using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour {
	
	public float scoreAdd; //add money if item = coin
	public int decreaseLife; //decrease life if item = obstacle 
	[HideInInspector] public int itemID; //item id
	public float speedMove; //speed move for moving obstacle
	public float duration; // duration item
	public float itemEffectValue; // effect value(if item star = speed , if item multiply = multiply number)
	public ItemRotate itemRotate; // rotate item
	public GameObject effectHit; // effect when hit item
	

	[HideInInspector] public bool itemActive;

	[HideInInspector] public bool isEditing;
	[HideInInspector] public Object targetPref;
	[HideInInspector] public string scenePath;


	[HideInInspector] public Color colorPattern =  new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f);
	
	public enum TypeItem{
		Null, Coin, Obstacle, Obstacle_Roll, ItemJump, ItemSprint, ItemMagnet, ItemMultiply, Moving_Obstacle
	}
	
	public TypeItem typeItem;
	
	[HideInInspector]
	public bool useAbsorb = false;
	
	public static Item instance;

	// AI variables
	// used to detect obstacles ahead of enemy and keep track of lane changing
	private float maxDistEnemyAvoid = 5f; // maximun distance enemies look for obstacles ahead (4 is a guess)
	private float decelerationSpeed = .5f; // speed with which to slow down for obstructed path
	private float origSpeedMove; // the initial speed of the enemies - initialized to speedMove upon start
	private float origDecelerationSpeed; // the initial deceleration speed of the enemies - initialized to decelerationSpeed upon start
	private float stopInNumSecs = 1f; // the number of seconds the enemies will stop in unless they get to close
	private float decelFrameRate = 20f; // basic refresh used to calculate deceleration speed

	private bool isDeviatingRight = false; // in the process of changing lanes (true) or not (false)
	private bool isDeviatingLeft = false; // in the process of changing lanes (true) or not (false)
	private bool isDeviatingUp = false; // in the process of changing lanes (true) or not (false)
	private bool isStopping = false; // in the process of stopping (may also be deviating)
	private float deviatingSpeed = 3f; // speedMove saved for lane changing completion in the case when stopping also
	private float origDeviatingSpeed = 3f; // saves deviating speed - useful to get stopped enemy moving again (not implemented)
	private bool hasStopped = false; // toggles if a moving obstacle WAS moving but has stopped - useful to get stopped enemy moving again (not implemented)

	private int currentLane; // 1, 2, or 3 left to right.
	private float leftLanePos = -1.8f;
	private float rightLanePos = 1.8f;
	private float centerLanePos = 0.0f;
	public static int difficulty; // the difficulty of the game as time progresses. Enemies get smarter as this approaches 100.
	public int killPlayerFactor; // the decision level of the enemy -- this holds the randomly generated decision level, zero is turned off.
	public int killPlayerThreshold; // the value above which the enemy will decide to steer for the player -- max 100
	private bool tryingToKillPlayer = false; // toggles if enemy is actively trying to kill player -- proximity and such
	private float maxDistSeePlayer; // maximum distance the enemies can target the player from
	public static int diffIncrPerSteps; // increases the difficulty this much every speedAddEveryDistance (originally 300 steps) number of steps


	// TESTING ONLY
	private bool rightLaneAheadFree;// = !Physics.Raycast (enemyRayStarbLane, out hitClearLane, maxDist);
	private bool leftLaneAheadFree;// = !Physics.Raycast (enemyRayPortLane, out hitClearLane, maxDist);
	private bool rightLaneFree;// = !Physics.Raycast (enemyRightNextTo, out hitClearNext, 1.5f);
	private bool leftLaneFree;// = !Physics.Raycast (enemyLeftNextTo, out hitClearNext, 1.5f);
	private bool notInRightLane;// = transform.position.x > leftLanePos; // make sure it isn't already as far right as it can go
	private bool notInLeftLane;// = transform.position.x < rightLanePos;

	// ##############################################################################################
	// ##############################################################################################

	void Start(){
		instance = this;
		origSpeedMove = speedMove;
		origDecelerationSpeed = decelerationSpeed;
		hasStopped = false;
		deviatingSpeed = origDeviatingSpeed;
		killPlayerFactor = GameAttribute.gameAttribute.origKillPlayer;
		killPlayerThreshold = GameAttribute.gameAttribute.origKillPlayerThreshold;
		difficulty = GameAttribute.gameAttribute.origDiffEnemy;
		diffIncrPerSteps = GameAttribute.gameAttribute.origDiffIncrPerSteps;
		maxDistSeePlayer = GameAttribute.gameAttribute.origMaxDistSeePlayer;
	}

	void Update(){

	}
	//Set item effect
	public void ItemGet(){
		if(GameAttribute.gameAttribute.deleyDetect == false){
			if(typeItem == TypeItem.Coin){
				HitCoin();
				//Play sfx when get coin
				SoundManager.instance.PlayingSound("GetCoin");
			}else if(typeItem == TypeItem.Obstacle){
				HitObstacle();
				//Play sfx when get hit
				SoundManager.instance.PlayingSound("HitOBJ");
			}else if(typeItem == TypeItem.Obstacle_Roll){
				if(Controller.instace.isRoll == false){
					HitObstacle();
					//Play sfx when get hit
					SoundManager.instance.PlayingSound("HitOBJ");
				}
			}else if(typeItem == TypeItem.ItemSprint){
				Controller.instace.Sprint(itemEffectValue,duration);
				//Play sfx when get item
				SoundManager.instance.PlayingSound("GetItem");
				HideObj();
				initEffect(effectHit);
			}else if(typeItem == TypeItem.ItemMagnet){
				Controller.instace.Magnet(duration);
				//Play sfx when get item
				SoundManager.instance.PlayingSound("GetItem");
				HideObj();
				initEffect(effectHit);
			}else if(typeItem == TypeItem.ItemJump){
				Controller.instace.JumpDouble(duration);
				//Play sfx when get item
				SoundManager.instance.PlayingSound("GetItem");
				HideObj();
				initEffect(effectHit);
			}else if(typeItem == TypeItem.ItemMultiply){
				Controller.instace.Multiply(duration);
				GameAttribute.gameAttribute.multiplyValue = itemEffectValue;
				//Play sfx when get item
				SoundManager.instance.PlayingSound("GetItem");
				HideObj();
				initEffect(effectHit);
			}else if(typeItem == TypeItem.Moving_Obstacle){
				HitObstacle();
				//Play sfx when get hit
				SoundManager.instance.PlayingSound("HitOBJ");
			}
		}
	}

	public void UseMovingItem(){

		if (transform.position.x < (leftLanePos+.3f))
			currentLane = 1;
		if (transform.position.x < (centerLanePos+.5f) && transform.position.x > (centerLanePos-.5f))
			currentLane = 2;
		if (transform.position.x > (rightLanePos-.3f))
			currentLane = 3;
		//////Debug.Log (currentLane + " == currenLane");

		StartCoroutine (MovingItem ());
	}


	IEnumerator MovingItem(){
		while (itemActive) {
			if(!GameAttribute.gameAttribute.pause){
				//only bother checking if obstacle is still moving
				if(speedMove > 0 || hasStopped){ // could also check if an enemy hasStopped and needs to start moving again (other enemy moved for example) -- not implemented
					string toDo = checkLaneAhead(maxDistEnemyAvoid);

					// if there is nothing to avoid or do, start accelerating back to moveSpeed
					if (toDo == "nothing"){
						//Debug.ClearDeveloperConsole();
						isStopping = false;
						speedMove += decelerationSpeed;
						if (hasStopped){
							// if it has stopped, and now it can move, reset flag and turn back on deviating speed
							hasStopped = false;
							deviatingSpeed = origDeviatingSpeed;
						}
						if (speedMove > origSpeedMove) speedMove = origSpeedMove;
						// no reason to ration deceleration, nothing is happening, return deceleration to default
						decelerationSpeed = origDecelerationSpeed;

					}
					// if there's a lane conflict for any reason, just stop
					if((isDeviatingLeft == true && toDo == "right") || (isDeviatingRight == true && toDo == "left") || isStopping){
						//Debug.Log("@@@@@@@@@@@@ LANE CONFLICT @@@@@@@@@@@@@@");
						toDo = "stop";
						isStopping = true;
						isDeviatingLeft = false;
						isDeviatingRight = false;

					}
					// if currently deviating or other action required
					if (toDo != "nothing" || isDeviatingLeft == true || isDeviatingRight == true){
						// if move left and not currently stopping, then move left
						if (toDo == "left" && !isStopping){
							isDeviatingLeft = true;
							// move left
							//Debug.Log("LEFT " + this); // remember, vectors point screen-right, which is portSide
							if(isProximalLaneFree("left")){
							transform.Translate(Vector3.right * deviatingSpeed * Time.deltaTime);
							}else {}//Debug.Log("9999999999999999999999999999999999999999999999999999999999999999");
						}
						// if move right and not currently stopping, then move right
						if (toDo == "right" && !isStopping){
							isDeviatingRight = true;
							// move right
							//Debug.Log("RIGHT " + this);
							if (isProximalLaneFree("right")){
							transform.Translate(Vector3.left * deviatingSpeed * Time.deltaTime);
							}else {}//Debug.Log("6666666666666666666666666666666666666666666666666666666666666666");
						}
						// if needs to stop (also arrives here in the case of an unresolved lane conflict)
						if (toDo == "stop"){
							// usually means something is in front of it, but if it is in the middle
							// of a lane change, it ALSO needs to continue its lane change WHILE stopping
							// also ends up here if there's some kind of conflict of lane (e.g., isDeviatingLeft == true && toDo == "right")

							// continue deviating while stopping -- NOTE: need to stop merge if lane is occupied
							if(isDeviatingLeft && isProximalLaneFree("left")){
								transform.Translate(Vector3.right * deviatingSpeed * Time.deltaTime);
							}
							if(isDeviatingRight && isProximalLaneFree("right")){
								transform.Translate(Vector3.left * deviatingSpeed * Time.deltaTime);
							}
							if(isDeviatingUp){
								// NEEDS WORK - 3/25/14
							}
							// begin/continue stopping
							//Debug.Log("STOPPING " + this);
							isStopping = true;
							speedMove -= decelerationSpeed;
							if (speedMove <= 0.0){
								speedMove = 0.0f; // path obstructed
								deviatingSpeed = 0f;
								hasStopped = true;
							}
						}
						if (toDo == "up"){
							isDeviatingUp = true;
							// move up (ramp)
							//////Debug.Log("MOVING UP " + this);
						}

					}
					// reorient stray lane merges to center the lane
					adjustLane();
					// move forward
					transform.Translate(Vector3.back * speedMove * Time.deltaTime);
				}
			}
			yield return 0;
			
		}
	}

	private string checkLaneAhead(float maxDist)
	// checks in front and to the sides of moving enemies to see if they need to deviate
		// takes a distance in front of enemy to check for obstacles 
		// returns string: "left", "right", "stop", "up", "nothing"
	// The returned string describes what direction the enemy should go to avoid collision
		//(stop is when there's no clear path, and up is for ramps)
	// Determines if currently changing lanes and continues that action while performing another 
		// Also adjusts deceleration speeds in relation to the obstacles ahead's distance
	// After enemy stops, it will seek to speed up again if it is given a path
		// As the game gets more difficult, enemies will randomly decide to steer toward the player
		// with an increasing probability.
	{
		// find position of moving enemy
		Vector3 p = transform.position;
		BoxCollider colItem = GetComponent<BoxCollider> ();
		Vector3 s = colItem.size;

		// setup center ray for initial detection
		Vector3 centerSide = new Vector3 (p.x, p.y + (s.y / 2), p.z);
		Ray enemyRayCenter = new Ray (centerSide, Vector3.back);
		Debug.DrawRay (enemyRayCenter.origin, enemyRayCenter.direction);

		RaycastHit hit; // hit detection variable

		// find left and right edge of enemy (right is negative, up is positive)
		Vector3 starboardSide = new Vector3 (p.x - (s.x / 2), p.y + (s.y / 2), p.z);
		Vector3 portSide = new Vector3 (p.x + (s.x / 2), p.y + (s.y / 2), p.z);
		// setup two rays on either side of enemy
		Ray enemyRayStarboard = new Ray (starboardSide, Vector3.back);
		Debug.DrawRay (enemyRayStarboard.origin, enemyRayStarboard.direction);
		Ray enemyRayPort = new Ray (portSide, Vector3.back);
		Debug.DrawRay (enemyRayPort.origin, enemyRayPort.direction);

		// check to see if the enemy should try to steer toward the player or if it already has decided to
		// if killPlayerFactor is greater than 0, the enemy has already made up its mind. This strategy can be improved in determineKillFactor()
		if (killPlayerFactor > 0) {
			if (difficulty > 100) difficulty=100;
			killPlayerFactor = determineKillFactor (difficulty);
			if (killPlayerFactor > killPlayerThreshold) tryingToKillPlayer = true;
			else tryingToKillPlayer = false;
		}

		// if killPlayerFactor is greater than killPlayerThreshold, the enemy will prefer to steer toward the player

		// check if anything is ahead of enemy (or to either side in the path of enemy)
		if (Physics.Raycast (enemyRayCenter, out hit, maxDist) ||
		    Physics.Raycast (enemyRayStarboard, out hit, maxDist) || 
		    Physics.Raycast (enemyRayPort, out hit, maxDist)) {
			// is it an item or ground? Otherwise ignore (e.g., player)
			if (hit.collider.gameObject.tag == "Item" ||  hit.collider.gameObject.tag == "Obstacle" ||
			    (hit.collider.transform.parent && hit.collider.transform.parent.tag == "Item") ) {
				if (hasStopped) return "stop"; // just stop if it is already at rest and something is ahead.
				// find the obstacle
				Item tempEnemy = hit.collider.transform.gameObject.GetComponent<Item>();
				if(tempEnemy){}//the collider is on the object
				else{//the collider is on the child object
					tempEnemy = hit.collider.transform.parent.GetComponent<Item>();
				}
				//Debug.Log(tempEnemy);
				//Debug.Log(tempEnemy.typeItem);
				if(!tempEnemy) Debug.Log("******************************* "+hit.collider.gameObject.tag);
				if (tempEnemy.typeItem == TypeItem.Obstacle_Roll) {
					//Debug.Log ("Obstacle_Roll HIT!!!");
				}
				if (tempEnemy.typeItem == TypeItem.Null) {
					//Debug.Log ("Null (ground) HIT!!!");
				}
				// is it NOT a coin (or a powerup (?))
				if (tempEnemy.typeItem != TypeItem.Coin) {
					// cast rays into other lanes to see if they are clear
					Vector3 starboardLane = new Vector3 (p.x - (s.x), p.y + (s.y / 2), p.z);
					Vector3 portLane = new Vector3 (p.x + (s.x), p.y + (s.y / 2), p.z);

					// setup rays in other (possible) lanes
					Ray enemyRayStarbLane = new Ray (starboardLane, Vector3.back);
					Debug.DrawRay (enemyRayStarbLane.origin, enemyRayStarboard.direction);
					Ray enemyRayPortLane = new Ray (portLane, Vector3.back);
					Debug.DrawRay (enemyRayPortLane.origin, enemyRayPort.direction);

					//  CHECK SIDES AND DECIDE WHICH WAY TO GO
					// check to the right side of the enemy to make sure there's room to merge
					Vector3 rightSide = new Vector3 (p.x - (s.x / 2), p.y + (s.y / 2), p.z+(s.z/2));
					Vector3 leftSide = new Vector3 (p.x + (s.x / 2), p.y + (s.y / 2), p.z+(s.z/2));
					Ray enemyRightNextTo = new Ray (rightSide, Vector3.left);
					Debug.DrawRay (enemyRightNextTo.origin, enemyRightNextTo.direction);
					// check to the left side of the enemy to make sure there's room to merge
					Ray enemyLeftNextTo = new Ray (leftSide, Vector3.right);
					Debug.DrawRay (enemyLeftNextTo.origin, enemyLeftNextTo.direction);
					RaycastHit hitClearNext = new RaycastHit(); // ray to clear lane to the side
					RaycastHit hitClearLane = new RaycastHit(); // ray to clear the lanes up ahead

					bool isPlayerRight= false;
					bool isPlayerLeft = false;

					// if deciding to steer toward the player, check where s/he is 
					if (killPlayerFactor > killPlayerThreshold){
						// in general, this should be mutually exclusive
						isPlayerRight = isPlayerInLane(enemyRayStarbLane, hitClearLane, maxDist);
						isPlayerLeft = isPlayerInLane(enemyRayPortLane, hitClearLane, maxDist);
						// can't see player, don't bother trying to kill
						if (isPlayerLeft || isPlayerRight) tryingToKillPlayer = true;
						//else tryingToKillPlayer = false;
						if (isPlayerLeft && isPlayerRight) {}//Debug.Log ("==================================== STRANGE PROBLEM!!! =====================");
					}
					//check other lanes and decide which way to go (defaults to the right)
					// hard codes in a lane check as being:middle = 0,screen-right = 1.8,screen-left = -1.8
					rightLaneAheadFree = isLaneFree (enemyRayStarbLane, hitClearLane, maxDist);
					leftLaneAheadFree = isLaneFree (enemyRayPortLane, hitClearLane, maxDist);
					// check lanes right next to the enemy
					 rightLaneFree = !Physics.Raycast (enemyRightNextTo, out hitClearNext, 1.5f);
					 leftLaneFree = !Physics.Raycast (enemyLeftNextTo, out hitClearNext, 1.5f);
					// make sure the enemy is not on the edge lane and therefore can't merge one way without hitting a building
					 notInRightLane = transform.position.x > leftLanePos; // make sure it isn't already as far right as it can go
					 notInLeftLane = transform.position.x < rightLanePos;


					// decide action
					if ((notInRightLane && (rightLaneAheadFree || isPlayerRight) && rightLaneFree)){
						//Debug.Log(this + " HIT! "+tempEnemy+" ("+hit.collider.tag+")"+" - "+tempEnemy.typeItem);
						//Debug.Log(transform.position.x);
						return "right";
					} else if ((notInLeftLane && (leftLaneAheadFree || isPlayerLeft) && leftLaneFree)){
						//Debug.Log(this + " HIT! "+tempEnemy+" ("+hit.collider.tag+")"+" - "+tempEnemy.typeItem);
						//Debug.Log(transform.position.x);
						return "left";
					} else { // nothing clear, must stop
						//Debug.Log(this + " HIT! "+tempEnemy+" ("+hit.collider.tag+")"+" - "+tempEnemy.typeItem + " NOTHING CLEAR!!");
						//Debug.Log(transform.position.x);
						
						// adjust deceleration speed relative to number of seconds to stop in and distance to obstacle
						// find distance between the obstacle and adjust deceleration speed by this
						// distToObstacle is set to closest distance to nearest imminent collision from middle or either side of enemy
						// (this isn't necessarily going to be the original obstacle that the enemy is trying to avoid)
						RaycastHit starObs;
						RaycastHit portObs;
						float starDist=0f;
						float portDist=0f;
						float distToObstacle=0f;

						if(Physics.Raycast (enemyRayStarboard, out starObs, maxDist)){
							starDist = Vector3.Distance(this.transform.position,starObs.collider.transform.position);
						}
						if(Physics.Raycast (enemyRayPort, out portObs, maxDist)){
							portDist = Vector3.Distance(this.transform.position,portObs.collider.transform.position);
						}

						distToObstacle = Vector3.Distance(this.transform.position,tempEnemy.transform.position);
						if (starDist > 0f && starDist < distToObstacle) distToObstacle = starDist;
						if (portDist > 0f && portDist < distToObstacle) distToObstacle = portDist;

						// decrease speed over a second over 20 frames over the distance left to stop
						decelerationSpeed -= (1f / (stopInNumSecs*decelFrameRate)) / distToObstacle;
						if (decelerationSpeed < 0) decelerationSpeed = 0f;
						//Debug.Log("DECELERATING ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
						//Debug.Log(decelerationSpeed);
						if (distToObstacle < 1.5f){
							decelerationSpeed = 1000f;
						}
						return "stop";
						}
				} // end if not coin
			} // end if item, ground, or obstacle
			if(hit.collider.transform.gameObject)//Debug.Log(hit.collider.transform.gameObject);
			if(hit.collider.transform.parent){
				//Debug.Log(hit.collider.transform.parent);
				//Debug.Log(hit.collider.transform.parent.tag);
			}
		} // end of if raycast hit something

		// if the center ray didn't hit anything, or what it hit was not an item, ground, or obstacle,
		// it may be in the middle of changing lanes - continue
		if (isDeviatingRight == true) {
						return "right";
				} else if (isDeviatingLeft == true) {
						return "left"; // otherwise, you may decide to steer toward the player (at this point it won't be in front of you)
		} else 		if (tryingToKillPlayer) { // NOTE: prob needs to check "isStopping" to make sure it stops before trying to kill the player and player proximity for efficiency
			//Debug.Log("KILL THE PLAYER!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
			//tryingToKillPlayer = true;
			// steer toward the player using a modified lane ray... see below.
			Vector3 starboardLane = new Vector3 (p.x - (s.x * 1.25f), p.y + (s.y / 2), p.z);
			Vector3 portLane = new Vector3 (p.x + (s.x * 1.25f), p.y + (s.y / 2), p.z);

			// NOTE: the mid-point of the lane DO NOT find the player in all cases.
			// This could be remedied with more rays, rays further out, but this will depend
			// on the size and shape of the different players.
			//Vector3 starboardLane = new Vector3 (p.x - (s.x), p.y + (s.y / 2), p.z);
			//Vector3 portLane = new Vector3 (p.x + (s.x), p.y + (s.y / 2), p.z);


			// setup rays in other (possible) lanes
			Ray enemyRayStarbLane = new Ray (starboardLane, Vector3.back);
			Debug.DrawRay (enemyRayStarbLane.origin, enemyRayStarboard.direction);
			Ray enemyRayPortLane = new Ray (portLane, Vector3.back);
			Debug.DrawRay (enemyRayPortLane.origin, enemyRayPort.direction);
			RaycastHit hitClearLane = new RaycastHit (); // ray to clear the lanes up ahead
			
			if (isPlayerInLane (enemyRayStarbLane, hitClearLane, maxDistSeePlayer)) {
				//Debug.Log("Player is in the right lane...");
				// check right lane and head right if clear
				Vector3 rightSide = new Vector3 (p.x - (s.x / 2), p.y + (s.y / 2), p.z + (s.z / 2));
				Ray enemyRightNextTo = new Ray (rightSide, Vector3.left);
				Debug.DrawRay (enemyRightNextTo.origin, enemyRightNextTo.direction);
				RaycastHit hitClearNext = new RaycastHit (); // ray to clear lane to the side
				if (!Physics.Raycast (enemyRightNextTo, out hitClearNext, 1.5f) && transform.position.x > leftLanePos) {
					// go right if there's noone to the right and you're not in the right lane
					return "right";
				}else return "nothing"; // can't turn now - something is in the way
			} else if (isPlayerInLane (enemyRayPortLane, hitClearLane, maxDistSeePlayer)) {
				//Debug.Log("Player is in the left lane...");
				// check left lane and head left if clear
				Vector3 leftSide = new Vector3 (p.x + (s.x / 2), p.y + (s.y / 2), p.z + (s.z / 2));
				Ray enemyLeftNextTo = new Ray (leftSide, Vector3.right);
				Debug.DrawRay (enemyLeftNextTo.origin, enemyLeftNextTo.direction);
				RaycastHit hitClearNext = new RaycastHit (); // ray to clear lane to the side
				if (!Physics.Raycast (enemyLeftNextTo, out hitClearNext, 1.5f) && transform.position.x < rightLanePos) {
					// go left if there's noone to the left and you're not in the left lane
					return "left";
				}else return "nothing"; // can't turn now -- something in the way
			} else{
				//tryingToKillPlayer = false;
				return "nothing"; // can't find the player -- just continue straight
			}
		}else return "nothing"; // just continue as before if I haven't decided to kill the player


	} // end of checkLaneAhead()

	private bool isLaneFree(Ray r, RaycastHit hit, float dist){
		// determines if the lane in question is free taking into consideration whether or not
		// the enemy is trying to steer for the player or not
		// Takes a ray, hit, and max distance to check
		// Returns true if the lane is free (or if the enemy is trying to steer toward the player and s/he is in the lane)
		if (!Physics.Raycast (r, out hit, dist)){
			return true; // nothing in lane at all
		} else{
			return false; // if there's something there but can't figure out what
		}
	}

	private bool isProximalLaneFree (string side){
		// this is mostly redundant, but may be worked in to the code later to streamline things
		Vector3 p = transform.position;
		BoxCollider colItem = GetComponent<BoxCollider> ();
		Vector3 s = colItem.size;
		RaycastHit hitClearNext = new RaycastHit (); // ray to clear lane to the side

		if (side == "right") {
						Vector3 rightSide = new Vector3 (p.x - (s.x / 2), p.y + (s.y / 2), p.z + (s.z / 2));
						Ray enemyRightNextTo = new Ray (rightSide, Vector3.left);
						Debug.DrawRay (enemyRightNextTo.origin, enemyRightNextTo.direction);
						// check lanes right next to the enemy
						return(!Physics.Raycast (enemyRightNextTo, out hitClearNext, 1.5f));
				} else {
			Vector3 leftSide = new Vector3 (p.x + (s.x / 2), p.y + (s.y / 2), p.z + (s.z / 2));
			// check to the left side of the enemy to make sure there's room to merge
			Ray enemyLeftNextTo = new Ray (leftSide, Vector3.right);
			Debug.DrawRay (enemyLeftNextTo.origin, enemyLeftNextTo.direction);
			return (!Physics.Raycast (enemyLeftNextTo, out hitClearNext, 1.5f));

		}

	}

	private bool isPlayerInLane(Ray r, RaycastHit hit, float dist){
		// determines if the player is in the lane in question
		// returns false if not in lane or soemthing else is

		if (!Physics.Raycast (r, out hit, dist)){
			return false; // nothing in lane at all
		} else{
			// something is in the right lane, check it for player
			// (need to make sure the wrong object heirarchy doesn't throw an error)
			if(hit.collider.transform.gameObject){
				if(hit.collider.transform.gameObject.tag == "Player"){
					// steer toward the player if enemy has decided to
					return true;
				} else if(hit.collider.transform.parent){
					if(hit.collider.transform.parent.tag == "Player"){
						// steer toward the player if enemy has decided to
						return true;
					}
				} else {
					// player is not in this lane
					return false;
				}
			}
			return false; // if there's something there but can't figure out what
		}
	}

	private int determineKillFactor (int difficulty){
		// Takes a int of the difficulty of the game (<=100)at this point and returns a (int) kill factor
		// It randomly decides to kill the player to a returned extent with increasing probability
		// based on the difficulty level

		// choose randomly with increasing change with difficulty
		int chanceToKill = Random.Range (difficulty, 101);

		// return float (<= 100) to represent level of kill
		return chanceToKill;
	}

	private void adjustLane(){
		// adjusts to the middle of the lane and sets any isDeviating flags to false

		// LEFT deviating
		if (isDeviatingLeft && currentLane == 1 && transform.position.x > (centerLanePos-.1f) && transform.position.x < (centerLanePos+.1)){
			currentLane = 2;
			transform.position = new Vector3 (centerLanePos,transform.position.y,transform.position.z);
			isDeviatingLeft = false;
			//Debug.Log("*************************************************");

		}
		if (isDeviatingLeft && currentLane == 2 && transform.position.x > (rightLanePos-.1f)){
			currentLane = 3;
			transform.position = new Vector3 (rightLanePos,transform.position.y,transform.position.z);
			isDeviatingLeft = false;
			//Debug.Log("*************************************************");

		}

		// RIGHT deviating
		if (isDeviatingRight && currentLane == 3 && transform.position.x > (centerLanePos-.1f) && transform.position.x < (centerLanePos+.1f)){
			currentLane = 2;
			transform.position = new Vector3 (centerLanePos,transform.position.y,transform.position.z);
			isDeviatingRight = false;
			//Debug.Log("*************************************************");

		}
		if (isDeviatingRight && currentLane == 2 && transform.position.x < (leftLanePos+.1f)){
			currentLane = 1;
			transform.position = new Vector3 (leftLanePos,transform.position.y,transform.position.z);
			isDeviatingRight = false;
			//Debug.Log("*************************************************");
		}

	}

	//Coin method
	private void HitCoin(){
		if(Controller.instace.isMultiply == false){
			GameAttribute.gameAttribute.coin += scoreAdd;
		}else{
			GameAttribute.gameAttribute.coin += (scoreAdd)*GameAttribute.gameAttribute.multiplyValue;
		}
		initEffect(effectHit);
		HideObj();
	}
	
	//Obstacle method
	private void HitObstacle(){
		if(GameAttribute.gameAttribute.ageless == false){
			if(Controller.instace.timeSprint <= 0){
				GameAttribute.gameAttribute.life -= decreaseLife;
				GameAttribute.gameAttribute.ActiveShakeCamera();
			}else{
				HideObj();
				GameAttribute.gameAttribute.ActiveShakeCamera();
			}
			
		}
	}
	
	//Spawn effect method
	private void initEffect(GameObject prefab){
		GameObject go = (GameObject) Instantiate(prefab, Controller.instace.transform.position, Quaternion.identity);
		go.transform.parent = Controller.instace.transform;
		go.transform.localPosition = new Vector3(go.transform.localPosition.x, go.transform.localPosition.y+0.5f, go.transform.localPosition.z);	
	}
	
	//Magnet method
	public IEnumerator UseAbsorb(GameObject targetObj){
		bool isLoop = true;
		useAbsorb = true;
		while(isLoop){
			this.transform.position = Vector3.Lerp(this.transform.position, targetObj.transform.position, GameAttribute.gameAttribute.speed*2f * Time.smoothDeltaTime);
			if(Vector3.Distance(this.transform.position, targetObj.transform.position) < 0.6f){
				isLoop = false;	
				SoundManager.instance.PlayingSound("GetCoin");
				HitCoin();
			}
			yield return 0;
		}
		Reset();
		StopCoroutine("UseAbsorb");
		yield return 0;
	}
	
	public void HideObj(){
		if(useAbsorb == false){
			this.transform.parent = null;
			this.transform.localPosition = new Vector3(-100,-100,-100);
		}
	}
	
	public void Reset(){
		itemActive = false;
		this.transform.position = new Vector3(-100,-100,-100);
		this.transform.parent = null;
		useAbsorb = false;
	}

	private bool isSelect = false;
	[HideInInspector] public GameObject point1, point2, point3;
	[HideInInspector] public GameObject textZ, textY;
	[HideInInspector] public Vector3 position1, position2, position3;

	[HideInInspector] public float distanceZ = 1, distanceY = 1;

	#if UNITY_EDITOR
	[ExecuteInEditMode]
	public void OnDrawGizmos(){
		if(Application.isPlaying == false && isEditing == true){
			if (UnityEditor.Selection.Contains (gameObject) && isSelect == false) {
				//Debug.Log("Select");
				CreatePointSelect();
				isSelect = true;
			}else if(!UnityEditor.Selection.Contains (gameObject) && !UnityEditor.Selection.Contains (point1) 
			         && !UnityEditor.Selection.Contains (point2) && !UnityEditor.Selection.Contains (point3)
			         && !UnityEditor.Selection.Contains (textZ) && !UnityEditor.Selection.Contains (textY) && isSelect == true){
				//Debug.Log("Discount Select");
				DistroyPointSelect();
				isSelect = false;
			}
			
			if(point1 != null && point2 != null && point3 != null){
				Gizmos.color = Color.blue;
				point2.transform.position = new Vector3(transform.position.x, transform.position.y, point2.transform.position.z);
				Gizmos.DrawLine (point1.transform.position, point2.transform.position);
				Gizmos.color = Color.green;
				point3.transform.position = new Vector3(transform.position.x, point3.transform.position.y, transform.position.z);
				Gizmos.DrawLine (point1.transform.position, point3.transform.position);
				Gizmos.color = Color.green;
				Gizmos.DrawLine (point3.transform.position, new Vector3(point2.transform.position.x, point3.transform.position.y, point2.transform.position.z));

				position1 = point1.transform.position;
				position2 = point2.transform.position;
				position3 = point3.transform.position;


				textZ.transform.rotation = UnityEditor.SceneView.lastActiveSceneView.rotation;
				textY.transform.rotation = UnityEditor.SceneView.lastActiveSceneView.rotation;

				textZ.transform.position = point1.transform.position + ((point2.transform.position - point1.transform.position).normalized * Vector3.Distance(point1.transform.position, point2.transform.position))/2;
				textY.transform.position = point1.transform.position + ((point3.transform.position - point1.transform.position).normalized * Vector3.Distance(point1.transform.position, point3.transform.position))/2;
				distanceZ = Vector3.Distance (point2.transform.position, point1.transform.position);
				distanceY = Vector3.Distance (point3.transform.position, point1.transform.position);
				textZ.GetComponent<TextMesh> ().text = Vector3.Distance (point2.transform.position, point1.transform.position).ToString ("0.00");
				textY.GetComponent<TextMesh> ().text = Vector3.Distance (point3.transform.position, point1.transform.position).ToString ("0.00");
			}
		}
	}

	private void CreatePointSelect(){
		point1 = new GameObject ("Point1");
		point1.transform.parent = transform;
		if(position1.x == 0 && position1.y == 0 && position1.z == 0){
			point1.transform.position = transform.position;
		}else{
			point1.transform.position = position1;
		}
		point1.transform.localRotation = Quaternion.identity;

		point2 = new GameObject ("Point2");
		point2.transform.parent = transform;
		if(position2.x == 0 && position2.y == 0 && position2.z == 0){
			point2.transform.position = transform.position + transform.forward;
		}else{
			point2.transform.position = position2;
		}
		point2.transform.localRotation = Quaternion.identity;

		point3 = new GameObject ("Point3");
		point3.transform.parent = transform;
		if(position3.x == 0 && position3.y == 0 && position3.z == 0){
			point3.transform.position = transform.position + transform.up;
		}else{
			point3.transform.position = position3;
		}
		point3.transform.localRotation = Quaternion.identity;

		textZ = (GameObject)Instantiate ((Object)Resources.Load ("TextMesh"), Vector3.zero, Quaternion.identity);
		textY = (GameObject)Instantiate ((Object)Resources.Load ("TextMesh"), Vector3.zero, Quaternion.identity);

		textZ.transform.parent = transform;
		textY.transform.parent = transform;

		textZ.transform.position = (point2.transform.position - point1.transform.position) / 2;
		textY.transform.position = (point3.transform.position - point1.transform.position) / 2;

		textZ.transform.localScale = new Vector3 (0.01f, 0.01f, 0.01f);
		textY.transform.localScale = new Vector3 (0.01f, 0.01f, 0.01f);


		textZ.GetComponent<TextMesh> ().text = Vector3.Distance (point2.transform.position, point1.transform.position).ToString ("0.00");
		textY.GetComponent<TextMesh> ().text = Vector3.Distance (point3.transform.position, point1.transform.position).ToString ("0.00");

		textZ.GetComponent<TextMesh> ().fontSize = 100;
		textY.GetComponent<TextMesh> ().fontSize = 100;

		textZ.GetComponent<TextMesh> ().anchor = TextAnchor.UpperCenter;
		textY.GetComponent<TextMesh> ().anchor = TextAnchor.UpperCenter;

		point1.AddComponent<PointSelection> ();
		point2.AddComponent<PointSelection> ();
		point3.AddComponent<PointSelection> ();
		point1.GetComponent<PointSelection> ().color =  (Color.yellow);
		point2.GetComponent<PointSelection> ().color = (Color.blue);
		point3.GetComponent<PointSelection> ().color = (Color.green);
	}

	private void DistroyPointSelect(){
		if(point1 != null && point2 != null && point3 != null){
			DestroyImmediate (point1.gameObject);
			DestroyImmediate (point2.gameObject);
			DestroyImmediate (point3.gameObject);
			DestroyImmediate (textZ.gameObject);
			DestroyImmediate (textY.gameObject);
		}
	}
	#endif
}
