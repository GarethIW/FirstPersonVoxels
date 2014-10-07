using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;

public class SpriteManager : MonoBehaviour
{
    public static SpriteManager Instance;

    public Dictionary<string, VoxelSpriteData> Sprites = new Dictionary<string, VoxelSpriteData>();

    private Mesh blankMesh; 

	// Use this for initialization
	public void Awake ()
	{
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            blankMesh = new Mesh();

            //preload
            GetFrame("slendy", 0);
            
        }
        else
        {
            Destroy(gameObject);
        }       
	}

    public Mesh GetFrame(string name, int frame)
    {
        if (Sprites.ContainsKey(name))
        {
            if (Sprites[name].Ready) 
                return Sprites[name].AnimFrames[frame];
            
            return blankMesh;
        }
        
        Sprites.Add(name, new VoxelSpriteData());
        LoadSprite(name);
        return blankMesh;
    }

    //public void Destruct(Vector3 pos, float scale, string name, int frame)
    //{
    //    Destruct(pos, scale, name, frame, Color.white);
    //}
    //public void Destruct(Vector3 pos, float scale, string name, int frame, Color tint)
    //{
    //    VoxelSpriteChunk vsc = Sprites[name].Chunks[frame];

    //    for (int z = vsc.Z_SIZE - 1; z >= 0; z--)
    //        for (int y = vsc.Y_SIZE - 1; y >= 0; y--)
    //            for (int x = 0; x < vsc.X_SIZE; x++)
    //            {
    //                if (vsc.Voxels[x, y, z].Active == false) continue;
    //                if (Random.Range(0, 5) != 0) continue;

    //                ParticleManager.Instance.Spawn(pos //+ (new Vector3(0f, vsc.Y_SIZE * VoxelSpriteVoxel.HALF_SIZE, 0f) * scale)
    //                                                    + (new Vector3(-(vsc.X_SIZE * VoxelSpriteVoxel.HALF_SIZE), -(vsc.Y_SIZE * VoxelSpriteVoxel.HALF_SIZE),-(vsc.Z_SIZE * VoxelSpriteVoxel.HALF_SIZE)) * scale)
    //                                                   + (new Vector3(x * VoxelSpriteVoxel.SIZE, y * VoxelSpriteVoxel.SIZE, z * VoxelSpriteVoxel.SIZE) * scale),
    //                                               new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)),
    //                                               vsc.Voxels[x,y,z].Color * tint,
    //                                               Voxel.SIZE *1.5f * scale, 3f, 50f, true, true);
    //            }

    //}

    public bool IsReady(string name)
    {
        if (!Sprites.ContainsKey(name)) return false;

        if (!Sprites[name].Ready) return false;

        return true;
    }

    void LoadSprite(string name)
    {
        
#if UNITY_WEBPLAYER && !UNITY_EDITOR
        StartCoroutine(DownloadSprite(name));
#else
        string fn = name + ".vxs";

        byte[] inbuff;

        using (FileStream str = new FileStream(Application.dataPath + "/Resources/Sprites/" + fn, FileMode.Open))
        {

            inbuff = new byte[str.Length];
            str.Read(inbuff, 0, (int)str.Length);
        }

        Sprites[name].Process(inbuff);
#endif
    }

    private IEnumerator DownloadSprite(string name)
    {
        string fn = name + ".vxs";

        WWW www = new WWW("Sprites/" + fn);

        yield return www;

        Sprites[name].Process(www.bytes);
    }

	// Update is called once per frame
	void Update () {
	
	}
}
