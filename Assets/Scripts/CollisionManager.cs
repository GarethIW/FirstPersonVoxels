using UnityEngine;
using System.Collections;

public class CollisionManager : MonoBehaviour
{
    private const int MAX_CUBES = 500;

    public GameObject CollisionCubePrefab;

    public Map Map;
    public Player Player;


	// Use this for initialization
	void Start () {
	    for (int i = 0; i < MAX_CUBES; i++)
	    {
	        GameObject newCube = (GameObject)Instantiate(CollisionCubePrefab, Vector3.zero, Quaternion.identity);
	        newCube.transform.parent = transform;
            newCube.SetActive(false);
	    }
	}
	
	// Update is called once per frame
	void FixedUpdate () {

	    for(int i=0;i<transform.childCount;i++)
            transform.GetChild(i).gameObject.SetActive(false);

	    int c = 0;
	    Vector3 centerPos = Player.transform.position;

	    for (float theta = 0f; theta <= 360f; theta += 20f)
	    {
	        for (float phi = 0f; phi <= 360f; phi += 20f)
	        {
	            for (float r = 1f; r < 3f; r += 0.25f)
	            {
	                Vector3 pos = centerPos + Helper.PointOnSphere(r, theta, phi);

	                if (Map.GetVoxel(pos).Active && c<MAX_CUBES)
	                {
                        transform.GetChild(c).gameObject.SetActive(true);

	                    Vector3 vpos = Map.FromSceneSpace(pos);
	                    transform.GetChild(c).position = (new Vector3((int)vpos.x,(int)vpos.y,(int)vpos.z)*Voxel.SIZE) + (Vector3.one * Voxel.HALF_SIZE);
	                    c++;
	                    break;
	                }
	            }

	        }
	    }
	}
}
