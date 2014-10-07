using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class VoxelSpriteChunk
{
    public int X_SIZE = 32, Y_SIZE = 32, Z_SIZE = 32;
    public VoxelSpriteVoxel[, ,] Voxels; 

    public bool Updated = false;

    public VoxelSpriteChunk(int xs, int ys, int zs)
    {
        X_SIZE = xs;
        Y_SIZE = ys;
        Z_SIZE = zs;

        Voxels = new VoxelSpriteVoxel[X_SIZE,Y_SIZE,Z_SIZE];
    }

    public void SetVoxel(int x, int y, int z, bool active, Color col)
    {
        if (x < 0 || y < 0 || z < 0 || x >= X_SIZE || y >= Y_SIZE || z >= Z_SIZE) return;

        Voxels[x, y, z].Active = active;
        Voxels[x, y, z].Color = col;

        Updated = true;
    }

    //public void UpdateMesh()
    //{
    //    Vector3 meshCenter = ((new Vector3(X_SIZE, Y_SIZE, Z_SIZE) * Voxel.SIZE) / 2f) - (Vector3.One * Voxel.HALF_SIZE);
    //    Vertices.Clear();
    //    Indexes.Clear();

    //    for (int z = Z_SIZE - 1; z >= 0; z--)
    //        for (int y = Y_SIZE-1; y >=0; y--)
    //            for(int x=0;x<X_SIZE;x++)
    //            {
    //                if (Voxels[x, y, z].Active == false) continue;

    //                Vector3 worldOffset = ((new Vector3(x, y, z) * Voxel.SIZE)) -meshCenter;

    //                Color topColor = Voxels[x, y, z].Color;
    //                Vector3 vc = topColor.ToVector3();
    //                Color sideColor = new Color(vc * 0.75f);

    //                if (!IsVoxelAt(x, y, z - 1)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(0f, 0f, -1f), sideColor);
    //                if (!IsVoxelAt(x, y, z + 1)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 0f, 1f), sideColor);
    //                if (!IsVoxelAt(x - 1, y, z)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-1f, 0f, 0f), sideColor);
    //                if (!IsVoxelAt(x + 1, y, z)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(1f, 0f, 0f), sideColor);
    //                if (!IsVoxelAt(x, y + 1, z)) MakeQuad(worldOffset, new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, 1f, 0f), sideColor);
    //                if (!IsVoxelAt(x, y - 1, z)) MakeQuad(worldOffset, new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, -Voxel.HALF_SIZE), new Vector3(-Voxel.HALF_SIZE, -Voxel.HALF_SIZE, Voxel.HALF_SIZE), new Vector3(0f, -1f, 0f), topColor); 
    //            }

    //    VertexArray = Vertices.ToArray();
    //    IndexArray = new short[Indexes.Count];

    //    for (int ind = 0; ind < Indexes.Count / 6; ind++)
    //    {
    //        for (int i = 0; i < 6; i++)
    //        {
    //            IndexArray[(ind * 6) + i] = (short)(Indexes[(ind * 6) + i] + (ind * 4));
    //        }
    //    }

    //    Vertices.Clear();
    //    Indexes.Clear();
    //    GC.Collect();
    //    Updated = false;
    //}

    //void MakeQuad(Vector3 offset, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Vector3 norm, Color col)
    //{
    //    Vertices.Add(new VertexPositionNormalColor(offset + tl, norm, col));
    //    Vertices.Add(new VertexPositionNormalColor(offset + tr, norm, col));
    //    Vertices.Add(new VertexPositionNormalColor(offset + br, norm, col));
    //    Vertices.Add(new VertexPositionNormalColor(offset + bl, norm, col));
    //    Indexes.Add(0);
    //    Indexes.Add(1);
    //    Indexes.Add(2);
    //    Indexes.Add(2);
    //    Indexes.Add(3);
    //    Indexes.Add(0);
    //}
       

    public bool IsVoxelAt(int x, int y, int z)
    {
        if (x >= 0 && x < X_SIZE && y >= 0 && y < Y_SIZE && z >= 0 && z < Z_SIZE) return Voxels[x, y, z].Active;

        return false;
    }

}
