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
using UnityEditor;
#endif
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
public partial class LevelEditor
{
    public const float cellSize = 1.75f;
    GameObject selectedGameObject;
    public enum Tool2 { Draw, Select, Duplicate, Move }
    public Tool2 tool2 = Tool2.Draw;
    internal float scalePow = 0;
    public Grid grid;
    Transform duplicate;
    Stack<ModelItem> stack = new Stack<ModelItem>();
    public ModelObject sgo;
    public ModelObject lastSgo;
    public List<ModelObject> selection = new List<ModelObject>();
    public Vector3 mouseDrag;
    public bool drawDragRect;
    public Vector3 startDrag;
    public void ModeViewStart()
    {
        folderStype = buttonSetup(win.skinDefault.button, 130);
        if (modelLib != null)
        {
            var gameObj = modelLib.models.First().gameObj;
            sgo = InitModel((GameObject)Instantiate(gameObj), gameObj.name);
        }
        grid = new GameObject("Grid").AddComponent<Grid>();
        grid.renderAwalys = true;
        grid.enabled = false;
        grid.cellBig = 0;
    }
    public Vector3 SnapSgo()
    {
        if (!snap) return Vector3.zero;
        var o1 = sgo.GetComponent<MeshOutline>();
        o1.Init();
        var bounds = o1.mf.renderer.bounds;
        bounds.Expand(3);
        List<Vector3> offsets = new List<Vector3>();
        foreach (var o2 in FindObjectsOfType<MeshOutline>().Where(a => a != o1 && a.Init().mf.renderer.bounds.Intersects(bounds)))
        {
            o2.Init();
            foreach (MyVert b in o1.meshFilter.myVerts)
            {
                Debug.DrawRay(b.v, Vector3.up, Color.blue, 10);
                var v = o2.meshFilter.myVerts.Where(a => (b.v - a.v).magnitude < 2).OrderBy(a => (b.v - a.v).magnitude).FirstOrDefault();
                if (v != null)
                {
                    offsets.Add(v.v - b.v);
                    Debug.DrawLine(b.v, v.v, Color.red, 10);
                }
            }
        }
        var gr = offsets.GroupBy(a => a);
        var g = gr.OrderByDescending(a => a.Count()).FirstOrDefault();
        if (g != null)
        {
            print("Found");
            return ZeroY(g.FirstOrDefault());
        }
        return Vector3.zero;
    }
    public void UpdateModelView()
    {
        grid.enabled = tool == Tool.Models && showGrid;
        grid.cellSmall = cellSize;
        grid.cellBig = 0;
        grid.pos = ModPos(cursor.position);
        grid.cellSmallColor = new Color(0, 1, 0, .5f);
        if (sgo)
            sgo.gameObject.SetActive(tool == Tool.Models && tool2 == Tool2.Draw);
        if ((tool != Tool.Models || tool2 != Tool2.Duplicate) && duplicate != null)
            Destroy(duplicate.gameObject);
        if (Input.GetKeyDown(KeyCode.T))
        {
            var p = camera.transform.position;
            p.x = cursor.position.x;
            p.z = cursor.position.z;
            camera.transform.position = p;
            camera.transform.eulerAngles = new Vector3(90, 0, 0);
        }
        if (tool != Tool.Models) return;
        drawDragRect = false;
        if (tool == Tool.Models && sgo != null)
        {
            var p = cursor.position - ZeroY(sgo.renderer.bounds.center - sgo.transform.position);
            p = ModPos(p);
            sgo.transform.position = p + modelViewOffset;
            RaycastHit h;
            var ray = camera.ScreenPointToRay(mpos);
            bool hited = Physics.Raycast(ray, out h, 1000, 1 << Layer.block);
            ModelObject hitedObject = hited ? h.transform.root.GetComponent<ModelObject>() : null;
            if (tool2 == Tool2.Move)
            {
                if (mouseButtonDown0)
                    startDrag = ModPos(cursor.position);
                if (mouseButton0)
                {
                    var delta = startDrag - ModPos(cursor.position);
                    foreach (var a in selection)
                    {
                        a.transform.Translate(-delta, Space.World);
                        a.initPos = a.pos - modelViewOffset;
                    }
                    startDrag = ModPos(cursor.position);
                }
            }
            if (tool2 == Tool2.Select)
            {
                var m = Input.mousePosition;
                if (mouseButton0)
                    drawDragRect = true;
                if (mouseButtonUp0)
                {
                    if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.LeftShift))
                        ClearSelection();
                    if (hited && (m - mouseDrag).magnitude < 5)
                    {
                        SelectObject(hitedObject);
                        cursor.position = hitedObject.transform.position;
                    }
                }
                if (mouseButtonUp0)
                {
                    foreach (ModelObject a in FindObjectsOfType(typeof(ModelObject)))
                    {
                        var vp = camera.WorldToScreenPoint(a.renderer.bounds.center);
                        if (Rect.MinMaxRect(Mathf.Min(mouseDrag.x, m.x), Mathf.Min(mouseDrag.y, m.y), Mathf.Max(m.x, mouseDrag.x), Mathf.Max(m.y, mouseDrag.y)).Contains(vp))
                            SelectObject(a);
                    }
                }
                if (selection.Count > 0)
                    modelViewOffset = new Vector3(selection[0].pos.x % cellSize, selection[0].pos.y % cellSize, selection[0].pos.z % cellSize);
                if (Input.GetKeyDown(KeyCode.Delete))
                {
                    Delete();
                }
            }
            if (mouseButtonUp1 && hited && (Input.mousePosition - mouseDrag).magnitude < 3)
            {
                print((Input.mousePosition - mouseDrag).magnitude);
                Destroy(hitedObject.gameObject);
            }
            if (tool2 != Tool2.Select && tool2 != Tool2.Move && selection.Count > 0)
                ClearSelection();
            if (tool2 == Tool2.Duplicate && duplicate != null)
            {
                duplicate.position = ModPos(cursor.position);
                if (mouseButtonDown0)
                {
                    var g = (Transform)Instantiate(duplicate);
                    foreach (ModelObject a in g.GetComponentsInChildren<ModelObject>())
                        a.ResetColor();
                    g.DetachChildren();
                    Destroy(g.gameObject);
                }
            }
            if (tool2 == Tool2.Draw)
            {
                if (mouseButtonDown0)
                {
                    initPos = sgo.transform.position;
                    CreateModelObject(sgo.transform.position + SnapSgo());
                }

                if (mouseButton0 && initPos != Vector3.zero)
                {
                    var position = boundsEx(sgo.transform.position, sgo.renderer.bounds);
                    if (!ModelObject.taken.Contains(position))
                        CreateModelObject(position);
                }
                if (!mouseButton0)
                    initPos = Vector3.zero;

            }
        }
    }
    private void CreateModelObject(Vector3 Position)
    {
        if (!sgo.name.StartsWith("coin"))
            mapSets.usedAdvancedTools = true;
        var g = lastSgo = (ModelObject)Instantiate(sgo, Position, sgo.transform.rotation);
        g.name = sgo.name;
        g.ResetColor();
        g.initPos = sgo.transform.position;
    }
    public Vector3 boundsEx(Vector3 a, Bounds b)
    {
        var s = b.size;

        Vector3 vector3 = new Vector3(initPos.x % s.x, initPos.y % s.y, initPos.z % s.z);
        a -= vector3 / 2;
        return new Vector3(((int)(a.x / s.x)) * s.x, ((int)(a.y / s.y)) * s.y, ((int)(a.z / s.z)) * s.z) + vector3;
    }
    private Vector3 initPos;
    Tool2 tooltmp;
    internal bool showGrid = false;
    private static Vector3 ModPos(Vector3 p)
    {
        var bx = cellSize / 2f;
        p.x -= Mod((p.x + bx), bx * 2) - bx;
        p.z -= Mod((p.z + bx), bx * 2) - bx;
        p.y -= Mod((p.y + bx), bx * 2) - bx;
        return p;
    }
    private void Delete()
    {
        foreach (var a in selection)
            Destroy(a.gameObject);
        selection.Clear();
    }
    private void SelectObject(ModelObject hitedObject)
    {
        if (!selection.Remove(hitedObject))
        {
            selection.Add(hitedObject);
            hitedObject.SetColor(Color.Lerp(Color.white, Color.green, .2f));
        }
        else
            hitedObject.ResetColor();
    }
    private void ClearSelection()
    {
        foreach (ModelObject a in selection)
            a.ResetColor();
        selection.Clear();
    }
    private static void splitGui(int j)
    {
        if (j % 4 == 0)
        {
            gui.EndHorizontal();
            gui.BeginHorizontal();
        }
    }
    private GUIStyle buttonSetup(GUIStyle guiStyle, float width)
    {
        var bt = new GUIStyle(guiStyle);
        bt.wordWrap = true;
        bt.alignment = TextAnchor.LowerCenter;
        bt.imagePosition = ImagePosition.ImageAbove;
        bt.fixedWidth = width;
        bt.fixedHeight = width;
        return bt;
    }
    string modelSearch = "";
    List<ModelFile> recent = new List<ModelFile>();
    GUIStyle folderStype;
    public static IEnumerable<ModelFile> GetFiles(ModelItem cur)
    {
        foreach (var a in cur.files.OrderByDescending(a => a.usedCountSqrt))
            yield return a;
        foreach (var a in cur.dirs)
            foreach (var b in GetFiles(a))
                yield return b;
    }
    private int loadI = 1;
    private ModelItem modelLibOld;
}
