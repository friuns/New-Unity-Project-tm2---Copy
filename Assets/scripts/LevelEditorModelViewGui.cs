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
    private void ModelPick()
    {
        Setup(600, 600);

        Label("Search:");
        modelSearch = gui.TextField(modelSearch);
        bool searchEmpty = string.IsNullOrEmpty(modelSearch);
        gui.BeginHorizontal();
        if (stack.Count > 0 && gui.Button(".. Back", gui.ExpandWidth(false)))
            if (searchEmpty)
                modelLibCur = stack.Pop();
            else
                modelSearch = "";
        StringBuilder sb = new StringBuilder();
        foreach (var a in stack.Reverse())
            sb.Append(a.Name + "/");
        sb.Append(modelLibCur.Name);
        gui.Label(sb.ToString());
        gui.EndHorizontal();
        modelLibCur.scroll = gui.BeginScrollView(modelLibCur.scroll);



        if (recent.Count > 0)
        {
            gui.BeginHorizontal();
            for (int i = recent.Count - 1; i >= 0; i--)
                DrawFile(recent[i], false);
            gui.EndHorizontal();
            gui.Space(10);
        }




        int j = 0;

        if (searchEmpty)
        {
            gui.BeginHorizontal();
            foreach (ModelItem a in modelLibCur.dirs)
                if (a.dirs.Count > 0 || a.files.Count > 0)
                {
                    splitGui(j++);
                    if (gui.Button(new GUIContent(a.Name, a.FolderTexture), folderStype))
                    {
                        stack.Push(modelLibCur);
                        modelLibCur = a;
                    }
                }

            gui.EndHorizontal();
            //gui.BeginHorizontal();
            //j = 0;
            //foreach (ModelFile file in modelLibCur.files.OrderByDescending(a => a.usedCountSqrt))
            //{
            //    splitGui(j++);
            //    DrawFile(file);
            //}
            //gui.EndHorizontal();
        }
        //if (!searchEmpty || modelLibCur.dirs.Count > 0)
        {
            gui.BeginHorizontal();
            j = 0;
            var sr = modelSearch.ToLower().Split(' ');
            IEnumerable<ModelFile> enumerable = GetFiles(modelLibCur);
            if (!searchEmpty)
                enumerable = enumerable.Where(a => sr.Any(b => a.name.ToLower().Contains(b)));

            foreach (var a in enumerable.Take(32 * loadI))
            {
                splitGui(j++);
                DrawFile(a);
            }
            gui.EndHorizontal();
            if (Button("Load More"))
                loadI++;
        }
        if (modelLibCur != modelLibOld)
            loadI = 1;
        modelLibOld = modelLibCur;

        gui.EndScrollView();

    }
    private void DrawFile(ModelFile file, bool big = true)
    {
        if (gui.Button(new GUIContent(big ? file.name : null, file.thumb), buttonSetup(skin.button, big ? 130 : 65)))
        {
            selectedGameObject = file.gameObj;
            if (sgo)
                Destroy(sgo.gameObject);

            recent.Remove(file);
            recent.Insert(0, file);
            if (recent.Count > 8)
                recent.RemoveAt(recent.Count - 1);

            lastSgo = null;
            sgo = InitModel((GameObject)Instantiate(selectedGameObject), selectedGameObject.name);
            if (sgo.collider != null)
                sgo.collider.enabled = false;
            sgo.SetColor(Color.white);
            tool2 = Tool2.Draw;
            tool = Tool.Models;
            win.Back();
        }
    }
    public void DrawModelView()
    {


        //if (gui.Button("View tutorial"))
        //{
        //    var link = "http://www.youtube.com/watch?v=6dYHfXuNF0I&list=SP2wDyKvGmWLXTnF6uwpyShstOYESR6JmO&index=1";
        //    Application.OpenURL(link);
        //    ShowWindow(delegate { Label("Select and Ctrl+C to copy and Ctrl+V to paste"); gui.TextArea(link); }, win.act);
        //}
        //if (tool != Tool.Models && Button("Active"))
        //    tool = Tool.Models;

        if (gui.Button(Tr("Pick Model") + " (P)") || Input.GetKeyDown(KeyCode.P))
            win.ShowWindow(ModelPick, win.act, true);
        tool2 = (Tool2)Toolbar(tool == Tool.Models ? (int)tool2 : -1, new[] { "Draw", "Select2", null, selection.Count > 0 ? "Move" : null }, false);

        
        if ((int)tool2 != -1)
            tool = Tool.Models;
        if (tool == Tool.Models)
        {
            var sgoOrSelection0 = tool2 == Tool2.Select || tool2 == Tool2.Move ? selection.FirstOrDefault() : this.sgo;
            if (selection.Count > 0)
            {
                gui.Label(selection[0].name2);
                if (Button("Duplicate"))
                {
                    //if (selection.Count == 1)
                    //{
                    //    //if (sgoOrSelection0 != null)
                    //    //    Destroy(sgoOrSelection0.gameObject);
                    //    sgo = (ModelObject)Instantiate(selection[0]);
                    //    sgo.SetColor(Color.white);
                    //    tool2 = Tool2.Draw;

                    //}
                    //else
                    //{
                    Bounds b = new Bounds();
                    foreach (var a in selection)
                        b.Encapsulate(a.renderer.bounds);
                    var d = new GameObject("Duplicate");
                    duplicate = d.transform;

                    //duplicate.position = new Vector3(b.center.x, b.max.y, b.center.z);
                    duplicate.position = selection[0].pos;
                    for (int i = 0; i < selection.Count; i++)
                    {
                        var a = (ModelObject)Instantiate(selection[i]);
                        a.collider.enabled = false;
                        a.transform.parent = duplicate;
                    }
                    tool2 = Tool2.Duplicate;
                    //}
                    var position = cursor.position;
                    position.y = selection[0].pos.y;
                    cursor.position = position;
                }
                if (Button("Delete"))
                    Delete();
            }
            showGrid = Toggle(showGrid, "Show Grid");
            snap = gui.Toggle(snap, "Snap");

            if (tool2 == Tool2.Duplicate)
            {
                DrawControls(duplicate);
            }
            if (sgoOrSelection0 != null && sgoOrSelection0.gameObject.activeSelf)
            {
                DrawControls(sgoOrSelection0.transform);
            }
            

        }

    }
    Vector3 modelViewOffset;
    private bool snap;

    private void DrawControls(Transform sgot)
    {
        
        gui.BeginHorizontal();
        if (Button("Rotate Left"))
            sgot.transform.Rotate(Vector3.up, -45, Space.World);
        if (Button("Rotate Right"))
            sgot.transform.Rotate(Vector3.up, 45, Space.World);
        gui.EndHorizontal();
        gui.BeginHorizontal();


        if (gui.Button("X-"))
            sgot.transform.Rotate(-45, 0, 0, Space.Self);
        if (gui.Button("X+"))
            sgot.transform.Rotate(45, 0, 0, Space.Self);

        if (gui.Button("Y-"))
            sgot.transform.Rotate(0, -45, 0, Space.Self);
        if (gui.Button("Y+"))
            sgot.transform.Rotate(0, 45, 0, Space.Self);

        if (gui.Button("Z-"))
            sgot.transform.Rotate(0, 0, -45, Space.Self);
        if (gui.Button("Z+"))
            sgot.transform.Rotate(0, 0, 45, Space.Self);

   
        gui.EndHorizontal();
        var pow = (int)Mathf.Pow(2, scalePow);
        if (Button(Trs("Scale:") + pow))
        {
            scalePow = (scalePow + 1)%2;
            pow = (int)Mathf.Pow(2, scalePow);
            sgot.transform.localScale = pow*Vector3.one;
        }

        if (BeginVertical("Offset"))
        {
            var old = modelViewOffset;
            if (Button("Reset Offset"))
                modelViewOffset = Vector3.zero;
            Label("x:" + (int)(modelViewOffset.x * 100f));
            modelViewOffset.x = gui.HorizontalSlider(modelViewOffset.x, -3.5f, 3.5f);

            Label("y:" + (int)(modelViewOffset.y * 100f));
            modelViewOffset.y = gui.HorizontalSlider(modelViewOffset.y, -3.5f, 3.5f);

            Label("z:" + (int)(modelViewOffset.z * 100f));
            modelViewOffset.z = gui.HorizontalSlider(modelViewOffset.z, -3.5f, 3.5f);
            //if (tool2 == Tool2.Draw)
            sgot.position += modelViewOffset - old;
            //else
            foreach (var a in selection.Where(a => a.transform != sgot))
                a.pos += modelViewOffset - old;


            if (lastSgo != null && tool2 == Tool2.Draw)
                lastSgo.pos += modelViewOffset - old;

            if (sgo != null)
                sgo.flying = Toggle(sgo.flying, "In Air");
            gui.EndVertical();
        }
    }
}