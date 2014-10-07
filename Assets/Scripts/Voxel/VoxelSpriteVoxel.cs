using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public struct VoxelSpriteVoxel
{
    public const float SIZE = 0.5f;
    public const float HALF_SIZE = SIZE / 2f;

    public bool Active;// = false;
    public Color Color;// = Color.White;

    public VoxelSpriteVoxel(bool active, Color col)
    {
        Active = active;
        Color = col;
    }
}

