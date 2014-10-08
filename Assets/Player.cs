using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    public Map Map;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update ()
	{
	    Screen.lockCursor = true;

        if(Map.State!=MapState.Loaded) GetComponent<CharacterMotor>().enabled = false;
        else GetComponent<CharacterMotor>().enabled = true;
	}
}
