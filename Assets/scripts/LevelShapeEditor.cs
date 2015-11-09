using System.Runtime.Remoting.Messaging;
using Curvy.Utils;
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

public partial class LevelEditor
{
    public Camera shapeCamera;
    public string shapeName = "My Shape";

    private void EnableShapeEditor(bool b)
    {
        drawRoad = true;
        if (b)
            ShowWindowNoBack(ShapeGuiWindow);
        else
            ShowWindowNoBack(OnEditorWindow);

        if (!b)
            foreach (CurvySpline2 a in splines)
                a.GenerateMesh();
        if (b)
            tool = Tool.Move;
        shapeEditor = b;
        shapeCamera.enabled = b;
        defCamera.enabled = !b;
        camera = b ? shapeCamera : defCamera;
        if (b)
            cursor.transform.position = Vector3.up * 10;
        Reset();
    }

    public void ShapeGuiWindow()
    {
        cnt = 10;
        WinSetupScroll();
        Label("");
        Label("ShapeEditor");
        if (BackButtonLeft())
            EnableShapeEditor(false);

        if (spline != null)
        {
            if (BeginVertical("Shape Settings"))
            {
                Label("Shape Name:");
                spline.name = GUILayout.TextField(spline.name);
                //Label("Shape Terrain Height:" + Mathf.Round(spline.heightOffset));
                //spline.heightOffset = GUILayout.HorizontalSlider(spline.heightOffset, -10, 10);
                if (roadTypes == null)
                    roadTypes = Enum.GetNames(typeof(RoadType));
                spline.roadType = (RoadType)gui.Toolbar((int)spline.roadType, roadTypes);
                if (_Loader.thumbnails.Count > 0)

                    gui.EndVertical();
            }
            if (BeginVertical("Texture"))
            {
                if (Button("Pick Texture"))
                    ShowWindow(PickTexture, win.act);

                if (spline.thumb != null)
                {
                    spline.wallTexture = Toggle(spline.wallTexture, "Wall Texture");
                    spline.rotateTexture = Toggle(spline.rotateTexture, "Rotate Texture");
                    var vector2 = spline.thumb.tile;
                    Label(Trs("horizontal tile:") + Math.Round(vector2.x, 2));
                    vector2.x = 10 - gui.HorizontalSlider(10 - vector2.x, 0, 10 - .1f);
                    Label(Trs("vertical tile:") + Math.Round(vector2.y, 2));
                    vector2.y = 10 - gui.HorizontalSlider(10 - vector2.y, 0, 10 - .1f);
                    spline.thumb.tile = vector2;
                }
                gui.EndVertical();
            }
        }

        if (BeginVertical("Show/Hide"))
        {
            foreach (var a in shapes)
                a.hide = !gui.Toggle(!a.hide, a.name);
            gui.EndVertical();
        }
        DrawTools();
        TutorialHelp();
        gui.EndArea();
    }

    private void PickTexture()
    {
        Setup(800, 700);
        //if (_Loader.thumbnails.Count > 0)
        //{
        //gui.BeginVertical();
        BeginScrollView(null, true);
        curFolder = Toolbar(curFolder, _Loader.thumbnailKeys, true, false, 99, 1);
        GUIStyle st = new GUIStyle(skin.button) { fixedHeight = 150, fixedWidth = 150 };
        int i = 0;
        gui.BeginHorizontal();
        foreach (Thumbnail a in _Loader.thumbnails[_Loader.thumbnailKeys[curFolder]])
        {
            if (i % 5 == 0)
            {
                gui.EndHorizontal();
                gui.BeginHorizontal();
            }
            i++;
            Texture2D old = st.normal.background;
            st.normal.background = a == spline.thumb ? st.active.background : st.normal.background;
            if (gui.Button(a.material.mainTexture, st))
            {
                spline.thumb = a;
                win.Back();
            }
            st.normal.background = old;
        }
        gui.EndHorizontal();
        gui.EndScrollView();
        //}
        //gui.EndVertical();
    }
}