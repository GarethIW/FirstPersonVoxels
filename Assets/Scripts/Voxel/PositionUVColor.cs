using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class PositionUVColor
{
    public Vector3 Position;
    public Vector2 UV;
    public Vector2 UV2;
    public Color Color;

    public PositionUVColor(Vector3 p, Vector2 u, Vector2 u2, Color c)
    {
        Position = p;
        UV = u;
        UV2 = u2;
        Color = c;
    }
}

