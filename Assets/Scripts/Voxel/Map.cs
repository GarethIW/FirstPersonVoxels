using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Assets.Scripts;
using UnityEngine;
using Ionic.Zlib;
using System.Collections;

public enum MapState
{
    NotLoaded,
    Loading,
    Loaded
}

public enum Theme
{
    Grasslands,
    Arctic,
    Desert,
    Urban,
    Jungle,
    Sand,
    Ice
}

public enum MapType
{
    Campaign,
    Deathmatch,
    CTF,
    KOTH
}

public class Map : MonoBehaviour {
    public static Map Instance;

    public string Name;
    public string Title;

    public MapState State = MapState.NotLoaded;
    public float LoadProgress = 0;

    public int X_CHUNKS = 10, Y_CHUNKS = 1, Z_CHUNKS = 10;
    public int X_SIZE;
    public int Y_SIZE;
    public int Z_SIZE;

    public Chunk[, ,] Chunks;

    public GameObject ChunkPrefab;
   
    public float Gravity = 0.05f;

    public List<Vector3> RecentDestroyedVoxels = new List<Vector3>();

    public List<Vector3[]> AllDestroyedVoxels = new List<Vector3[]>(); 
    public List<Vector3> AllDestroyedVoxelsBuffer = new List<Vector3>();

    public Theme Theme;

    public bool EditorTest = false;
    public bool LoadFromName = false;

    private Queue<Chunk> chunksToUpdate = new Queue<Chunk>();

    private WWW www;

    private float loadTimer = 0f;

    byte[] inbuff;
    byte[] buffer;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            FirstStart();
        }
        else
        {
            Destroy(gameObject);
        }
    }

	void FirstStart ()
	{
        Chunks = new Chunk[50, 1, 50];

        for (int x = 0; x < 50; x++)
        {
            for (int y = 0; y < 1; y++)
            {
                for (int z = 0; z < 50; z++)
                {
                    Vector3 mapOffset = new Vector3(x * (Chunk.X_SIZE * Voxel.SIZE), y * (Chunk.Y_SIZE * Voxel.SIZE), z * (Chunk.Z_SIZE * Voxel.SIZE));
                    GameObject newChunk = (GameObject)Instantiate(ChunkPrefab, mapOffset, Quaternion.identity);

                    MeshFilter newMesh = newChunk.GetComponent<MeshFilter>();
                    MeshCollider newMeshC = newChunk.GetComponent<MeshCollider>();
                    newMesh.transform.parent = transform;
                    //DontDestroyOnLoad(newMesh);
                    newMesh.name = "Chunk-" + x + "," + z;

                    Chunks[x, y, z] = new Chunk(this, x, y, z, newMesh, newMeshC);
                }
            }
        }
	}

    void Update()
    {
        if (State == MapState.NotLoaded && Name != "" && LoadFromName)
        {
            loadTimer += Time.deltaTime;
            if (loadTimer > 0.5f) Load();
        }

        for (int i = 0; i < 10; i++)
            if (chunksToUpdate.Count > 0)
            {
                if(LoadProgress<100) LoadProgress += (70f / (X_CHUNKS*Z_CHUNKS));
                Chunk uC = chunksToUpdate.Dequeue();
                uC.UpdateMesh();
            }

        if(State!=MapState.Loaded && chunksToUpdate.Count==0 && LoadProgress>30) State = MapState.Loaded;
    }

  

 

    public void Init()
    {
        //for(int i=transform.childCount-1;i>=0;i--) Destroy(transform.GetChild(i).gameObject);
        //GC.Collect();

        foreach (Chunk c in Chunks)
        {
            c.Mesh.mesh.Clear();
            c.Voxels = new Voxel[Chunk.X_SIZE,Chunk.Y_SIZE,Chunk.Z_SIZE];
        }

        //Chunks = new Chunk[X_CHUNKS, Y_CHUNKS, Z_CHUNKS];

        X_SIZE = X_CHUNKS * Chunk.X_SIZE;
        Y_SIZE = Y_CHUNKS * Chunk.Y_SIZE;
        Z_SIZE = Z_CHUNKS * Chunk.Z_SIZE;
    }

    //public void Explode(Vector3 pos, float radius)
    //{
    //    GA.API.Design.NewEvent("Explosion", pos);

    //    ExplosionManager.Instance.ExplodeLocal(pos);

    //    int partcount = 0;
    //    //Sphere sphere = new BoundingSphere(pos, radius);
    //    for (float x = pos.x - radius; x <= pos.x + radius; x += Voxel.SIZE)
    //        for (float y = pos.y - radius; y <= pos.y + radius; y += Voxel.SIZE)
    //            for (float z = pos.z - radius; z <= pos.z + radius; z += Voxel.SIZE)
    //            {
    //                Vector3 screen = new Vector3(x, y, z);
    //                Vector3 world = FromSceneSpace(screen);

    //                if ((int)world.y ==0) continue;

    //                //if (sphere.Contains(screen) == ContainmentType.Contains)
    //                //{
    //                if(Vector3.Distance(pos,screen)<=radius)
    //                { 
    //                    Voxel v = GetVoxel(screen);
    //                    if (v.Active && (v.Destructable > 0 || v.Type == VoxelType.Ground))
    //                    {
    //                        //AllChangedVoxels.Add(((int)world.X).ToString() + "," + ((int)world.Y).ToString() + "," + ((int)world.Z).ToString() + ",0,0,0,0,0,0,0,0,0");
    //                        //RecentChangedVoxels.Add(((int)world.X).ToString() + "," + ((int)world.Y).ToString() + "," + ((int)world.Z).ToString() + ",0,0,0,0,0,0,0,0,0");
    //                        SetVoxel((int)world.x, (int)world.y, (int)world.z, false, true, partcount==0);
    //                        partcount++;
    //                        if (partcount == 20) partcount = 0;
    //                        //if (Helper.Random.Next(10) == 1) ParticleController.Instance.Spawn(screen, new Vector3(-0.05f + ((float)Helper.Random.NextDouble() * 0.1f), -0.05f + ((float)Helper.Random.NextDouble() * 0.1f), -((float)Helper.Random.NextDouble() * 1f)), 0.25f, new Color(v.SR, v.SG, v.SB), 1000, true);
    //                    }
    //                }
    //            }
    //}

   

    public void DestroyVoxels(Vector3[] vox)
    {
        foreach (var v in vox)
        {
            SetVoxel((int)v.x, (int)v.y, (int)v.z, false, false, false);
        }
        //DestroyedVoxels.Clear();
    }

    public void SetVoxel(Vector3 scenePos, bool a)
    {
        Vector3 vpos = FromSceneSpace(scenePos);
        
        SetVoxel((int)vpos.x,(int)vpos.y,(int)vpos.z,a,true,true);
    }

    public void SetVoxel(int x, int y, int z, bool a, bool addToRecent, bool particle)
    {
        // Particle idea: pass a bool here for particle creation, then in the explode/destroy methods, count every 4th SetVoxel for particle

        if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

        Chunk c = GetChunkAtWorldPosition(x, y, z);

        //Debug.Log(c.MapX + "," + c.MapY + "," + c.MapZ);

        if(c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].Active!=a)
        { 
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].Active = a;

            if (!a)
            {
                if (particle)
                {
                    Color col = new Color(Helper.ByteToFloat(c.Voxels[x - ((x/Chunk.X_SIZE)*Chunk.X_SIZE), y - ((y/Chunk.Y_SIZE)*Chunk.Y_SIZE),z - ((z/Chunk.Z_SIZE)*Chunk.Z_SIZE)].SR),
                                        Helper.ByteToFloat(c.Voxels[x - ((x/Chunk.X_SIZE)*Chunk.X_SIZE), y - ((y/Chunk.Y_SIZE)*Chunk.Y_SIZE),z - ((z/Chunk.Z_SIZE)*Chunk.Z_SIZE)].SG),
                                        Helper.ByteToFloat(c.Voxels[x - ((x/Chunk.X_SIZE)*Chunk.X_SIZE), y - ((y/Chunk.Y_SIZE)*Chunk.Y_SIZE),z - ((z/Chunk.Z_SIZE)*Chunk.Z_SIZE)].SB));
                    //ParticleManager.Instance.Spawn(ToSceneSpace(x, y, z) + (Vector3.one * Voxel.HALF_SIZE), new Vector3(UnityEngine.Random.Range(-3f, 3f), UnityEngine.Random.Range(0f, 15f), UnityEngine.Random.Range(-3f, 3f)), col, 0.3f, 1f, 50f, true, true);
                }

                AddDestroyedVoxel(new Vector3(x,y,z));
                if(addToRecent) RecentDestroyedVoxels.Add(new Vector3(x, y, z));
            }

            for (int xx = c.MapX - 1; xx <= c.MapX + 1; xx++)
               for (int yy = c.MapY - 1; yy <= c.MapY + 1; yy++)
                  for (int zz = c.MapZ - 1; zz <= c.MapZ + 1; zz++)
                    if (xx >= 0 && xx < X_CHUNKS && yy >= 0 && yy < Y_CHUNKS && zz >= 0 && zz < Z_CHUNKS) AddToUpdateQueue(Chunks[xx, yy, zz]);  //Chunks[xx, yy, zz].UpdateMesh();
        }
    }

    public void SetVoxelColor(int x, int y, int z, Color top, Color side)
    {
        
        if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

        Chunk c = GetChunkAtWorldPosition(x, y, z);

        if (c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].Active)
        {
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].TR = Helper.FloatToByte(top.r);
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].TG = Helper.FloatToByte(top.g);
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].TB = Helper.FloatToByte(top.b);
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].SR = Helper.FloatToByte(side.r);
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].SG = Helper.FloatToByte(side.g);
            c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)].SB = Helper.FloatToByte(side.b);

            for (int xx = c.MapX - 1; xx <= c.MapX + 1; xx++)
                for (int yy = c.MapY - 1; yy <= c.MapY + 1; yy++)
                    for (int zz = c.MapZ - 1; zz <= c.MapZ + 1; zz++)
                        if (xx >= 0 && xx < X_CHUNKS && yy >= 0 && yy < Y_CHUNKS && zz >= 0 && zz < Z_CHUNKS) AddToUpdateQueue(Chunks[xx, yy, zz]);  //Chunks[xx, yy, zz].UpdateMesh();
        }
    }

    public Voxel GetVoxel(Vector3 scenePos)
    {
        Vector3 vpos = FromSceneSpace(scenePos);

        return GetVoxel((int)vpos.x, (int)vpos.y, (int)vpos.z);
    }

    public Voxel GetVoxel(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return new Voxel();

        Chunk c = GetChunkAtWorldPosition(x, y, z);

        return c.Voxels[x - ((x / Chunk.X_SIZE) * Chunk.X_SIZE), y - ((y / Chunk.Y_SIZE) * Chunk.Y_SIZE), z - ((z / Chunk.Z_SIZE) * Chunk.Z_SIZE)];
    }

    public Chunk GetChunkAtWorldPosition(int x, int y, int z)
    {
        if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return null;

        return Chunks[x / Chunk.X_SIZE, y / Chunk.Y_SIZE, z / Chunk.Z_SIZE];
    }

    public Vector3 FromSceneSpace(Vector3 scenePos)
    {
        Vector3 vox = new Vector3(scenePos.x / Voxel.SIZE, scenePos.y / Voxel.SIZE, scenePos.z / Voxel.SIZE);

        return vox;
    }

    public Vector3 ToSceneSpace(int x, int y, int z)
    {
        Vector3 scenePos = new Vector3(x * Voxel.SIZE, y * Voxel.SIZE, z * Voxel.SIZE);

        return scenePos;
    }

    public float GetGroundHeight(Vector3 scene)
    {
        Vector3 voxSpace = FromSceneSpace(scene);
        int x = (int)voxSpace.x;
        int y = (int)voxSpace.y;
        int z = (int)voxSpace.z;

        for (int h = y; h >= 0; h--)
        {
            Voxel v = GetVoxel(x, h, z);
            if ((v.Active && v.Collider()) || v.Type == VoxelType.Water)
            {
                if (v.Type == VoxelType.Water)
                {
                    for (int wh = h; wh < Chunk.Y_SIZE; wh++)
                    {
                        Voxel v2 = GetVoxel(x, wh, z);
                        if (!v2.Active)
                        {
                            y = wh - 3;
                            break;
                        }
                    }
                }
                else y = h;
                break;
            }
        }

        return y * Voxel.SIZE;
    }

    void AddDestroyedVoxel(Vector3 v)
    {
        AllDestroyedVoxelsBuffer.Add(v);
        if (AllDestroyedVoxelsBuffer.Count == 1000)
        {
            AllDestroyedVoxels.Add(AllDestroyedVoxelsBuffer.ToArray());
            AllDestroyedVoxelsBuffer.Clear();
        }
    }

    void AddToUpdateQueue(Chunk c)
    {
        c.Updated = true;

        foreach (Chunk cc in chunksToUpdate) if (cc == c) return;

        chunksToUpdate.Enqueue(c);
    }

    private void UpdateAllChunkMeshes()
    {
        foreach (var chunk in Chunks)
        {
            
            chunk.UpdateMesh();
        }
    }

    // Update is called once per frame
	

#region Loading
    public void Load()
    {
        State = MapState.Loading;
        LoadProgress = 0;

        if (EditorTest)
        {
            StartCoroutine(LoadMap(Name));
            return;
        }

        string fn = Name;

        if (!fn.EndsWith(".vxl")) fn += ".vxl";

#if UNITY_WEBPLAYER && !UNITY_EDITOR
        StartCoroutine(DownloadMap(fn));       
#else

        StartCoroutine(LoadMap(Application.dataPath + "/Resources//Maps/" + fn));
#endif
    }

    private IEnumerator LoadMap(object fn)
    {
        byte[] inbuff;

        using (FileStream str = new FileStream((string)fn, FileMode.Open))
        {

            inbuff = new byte[str.Length];
            str.Read(inbuff, 0, (int)str.Length);
        }

        buffer = Ionic.Zlib.GZipStream.UncompressBuffer(inbuff);
        inbuff = null;

        yield return Process();
    }

    private IEnumerator DownloadMap(string fn)
    {
        www = new WWW("Maps/" + fn);

        yield return www;

        Debug.Log(www.bytes.Length);

        inbuff = www.bytes;
        buffer = Ionic.Zlib.GZipStream.UncompressBuffer(inbuff);
        inbuff = null;
        www.Dispose();

        yield return Process();

        
    }




    private IEnumerator Process()
    {
        LoadProgress = 10;

        int pos = 0;

        var sw = new StreamReader(new MemoryStream(buffer));
        string codename = sw.ReadLine();
        Title = sw.ReadLine();
        sw.Close();

        int foundcount = 0;
        while (foundcount < 2)
        {
            pos++;
            if (buffer[pos - 1] == 13) foundcount++;
        }

        while (buffer[pos] == 13 || buffer[pos] == 10) pos++;

        int xs = buffer[pos + 2];
        int ys = buffer[pos + 3];
        int zs = buffer[pos + 4];

        X_CHUNKS = xs;
        Z_CHUNKS = ys;
        Y_CHUNKS = zs;
        Init();
        //gameWorld.DisplayName = dispname;
        //gameWorld.CodeName = codename;
        //gameWorld.Type = (MapType)buffer[pos];
        Theme = (Theme)buffer[pos + 1];

        int numSpawns = buffer[pos + 5];

        pos += 6;

        for (int i = 0; i < numSpawns; i++)
        {
            //Spawn s = (Spawn)(Instantiate(SpawnPrefab, Vector3.zero, Quaternion.identity));
            //s.transform.parent = transform;
            //s.transform.position = new Vector3(BitConverter.ToInt32(buffer, pos) * Voxel.SIZE, (Y_SIZE - BitConverter.ToInt32(buffer, pos + 8)) * Voxel.SIZE, (Z_SIZE - BitConverter.ToInt32(buffer, pos + 4)) * Voxel.SIZE);
            //s.Type = (SpawnType)buffer[pos + 12];
            //s.transform.rotation = Quaternion.Euler(0f, (90 * buffer[pos + 13])+180 , 0f);
            //s.name = "Spawn - " + s.Type;
            //Spawns.Add(s);
            //s.Position = 
            //s.Type = (SpawnType)buffer[pos + 12];
            //s.Rotation = buffer[pos + 13];
            //gameWorld.Spawns.Add(s);
            pos += 14;
        }

        LoadProgress = 20;

        for (int y = 0; y < Y_CHUNKS; y++)
        {
            for (int z = Z_CHUNKS-1; z >= 0; z--)
            {
                for (int x = 0; x < X_CHUNKS; x++)
                {
                    Chunk c = Chunks[x, y, z];

                    while (pos < buffer.Length)
                    {
                        if (Convert.ToChar(buffer[pos]) != 'c')
                        {
                            int vx = buffer[pos];
                            int vz = (Chunk.Z_SIZE-1) - buffer[pos + 1];
                            int vy = (Chunk.Y_SIZE-1) - buffer[pos + 2];
                            VoxelType type = (VoxelType)buffer[pos + 3];
                            byte destruct = buffer[pos + 4];
                            Color top = new Color(Helper.ByteToFloat(buffer[pos + 5]), Helper.ByteToFloat(buffer[pos + 6]), Helper.ByteToFloat(buffer[pos + 7]));
                            Color side = new Color(Helper.ByteToFloat(buffer[pos + 8]), Helper.ByteToFloat(buffer[pos + 9]), Helper.ByteToFloat(buffer[pos + 10]));

                            c.SetVoxel(vx, vy, vz, true, destruct, type, top, side);
                            pos += 11;
                        }
                        else
                        {
                            pos++;
                            break;
                        }

                    }

                    AddToUpdateQueue(c);
                }
            }

        }

        LoadProgress = 30;

        //UpdateAllChunkMeshes();

        //foreach (Chunk c in Chunks)
            

        GC.Collect();

        return null;
    }

    public void Dispose()
    {

        try
        {
           // for (int i = Spawns.Count - 1; i >= 0; i--) Destroy(Spawns[i]);
           // Spawns.Clear();
            for (int i = transform.childCount - 1; i >= 0; i--)
                if (transform.GetChild(i).name.StartsWith("Spawn")) Destroy(transform.GetChild(i).gameObject);

            RecentDestroyedVoxels.Clear();
            AllDestroyedVoxels.Clear();
            AllDestroyedVoxelsBuffer.Clear();

            State = MapState.NotLoaded;
            LoadProgress = 0;

            GC.Collect();
        }
        catch (Exception)
        {
            
       
        }
        
        
    }
#endregion

   
}
