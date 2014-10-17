using UnityEngine;
using System.Collections;

public class Slendy : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
	{
	    GetComponent<MeshFilter>().mesh = SpriteManager.Instance.GetFrame("slendy", 0);
        GetComponent<MeshRenderer>().sharedMaterial.SetColor("_Tint", new Color(0.5f,0.5f,0.5f));
	}
}
