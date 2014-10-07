using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Linq;
using System.Text;
using Assets.Scripts;
using UnityEngine;


public class VoxelSpriteData
{
    public bool Ready = false;
    public List<VoxelSpriteChunk> Chunks = new List<VoxelSpriteChunk>(); 
    public List<Mesh> AnimFrames = new List<Mesh>();

    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<Vector2> uv2s = new List<Vector2>();
    List<Color> colors = new List<Color>();
    List<int> indexes = new List<int>();

    private WWW www;

    public VoxelSpriteData()
    {
    }

    public void Process(byte[] inbuff)
    {
        Debug.Log("processing sprite");

        

        byte[] buffer = Ionic.Zlib.GZipStream.UncompressBuffer(inbuff);

        int pos = 0;

        int xs = buffer[0];
        int ys = buffer[1];
        int zs = buffer[2];
        int frames = buffer[3];

        pos = 4;

        for (int i = 0; i < 10; i++)
        {
            pos += 3;
        }

        for (int frame = 0; frame < frames; frame++)
        {
            VoxelSpriteChunk c = new VoxelSpriteChunk(xs,ys,zs);

            while (pos < buffer.Length)
            {
                if (Convert.ToChar(buffer[pos]) != 'c')
                {
                    int vx = buffer[pos];
                    int vy = buffer[pos + 1];
                    int vz = buffer[pos + 2];
                    Color top = new Color(Helper.ByteToFloat(buffer[pos + 3]), Helper.ByteToFloat(buffer[pos + 4]), Helper.ByteToFloat(buffer[pos + 5]));

                    c.SetVoxel((c.X_SIZE-1)-vx, (c.Y_SIZE-1)-vy, (c.Z_SIZE-1)-vz, true, top);
                    pos += 6;

                }
                else
                {
                    pos++;
                    break;
                }

            }

            Chunks.Add(c);
        }

        foreach (VoxelSpriteChunk c in Chunks)
        {
            vertices.Clear();
            uvs.Clear();
            uv2s.Clear();
            colors.Clear();
            indexes.Clear();

            Vector3 meshCenter = ((new Vector3(c.X_SIZE, c.Y_SIZE, c.Z_SIZE) * VoxelSpriteVoxel.SIZE) / 2f) - (Vector3.one * VoxelSpriteVoxel.HALF_SIZE);

            for (int z = c.Z_SIZE - 1; z >= 0; z--)
                for (int y = c.Y_SIZE - 1; y >= 0; y--)
                    for (int x = 0; x < c.X_SIZE; x++)
                    {
                        if (c.Voxels[x, y, z].Active == false) continue;

                        Vector3 worldOffset = ((new Vector3(x, y, z) * Voxel.SIZE)) - meshCenter;

                        Color topColor = c.Voxels[x, y, z].Color;
                        Color sideColor = topColor * 0.75f;

                        float hs = VoxelSpriteVoxel.HALF_SIZE;

                        if (!c.IsVoxelAt(x, y, z - 1)) MakeQuad(worldOffset, new Vector3(-hs, hs, -hs), new Vector3(hs, hs, -hs), new Vector3(hs, -hs, -hs), new Vector3(-hs, -hs, -hs), sideColor);
                        if (!c.IsVoxelAt(x, y, z + 1)) MakeQuad(worldOffset, new Vector3(hs, hs, hs), new Vector3(-hs, hs, hs), new Vector3(-hs, -hs, hs), new Vector3(hs, -hs, hs), sideColor);
                        if (!c.IsVoxelAt(x - 1, y, z)) MakeQuad(worldOffset, new Vector3(-hs, hs, hs), new Vector3(-hs, hs, -hs), new Vector3(-hs, -hs, -hs), new Vector3(-hs, -hs, hs), sideColor);
                        if (!c.IsVoxelAt(x + 1, y, z)) MakeQuad(worldOffset, new Vector3(hs, hs, -hs), new Vector3(hs, hs, hs), new Vector3(hs, -hs, hs), new Vector3(hs, -hs, -hs), sideColor);
                        if (!c.IsVoxelAt(x, y - 1, z)) MakeQuad(worldOffset, new Vector3(-hs, -hs, -hs), new Vector3(hs, -hs, -hs), new Vector3(hs, -hs, hs), new Vector3(-hs, -hs, hs), sideColor);
                        if (!c.IsVoxelAt(x, y + 1, z)) MakeQuad(worldOffset, new Vector3(-hs, hs, hs), new Vector3(hs, hs, hs), new Vector3(hs, hs, -hs), new Vector3(-hs, hs, -hs), topColor);
                    }

            Mesh m = new Mesh();
            m.vertices = vertices.ToArray();
            m.colors = colors.ToArray();
            m.uv = uvs.ToArray();
            m.uv2 = uv2s.ToArray();
            m.triangles = indexes.ToArray();
            m.Optimize();
            m.RecalculateNormals();
            AnimFrames.Add(m);
        }

        Ready = true;

        GC.Collect();
    }

    void MakeQuad(Vector3 offset, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bl, Color col)
    {
        vertices.Add(offset + tl);
        vertices.Add(offset + tr);
        vertices.Add(offset + br);
        vertices.Add(offset + bl);

        for(int i=0;i<4;i++)
            colors.Add(col);

        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(0f, 1f));
        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(1f, 0f));

        uv2s.Add(new Vector2(0f, 0f));
        uv2s.Add(new Vector2(0f, 0f));
        uv2s.Add(new Vector2(0f, 0f));
        uv2s.Add(new Vector2(0f, 0f));

        int q = vertices.Count-4;
        indexes.Add(q + 2);
        indexes.Add(q + 3);
        indexes.Add(q + 0);
        indexes.Add(q + 0);
        indexes.Add(q + 1);
        indexes.Add(q + 2);
    }
}

