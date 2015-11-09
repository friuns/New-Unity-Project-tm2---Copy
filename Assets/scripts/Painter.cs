#define GA
using gui = UnityEngine.GUILayout;
#if !UNITY_FLASH || UNITY_EDITOR
using System.Linq;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Prefs = UnityEngine.PlayerPrefs;
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;


public class Painter : GuiClasses
{
    public static Texture2D PaintLine(Vector2 from, Vector2 to, float rad, Color col, float hardness, Texture2D tex)
    {
        float num2 = rad;
        float y = Mathf.Clamp(Mathf.Min(from.y, to.y) - num2, 0, tex.height);
        float x = Mathf.Clamp(Mathf.Min(from.x, to.x) - num2, 0, tex.width);
        float num5 = Mathf.Clamp(Mathf.Max(from.y, to.y) + num2, 0, tex.height);
        float num7 = Mathf.Clamp(Mathf.Max(from.x, to.x) + num2, 0, tex.width) - x;
        float num8 = num5 - y;
        float num10 = (rad + 1) * (rad + 1);
        Color[] colors = tex.GetPixels((int)x, (int)y, (int)num7, (int)num8, 0);
        Vector2 vector = new Vector2(x, y);
        for (int i = 0; i < num8; i++)
        {
            for (int j = 0; j < num7; j++)
            {
                Vector2 vector2 = new Vector2(j, i) + vector;
                Vector2 point = vector2 + new Vector2(0.5f, 0.5f);
                Vector2 vector4 = point - Mathfx.NearestPointStrict(from, to, point);
                float sqrMagnitude = vector4.sqrMagnitude;
                if (sqrMagnitude <= num10)
                {
                    Color color;
                    sqrMagnitude = Mathfx.GaussFalloff(Mathf.Sqrt(sqrMagnitude), rad) * hardness;
                    var index0 = ((int) (i*num7)) + j;
                    //print(index0);
                    //print(colors.Length);
                    if (sqrMagnitude > 0)
                        color = Color.Lerp(colors[index0], col, sqrMagnitude);
                    else
                        color = colors[index0];
                    colors[index0] = color;
                }
            }
        }
        tex.SetPixels((int)vector.x, (int)vector.y, (int)num7, (int)num8, colors, 0);
        return tex;
    }
}
public class Mathfx
{
    public static float GaussFalloff(float distance, float inRadius)
    {
        return Mathf.Clamp01(Mathf.Pow(360f, -Mathf.Pow(distance / inRadius, 2.5f) - 0.01f));
    }
    public static Vector2 NearestPointStrict(Vector2 lineStart, Vector2 lineEnd, Vector2 point)
    {
        Vector2 p = lineEnd - lineStart;
        Vector2 rhs = Normalize(p);
        float num = Vector2.Dot(point - lineStart, rhs) / Vector2.Dot(rhs, rhs);
        return (lineStart + ((Mathf.Clamp(num, 0, p.magnitude) * rhs)));
    }
    public static Vector2 Normalize(Vector2 p)
    {
        float magnitude = p.magnitude;
        return (p / magnitude);
    }
}