using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public enum ChunkEnvironmentType
    {
        None,
        Water,
        Trees
    }

    public class Chunk
    {
       
        public const int X_SIZE = 16, Y_SIZE = 32, Z_SIZE = 16;

        public Voxel[, ,] Voxels = new Voxel[X_SIZE,Y_SIZE,Z_SIZE];

        public List<PositionUVColor> PUCs = new List<PositionUVColor>();

        public int MapX, MapY, MapZ;

        public Vector3 WorldCenterPosition;

        public ChunkEnvironmentType Environment = ChunkEnvironmentType.None;

        public MeshFilter Mesh;
        public MeshCollider MeshCollider;

        Map parentMap;

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<Vector2> uv2s = new List<Vector2>();
        List<Color> colors = new List<Color>();
        List<int> indexes = new List<int>();

        public bool Visible = false;
        public bool Updated = false;

        int[] typeCount;

        public Chunk() {

        }

        public Chunk(Map map, int wx, int wy, int wz, MeshFilter mf, MeshCollider mc)
        {
            Mesh = mf;
            MeshCollider = mc;

            parentMap = map;
            MapX = wx;
            MapY = wy;
            MapZ = wz;

            WorldCenterPosition = new Vector3((MapX * X_SIZE * Voxel.SIZE) + (X_SIZE * Voxel.HALF_SIZE), (MapY * Y_SIZE * Voxel.SIZE) + (Y_SIZE * Voxel.HALF_SIZE), (MapZ * Z_SIZE * Voxel.SIZE) + (Z_SIZE * Voxel.HALF_SIZE));

            typeCount = new int[Enum.GetValues(typeof(VoxelType)).Length];

            // generate some ground
            //for (int z = 0; z < Z_SIZE; z++)
            //    for (int x = 0; x < X_SIZE; x++)
            //    {
            //        for (int y = Y_SIZE - 1; y >= Y_SIZE - (3 + UnityEngine.Random.Range(0, 10)); y--)
            //        {
            //            SetVoxel(x, y, z, true, 0, VoxelType.Ground, new Color(0f, 0.5f + (UnityEngine.Random.Range(0f, 1f) * 0.1f), 0f), new Color(0f, 0.3f, 0f));
            //        }
            //    }
        }

        public void SetVoxel(int x, int y, int z, bool active, byte destruct, VoxelType type, Color top, Color side)
        {
            if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

            Voxels[x, y, z].Active = active;
            Voxels[x, y, z].Type = type;
            Voxels[x, y, z].Destructable = destruct;
            Voxels[x, y, z].TR = Helper.FloatToByte(top.r);
            Voxels[x, y, z].TG = Helper.FloatToByte(top.g);
            Voxels[x, y, z].TB = Helper.FloatToByte(top.b);
            Voxels[x, y, z].SR = Helper.FloatToByte(side.r);
            Voxels[x, y, z].SG = Helper.FloatToByte(side.g);
            Voxels[x, y, z].SB = Helper.FloatToByte(side.b);

            Updated = true;
        }

        public void UpdateMesh()
        {
            PUCs.Clear();
            vertices.Clear();
            indexes.Clear();
            colors.Clear();
            uvs.Clear();
            for (int i = 0; i < typeCount.Length; i++) typeCount[i] = 0;

            Vector3 mapOffset;
            Color topCol;
            Color sideCol;

            for (int z = Z_SIZE - 1; z >= 0; z--)
                for (int y = 0; y < Y_SIZE; y++)
                     for(int x=0;x<X_SIZE;x++)
                    {
                        if (Voxels[x, y, z].Active == false) continue;

                        mapOffset = ((new Vector3(x, y, z) * Voxel.SIZE));
                        topCol = new Color(Helper.ByteToFloat(Voxels[x, y, z].TR), Helper.ByteToFloat(Voxels[x, y, z].TG), Helper.ByteToFloat(Voxels[x, y, z].TB),1f);
                        sideCol = new Color(Helper.ByteToFloat(Voxels[x, y, z].SR), Helper.ByteToFloat(Voxels[x, y, z].SG), Helper.ByteToFloat(Voxels[x, y, z].SB), 1f);

                        if (!IsVoxelAt(x, y +1, z)) MakeQuad(mapOffset, new Vector3(0f, Voxel.SIZE, 0f), new Vector3(Voxel.SIZE, Voxel.SIZE, 0f), new Vector3(Voxel.SIZE, Voxel.SIZE, Voxel.SIZE), new Vector3(0f, Voxel.SIZE, Voxel.SIZE), CalcShadow(x,y,z,topCol), Voxels[x,y,z].Type);
                        if (!IsVoxelAt(x, y - 1, z)) MakeQuad(mapOffset, new Vector3(0f, 0f, Voxel.SIZE), new Vector3(Voxel.SIZE, 0f, Voxel.SIZE), new Vector3(Voxel.SIZE, 0f, 0f), new Vector3(0f, 0f, 0f), CalcShadow(x, y-1, z, sideCol), Voxels[x, y, z].Type);
                        if (!IsVoxelAt(x - 1, y, z)) MakeQuad(mapOffset, new Vector3(0f, Voxel.SIZE, 0f), new Vector3(0f, Voxel.SIZE, Voxel.SIZE), new Vector3(0f, 0f, Voxel.SIZE), new Vector3(0f, 0f, 0f), CalcShadow(x - 1, y, z, sideCol), Voxels[x, y, z].Type);
                        if (!IsVoxelAt(x + 1, y, z)) MakeQuad(mapOffset, new Vector3(Voxel.SIZE, Voxel.SIZE, Voxel.SIZE), new Vector3(Voxel.SIZE, Voxel.SIZE, 0f), new Vector3(Voxel.SIZE, 0f, 0f), new Vector3(Voxel.SIZE, 0f, Voxel.SIZE), CalcShadow(x + 1, y, z, sideCol), Voxels[x, y, z].Type);
                        if (!IsVoxelAt(x, y, z - 1)) MakeQuad(mapOffset, new Vector3(Voxel.SIZE, Voxel.SIZE, 0f), new Vector3(0f, Voxel.SIZE, 0f), new Vector3(0f, 0f, 0f), new Vector3(Voxel.SIZE, 0f, 0f), CalcShadow(x, y, z - 1, sideCol), Voxels[x, y, z].Type);
                        if (!IsVoxelAt(x, y, z + 1)) MakeQuad(mapOffset, new Vector3(0f, Voxel.SIZE, Voxel.SIZE), new Vector3(Voxel.SIZE, Voxel.SIZE, Voxel.SIZE), new Vector3(Voxel.SIZE, 0f, Voxel.SIZE), new Vector3(0f, 0f, Voxel.SIZE), CalcShadow(x, y, z + 1, sideCol), Voxels[x, y, z].Type);

                        typeCount[(int)Voxels[x, y, z].Type]++;
                    }


            int water = typeCount[(int)VoxelType.Water];
            int trees = typeCount[(int)VoxelType.Tree] + typeCount[(int)VoxelType.Leaf];
            if (water == 0 && trees == 0) Environment = ChunkEnvironmentType.None;
            else if (water > trees) Environment = ChunkEnvironmentType.Water;
            else Environment = ChunkEnvironmentType.Trees;

            Updated = false;
            Visible = true;

            for (int q = 0; q < PUCs.Count; q++)
            {
                vertices.Add(PUCs[q].Position);
                colors.Add(PUCs[q].Color);
                uvs.Add(PUCs[q].UV);
                uv2s.Add(PUCs[q].UV2);
            }

            for (int q = 0; q < vertices.Count; q+=4)
            {
                indexes.Add(q + 0);
                indexes.Add(q + 3);
                indexes.Add(q + 2);
                indexes.Add(q + 2);
                indexes.Add(q + 1);
                indexes.Add(q + 0);
            }

            Mesh.mesh.Clear();
            Mesh.mesh.vertices = vertices.ToArray();
            Mesh.mesh.triangles = indexes.ToArray();
            Mesh.mesh.colors = colors.ToArray();
            Mesh.mesh.uv = uvs.ToArray();
            //Mesh.mesh.uv2 = uv2s.ToArray();

            Mesh.mesh.Optimize();
            Mesh.mesh.RecalculateNormals();

            MeshCollider.sharedMesh = Mesh.mesh;
         


            PUCs.Clear();
            vertices.Clear();
            indexes.Clear();
            colors.Clear();
            uvs.Clear();
            uv2s.Clear();
        }

       

        void MakeQuad(Vector3 offset, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Color col, VoxelType type)
        {
            Vector2 uv2 = Vector2.zero;
            if(type== VoxelType.Water) uv2=new Vector2(UnityEngine.Random.Range(0.1f,0.5f),0);

            PUCs.Add(new PositionUVColor(offset+tl, new Vector2(1f,1f), uv2, col));
            PUCs.Add(new PositionUVColor(offset + tr, new Vector2(0f, 1f), uv2, col));
            PUCs.Add(new PositionUVColor(offset + br, new Vector2(0f, 0f), uv2, col));
            PUCs.Add(new PositionUVColor(offset + bl, new Vector2(1f, 0f), uv2, col));
            
        }

        bool[] lightingDirs = new bool[9];
        Color CalcShadow(int x, int y, int z, Color currentColor)
        {
            //return currentColor;

            float intensityFactor = 0.12f;
            float light = 1f;

            for (int i = 0; i < 9; i++) lightingDirs[i] = false;

            for (int yy = 1; yy < 6; yy++)
            {
                float intensity = ((intensityFactor / 7f) * (7f - (float)yy));
                if ((!lightingDirs[0]) && IsVoxelAt(x, y+yy,z)) { light -= (intensity * 4f); lightingDirs[0] = true; }
                if ((!lightingDirs[0]) && IsVoxelAt(x, y+(yy+5), z)) { light -= intensity; lightingDirs[0] = true; }
                if ((!lightingDirs[0]) && IsVoxelAt(x, y + (yy + 10), z)) { light -= intensity; lightingDirs[0] = true; }
                if ((!lightingDirs[1]) && IsVoxelAt(x - yy, y + yy, z - yy)) { light -= intensity; lightingDirs[1] = true; }
                if ((!lightingDirs[2]) && IsVoxelAt(x, y + yy, z - yy)) { light -= intensity; lightingDirs[2] = true; }
                if ((!lightingDirs[3]) && IsVoxelAt(x + yy, y + yy, z - yy)) { light -= intensity; lightingDirs[3] = true; }
                if ((!lightingDirs[4]) && IsVoxelAt(x - yy, y+yy, z )) { light -= intensity; lightingDirs[4] = true; }
                if ((!lightingDirs[5]) && IsVoxelAt(x + yy, y+yy, z)) { light -= intensity; lightingDirs[5] = true; }
                if ((!lightingDirs[6]) && IsVoxelAt(x - yy, y + yy, z + yy)) { light -= intensity; lightingDirs[6] = true; }
                if ((!lightingDirs[7]) && IsVoxelAt(x, y + yy, z + yy)) { light -= intensity; lightingDirs[7] = true; }
                if ((!lightingDirs[8]) && IsVoxelAt(x + yy, y + yy, z + yy)) { light -= intensity; lightingDirs[8] = true; }
            }

            light = Mathf.Clamp(light, 0.2f, 1f);

            currentColor *= light;
            currentColor.a = 1f;

            return currentColor;
        }

        public bool IsVoxelAt(int x, int y, int z)
        {
            if (x >= 0 && x < X_SIZE && y >= 0 && y < Y_SIZE && z >= 0 && z < Z_SIZE) return Voxels[x, y, z].Active;

            if (x < 0)
                if (MapX == 0) return false;
                else return parentMap.Chunks[MapX - 1, MapY, MapZ].IsVoxelAt(X_SIZE + x, y, z);

            if (x >= X_SIZE)
                if (MapX >= parentMap.X_CHUNKS - 1) return false;
                else return parentMap.Chunks[MapX + 1, MapY, MapZ].IsVoxelAt(x - X_SIZE, y, z);

            if (y < 0)
                if (MapY == 0) return false;
                else return parentMap.Chunks[MapX, MapY - 1, MapZ].IsVoxelAt(x, Y_SIZE + y, z);

            if (y >= Y_SIZE)
                if (MapY >= parentMap.Y_CHUNKS - 1) return false;
                else return parentMap.Chunks[MapX, MapY + 1, MapZ].IsVoxelAt(x, y - Y_SIZE, z);

            if (z < 0)
                if (MapZ == 0) return false;
                else return parentMap.Chunks[MapX, MapY, MapZ - 1].IsVoxelAt(x, y, Z_SIZE + z);

            if (z >= Z_SIZE)
                if (MapZ >= parentMap.Z_CHUNKS - 1) return false;
                else return parentMap.Chunks[MapX, MapY, MapZ + 1].IsVoxelAt(x, y, z - Z_SIZE);

            return false;
        }

        public void Dispose()
        {
            PUCs.Clear();
            vertices.Clear();
            indexes.Clear();
            colors.Clear();
            uvs.Clear();
            uv2s.Clear();

            Mesh.mesh.Clear();
            Mesh.mesh = null;
            Voxels = null;

            PUCs = null;
            vertices = null;
            indexes = null;
            colors = null;
            uvs = null;
            uv2s = null;
        }
    }
}
