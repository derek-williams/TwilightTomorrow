/// <summary>
/// Sound manager.
/// This script use for manager all sound(bgm,sfx) in game
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour {
	
	[System.Serializable]
	public class SoundGroup{
		public string soundName;
		public AudioClip audioClip;
	}

	[System.Serializable]
	public class LevelList{
		// example: level 1, level 2
		public string levelName;
		public List<SoundGroup> sounds = new List<SoundGroup> ();
	}
	[System.Serializable]
	public class TypeList{
		// example: jump, hitObj
		public string typeName;
		public List<LevelList> levels = new List<LevelList> ();
	}

	[System.Serializable]
	public class SoundList{
		// example: characters, enemies
		public string soundType;
		public List<TypeList> sound_Kinds = new List<TypeList> ();

	}
	public AudioSource bgmSound;
	
	public List<SoundGroup> sound_List = new List<SoundGroup>();
	public List<SoundList> sound_Types = new List<SoundList> ();


	public static SoundManager instance;
	
	public void Start(){
		instance = this;	
		StartCoroutine(StartBGM());
	}
	
	public void PlayingSound(string _soundName){
		AudioSource.PlayClipAtPoint(sound_List[FindSound(_soundName)].audioClip, Camera.main.transform.position);
	}
	
	private int FindSound(string _soundName){
		int i = 0;
		while( i < sound_List.Count ){
			if(sound_List[i].soundName == _soundName){
				return i;	
			}
			i++;
		}
		return i;
	}

	public void PlayingSound2(string _soundName, string _type, string _kind, string _level){
		AudioClip useThis = FindSound2(_soundName, _type, _kind, _level);

		AudioSource.PlayClipAtPoint(useThis, Camera.main.transform.position);
	}

	private AudioClip FindSound2(string _soundName, string _type, string _kind, string _level){
		// finds the requested audio clip and returns it or NULL

		int t = 0;
		int k = 0;
		int l = 0;
		int n = 0;
		while( t < sound_Types.Count ){
			// look through each sound list  for the character name
			if (sound_Types[t].soundType == _type){
				// look through type list for the kind name (could possibly use the sound name here instead... ?)
				k = 0;
				while(k < sound_Types[t].sound_Kinds.Count){
					if (sound_Types[t].sound_Kinds[k].typeName == _kind){
						// look through level list for the level name
						l=0;
						while(l < sound_Types[t].sound_Kinds[k].levels.Count){
							if (sound_Types[t].sound_Kinds[k].levels[l].levelName == _level){
								// look through sound group for sound name
								n = 0;
								while (n < sound_Types[t].sound_Kinds[k].levels[l].sounds.Count){
									if (sound_Types[t].sound_Kinds[k].levels[l].sounds[n].soundName == _soundName){
										return sound_Types[t].sound_Kinds[k].levels[l].sounds[n].audioClip;
									}
									n++;
								}
							}
							l++;
						}
					}
					k++;
				}
			}
			t++;
		} // end of function FindSound2
		
		Debug.Log ("Error:  cannot find requested audio clip");

		return null;  // return an audio clip
	}

	void ManageBGM()
	{
		StartCoroutine(StartBGM());
	}
	
	//Start BGM when loading complete
	IEnumerator StartBGM()
	{
		yield return new WaitForSeconds(0.5f);
		
		while(PatternSystem.instance.loadingComplete == false)
		{
			yield return 0;
		}
		
		//Debug.Log("play");
		bgmSound.Play();
	}
	
}
