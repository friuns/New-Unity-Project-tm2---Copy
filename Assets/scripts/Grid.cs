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



public class Grid:bs
{
    private Transform t;
    public void Start()
    {
        t = transform;
        if (android)
            gameObject.SetActive(false);
    }
    internal bool renderAwalys;
    public void OnRenderObject()
    {
        if (!renderAwalys)
            if (_Loader.levelEditor == null || !_Loader.levelEditor.shapeEditor) return;

        bs.res.lineMaterialYellow.SetPass(0);        

        GL.Begin(GL.LINES);
        GL.Color(cellSmallColor);
        Draw(cellSmall);
        if (cellBig != 0)
        {
            GL.Color(cellBigColor);
            Draw(cellBig, .3f);
        }
        
        GL.End();
    }
    internal float cellSmall = 1;
    internal float cellBig = 10;
    internal Color cellSmallColor = Color.white * .3f;
    internal Color cellBigColor = Color.white * .5f;

    private void Draw(float sz,float h=0)
    {
        var cells = 300f / sz;
        float w = sz*cells;
        for (int i = (int)-cells; i < cells; i++)
        {
            Vertex3(i*sz, h, w);
            Vertex3(i*sz, h, -w);
        }
        for (int j = (int)-cells; j < cells; j++)
        {
            Vertex3(-w, h, j*sz);
            Vertex3(w, h, j*sz);
        }
    }
    private void Vertex3(float x, float y, float z)
    {
        var p = t.position;
        GL.Vertex3(x + p.x, y + p.y, z + p.z);
    }
}