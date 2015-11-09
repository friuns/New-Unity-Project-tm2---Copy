using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CurvySpline2 : CurvySpline
{
    //private Color m_color;
    public RoadType roadType;
    internal Color color = new Color(.7f, .7f, .7f, 1);
    //{ get { return m_color; } set { m_color = value; if (renderer != null) { renderer.material.color = value; } } }
    //private int m_materialId;
    public int materialId;
    internal Thumbnail thumb;

    //{ get { return m_materialId; } set { m_materialId = value; if (renderer != null) { renderer.material = bs.res.levelTextures[value]; } } }
    public bool shape;
    private LevelEditor levelEditor;
    public Mesh mesh;
    internal bool tunnel;
    internal bool hide;
    public CurvySplineSegment pivot;
    private new GUIText guiText;
    public int saveId;
    private int random;
    public override void Awake()
    {
        //print("Spline Awake");
        _MapLoader.splines.Add(this);
        base.Awake();
    }
    public IEnumerator Start()
    {
        random = UnityEngine.Random.Range(0, 100);
        levelEditor = _Loader.levelEditor; //(LevelEditor)FindObjectOfType(typeof(LevelEditor)); 
        if (!_MapLoader.shapes.Contains(this) && shape)
            _MapLoader.shapes.Add(this);
        while (!IsInitialized || ControlPointCount == 0)
            yield return null;
        if(shape)
            GenerateMesh();
        
    }
    public int SetGranularity
    {
        set
        {
            Granularity = value;
            Interpolation = value == 1 ? CurvyInterpolation.Linear : CurvyInterpolation.CatmulRom;
        }
    }
    public override void Refresh(bool refreshLength, bool refreshOrientation, bool skipIfInitialized)
    {
        base.Refresh(refreshLength, refreshOrientation, skipIfInitialized);
        SplineUpdateTf();
    }

    public static float DistancePointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        return Vector3.Magnitude(ProjectPointLine(point, lineStart, lineEnd) - point);
    }
    public static Vector3 ProjectPointLineVector2(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector2 = lineEnd - lineStart;
        float magnitude = vector2.magnitude;
        Vector3 lhs = vector2;
        if (magnitude > 1E-06f)
        {
            lhs = (Vector3)(lhs / magnitude);
        }
        float num2 = Mathf.Clamp(Vector2.Dot(lhs, rhs), 0f, magnitude);
        return (lineStart + ((Vector3)(lhs * num2)));
    }

    public static Vector3 ProjectPointLine(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
    {
        Vector3 rhs = point - lineStart;
        Vector3 vector2 = lineEnd - lineStart;
        float magnitude = vector2.magnitude;        
        Vector3 lhs = vector2;
        if (magnitude > 1E-06f)
        {
            lhs = (Vector3)(lhs / magnitude);
        }
        float num2 = Mathf.Clamp(Vector3.Dot(lhs, rhs), 0f, magnitude);
        return (lineStart + ((Vector3)(lhs * num2)));        
    }

    public new void Update()
    {

        if (KeyDebug(KeyCode.E))
            if (shape)
                print(name + GetBounds(true) + GetBounds(true).max);
        var game = _Game != null && _Game.gameObject.activeSelf;

        if (game && FramesElapsed(10, random))
        {
            foreach (var a in Segments)
            {
                float d = DistancePointLine(mainCameraTransform.position, a.Position, a.NextControlPoint.Position);
                if (splitScreen)
                    d = Mathf.Min(DistancePointLine(_Player2.pos, a.Position, a.NextControlPoint.Position), d);
                float ext;
                if (d < 100)
                    ext = .1f;
                else 
                    ext = 3;
                foreach (SplinePathMeshBuilder b in a.sbs)
                {
                    if (b.ExtrusionParameter != ext)//&& (b.refreshed != Time.frameCount || b.ExtrusionParameter > ext)
                    {
                        b.ExtrusionParameter = ext;
                        //print("Set Ext "+ext);                        
                        b.Refresh();
                    }
                    
                    //b.refreshed = Time.frameCount;
                }
            }
            //_Player.cam .position-pos
        }
        if (game) return;
        if (levelEditor == null)
        {
            Closed = shape || Closed;
        }
        else
        {
            if (levelEditor.shapeEditor && shape)
            {
                var closed = shape && (levelEditor.spline != this);
                if (Closed != closed)
                {
                    Closed = closed;
                    if (closed)
                        SetPivot();
                    Refresh();
                }
            }
            if (shape && guiText == null)
                guiText = new GameObject(name + " Text").AddComponent<GUIText>();
            if (guiText != null)
            {
                guiText.enabled = Closed && levelEditor.shapeEditor;
                guiText.text = name;
                guiText.transform.position = levelEditor.shapeCamera.WorldToViewportPoint(GetCenter() + Vector3.forward * 3);
            }
        }

        if (!shape)
        {
            var updated = GenerateSbs();
            if (updated) Refresh();
        }

        Bounds screen = new Bounds();
        screen.SetMinMax(Vector3.zero, new Vector3(Screen.width, Screen.height, 10000));
        //if (!shape)
        if (_Loader.levelEditor != null)
            foreach (var a in Segments)
            //if (a.NextControlPoint != null)
            {
                var mpos = Input.mousePosition;
                var camera = _Loader.levelEditor.camera;

                var s1 = camera.WorldToScreenPoint(a.Position);
                var s2 = camera.WorldToScreenPoint(a.NextControlPoint.Position);
                a.z = a.dist = MaxValue;
                if (screen.Contains(s1) || screen.Contains(s2))
                {
                    Vector3 project = CurvySpline2.ProjectPointLineVector2(mpos, s1, s2);
                    if (screen.Contains(project))
                    {
                        a.z = project.z;
                        var p1 = camera.ScreenToWorldPoint(mpos + Vector3.forward * project.z);
                        var p2 = a.pivot = camera.ScreenToWorldPoint(project);
                        //Debug.DrawLine(p1, p2, Color.blue);
                        a.dist = (p1 - p2).magnitude;
                    }
                }

            }
        //else
        //    a.dist = float.MaxValue;

        base.Update();
    }
    public bool GenerateSbs()
    {
        bool updated = false;
        foreach (var segment in ControlPoints)
        {
            if (segment.IsValidSegment)
            {
                //IEnumerable<CurvySpline2> nw = segment.spl.Where(spl => !segment.sb.Any(b => b.StartMesh == spl.mesh));
                foreach (CurvySpline2 spl in segment.spls)
                    if (!segment.sbs.Any(b => b.StartMesh == spl.mesh))
                    {
                        if (!levelEditor)
                        {
                            bool brk = false;
                            foreach (
                                var a in
                                    new[] { "Road", "LargeRoad", "Speed Track" }.SkipWhile(a => a != spl.name).Skip(1))
                            {
                                if (segment.spls.Any(b => b.name == a))
                                    brk = true;
                            }
                            if (brk) continue;
                        }
                        //print("Spline Added");
                        SplinePathMeshBuilder sb;
                        if (_MapLoader.OneMesh && segment.PreviousSegment != null && (sb = segment.PreviousSegment.sbs.FirstOrDefault(a => a.StartMesh == spl.mesh)) != null)
                            segment.sbs.Add(sb);
                        else
                        {
                            sb = new GameObject(segment.name + spl.name).AddComponent<SplinePathMeshBuilder>();
                            segment.sbs.Add(sb);
                            sb.transform.parent = this.transform;
                            sb.transform.localRotation = Quaternion.identity;
                            sb.Spline = this;
                            sb.ScaleModifier = SplinePathMeshBuilder.MeshScaleModifier.ControlPoint;
                            sb.CapShape = spl.mesh == null ? SplinePathMeshBuilder.MeshCapShape.Rectangle : SplinePathMeshBuilder.MeshCapShape.Custom;
                            sb.StartMesh = spl.mesh;
                            //if (spl != null && spl.tunnel)
                            sb.EndCap = sb.StartCap = _MapLoader.OneMesh;
                            sb.CapHeight = 1;
                            sb.CapWidth = 10;
                            sb.UV = SplinePathMeshBuilder.MeshUV.Absolute;
                            sb.curvySpline2 = spl;
                            sb.UVParameter = 10f;
                            sb.gameObject.layer = Layer.level;

                            if (spl.thumb != null)
                                sb.renderer.sharedMaterial = spl.thumb.material;
                            else if (spl.materialId < res.levelTextures.Length)
                            {
                                sb.renderer.material = res.levelTextures[Mathf.Max(0, spl.materialId)];
                                sb.renderer.sharedMaterial.color = spl.color;
                            }
                            sb.gameObject.tag = spl.roadType.ToString();
                            sb.ExtrusionParameter = _MapLoader.OneMesh ? .1f : 1;
                        }
                        //if (_Loader.levelEditor != null)
                            //_Loader.levelEditor.HideGroupAdd(sb.gameObject);
                        updated = true;
                    }
            }

            IEnumerable<SplinePathMeshBuilder> rm = segment.IsValidSegment ? segment.sbs.Where(a => !segment.spls.Any(b => b.mesh == a.StartMesh)) : segment.sbs;
            foreach (SplinePathMeshBuilder a in rm.ToArray())
            {
                print("Spline Removed");
                segment.sbs.Remove(a);
                if (!_MapLoader.OneMesh || !(segment.NextSegment != null && segment.NextSegment.sbs.Contains(a) || segment.PreviousControlPoint != null && segment.PreviousControlPoint.sbs.Contains(a)))
                    Destroy(a.gameObject);
                updated = true;
            }

        }
        return updated;
    }


    public void SplineUpdateTf()
    {
        var ds = new List<SplinePathMeshBuilder>();
        foreach (CurvySplineSegment a in Segments)
        {
            foreach (SplinePathMeshBuilder b in a.sbs)
            {
                if (b.UseWorldPosition)
                    b.transform.position = a.Position;
                else
                    b.transform.position = a.Spline.pos;
                b.EndCap = b.StartCap = true;

                if (_MapLoader.OneMesh)
                {
                    if (!ds.Contains(b))
                    {
                        b.FromTF = a.LocalFToTF(0);
                        ds.Add(b);
                    }
                    b.ToTF = a.LocalFToTF(1);
                }
                else
                {
                    b.FromTF = a.LocalFToTF(0);
                    b.ToTF = a.LocalFToTF(1);
                }
            }
            if (_MapLoader.OneMesh)
                for (int i = ds.Count - 1; i >= 0; i--)
                {
                    if (!a.sbs.Contains(ds[i]))
                        ds.Remove(ds[i]);
                }

        }
    }

    public void GenerateMesh()
    {
        print("GenerateMeshes " + ControlPointCount);
        if (shape && ControlPointCount > 1)
        {
            SetPivot();
            Closed = true;
            Refresh();
            int IgnoreAxis;
            CurvyUtility.isPlanar(this, out IgnoreAxis);
            Debug.Log("IgnoreAxis: " + IgnoreAxis);
            mesh = MeshHelper.CreateSplineMesh(this, tunnel ? 1 : 2, true);
        }
    }
    public void SetPivot()
    {
        if (!shape) return;


        Vector3 av = Vector3.zero;

        foreach (var a in ControlPoints)
            a.transform.parent = null;

        if (pivot != null)
            av = pivot.Position;
        else if (ControlPoints.Count > 0)
        {
            av = GetCenter();
        }
        else
            av = pos;

        transform.position = av;

        transform.eulerAngles = new Vector3(-90, 0, 180);
        foreach (var a in ControlPoints)
            a.transform.parent = transform;

        if (pivot == null)
            CreatePivot(transform.position);
    }
    private Vector3 GetCenter()
    {
        var av = Vector3.zero;
        foreach (var a in ControlPoints)
            av += a.Position;
        av /= ControlPointCount;

        if (!tunnel)
            foreach (var a in ControlPoints)
                av.z = Math.Max(a.Position.z, av.z);
        return av;
    }
    public void CreatePivot(Vector3 pos)
    {
        if (pivot == null)
        {
            var g = new GameObject().transform;

            var sphere = (Transform) Instantiate(res.dot);
            sphere.parent = g;
            sphere.localScale = Vector3.one * 2;
            sphere.renderer.material.color = Color.yellow;
            sphere.gameObject.layer = Layer.node;
            sphere.name = "model";

            g.name = name + " pivot";
            pivot = g.gameObject.AddComponent<CurvySplineSegment>();
            pivot.mSpline = this;
        }
        //else
        pivot.transform.position = pos;
    }

    public void OnDestroy()
    {
        if (Loader.loadingLevelQuit)
            return;
        if (_MapLoader != null)
            _MapLoader.splines.Remove(this);
        if(pivot!=null)
            Destroy(pivot.gameObject);
        if (guiText != null)
            Destroy(guiText.gameObject);

        foreach (CurvySplineSegment a in FindObjectsOfType(typeof(CurvySplineSegment)))
            a.spls.Remove(this);

        //if (_MapLoader != null)
        {
            //print(_MapLoader==null);
            //print(_Loader.levelEditor == null);
            _MapLoader.shapes.Remove(this);
            _MapLoader.brushShapes.Remove(this);
            //_MapLoader.selectedShapes.Remove(this);
        }
    }
    public void OnApplicationQuit()
    {
        Loader.loadingLevelQuit = true;
    }
    private void DrawLine(Vector3 a, Vector3 b)
    {
        GL.Vertex3(a.x, a.y, a.z);
        GL.Vertex3(b.x, b.y, b.z);
    }
    public void OnRenderObject()
    {
        //return;
        if(_Loader.levelEditor==null)
            return;
        if (Camera.current != _Loader.levelEditor.curCamera) return;
        
        var lm = bs.res.lineMaterialYellow;
        lm.SetPass(0);

        GL.Begin(GL.LINES);

        foreach (var a in ControlPoints)
        {
            Color color1 = color;
            if (hide)
                color1.a = .7f;
            GL.Color(_Loader.levelEditor.np != null && _Loader.levelEditor.np == a ? Color.red :  color1);
            if (a.Approximation.Length == 0) continue;
            Vector3 p3 = a.Approximation[0];
            for (int i = 1; i < a.Approximation.Length; i++)
            {
                Vector3 p1 = a.Approximation[i];
                DrawLine(p3, p1);                
                p3 = p1;
            }
        }

        GL.End();
    }

    public float heightOffset;
    public bool wallTexture;
}

public partial class SplinePathMeshBuilder
{
    public CurvySpline2 curvySpline2;
}