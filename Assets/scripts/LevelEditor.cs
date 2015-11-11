using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
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

public partial class LevelEditor : MapLoader
{
    private new Camera camera;
    public Transform cursor;
    private Vector3 cursorPos { get { return cursor.position; } }
    public static bool scriptRefresh;
    string[] toolStrs = new string[] { null, null, "Height", "Move", "Rotate", "Scale", "Insert", "CheckPoint", "StartPoint", "Brush", null, null, "Cam. Rot", null, "Models" };
    string[] toolStrsAndroid = new string[] { null, "Erase", "Height", "Move", "Rotate", "Scale", null, "CheckPoint", "StartPoint", "Brush", "Insert", "Cam.Move", "Cam.Rot", "Cam.Zoom" };
    string[] toolStrsShape = new string[] { null, null, null, "Move", null, null, "Insert", null, null, null, null, "Cam.Move", null, "cam zoom" };
    private bool transparent;
    public enum Tool { Draw, Erase, Height, Move, Rotate, Scale, Insert, CheckPoint, StartPoint, Brush, BrushErase, CameraMove, CameraRotate, CameraZoom, Models }
    internal Tool tool = Tool.Height;
    private RaycastHit hit;
    private Vector3 rotateCamPivot;
    public List<Material> materials;
    public Texture[] textures;

    //private float heightScroll;
    //private bool advanced=true;
    //private Color hueColor = Color.white;
    private float mouseScroll;
    internal bool mouseButtonDown1;
    private bool mouseButton1;
    private bool mouseButtonDown0;
    internal bool mouseButtonAny;
    internal bool mouseButtonDownAny;
    internal bool mouseButtonDown2;
    private bool mouseButtonUp0;
    private bool mouseButtonUp1;
    private bool mouseButton0;
    private bool onHeight;
    private bool alt;
    private bool onRotate;
    private bool onScale;
    private bool onRotate2;
    private bool onScale2;
    private bool onMove2;
    private bool onHeight2;
    private bool windowHit;
    public Camera defCamera;
    private Ray ray;
    private string search = "";
    private bool draw
    {
        get
        {
            return //android ? tool == Tool.Draw :
                tool != Tool.Erase && tool != Tool.CheckPoint && tool != Tool.StartPoint && tool != Tool.Insert && tool != Tool.BrushErase && tool != Tool.CameraMove && tool != Tool.CameraRotate && tool != Tool.CameraZoom && tool != Tool.Brush && !Input.GetKey(KeyCode.LeftShift) && tool != Tool.Models
                && (drawRoad);
        }
    }

    public new void Start()
    {
        LogEvent(EventGroup.LevelEditor, "Open Level editor");
        if (!_Loader.menuLoaded)
            _Loader.SetOffline(true);
        if (isDebug)
            hideMenu = false;
        if (!_Loader.menuLoaded)
            _Loader.sGameType = SGameType.VsPlayers;
        Time.timeScale = 1;
        var txt = new List<Material>();
        foreach (var a in res.levelTextures)
            txt.Add(a);
        materials = txt;
        textures = txt.Select(a => a.mainTexture).ToArray();
        print("Editor Start");
        camera = base.camera;
        _Loader.levelEditor = this;

        LoadTranslate();

        if (tutorialPopup && !android)
            ShowWindowNoBack(TutorialPopup);
        else
            ShowWindowNoBack(OnEditorWindow);
        StartCoroutine(_AutoQuality.OnLevelWasLoaded2(0));
        ModeViewStart();
        if (loadMap != null)
            StartLoadMap();
        if (unityMap != null)
            StartCoroutine(LoadUnityMap());
        _Loader.guiText.anchor = TextAnchor.LowerRight;
        _Loader.guiText.transform.position = new Vector3(1, 0, 0);
        base.Start();
    }

    private IEnumerator LoadUnityMap()
    {
        LogEvent(EventGroup.LevelEditor, "Test Map");
        hideTerrain = true;
        yield return StartCoroutine(LoadUnityMap(unityMap));
        ResetCam();
    }
    public void TutorialPopup()
    {
        win.Setup(400, 200, "Tutorials", Dock.Center);
        Label("Check out video tutorials on Youtube");
        VideoYoutube();
        gui.FlexibleSpace();
        gui.BeginHorizontal();
        tutorialPopup = !gui.Toggle(!tutorialPopup, "don't show me this again");
        if (Button("Continue"))
            ShowEditorWindow();
        gui.EndHorizontal();

    }
    //private bool YoutubeClicked;
    private void VideoYoutube()
    {
        if (android)
        {
            if (Button("Video Tutorial"))
            {
                Application.OpenURL("http://www.youtube.com/playlist?list=PLAAP2vqx0GEEtwLHD9_ah38jLRhP5512o");
            }
            return;
        }
        //if (YoutubeClicked)
        //{
        //    gui.BeginHorizontal();
        //    gui.TextField(link);
        //    gui.Label("Ctrl+C to copy");
        //    gui.EndHorizontal();
        //}
        if (gui.Button("Video Tutorial", gui.ExpandWidth(false)))
        {
            ShowWindow(delegate
            {
                var link = "http://www.youtube.com/playlist?list=PLAAP2vqx0GEFZ6sk5QDmkEmk16GcNFAHW";
                gui.TextField(link);
                Loader.SelectAll(link.Length);
                gui.Label("Link to tutorial, use Ctrl+C to copy");
            }, win.act);
#if UNITY_WEBPLAYER
            Screen.fullScreen = false;
            bs.ExternalEval("ShowYoutube()");
#endif
        }
    }
    private void StartLoadMap()
    {
        Popup("LoadingMap", win.act, null, true);
        //Clear();
        StartCoroutine(LoadMap(loadMap, delegate { Popup2(www.error, OnEditorWindow); }, delegate
        {
            Reset();
            ResetCam();
            ShowEditorWindow();
        }));
    }

    public void ShowEditorWindow()
    {
        if (shapeEditor)
            ShowWindowNoBack(ShapeGuiWindow);
        else
            ShowWindowNoBack(OnEditorWindow);
    }
    private void Clear()
    {
        if (start != null)
            DestroyImmediate(start.gameObject);
        foreach (CurvySpline2 a in splines.ToArray())
            if (!a.shape)
                DestroyImmediate(a.gameObject);
        tool = Tool.Height;
        ResetCam();
        affectPrev = affectNext = false;
        UpdateTerrain(null, true);
        foreach (var a in FindObjectsOfType<ModelObject>())
            Destroy(a.gameObject);
        Application.LoadLevel(Levels.levelEditor);
    }

    internal void ResetCam()
    {
        //transform.position = Vector3.up*168.399f;
        //transform.forward = Vector3.down + Vector3.forward;
        Bounds b = new Bounds();
        foreach (var a in FindObjectsOfType<MeshCollider>())
            b.Encapsulate(a.bounds);
        transform.position = b.center + Vector3.up * 168.399f;
        transform.forward = Vector3.down + Vector3.forward;
        cursor.position = b.center + Vector3.up * 50;
    }


    internal bool hitTest;
    internal CurvySplineSegment hitTestSegment;
    private void DrawLine(Vector3 a, Vector3 b)
    {
        GL.Vertex3(a.x, a.y, a.z);
        GL.Vertex3(b.x, b.y, b.z);
    }
    private void DrawLine2D(Vector3 a, Vector3 b)
    {
        a = camera.WorldToViewportPoint(a);
        b = camera.WorldToViewportPoint(b);
        b.z = a.z = 0;
        GL.PushMatrix();
        res.lineMaterialYellow.SetPass(0);
        GL.LoadOrtho();
        GL.Color(Color.yellow);
        GL.Begin(GL.LINES);
        GL.Vertex3(a.x, a.y, a.z);
        GL.Vertex3(b.x, b.y, a.z);
        GL.End();
        GL.PopMatrix();
    }
    internal CurvySplineSegment np;
    private bool mouseButton2;
    private Vector3 mpos;
    private Vector2 mouseDelta;
    private bool mouseButtonUpAny;
    public Camera curCamera { get { return shapeEditor ? shapeCamera : camera; } }
    private Color Transparement(Color c)
    {
        c.a = .5f;
        return c;
    }
    bool shift;
    public void UpdateAlways()
    {
        ray = camera.ScreenPointToRay(mpos);
        UpdateNodeRenderer();
        if (segment != null && segment.Spline2.shape && !mapLoading)
        {
            segment.Spline2.materialId = materialId;
            //segment.Spline2.color = color;
        }
        if (Input.GetKeyDown(KeyCode.R) && isDebug)
            Application.LoadLevel(Application.loadedLevelName);
        mouseButton1 = !android && Input.GetMouseButton(1);
        mouseButtonDown0 = Input.GetMouseButtonDown(0);
        mouseButtonDown1 = !android && Input.GetMouseButtonDown(1);
        mouseButtonDown2 = !android && Input.GetMouseButtonDown(2);
        mouseButtonAny = mouseButton0 || mouseButton1 || mouseButton2;
        mouseButtonDownAny = mouseButtonDown0 || mouseButtonDown1 || mouseButtonDown2;
        mouseButtonUp0 = Input.GetMouseButtonUp(0);
        mouseButtonUp1 = !android && Input.GetMouseButtonUp(1) || mouseButtonUp0 && tool == Tool.Erase;

        mouseButtonUpAny = Input.GetMouseButtonUp(0);
        mouseButton0 = Input.GetMouseButton(0) || Input.GetMouseButtonUp(0);

        if (mouseButton0 || mouseButtonDown0 || !android)
            mpos = Input.mousePosition;
        if (mouseButtonDownAny)
            mouseDrag = Input.mousePosition;

        mouseButton2 = !android && Input.GetMouseButton(2);
        //if (mouseButtonDown0 || mouseButtonDown1 || mouseButtonDown2)
        //    mouseDrag = Vector3.zero;
        alt = Input.GetKey(KeyCode.LeftAlt) && !shapeEditor;
        shift = Input.GetKey(KeyCode.LeftShift);
        if (!mouseButton0)
            onScale2 = onRotate2 = onMove2 = onHeight2 = false;
        onScale = onScale2 = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.R) || onScale2;
        onRotate = onRotate2 = Input.GetKey(KeyCode.Tab) || Input.GetKey(KeyCode.E) || onRotate2;
        onMove = onMove2 = Input.GetKey(KeyCode.CapsLock) || onMove2;
        onHeight = onHeight2 = shift || onHeight2;

        if (!onScale && !onRotate && !onMove && !onHeight)
        {
            onScale = tool == Tool.Scale;
            onRotate = tool == Tool.Rotate;
            onMove = tool == Tool.Move || tool == Tool.Insert || tool == Tool.Draw;
            onHeight = tool == Tool.Height;
        }
        mouseScroll = Input.GetAxis("Mouse ScrollWheel");

        //mouseDelta = mouseButtonAny ? new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) : Vector2.zero;
        mouseDelta = getMouseDelta();

        //print(new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")));
        //if (mouseButton0 && !mouseButtonDown0)
        //    mouseDrag += mouseDelta;
        if (shapeEditor && tool == Tool.Height)
            tool = Tool.Move;
        else
            for (int i = 0, j = 0; i < toolStrs.Length; i++)
            {
                if (toolStrs[i] == null) continue;
                if (Input.GetKeyDown(KeyCode.Alpha1 + j))
                    tool = (Tool)i;
                j++;
            }
        UpdateMove();

        if (mouseButtonUpAny && !starting && autoRefreshTerrain && (segment != null && !segment.flying || tool == Tool.Models && tool2 != Tool2.Select && lastSgo))
            if (tool == Tool.Models)
                UpdateTerrain(null, false, false, lastSgo);
            else
                UpdateTerrain(segment.IsValidSegment ? segment : segment.PreviousControlPoint, affectPrev || affectNext);


        if (!mouseButton0)
            windowHit = false;

        if (!android || mouseButtonDown0 || mouseButtonUp0)
            if (Input.mousePosition.y > Screen.height - 23 * win.scale.y || win.WindowHit)
                windowHit = true;

    }
    private void UpdateMove()
    {
        if (!android)
            if (alt && !mouseButton2 || mouseButton0) return;
        Vector3 move = Vector3.zero;
        move += new Vector3(Input.GetAxisRaw("Horizontalz"), 0, Input.GetAxisRaw("Verticalz")) * Time.deltaTime * 100;

        //float tmp = 3;
        //if(!isDebug)
        //move += new Vector3(mpos.x > Screen.width - tmp ? 1 : mpos.x < tmp ? -1 : 0, 0, mpos.y > Screen.height - tmp ? 1 : mpos.y < 50 ? -1 : 0)*30*Time.deltaTime;
        //print(mouseDelta);
        if (mouseButton2 || mouseButton0 && tool == Tool.CameraMove)
            move += new Vector3(-mouseDelta.x, 0, -mouseDelta.y) * scaleFactor * 2;

        move = Quaternion.LookRotation(ZeroY(!shapeEditor ? transform.forward : Vector3.forward)) * move * (shapeEditor ? .3f : 1);
        //move = camera.projectionMatrix.MultiplyVector(move);
        if (shapeEditor)
            move = new Vector3(-move.x, 0, move.z);
        if (move != Vector3.zero)
            pos += move;
    }

    private float scaleFactor
    {
        get { return Mathf.Max(2, (pos - cursorPos).magnitude * .01f); }
    }
    Vector3 dragStart;
    Vector3 dragMove;
    Vector3 oldCursorPos;
    public void UpdateCheckPoint()
    {
        if (oldCursorPos != Vector3.zero)
            dragMove += cursor.position - oldCursorPos;
        oldCursorPos = cursor.position;
        if (!mouseButtonAny)
            dragMove = Vector3.zero;

        RaycastHit h;
        if (tool == Tool.CheckPoint && Input.GetMouseButtonUp(1) && Physics.Raycast(camera.ScreenPointToRay(mpos), out h, 10000) && h.collider.gameObject.layer == Layer.CheckPoint)
        {
            Destroy(h.collider.gameObject);
        }
        if (tool == Tool.CheckPoint || tool == Tool.StartPoint)
        {
            if (np != null && np.dist < np.GetBounds().x / 2 && mouseButtonDown0)
            {
                dragStart = np.point;
                oldCursorPos = cursor.position = dragStart;
                var forward = (np.NextControlPoint.Position - np.Position).normalized;
                dragMove = forward * 10;
                if (tool == Tool.CheckPoint)
                    SetCheckPoint(np.point).forward = forward;
                if (tool == Tool.StartPoint)
                    SetStartPoint(np.point + Vector3.up, forward);
            }
            else if (Physics.Raycast(camera.ScreenPointToRay(mpos), out h, 10000, Layer.levelMask))
            {
                if (mouseButtonDown0)
                    dragStart = h.point;

                if (mouseButton0 && dragStart != Vector3.zero)
                    if (tool == Tool.StartPoint)
                        SetStartPoint(dragStart + Vector3.up, dragMove);
            }
        }
    }
    public void Update()
    {
        UpdateAlways();
        if (windowHit) /*window hit*/
            return;

        if ((!(onHeight && (mouseButton0 || shift)) && !android) || (android && mouseButtonAny))
            cursor.position = GetPoint();

        if (mouseScroll != 0)
            UpdateHeight(mouseScroll * 3, true);
        if (!scriptRefresh)
            foreach (SplinePathMeshBuilder a in FindObjectsOfType(typeof(SplinePathMeshBuilder)))
                a.Refresh();
        scriptRefresh = true;


        if (KeyDebug(KeyCode.Alpha1, "break"))
            Debug.Break();
        var segments = splines.Where(a => !a.shape).SelectMany(a => a.Segments);
        np = segments.Where(a => a.dist < 10)
            .OrderBy(a => a.z).FirstOrDefault();
        if (np == null)
            np = segments.Where(a => a.dist != MaxValue).OrderBy(a => a.dist).FirstOrDefault();
        if (np != null)
        {
            np.point = np.Spline2.Interpolate(np.Spline2.GetNearestPointTF(np.pivot));
            Debug.DrawLine(cursorPos, np.pivot, Color.blue);
        }
        if (!mouseButton0 || mouseButtonDown0 && android)
            //if (np != null && !np.Spline2.shape && !android && (np.pivot - pos).magnitude < 1000)
            //    rotateCamPivot = np.pivot;
            //else
            rotateCamPivot = GetPoint();

        if (Input.GetKeyDown(KeyCode.Z) && segment != null && (segment.NextControlPoint == null))
        {
            if (segment.Spline.Closed && enableClosed2)
                segment.Spline.Closed = false;
            else
                Remove(segment.gameObject);
        }
        if (Input.GetKeyDown(KeyCode.Delete) && spline != null)
            Destroy(spline.gameObject);

        if (mouseButton0 && draw)
        {
            Cursor.SetCursor(_Loader.guiSkins.updownCursor, Vector2.one * 16, CursorMode.Auto);
        }
        else
            Cursor.SetCursor(null, Vector3.zero, CursorMode.Auto);
        if (mouseButton0 && (alt || tool == Tool.CameraRotate))
        {
            //if ((rotateCamPivot - pos).magnitude < 600)
            {
                transform.RotateAround(rotateCamPivot, Vector3.up, mouseDelta.x * 10);
                transform.RotateAround(rotateCamPivot, transform.right, -mouseDelta.y * 5);
            }
        }
        if (mouseButton1 /*&& (alt || draw||tool == Tool.Models)*/ || mouseButton0 && tool == Tool.CameraZoom)
            if (!shapeEditor)
                pos += transform.forward * (mouseDelta.y) * scaleFactor * 2;
            else
                shapeCamera.orthographicSize += mouseDelta.y;
        if (alt)         /***********************alt*/
            return;
        UpdateCheckPoint();
        UpdateModelView();

        hitTestSegment = null;
        if (tool != Tool.Brush && tool != Tool.BrushErase && tool != Tool.CheckPoint)//&& (!android || tool != Tool.Draw || segment == null)
        {
            if (android)
                hitTest = Physics.SphereCast(ray, 3, out hit, 1000, 1 << Layer.node);
            else
                hitTest = Physics.Raycast(ray, out hit, 1000, 1 << Layer.node);
            if (hitTest)
                hitTestSegment = hit.transform.parent.GetComponent<CurvySplineSegment>();

            if (hitTest && mouseButtonUp1 && (Input.mousePosition - mouseDrag).magnitude < 3)
            {
                var checkPoint = getCheckPoint(hit.transform.parent);
                if (checkPoint)
                    Destroy(checkPoint.gameObject);
                else
                    Remove(hit.transform.parent.gameObject);
                return;
            }
            if (mouseButtonDown0)
            {
                if (hitTest)
                {
                    CurvySplineSegment s = hit.transform.parent.GetComponent<CurvySplineSegment>();
                    if (enableClosed2)
                        if (segment != null && s != segment && s.isEnd && segment.isEnd && s.Spline == segment.Spline && draw && !s.Spline2.shape && segment.Spline2.Count > 2)
                        {
                            s.Spline.Closed = true;
                            s.Spline.Update();
                            print("Close");
                            return;
                        }

                    segment = s;

                    drag = segment.transform;
                    //heightScroll = 0;
                    CopyFrom(segment, segment.Position);
                    segment.dragOffset = drag.position - cursor.position;

                    if (draw)
                        return;
                }
                else if (!drawRoad)
                    segment = null;
                //else if ((tool == Tool.CheckPoint || tool == Tool.StartPoint) && mouseButtonDown0)
                //Popup2("You must click on white sphere to add " + tool, OnEditorWindow);
            }

        }
        if (!mouseButton0)
            drag = null;
        if (onHeight && (mouseButton0 || shift))
        {
            UpdateHeight(mouseDelta.y, drag == null);
            if (tool != Tool.Height)
                return;
        }
        if (np != null && np.dist < 100 && drag == null)
        {

            var sg = np;
            if (!mouseButtonDown0)
            {
                if (mouseButton0 && tool == Tool.Brush)
                {
                    //foreach (var a in brushShapes)
                    //    if (!sg.spls.Contains(a))
                    //        sg.spls.Add(a);
                    sg.spls = brushShapes.ToList();
                }
                //if (mouseButton0 && tool == Tool.BrushErase)
                //{
                //    foreach (var a in brushShapes)
                //        sg.spls.Remove(a);
                //}
            }
            if (tool == Tool.Insert && mouseButtonDown0)
            {
                CopyFrom(sg, np.point);
                AddPoint(np.point, sg, false, true);
                return;
            }
        }
        if (segment != null)
        {
            var delta = mouseDelta.y;

            if (mouseButton0 && drag != null && onRotate)
            {
                if (affectNext || affectPrev)
                {
                    foreach (var a in GetAffectBy())
                        a.transform.RotateAround(segment.transform.position, Vector3.up, delta * 6);
                }
                else
                {
                    swirl += delta * 3;
                    UpdateSwirl();
                }
            }
            if (mouseButton0 && onScale && drag != null)
            {
                UpdateScale(delta);
                scale += delta;
            }
            if (mouseButton0 && onMove && drag != null)
            {
                var d = cursorPos - segment.Position + segment.dragOffset;
                foreach (CurvySplineSegment a in GetAffectBy())
                    a.Position += d;
            }
            //if (mouseButtonUp0 && tool == Tool.CheckPoint && !getCheckPoint(segment.transform) && hitTest)
            //{
            //    if (start == null || CheckStartPoint(start.position, segment.Position))
            //        SetCheckPoint(segment);
            //}
            //if (tool == Tool.StartPoint)
            //{
            //    if (mouseButton0 && drag != null)
            //    {
            //        SetStartPoint(segment);
            //        start.LookAt(mousePos + segment.transform.forward * 10 + segment.transform.up * 3);
            //    }
            //}
        }
        if (mouseButtonDown0)
        {
            //if (tool == Tool.Height || advanced)
            //{
            if (draw)
            {
                if ((segment == null || segment.PreviousControlPoint == null || segment.NextControlPoint == null))
                {
                    if (shapeEditor)
                        mapSets.usedAdvancedTools = true;

                    if (segment == null)
                        StartCoroutine(CreateSpline(delegate { AddPoint(cursorPos, null); }, shapeEditor));
                    else
                    {
                        if (brushShapes.Count == 0)
                            brushShapes.Add(shapes[0]);

                        var b = Vector3.Dot(segment.transform.forward, (cursorPos - segment.Position).normalized) > 0 || segment.IsFirstSegment || segment.IsLastSegment;

                        AddPoint(cursorPos, b ? segment : segment.PreviousControlPoint, segment == segment.Spline.ControlPoints[0] && segment.Spline.ControlPoints.Count > 1);
                        return;
                    }
                }
                else
                    Reset();
            }

            //}
            //else if (tool != Tool.Insert && tool != Tool.CheckPoint && tool != Tool.StartPoint && tool != Tool.Brush)
            //{
            //    Popup("To " + tool + " road please click and drag white sphere", OnEditorWindow, 600, 250);
            //}
        }
        if (mouseButtonUp1 && win.mouseDrag < (android ? 5 : 1) / 1024f)
            Reset();
        UpdateSwirl2();
    }
    //private Vector2 mouseDrag;
    internal bool autoRefreshTerrain;

    public class ObjectList
    {
        public List<GameObject> list = new List<GameObject>();
        public bool enabled = true;
    }
    //public void HideGroupAdd(GameObject g)
    //{
    //    var r = Regex.Replace(g.name, @"CP\d*|.Clone.", "");
    //    var o = hideGroup.TryGet(r, new ObjectList());
    //    o.list.Add(g);

    //}

    private void UpdateNodeRenderer()
    {
        foreach (CurvySpline2 sp in splines)
        {
            foreach (CurvySplineSegment s in sp.ControlPoints)
            {
                var node = s.transform.Find("model");

                node.renderer.enabled = node.collider.enabled = tool != Tool.Models && !sp.hide;

                if (s != null && s.transform != null && s.transform.childCount > 0 && node.renderer != null)
                {
                    node.renderer.material.color = Transparement(s == segment ? Color.red : s == hitTestSegment ? Color.green : Color.white);
                    //node.renderer.enabled = node.collider.enabled = !sp.hide;
                    if (!s.Spline2.shape)
                        node.localScale = Vector3.one * (3f + (node.position - camera.transform.position).magnitude * .03f);
                }
            }
            if (sp.pivot != null)
            {
                var child = sp.pivot.transform.GetChild(0);
                child.collider.enabled = child.renderer.enabled = !sp.hide;
            }
        }
    }
    //private bool CheckStartPoint(Vector3 a, Vector3 b)
    //{
    //    if (start != null && (a - b).magnitude < 10)
    //    {
    //        Popup("CheckPoint is too near to StartPoint", OnEditorWindow);
    //        return false;
    //    }
    //    return true;
    //}
    private Transform getFinnish(Transform cv)
    {
        return cv.transform.Find("Finnish");
    }
    private Transform getCheckPoint(Transform cv)
    {
        var find = cv.transform.Find(res.CheckPoint.name);
        if (find != null)
            return find;
        return getFinnish(cv);
    }
    private bool onMove;
    public bool flying;
    private void CopyFrom(CurvySplineSegment segment, Vector3 position)
    {
        swirl = segment.swirl;
        scale = segment.scale;
        flying = segment.flying;

        spline = (CurvySpline2)segment.Spline;
        if (spline.shape)
        {
            //color = spline.color;
            materialId = spline.materialId;
        }
        //selectedColorTexture = pickTexture = null;
        //selectedShapes = segment.spls;
        if (Math.Abs(position.y - cursor.position.y) > .01f)
        {
            cursor.position = position;
            cursor.position = GetPoint();
        }
    }
    public void SaveMapWindow()
    {
        if (www == null)
        {
            Label("Map Name");
            mapName = Loader.Filter(gui.TextField(mapName, 20));
            if (Button("Save"))
                if (mapName.Length < 3)
                    Popup("Enter Map Name", SaveMapWindow, "Continue");
                else
                    StartCoroutine(SaveMap());
        }
        else
        {
            Label(Trs("Uploading: ") + (int)(www.progress * 100) + "%");
            if (www.isDone)
                gui.Label(string.IsNullOrEmpty(www.error) ? www.text : www.error);
        }
    }
    public IEnumerator SaveMap()
    {
        LogEvent(EventGroup.LevelEditor, "Save Map");
        tool = Tool.Height;
        Update();
        UpdateModelView();
        //foreach (var a in hideGroup)
        //    foreach (var b in a.Value.list)
        //        if (b != null)
        //            b.SetActive(true);
        //if (FindObjectsOfType<Coin>().Length > 5)
        //    mapSets.enableCoins = true;
        //if (GameObject.FindGameObjectWithTag(Tag.blueSpawn) && GameObject.FindGameObjectWithTag(Tag.redSpawn))
        //    mapSets.enableCtf = true;
        //mapSets.race = GameObject.FindGameObjectsWithTag("CheckPoint").Length >= 2;
        using (var ms = new BinaryWriter())
        {
            ms.Write((int)LevelPackets.Version);
            ms.Write(setting.version);
            Debug.Log("write map name " + unityMap);
            if (!string.IsNullOrEmpty(unityMap))
            {
                ms.Write((int)LevelPackets.unityMap);
                ms.Write(unityMap);
            }
            ms.Write((int)LevelPackets.Nitro);
            ms.Write(nitro);
            if (hideTerrain)
                ms.Write((int)LevelPackets.disableTerrain);
            ms.Write((int)LevelPackets.AntiFly);
            ms.Write(_GameSettings.gravitationAntiFly);
            ms.Write(_GameSettings.gravitationFactor);

            ms.Write((int)LevelPackets.levelTime);
            ms.Write(_GameSettings.levelTime);
            if (start != null)
            {
                ms.Write((int)LevelPackets.StartPos);
                ms.Write(start.position);
                ms.Write(start.forward);
            }


            foreach (ModelObject a in FindObjectsOfType(typeof(ModelObject)))
            {
                if (modelLib != null && modelLib.dict.ContainsKey(a.name2))
                {
                    ms.Write((int)LevelPackets.Block);
                    ms.Write(a.name2);
                    ms.Write(a.pos);
                    ms.Write(a.rot.eulerAngles);
                    ms.Write(a.transform.localScale);
                    if (a.flying)
                        ms.Write((int)LevelPackets.FlyingModel);
                }
                else
                {
                    print(a.name2 + " not found");
                }
            }


            var splines = this.splines.OrderByDescending(a => a.shape).ToArray();
            for (int i = 0; i < splines.Length; i++)
                splines[i].saveId = i;

            foreach (var a in checkPoints.Where(a => a.name.StartsWith("CheckPoint2")))
            {
                ms.Write((int)LevelPackets.CheckPoint2);
                ms.Write(a.transform.position);
                ms.Write(a.transform.eulerAngles);
            }
            foreach (CurvySpline2 sp in splines)
            {
                if (sp.shape)
                {
                    ms.Write((int)LevelPackets.shape);
                    ms.Write(sp.pivot.Position);
                    ms.Write(sp.saveId);
                    ms.Write(sp.tunnel);
                    ms.Write(sp.materialId);
                    ms.Write(sp.color);
                    ms.Write(sp.name);
                    if (sp.thumb != null)
                    {
                        ms.Write((int)LevelPackets.shapeMaterial);
                        ms.Write(sp.thumb.url);
                        ms.Write((int)LevelPackets.textureTile);
                        ms.Write(sp.thumb.material.mainTextureScale);
                    }
                    ms.Write((int)LevelPackets.roadtype);
                    ms.Write((byte)sp.roadType);
                    if (sp.wallTexture)
                        ms.Write((int)LevelPackets.Wall);
                    if (sp.rotateTexture)
                        ms.Write((int)LevelPackets.rotateTexture);

                }
                else
                    ms.Write((int)LevelPackets.Spline);

                foreach (var b in sp.ControlPoints)
                {
                    ms.Write((int)LevelPackets.Point);
                    ms.Write(b.Position);
                    ms.Write(b.swirl);
                    ms.Write((int)LevelPackets.scale);
                    ms.Write(b.scale);
                    ms.Write((int)LevelPackets.Flying);
                    ms.Write(b.flying);

                    if (getCheckPoint(b.transform))
                    {
                        if (getFinnish(b.transform))
                            ms.Write((int)LevelPackets.Finnish);
                        //ms.Write((int)LevelPackets.CheckPoint);
                        print("Write Checkpoint");
                    }
                    if (!sp.shape)
                    {
                        foreach (var c in b.spls)
                        {
                            ms.Write((int)LevelPackets.brush);
                            ms.Write(c.saveId);
                        }
                    }
                }
                if (sp.Closed)
                    ms.Write((int)LevelPackets.ClosedSpline);
            }

            ms.Write((int)LevelPackets.Laps);
            ms.Write(laps);
            if (_Loader.curSceneDef.disableJump)
                ms.Write((int)LevelPackets.disableJump);
            if (flatTerrain)
                ms.Write((int)LevelPackets.flatTerrain);
            www = Download(mainSite + "scripts/sendMap.php", null, true, "name", _Loader.playerNamePrefixed, "map", mapName, "file", ms.ToArray(), "version", setting.levelEditorVersion, "flags", (int)mapSets.levelFlags);
            yield return www;
            www = null;
            myMaps.Add(_Loader.playerNamePrefixed + "." + mapName);
            myMaps = myMaps.Distinct().ToList();
            ShowWindowNoBack(TestMapWindow);
        }

    }
    public List<string> myMaps { get { return PlayerPrefsGetStrings("savedMaps"); } set { PlayerPrefsSetStringList("savedMaps", value); } }
    private void TestMapWindow()
    {
        Label("Do you want to publish map? you have to test it before it get published", 16, true);
        gui.BeginHorizontal();
        if (Button("No"))
            ShowEditorWindow();
        if (Button("Yes"))
        {
            //if (mapSets.enableCoins && FindObjectsOfType<Coin>().Length < 5)
            //{
            //    Popup("You must add at least 5 coins");
            //    return;
            //}
            if (mapSets.enableDm && GameObject.FindGameObjectsWithTag(Tag.Spawn).Length < 3)
            {
                Popup("You must add at least 5 spawns");
                return;
            }
            if (mapSets.enableCtf && (!GameObject.FindGameObjectWithTag(Tag.blueSpawn) || !GameObject.FindGameObjectWithTag(Tag.redSpawn)))
            {
                Popup("You must add red and blue team spawns");
                return;
            }

            if (mapSets.enableDm || mapSets.enableCtf || isDebug)
                SubmitMap();
            else if (mapSets.race)
                StartCoroutine(StartTest(true));

        }

        gui.EndHorizontal();
    }
    public IEnumerable<CurvySplineSegment> GetAffectBy()
    {
        if (affectNext)
            for (int i = segment.ControlPointIndex; i < segment.Spline.ControlPointCount; i++)
                yield return segment.Spline.ControlPoints[i];
        if (affectPrev)
            for (int i = segment.ControlPointIndex - (affectNext ? 1 : 0); i >= 0; i--)
                yield return segment.Spline.ControlPoints[i];
        if ((affectPrev || affectNext) && segment.Spline2.pivot != null)
            yield return segment.Spline2.pivot;
        if (!affectNext && !affectPrev)
            yield return segment;
    }
    private bool hideMenu;
    protected void Menu() { }
    public void OnGUI()
    {

        if (win.act != OnEditorWindow && !isDebug)
            return;
        CustomWindow.GUIMatrix();
        GUI.depth = -1;
        gui.BeginHorizontal("", skin.GetStyle("flow background"));
        {
            if (Button("Start Test"))
                if (shapeEditor)
                    EnableShapeEditor(false);
                else
                    StartCoroutine(StartTest(false));

            if (!hideMenu)
            {

                if (Button("Save"))
                {
                    //if ((GameObject.FindGameObjectWithTag(Tag.CheckPoint) == null ||
                    //     GameObject.FindGameObjectWithTag(Tag.Start) == null) && !isDebug)
                    //    ShowPopup("Please Add CheckPoints first", OnEditorWindow);
                    //else
                    ShowWindow(SaveMapWindow, win.act);
                }

                if (Button("Load"))
                {
                    _Loader.userMaps.Clear();
                    ShowWindow(LoadMapWindow, win.act);
                    StartCoroutine(_Loader.DownloadUserMaps(0, 0));
                }
                if (Button("Load (.Unity3D)"))
                    ShowWindow(LoadUnityMapWindow, win.act);
            }
            else if (Button("Show More Tools"))
            {
                ShowEditorWindow();
                hideMenu = false;
            }

            if (BackButton("Back to menu"))
                ShowWindowNoBack(delegate
                {
                    Label("Exit level editor?");
                    gui.BeginHorizontal();
                    if (Button("Yes")) BackToMenu();
                    if (Button("No")) ShowEditorWindow();
                    gui.EndHorizontal();
                });
        }
        gui.EndHorizontal();
        if (Event.current.type == EventType.Repaint && !string.IsNullOrEmpty(GUI.tooltip))
            win.tooltip = GUI.tooltip;
    }

    public void LoadUnityMapWindow()
    {
        BeginScrollView();
        gui.BeginHorizontal();
        TextField("Url:", unityMap);
        if (Button("Load", false))
            Clear();

        gui.EndHorizontal();
        foreach (var a in _Loader.scenes.Where(a => !a.userMap))
            if (Button(a.name))
                unityMap = mapName = a.name;
        gui.EndScrollView();
    }

    private string[] tutorialUrls;
    private IEnumerator LoadTutorial()
    {
        tutorialTextures = new List<Texture2D>();
        var w = new WWW(mainSite + "/docs/tutorial/getFiles.php");
        yield return w;
        tutorialUrls = SplitString(w.text);
        foreach (var a in tutorialUrls)
        {

            w = new WWW(mainSite + "/docs/tutorial/" + Uri.EscapeUriString(a));
            yield return w;
            if (w.texture != null)
                tutorialTextures.Add(w.texture);
        }
    }
    private List<Texture2D> tutorialTextures;
    private int selectedSlide;
    private void Tutorial()
    {
        Setup(Screen.width, Screen.height, "Tutorial");
        //win.offset2.y = 20;
        Label("");
        if (tutorialTextures.Count == 0)
        {
            VideoYoutube();
            Label("Loading");

            if (BackButtonLeft())
                ShowEditorWindow();
        }
        else
        {
            gui.BeginHorizontal();
            gui.FlexibleSpace();
            if (selectedSlide > 0 && Button("<<Prev"))
                selectedSlide--;
            gui.Label((selectedSlide + 1) + "/" + tutorialUrls.Length + " Slide");
            if (selectedSlide < tutorialTextures.Count - 1 && Button("Next>>"))
                selectedSlide++;

            gui.FlexibleSpace();
            if (gui.Button("Close"))
                ShowEditorWindow();
            gui.EndHorizontal();
            var t = tutorialTextures[selectedSlide];
            gui.Label(t, gui.Height(Screen.height - 50));
        }
    }
    private void BackToMenu()
    {
        _Loader.LoadLevel(Levels.menu);
    }




    //public void OnEditorGui2()
    //{
    //    OnEditorGui2();
    //}

    private float windowScroll;
    public Vector2 roadShapesScroll;
    //public string hideSearch="";
    public void OnEditorWindow()
    {
        if (hideMenu)
        {
            win.CloseWindow();
            return;
        }
        WinSetupScroll();

        //gui.Label("");


        if (!shapeEditor)
        {
            if (BeginVertical("Road Shapes"))
            {
                if (Button("Road Shape Editor"))
                    EnableShapeEditor(true);
                foreach (CurvySpline2 a in shapes)
                {
                    var ss = brushShapes;
                    bool contains = ss.Contains(a);
                    var toggle = gui.Toggle(contains, a.name);
                    if (toggle != contains)
                    {
                        if (toggle)
                            ss.Add(a);
                        else
                            ss.Remove(a);
                    }
                }
                gui.EndVertical();
            }
        }

        //if (BeginVertical("Hide Objects"))
        //{
        //    gui.BeginHorizontal();
        //    Label("search");
        //    hideSearch = gui.TextField(hideSearch).ToLower();
        //    gui.EndHorizontal();

        //    foreach (var pair in hideGroup)
        //        if (string.IsNullOrEmpty(hideSearch) || pair.Key.ToLower().Contains(hideSearch))
        //        {
        //            var toggle = Toggle(pair.Value.enabled, pair.Key);
        //            if (toggle != hideGroup[pair.Key].enabled)
        //            {
        //                pair.Value.enabled = toggle;
        //                foreach (var a in pair.Value.list)
        //                {
        //                    if (a != null)
        //                    {
        //                        a.SetActive(toggle);
        //                        var s = a.GetComponent<SplinePathMeshBuilder>();
        //                        if (s != null)
        //                            s.Refresh();
        //                    }
        //                }
        //                pair.Value.list.Remove(null);
        //            }
        //        }
        //    gui.EndVertical();
        //}
        DrawTools();

        if (BeginVertical("Models"))
        {
            if (modelLib==null)
                Label("Loading");
            else
                DrawModelView();

            gui.EndVertical();
        }

        if (BeginVertical("Terrain"))
        {
            var ht = gui.Toggle(hideTerrain, "Hide Terrain");
            if (ht != hideTerrain)
            {
                CreateTerrain();
                hideTerrain = ht;
            }

            if (!ht)
            {
                var toggle = Toggle(flatTerrain, "Flat Terrain");
                if (toggle != flatTerrain)
                {
                    SetFlatTerrain(toggle);
                    UpdateTerrain(null, true, showTrees);
                }
                if (Button("Refresh Terrain"))
                    UpdateTerrain(null, true, showTrees);
                autoRefreshTerrain = Toggle(autoRefreshTerrain, "Auto Refresh Terrain");
                showTrees = gui.Toggle(showTrees, "Show Trees");

            }
            gui.EndVertical();
        }
        if (BeginVertical("Settings"))
        {
            if (Button("Level Settings"))
                ShowWindow(LevelSettingsWindow, win.act);
            if (Button("Clear"))
            {
                unityMap = "";
                Clear();
            }
            if (Button("Reset Camera"))
                ResetCam();
            gui.EndVertical();
        }
        TutorialHelp();
        var xMax = GUILayoutUtility.GetLastRect().yMin;
        if (xMax > 1)
            scrollMax = xMax;
        gui.EndArea();

        //gui.EndScrollView();


    }

    private void WinSetupScroll()
    {
        //int width = Mathf.Min(300, (int)(Screen.width * .3f / CustomWindow.guiscale.x));
        int width = android ? 170 : 250;
        win.Setup(width, 1000, "", Dock.Left, null, null, 1.3f);
        //win.offset2.y = 20;
        if (windowHit && mouseScroll != 0)
            windowScroll = Mathf.Clamp(windowScroll + mouseScroll * 600, -scrollMax + 100, 0);

        gui.BeginArea(new Rect(0, windowScroll + 30, width - 2, 1000));
    }
    private int brushToolsTab;
    private bool drawRoad = true;
    private void DrawTools()
    {
        if (BeginVertical("Road Tools"))
        {
            if (!shapeEditor)
                drawRoad = Toggle(drawRoad, "Draw Road and");
            Tool toolbar = (Tool)Toolbar((int)tool, shapeEditor ? toolStrsShape : android ? toolStrsAndroid : toolStrs, true, false, 99, 2);
            if (tool != toolbar)
            {
                if (toolbar == Tool.Brush)
                    ToggleTab("Road Shapes");
            }
            tool = toolbar;

            if (!hideTerrain && !shapeEditor)
            {
                flying = Toggle(flying, "In Air");
                if (segment != null)
                    segment.flying = flying;
            }
            affectPrev = Toggle(affectPrev, "Affect to Left Side");
            affectNext = Toggle(affectNext, "Affect to Right Side");
            //if (!shapeEditor && BeginVertical("Point Settings", skin.window) && isDebug)
            //{
            //    Label(Trs("Height (mouse scroll):") + Mathf.Round(heightScroll));
            //    var h = gui.HorizontalSlider(heightScroll, -30, 30);
            //    if (h != heightScroll)
            //    {
            //        UpdateHeight(h - heightScroll);
            //        heightScroll = h;
            //    }
            //    Label(Trs("Swirl:") + Mathf.Round(swirl));
            //    var sw = gui.HorizontalSlider(swirl, -180, 180);
            //    if (sw != swirl)
            //    {
            //        swirl = sw;
            //        if (segment != null)
            //            UpdateSwirl();
            //    }
            //    Label(Trs("Size:") + Mathf.Round(scale));
            //    var sc = gui.HorizontalSlider(scale, 1, 5);
            //    if (sc != scale)
            //    {
            //        if (segment != null)
            //            UpdateScale(sc - scale);
            //        scale = sc;
            //    }
            //    flying = Toggle(flying, "Flying");
            //    if (segment != null)
            //    {
            //        segment.flying = flying;
            //        spline.Closed = Toggle(spline.Closed, "Closed");
            //    }

            //    gui.EndVertical();
            //}
            gui.EndVertical();
        }

    }
    private float scrollMax;
    //private void Duplicate()
    //{
    //    foreach (CurvySplineSegment a in segment.Spline2.ControlPoints)
    //    {
    //        var findChild = a.transform.FindChild("Start");
    //        if (findChild != null)
    //            Destroy(findChild.gameObject);
    //    }

    //    var t = (CurvySpline2) Instantiate(spline);
    //    foreach (CurvySplineSegment a in t.GetComponentsInChildren<CurvySplineSegment>())
    //    {
    //        a.spls = new List<CurvySpline2>(a.spls);
    //        foreach (var c in a.sbs)
    //            Destroy(c.gameObject);
    //        a.sbs.Clear();
    //    }

    //    t.pos += ZeroY(Vector3.one* 10);

    //    if (spline.shape)
    //        t.pivot = (CurvySplineSegment)Instantiate(spline.pivot);
    //    tool = Tool.Move;
    //    affectPrev = affectNext = true;
    //}

    private string[] roadTypes;
    private int curFolder;

    private void LevelSettingsWindow()
    {
        BeginScrollView();
        if (Button("Game Settings"))
            _Loader.ShowWindow(_Loader.SettingsWindow, win.act);
        Label(Trs("Laps:") + laps);
        laps = (int)gui.HorizontalSlider(laps, 1, 10);
        Label(Trs("Nitro:") + nitro);
        nitro = (int)gui.HorizontalSlider(nitro, 0, 10);
        Label(Trs("AntiFly:") + _GameSettings.gravitationFactor);
        _GameSettings.gravitationFactor = gui.HorizontalSlider(_GameSettings.gravitationFactor, 0, 3);
        Label(Trs("Gravity:") + _GameSettings.gravitationAntiFly);
        _GameSettings.gravitationAntiFly = gui.HorizontalSlider(_GameSettings.gravitationAntiFly, 1, 3);
        _Loader.curSceneDef.disableJump = gui.Toggle(_Loader.curSceneDef.disableJump, "Disable jumping");

        bool trs = gui.Toggle(transparent, "Transparent");
        if (trs != transparent)
        {
            if (trs)
                camera.SetReplacementShader(res.transparmentEditor, "");
            else
                camera.ResetReplacementShader();
            transparent = trs;
        }

#if oldFlags
        gui.BeginHorizontal();
        LabelCenter("Game Type:" + mapSets.levelFlags);
        if (GlowButton("Race", mapSets.race))
            mapSets.levelFlags = LevelFlags.race;
        if (GlowButton("Deathmatch", mapSets.enableDm))
            mapSets.levelFlags = LevelFlags.dm;
        if (GlowButton("Capture Flag", mapSets.enableCtf))
            mapSets.levelFlags = LevelFlags.Ctf;
        gui.EndHorizontal();
#endif

        //if(mapSets.enableCoins)
        //{
        //    gui.Label(Tr("Seconds:")+(int)_GameSettings.levelTime);
        //    _GameSettings.levelTime = gui.HorizontalSlider(_GameSettings.levelTime, 10, 120);
        //}
        if (isDebug)
            Toggle(mapSets.usedAdvancedTools, "Advanced Tools");
        gui.EndScrollView();
    }
    private void TutorialHelp()
    {
        if (BeginVertical("Help"))
        {
            //BeginScrollView();            
            if (android)
                VideoYoutube();
            if (!android)
            {
                //Label("Documentation");
                skin.label.alignment = TextAnchor.UpperLeft;
                skin.label.wordWrap = true;
                skin.label.fontSize = 11;
                if (!string.IsNullOrEmpty(win.tooltip))
                    gui.Label(win.tooltip);
                else
                {
                    VideoYoutube();
                    if (Button("View tutorial"))
                    {
                        if (tutorialTextures == null)
                            StartCoroutine(LoadTutorial());
                        ShowWindowNoBack(Tutorial);
                    }

                    gui.Label(Tr(shapeEditor ? "shapeeditorhelp" : "editorhelp"));
                    gui.Label(Tp(toolStrs[Mathf.Min(toolStrs.Length - 1, (int)tool)]));
                }
            }
            //gui.EndScrollView();
            gui.EndVertical();
        }

        gui.FlexibleSpace();
    }
    private void UpdateHeight(float height, bool scroll = false)
    {
        var f = Vector3.up * height * 4;
        pos += f;
        cursor.position += f;
        if (!scroll)
            foreach (var segment in GetAffectBy())
            {
                if (segment != null)
                {
                    segment.Position += f;
                    segment.Spline.Refresh();
                }
            }
    }
    protected void UpdateRotation() { }
    private void UpdateSwirl()
    {
        segment.swirl = swirl;
        UpdateSwirl2();
        segment.Spline.Refresh();
    }
    private void UpdateScale(float dt)
    {
        foreach (CurvySplineSegment a in GetAffectBy())
            a.scale = a.transform.localScale.x + dt;
        segment.Spline.Refresh();
    }
    public void LoadMapWindow()
    {
        gui.BeginHorizontal();
        search = gui.TextField(search);
        if (Button("Search")) { StartCoroutine(_Loader.DownloadUserMaps(0, (int)_Loader.mapSets.levelFlags, search)); }
        gui.EndHorizontal();
        BeginScrollView();


        if (Button("Sample Map"))
        {
            Popup2("Loading Map");
            mapName = "primer1";
            loadMap = "usermaps/soulkey.primer1.map";
            Application.LoadLevel(Levels.levelEditor);
        }


        foreach (Scene a in _Loader.userMaps)
        {
            if (Button(a.name))
            {
                Debug.LogWarning("Loading Map\n" + a.title + "\n" + a.url);
                Popup2("Loading Map\n" + a.title + "\n" + a.url);
                mapName = a.title;
                loadMap = a.url + "?r=" + Random.value;
                Application.LoadLevel(Levels.levelEditor);
            }
        }
        //foreach (var a in myMaps)
        //{
        //    if (Button(a))
        //    {
        //        //Debug.LogWarning("Loading Map\n" + a.title + "\n" + a.url);
        //        //Popup2("Loading Map\n" + a.title + "\n" + a.url);
        //        mapName = a;
        //        loadMap = "usermaps/" + a + ".map";
        //        Application.LoadLevel(Levels.levelEditor);
        //    }
        //}
        gui.EndScrollView();
    }
    private IEnumerator StartTest(bool submit)
    {
        _Loader.gameType = GameType.race;
        tool = Tool.Height;
        UpdateModelView();
        if (submit && checkPoints.Length < 2)
        {
            Popup("You need to put at least 2 CheckPoints", OnEditorWindow);
            yield break;
        }
        starting = true;

        //hideTerrain = false;

        if (submit)
            UpdateTerrain(null, true, showTrees);


        submitMapPublish = submit;
        win.CloseWindow();
        _Loader.replays.Clear();
        _GameSettings.laps = laps;
        _Loader.night = _Loader.rain = false;
        _Loader.mapName = _Loader.playerNamePrefixed + "." + mapName;
        Time.timeScale = 1;
        yield return StartCoroutine(ActiveEditor(false));
        if (_Game == null)
            LoadLevelAdditive(Levels.game);
        else
        {
            ReEnable();
            _Player.cam.gameObject.SetActive(true);
            _Game.SetStartPoint();
            _Game.ResetCoins();
            if (submit || _Game.finnish || !_Game.started)
            {
                _Game.gameState = GameState.started;
                _Game.RestartLevel();
            }
        }
        gameObject.SetActive(false);
        starting = false;
    }
    internal bool submitMapPublish;
    private bool starting;
    public IEnumerator Resume()
    {
        if ((_Game.finnish || isDebug) && submitMapPublish)
        {
            if (_Game.timeElapsed > 20 || isDebug)
            {
                bool yes = false, no = false;
                ShowWindowNoBack(delegate { Label("Do you want to publish map right now?"); gui.BeginHorizontal(); no = Button("No"); yes = Button("Yes"); gui.EndHorizontal(); });
                while (!no && !yes)
                    yield return null;
                if (yes)
                {
                    SubmitMap();
                    yield break;
                }
            }
        }
        yield return null;
        Time.timeScale = 0;
        ShowEditorWindow();
        _Player.cam.gameObject.SetActive(false);
        Disable(typeof(GameSettings));
        Disable(typeof(Player));
        Disable(typeof(Game));
        Disable(typeof(Hud));
        gameObject.SetActive(true);
        StartCoroutine(ActiveEditor(true));

        if (submitMapPublish && _Game.finnish)
            Popup("Your map is too short for publishing");



    }

    private void SubmitMap()
    {
        LogEvent(EventGroup.LevelEditor, "Submit Map");
        _Awards.LevelCreator.count++;
        _Loader.mapName = _Loader.playerNamePrefixed + "." + mapName;
        _Loader.AproveMap(1, mapSets.usedAdvancedTools ? 1 : 0);
        //_Loader.gamePlayed = true;
        //_Loader.tabSelected = Tag.userTab;
        ShowWindowNoBack(delegate { Label("Map published "); if (BackButtonLeft()) BackToMenu(); });
    }

    private void Reset()
    {
        if (segment != null && segment.Spline.ControlPointCount < (segment.Spline2.shape ? 3 : 2))
            Remove(segment.gameObject);
        spline = null;
        segment = null;
        affectPrev = affectNext = false;
        print("Reset");
    }
    protected void Destroy() { }
    public bool tutorialPopup { get { return PlayerPrefs.GetInt("tutorialPopup", 1) == 1; } set { PlayerPrefs.SetInt("tutorialPopup", value ? 1 : 0); } }
    protected bool enableClosed2 = true;
    private void Remove(GameObject o)
    {
        CurvySplineSegment c = o.GetComponent<CurvySplineSegment>();
        if (c.Spline2.pivot == c) return;

        if (enableClosed2)
            if ((c.IsFirstSegment || c.IsLastSegment) && c.Spline2.Closed && !c.Spline2.shape)
            {
                c.Spline2.Closed = false;
                return;
            }

        var curvySplineSegment = c.PreviousSegment == null ? c.NextSegment : c.PreviousSegment;
        segment = c.NextControlPoint == null ? c.PreviousControlPoint : c.NextControlPoint;

        if (c.Spline.Segments.Count <= (shapeEditor ? 2 : 1))
        {
            print("Destr");
            Destroy(c.Spline.gameObject);
        }
        else
            c.Spline.Delete(c);

        if (segment != null && segment.NextControlPoint != null && segment.PreviousControlPoint != null)
            segment = null;

        if (curvySplineSegment != null)
        {

            foreach (var a in curvySplineSegment.sbs)
                a.Refresh();
            curvySplineSegment.Spline2.Refresh();
            UpdateTerrain(curvySplineSegment);
        }

        if (segment != null)
            CopyFrom(segment, segment.Position);

    }
    private Vector3 GetPoint()
    {
        float dist;
        var planePont = new Plane(Vector3.up, cursor.position);
        planePont.Raycast(ray, out dist);
        var p = ray.GetPoint(dist);
        if (shapeEditor)
        {
            p.x -= Mod((p.x + .5f), 1) - .5f;
            p.z -= Mod((p.z + .5f), 1) - .5f;
        }
        return p;
    }




    public void OnPostRender()
    {

        res.lineMaterialYellow.SetPass(0);
        GL.Color(Color.green);

        GL.Begin(GL.LINES);

        if (drawDragRect)
        {
            var m = Input.mousePosition;
            var a = new Vector3(mouseDrag.x / Screen.width, mouseDrag.y / Screen.height, 10);
            var b = new Vector3(m.x / Screen.width, m.y / Screen.height, 10);

            var i = 20;
            GL.Vertex(camera.ViewportToWorldPoint(new Vector3(a.x, a.y, i)));
            GL.Vertex(camera.ViewportToWorldPoint(new Vector3(b.x, a.y, i)));

            GL.Vertex(camera.ViewportToWorldPoint(new Vector3(b.x, a.y, i)));
            GL.Vertex(camera.ViewportToWorldPoint(new Vector3(b.x, b.y, i)));

            GL.Vertex(camera.ViewportToWorldPoint(new Vector3(b.x, b.y, i)));
            GL.Vertex(camera.ViewportToWorldPoint(new Vector3(a.x, b.y, i)));

            GL.Vertex(camera.ViewportToWorldPoint(new Vector3(a.x, b.y, i)));
            GL.Vertex(camera.ViewportToWorldPoint(new Vector3(a.x, a.y, i)));
        }


        GL.Vertex(cursorPos);
        GL.Vertex(cursorPos + Vector3.down * 100);
        if (np != null)
            if (tool == Tool.Insert || tool == Tool.Brush)
                DrawLine2D(cursorPos, np.point);
        if (alt)
        {
            GL.Color(Color.white);
            DrawLine(cursorPos, rotateCamPivot);
        }
        else if (segment != null && draw && !windowHit)
        {
            GL.Color(Color.white);
            DrawLine(cursorPos, segment.Position);
        }
        GL.End();

    }
    private void ReEnable()
    {
        foreach (var a in mbs)
            if (a != null)
                a.SetActive(true);
    }
    List<GameObject> mbs = new List<GameObject>();
    private void Disable(Type type)
    {
        foreach (MonoBehaviour a in FindObjectsOfType(type))
        {
            a.gameObject.SetActive(false);
            mbs.Add(a.gameObject);
        }
    }
    private string mapName { get { return PlayerPrefsGetString("mapName"); } set { PlayerPrefsSetString("mapName", value); } }
    public new Vector3 pos
    {
        get
        {
            return camera.transform.position;
        }
        set
        {
            camera.transform.position = value;
        }
    }


    internal static GameObject[] checkPoints
    {
        get { return GameObject.FindGameObjectsWithTag(Tag.CheckPoint); }
    }
    public MapSets mapSets = new MapSets() { levelFlags = LevelFlags.race };

}

