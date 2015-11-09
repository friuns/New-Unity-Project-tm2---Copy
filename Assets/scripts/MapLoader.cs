using System.Text.RegularExpressions;
#if UNITY_EDITOR
using Curvy.Utils;
using UnityEditor;
//using Exception = System.AccessViolationException;
#endif
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
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using System.Reflection;
public partial class MapLoader : GuiClasses
{
    internal float nitro = 0;
    public Light mylight;
    public static string loadMap;
    internal bool userMapSucces;
    internal bool flatTerrain;
    internal int laps = 1;
    internal int materialId;
    //internal Color color = new Color(.7f, .7f, .7f, 1);
    public TerrainData td { get { return  terrain.terrainData; } }
    internal float[,] oldHeightsFlat;//{get { return _TerrainHelper.oldHeights; } set { _TerrainHelper.oldHeights = value; }}
    public Transform disable;
    public override void Awake()
    {
        _Loader.mapLoader = this;
        CreateTerrain();
        base.Awake();
        if (terrain != null)
        {
            oldHeightsFlat = new float[td.heightmapHeight, td.heightmapWidth];
            for (int i = 0; i < td.heightmapHeight; i++)
                for (int j = 0; j < td.heightmapWidth; j++)
                    oldHeightsFlat[i, j] = .36f;
        }
        disable = new GameObject("disable").transform;
        disable.gameObject.SetActive(false);
    }
    public TreeInstance[] oldTreeInstances;
    public void CreateTerrain()
    {
        if (!lowQualityAndAndroid && terrain == null)
        {
            var loadRes = LoadRes("terrain");
            if (loadRes != null)
            {
                terrain = ((GameObject) Instantiate(loadRes)).GetComponent<Terrain>();
                oldTreeInstances = td.treeInstances;
                //res.terrainBounds = terrain.collider.bounds;
            }
            else
                print("couldn't load terrain");
            //_TerrainHelper.enabled = true;
        }
    }
    public void Start()
    {
        //terrain.heightmapPixelError = _Loader.levelEditor != null ? 1 : 50;
        _GameSettings.gravitationFactor = _GameSettings.gravitationAntiFly = 1.5f;
        
        //SetDirty(res);
    }
    


    public List<Vector2> minimap = new List<Vector2>();
    static internal Bounds levelBounds { get { return _GameSettings.levelBounds; } set { _GameSettings.levelBounds = value; } }

    private static  Vector2 miniMapCenter = new Vector2(.5f, .7f);
    public void UpdateMinimap()
    {
        minimap = new List<Vector2>();
        levelBounds = new Bounds(Vector3.zero, -Vector3.one * 10000);

        foreach (var a in splines)
            if (!a.shape)
            {
                Bounds b = a.GetBounds(false);
                _GameSettings.levelBounds.min = new Vector3(Mathf.Min(levelBounds.min.x, b.min.x), 0, Mathf.Min(levelBounds.min.z, b.min.z));
                _GameSettings.levelBounds.max = new Vector3(Mathf.Max(levelBounds.max.x, b.max.x), 0, Mathf.Max(levelBounds.max.z, b.max.z));
            }

        _GameSettings.levelBounds.size = Vector3.Max(levelBounds.size, Vector3.one * 500);
        foreach (CurvySpline2 a in splines)
            if (!a.shape)
            {
                foreach (var p in a.ControlPoints)
                {
                    var vector2 = ResizeToMinimap(p.Position);
                    minimap.Add(vector2);
                    //a.GetBounds()
                }
                minimap.Add(Vector2.zero);
            }
    }
    private Vector3 ratio;
    public static Vector2 ResizeToMinimap(Vector3 pos)
    {
        var bounds = levelBounds;

        
        var v = (pos - bounds.min);
        //var v = _Player.transform.InverseTransformPoint(pos);

        
        v.x /= bounds.size.x;
        v.z /= bounds.size.z;

        if (bounds.size.z > bounds.size.x)
            v.x *= bounds.size.x/bounds.size.z;
        else
            v.z *= bounds.size.z/bounds.size.x;
        var vector2 = new Vector2(v.x, v.z)*.2f + miniMapCenter;
        return vector2;
    }

    public bool OneMesh;
    internal List<GameObject> nodes = new List<GameObject>();
    internal IEnumerator ActiveEditor(bool editor)
    {
        //if (_Loader.levelEditor != null)
        //    StartCoroutine(_AutoQuality.OnLevelWasLoaded2(0));
        Time.timeScale = 1;
        if (editor)
        {
            foreach (GameObject a in nodes)
                if (a != null)
                    a.SetActive(true);
        }
        else
        {
            nodes.Clear();
            foreach (var a in GameObject.FindGameObjectsWithTag(Tag.node))
            {
                nodes.Add(a);
                a.SetActive(false);
            }
        }
        OneMesh = !editor;
        if (OneMesh)
        {
            ClearSbs();
            yield return null;
        }
        var sbs = FindObjectsOfType(typeof(SplinePathMeshBuilder));
        print("ActiveEditor " + sbs.Length);
        foreach (SplinePathMeshBuilder sb in sbs)
        {
            if (editor)
            {
                sb.ExtrusionParameter = 1;
                foreach (var a in cls)
                    Destroy(a.gameObject);
                cls.Clear();
            }
            else
            {
                if (OneMesh)
                {
                    var t = new GameObject("Coll").transform;
                    MeshCollider c = t.gameObject.AddComponent<MeshCollider>();
                    c.smoothSphereCollisions = true;
                    c.tag = sb.tag;// ((CurvySpline2)sb.Spline).roadType.ToString();
                    c.sharedMesh = (Mesh)Instantiate(sb.Mesh);
                    cls.Add(c);
                }
                else
                {
                    sb.ExtrusionParameter = .1f;
                    sb.StartCap = sb.EndCap = false;
                    var old = sb.mMesh;
                    sb.mMesh = new Mesh();
                    sb.Refresh();
                    MeshCollider c = sb.gameObject.AddComponent<MeshCollider>();
                    cls.Add(c);
                    c.smoothSphereCollisions = true;
                    ((MeshCollider)sb.collider).sharedMesh = sb.mMesh;
                    sb.mMesh = old;
                    sb.StartCap = sb.EndCap = true;
                    sb.ExtrusionParameter = 1f;
                }
            }
        }
        OneMesh = false;
        ClearSbs();
        yield return null;
        if (mylight != null)
            mylight.enabled = editor;
        if (start != null && editor)
            start.gameObject.SetActive(true);
        //if (editor && showTrees) UpdateTerrain(null, true, showTrees); //todo
        yield return null;
        if (terrain != null)
            terrain.heightmapPixelError = editor ? 1 : 50;
    }
    internal bool showTrees;
    private void ClearSbs()
    {
        foreach (CurvySpline2 a in splines)
            foreach (var b in a.ControlPoints)
            {
                foreach (var c in b.sbs)
                    Destroy(c.gameObject);
                b.sbs.Clear();
            }
    }
    List<Collider> cls = new List<Collider>();
    internal bool mapLoading;
    internal ModelItem modelLibCur;
    private ModelLibrary m_modelLib;
    internal ModelLibrary modelLib
    {
        get
        {

            if (m_modelLib == null)
            {
//#if UNITY_EDITOR
//                m_modelLib = resEditor.ModelLibrary;
//                modelLibCur = m_modelLib.RootItem;
//#else
                var g = (GameObject)LoadRes("ModelLibrary");
                if (g != null)
                {
                    m_modelLib = g.GetComponent<ModelLibrary>();
                    modelLibCur = m_modelLib.RootItem;
                }
//#endif
            }
            return m_modelLib;
        }
    }
    public static string unityMap;

    public IEnumerator LoadUnityMap(string map)
    {
        Debug.Log("Load unity map "+map);
        unityMap = map;
        Action act = win.act;
        win.ShowWindow(delegate
        {
            Label(_Loader.LoadingLabelMap());
            if (!_Loader.isLoading)
                win.ShowWindow(act, null, true);
        },null,true);
        if (!CanStreamedLevelBeLoaded(map))
            yield return StartCoroutine(LoadingScreen.LoadMap(map));
        LoadLevelAdditive(map);
    }
    public void SetFlatTerrain(bool toggle)
    {
        flatTerrain = toggle;
    }
    internal IEnumerator LoadMap(string s, Action onError = null, Action onLoaded = null)
    {
        mapLoading = true;
        loadMap = null;
        swirl = 0;
        scale = 1;
        print(mainSite + s);
        WWW www = new WWW(mainSite + s);
        yield return www;
        if (!string.IsNullOrEmpty(www.error)) { if (onError != null)onError(); yield break; }
        BinaryReader ms = new BinaryReader(www.bytes);
        Debug.LogWarning("Loading Map " + ms.Length + " " + s);
        int version = 0;
        Dictionary<int, CurvySpline2> saveIds = new Dictionary<int, CurvySpline2>();
        //bool ingame = _Game != null;
        //HashSet<KeyValuePair<Vector3, string>> cells = new HashSet<KeyValuePair<Vector3, string>>();
        ModelObject modelObject=null;
        var enumType = typeof (LevelPackets);
        //var levelPackets = Enum.GetValues(enumType);
        LevelPackets P, oldP = LevelPackets.Unknown;
        while (ms.Position < ms.Length)
        {
            try
            {
                P = (LevelPackets)ms.ReadInt();
                if (!Enum.IsDefined(enumType, P))
                {
                    ms.Position++;
                    if (isDebug)
                        Debug.LogError("wrong Levelpacked " + oldP);
                    else
                        Loader.errors++;
                    P = (LevelPackets)ms.ReadInt();
                    if (!Enum.IsDefined(enumType, P))
                    {
                        ms.Position--;
                        continue;
                    }

                }
            } catch (Exception e)
            {
                Debug.LogError(e);
                continue;
            }
            oldP = P;

            if (P == LevelPackets.unityMap)
            {
                var map = ms.ReadString();
                yield return StartCoroutine(LoadUnityMap(map));
            }
            
            if (P == LevelPackets.Spline)
                yield return StartCoroutine(CreateSpline());
            
            
            if (P == LevelPackets.shape)
            {
                yield return StartCoroutine(CreateSpline(null, true));
                spline.CreatePivot(ms.ReadVector());
                spline.saveId = ms.ReadInt();
                saveIds[spline.saveId] = spline;
                spline.tunnel = ms.ReadBool();
                spline.materialId = ms.ReadInt();
                spline.color = ms.readColor();
                spline.name = ms.ReadString();
            }
            try
            {
                if (P == LevelPackets.flatTerrain)
                    SetFlatTerrain(true);
                if (P == LevelPackets.ClosedSpline)
                    spline.Closed = true;

                if (P == LevelPackets.CheckPoint2)
                {
                    var t = SetCheckPoint(ms.ReadVector());
                    t.eulerAngles = ms.ReadVector();
                }
                if (P == LevelPackets.disableTerrain)
                    hideTerrain =  true;
                if (P == LevelPackets.Block)
                {
                    var readString = ms.ReadString();
                    GameObject go;
                    if (!modelLib)
                    {
                        Debug.LogWarning("Model lib not loaded");
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        
                    }
                    else if (modelLib.dict.ContainsKey(readString) && modelLib.dict[readString].gameObj != null)
                    {
                        ModelFile modelFile = modelLib.dict[readString];
                        var gameObj = modelFile.gameObj;
                        modelFile.usedCount++;
                        go = (GameObject)Instantiate(gameObj);
                        go.name = gameObj.name;
                    }
                    else
                    {
                        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        Debug.LogWarning(readString + " not found " + dict.Count);
                    }
                    go.name = readString;
                    modelObject = InitModel(go, readString);                    
                    modelObject.transform.position = ms.ReadVector();
                    modelObject.transform.eulerAngles = ms.ReadVector();
                    modelObject.transform.localScale = ms.ReadVector();

                    //if (!cells.Add(new KeyValuePair<Vector3, string>(g.pos, go.name)))
                    //{
                    //    Debug.Log("Destroy duplicate " + go.name, go);
                    //    Debug.Log("with " + go.name, go);
                    //    if (ingame)
                    //    Destroy(go);
                    //}
                }
                if (P == LevelPackets.FlyingModel && modelObject != null)
                    modelObject.flying = true;
                if (P == LevelPackets.roadtype)
                    spline.roadType = (RoadType)ms.ReadByte2();
                if (P == LevelPackets.Wall)
                    spline.wallTexture = true;
                if (P == LevelPackets.shapeMaterial)
                {
                    string readString = ms.ReadString();
                    spline.thumb = new Thumbnail() { url = Regex.Replace(readString, @"https?://server.critical-missions.com/tm/|https?://tmrace.net/tm/","") };
                    //Debug.LogWarning(readString);
                }
                if( P == LevelPackets.textureTile)
                    spline.thumb.material.mainTextureScale = ms.ReadVector();
                if (P == LevelPackets.AntiFly)
                {
                    _GameSettings.gravitationAntiFly = ms.ReadFloat();
                    _GameSettings.gravitationFactor = ms.ReadFloat();
                }
                if (P == LevelPackets.Flying)
                    segment.flying = ms.ReadBool();

                if (P == LevelPackets.heightOffset)
                    spline.heightOffset = ms.ReadFloat();
                if (P == LevelPackets.Version)
                {
                    version = ms.ReadInt();
                    if (version >= 702)
                    {
                        foreach (var a in shapes.ToArray())
                        {
                            print("removing default brush " + a.name);
                            Destroy(a.gameObject);
                        }
                    }
                }
                if (P == LevelPackets.Nitro)
                {
                    Debug.Log("Nitro Loaded " + nitro);
                    nitro = ms.ReadFloat();
                }
                if (P == LevelPackets.Point)
                {
                    var readVector = ms.ReadVector();
                    AddPoint(readVector);
                    if (version >= 702)
                        segment.spls = new List<CurvySpline2>();
                    else
                        segment.spls = new List<CurvySpline2>(new[] { brushShapes[0] });
                    segment.swirl = ms.ReadFloat();
                }
                if (P == LevelPackets.brush)
                    segment.spls.Add(saveIds[ms.ReadInt()]);
                if (P == LevelPackets.Material)
                {
                    spline.materialId = ms.ReadInt();
                    print("Set Material " + spline.materialId);
                    spline.color = ms.readColor();
                }
                if (P == LevelPackets.Finnish)
                    finnish = true;
                if (P == LevelPackets.CheckPoint)
                    SetCheckPoint(segment);
                if (P == LevelPackets.Start)
                {
                    SetStartPoint(segment);
                    start.transform.parent = segment.transform;
                }
                if (P == LevelPackets.StartPos)
                    SetStartPoint(ms.ReadVector(), ms.ReadVector());
                if (P == LevelPackets.Laps)
                {
                    laps = ms.ReadInt();
                    _GameSettings.laps = laps;
                }
                if (P == LevelPackets.scale)
                    segment.scale = ms.ReadFloat();
                if (P == LevelPackets.levelTime)
                    _GameSettings.levelTime = ms.ReadFloat();
                if (P == LevelPackets.rotateTexture)
                    spline.rotateTexture = true;
                if (P == LevelPackets.disableJump)
                    _Loader.curSceneDef.disableJump = true;

            } catch (Exception e) { Debug.LogError(e); }
        }
        print("Map Version " + version);
        if (onLoaded != null)
            onLoaded();
        userMapSucces = true;
        UpdateTerrain(null, true, _Loader.levelEditor==null);
        if (!_Loader.levelEditor)
            Optimize2();

        mapLoading = false;
        if (isDebug && modelLib != null)
        {
            bool parsed = setting.parsedLevels.Contains(_Loader.mapName);
            if (!parsed)
                setting.parsedLevels.Add(_Loader.mapName);
            foreach (var a in modelLib.models)
            {
                if (!parsed)
                    setting.Popularity[a.path] = setting.Popularity.TryGet(a.path, 0) + Mathf.Sqrt(Mathf.Sqrt(a.usedCount));
                //if (a.usedCountSqrt != 0)
                //    print(a.name + ":" + a.usedCountSqrt);
                a.usedCount = 0;
            }
            setting.populartyKeys = new List<string>(setting.Popularity.Keys);
            setting.populartyValues = new List<float>(setting.Popularity.Values);
            SetDirty(res);
        }

    }

    protected void Combine() { }
    
    public void Optimize2()
    {
        var findObjectsOfType = FindObjectsOfType(typeof(ModelObject));
        var t = new GameObject("combine").transform;
        foreach (ModelObject a in findObjectsOfType.Where(a => a.name != "coin"))
        {
            a.gameObject.isStatic = true;
            a.transform.parent = t;
        }
        
        StaticBatchingUtility.Combine(t.gameObject);

    }
    //protected void Optimize()
    //{
    //    var findObjectsOfType = FindObjectsOfType(typeof(ModelObject));
    //    if (findObjectsOfType.Length == 0) return;

    //    var dict = new Dictionary<KeyValuePair<int, int>, List<ModelObject>>();
    //    foreach (ModelObject a in findObjectsOfType.Where(a => a.name != "coin"))
    //    {
    //        List<ModelObject> list ;
    //        var kv = new KeyValuePair<int, int>((int)(a.pos.x / 100), (int)(a.pos.z / 100));
    //        if (!dict.TryGetValue(kv, out list))
    //            list = dict[kv] = new List<ModelObject>();
    //        list.Add(a);
    //    }

    //    foreach (var l in dict)
    //    {
    //        Transform t = null;
    //        bool set = false;
    //        foreach (ModelObject a in l.Value)
    //        {
    //            if (!set)
    //            {
    //                set = true;
    //                t = new GameObject("combine").transform;
    //                t.gameObject.isStatic = true;
    //                t.position = a.pos;
    //            }
    //            a.gameObject.isStatic = true;
    //            a.transform.parent = t;
    //        }
    //        if (set)
    //            StaticBatchingUtility.Combine(t.gameObject);
    //    }
    //}
    public static ModelObject InitModel(GameObject go,string s)
    {
        var sgo = go.AddComponent<ModelObject>();
        go.name = s;
        //print(s);
        //if (s.StartsWith("Curb-") || s.StartsWith("Sidewalk1") || s.StartsWith("Sidewalk2") || s.StartsWith("Sidewalk3"))
            //c.convex = true;
        if (_Loader.levelEditor)
            go.AddComponent<MeshOutline>();

        if (_Loader.levelEditor && sgo.renderer.gameObject.GetComponent<Collider>() == null)
        {
            var c = sgo.renderer.gameObject.AddComponent<BoxCollider>();
            c.isTrigger = true;
        }
        sgo.renderer.gameObject.layer = Layer.block;
        sgo.oldMaterials = sgo.renderer.sharedMaterials;
        

        if (s.Contains("Turbo"))
            foreach (var a in go.GetComponentsInChildren<Transform>())
                a.tag = Tag.Speed;
        sgo.name2 = s;
        //if (g.renderer != null)
            //g.oldMaterials = g.renderer.sharedMaterials;
        return sgo;
    }

    public Transform SetCheckPoint(Vector3 pos)
    {
        var ch = (Transform)Instantiate(res.CheckPoint,pos,Quaternion.identity);
        //ch.name = res.CheckPoint.name;
        //ch.position = segment.Position;
        //ch.forward = fv;
        //ch.parent = segment.transform;
        //if (finnish)
        //    ch.name = "Finnish";
        //finnish = false;
        return ch;
    }

    public void SetCheckPoint(CurvySplineSegment segment)
    {
        var ch = (Transform)Instantiate(res.CheckPoint);
        ch.name = res.CheckPoint.name;
        ch.position = segment.Position;
        ch.rotation = segment.transform.rotation;
        ch.parent = segment.transform;
        if (finnish)
            ch.name = "Finnish";
        finnish = false;
    }
    internal bool finnish;
    private int shapeCnt = 1;
    public IEnumerator CreateSpline(Action onCreated = null, bool shape = false)
    {
        spline = new GameObject(shape ? "Shape" + shapeCnt : "Spline").AddComponent<CurvySpline2>();
        if (shape)
            shapeCnt++;
        spline.Closed = false;
        spline.AutoRefresh = true;
        spline.Orientation = CurvyOrientation.ControlPoints;
        spline.Granularity = 1;
        spline.Interpolation = shape ? CurvyInterpolation.Linear : CurvyInterpolation.CatmulRom;
        spline.ShowGizmos = false;
        spline.shape = shape;
        while (!spline.IsInitialized)
            yield return null;
        if (onCreated != null)
            onCreated();
        //print("Spline Created");
        yield return null;
    }
    internal WWW www;
    internal void SetStartPoint(CurvySplineSegment s)
    {
        if (start == null)
        {
            start = (Transform)Instantiate(res.startPrefab);
            start.name = res.startPrefab.name;
        }
        start.position = s.Position + Vector3.up * 3 + s.transform.forward * 6;
        start.rotation = s.transform.rotation;
        start.parent = s.transform;
    }
    internal void SetStartPoint(Vector3 pos, Vector3 fv)
    {
        if (start == null)
        {
            start = (Transform)Instantiate(res.startPrefab);
            start.name = res.startPrefab.name;
        }
        start.position = pos;
        start.forward = fv;
    }
    internal Transform start;
    internal float swirl = 0;
    internal float scale = 1;
    internal void UpdateSwirl2()
    {
        if (segment == null) return;
        if (segment.Spline.ControlPoints.Count > 1)
            foreach (var a in segment.Spline.ControlPoints)
            {
                Vector3 forward;
                if (a.PreviousControlPoint == null)
                    forward = -a.Position + a.NextControlPoint.Position;
                else if (a.NextControlPoint == null)
                    forward = -a.PreviousControlPoint.Position + a.Position;
                else
                    forward = a.GetTangent(0);
                if (forward == Vector3.zero)
                {
                    a.Position += Vector3.right;
                    print("move right");
                }
                a.transform.forward = forward;
                var localEulerAngles = a.transform.localEulerAngles;
                localEulerAngles.z = a.swirl;
                a.transform.localEulerAngles = localEulerAngles;
                Debug.DrawRay(a.transform.position, a.transform.forward * 5, Color.red);
            }
    }
    internal Transform drag;
    public bool shapeEditor;
    internal bool affectNext;
    internal bool affectPrev;
    internal void AddPoint(Vector3 point, CurvySplineSegment after = null, bool before = false, bool insert = false)
    {
        affectNext = affectPrev = false;
        if (shapeEditor && segment != null && (point - segment.Spline.ControlPoints[0].Position).magnitude < 1)
            return;
        if (after != null)
            spline = (CurvySpline2)after.Spline;
        var old = segment;

        if (terrain != null && !res.terrainBounds.Contains(point) && !spline.shape)
        {
            if (!hideTerrain && _Loader.levelEditor != null)
                Popup("Warning you are out of terrain bounds, terrain will be disabled");
            Debug.LogWarning("Hide Terrain");
            hideTerrain = true;
        }
        
        if (before)
            segment = spline.Add(false, null);
        else
            segment = spline.Add(after, false);

        if (after != null && start == null && _Loader.levelEditor != null && !shapeEditor)
            SetStartPoint(after);
        
        segment.Position = point;
        if (insert)
            segment.spls = new List<CurvySpline2>(after.spls);
        else if (after != null && !before)
        {
            segment.spls = after.spls = brushShapes;
            //segment.spls = brushShapes;
        }
        else
            segment.spls = brushShapes;

        brushShapes = new List<CurvySpline2>(brushShapes);
        var t = (Transform)Instantiate(res.dot, point, Quaternion.identity);
        t.parent = segment.transform;
        t.localScale = spline.shape ? Vector3.one * 2 : Vector3.one * 6;
        t.name = "model";
        if (old != null)
            segment.transform.forward = segment.Position - old.Position;
        drag = segment.transform;
        segment.scale = scale;
        segment.swirl = swirl;
        spline.Refresh();
        UpdateSwirl2();
        
    }
    //public List<CurvySpline2> selectedShapes = new List<CurvySpline2>();
    public List<CurvySpline2> shapes = new List<CurvySpline2>();
    public List<CurvySpline2> brushShapes = new List<CurvySpline2>();
    internal CurvySplineSegment segment;

    internal CurvySpline2 spline;
    private float[,] heights;
    private float[, ,] alphas;

    

    public List<CurvySpline2> splines = new List<CurvySpline2>();
    public float miny = MaxValue;
    public struct Vector5
    {
        public Vector3 v;
        public float dist;
        public float width;
        public Vector5(float x, float y, float z, float dist, float width)
        {
            v = new Vector3(x, y, z);
            this.dist = dist;
            this.width = width;
        }
    }

    private bool[,] used;
    private bool warkingShown;
    
    
    //private Terrain m_terrain;
    
    //{
    //    get
    //    {
    //        if (!m_terrain)
    //        {
    //            print("<<<<<<<<<Terrain Instantiate>>>>>>>>>");
    //            m_terrain = ((GameObject)Instantiate(LoadRes("terrain"))).GetComponent<Terrain>();
    //            if (_TerrainHelper != null)
    //                _TerrainHelper.enabled = true;
    //        }
    //        return m_terrain;
    //    }
    //}

    //internal bool terrainInited;
}