using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{

    public Map Map;

    public Camera Camera;

	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update ()
	{
	    Screen.lockCursor = true;

        if(Map.State!=MapState.Loaded) GetComponent<CharacterMotor>().enabled = false;
        else GetComponent<CharacterMotor>().enabled = true;

	    if (Input.GetMouseButtonUp(0))
	    {
	        Ray desRay = Camera.ScreenPointToRay(new Vector3(Screen.width/2f, Screen.height/2f, 0f));

            Debug.DrawRay(desRay.origin,desRay.direction * 10f,Color.yellow,0.1f);

            

	        for (float d = 0; d < 10f; d += 0.1f)
	        {
	            if (Map.GetVoxel(desRay.GetPoint(d)).Active)
	            {
	                Map.SetVoxel(desRay.GetPoint(d), false);
	                break;
	            }
	        }
	    }
	}
}
