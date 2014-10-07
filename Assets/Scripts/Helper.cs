using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public static class Helper
{
    public static byte FloatToByte(float f)
    {
        return Convert.ToByte(Math.Floor(f*255));
    }

    public static float ByteToFloat(byte b)
    {
        return (1f/255f) * (float)b;
    }

    public static Vector2 PointOnCircle(Vector2 c, float r, float a)
    {
        //A = A - 90;
        float endX = (c.x + (r * (Mathf.Cos(a))));
        float endY = (c.y + (r * (Mathf.Sin(a))));
        return new Vector2(endX, endY);
    }

    
}

