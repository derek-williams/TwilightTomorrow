using UnityEngine;
using System.Collections;

public class UICallAnim : MonoBehaviour {

    public static string UIPanel;
    bool isOn;

	// Update is called once per frame
	void OnClick () {

        GameObject.Find(UIPanel);
	
	}
}
