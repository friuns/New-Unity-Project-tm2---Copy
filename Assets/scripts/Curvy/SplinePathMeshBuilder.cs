// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================

using System;
using UnityEngine;
using System.Collections;
using Curvy.Utils;
using System.Collections.Generic;


/// <summary>
/// Class to dynamically generate spline meshes
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public partial class SplinePathMeshBuilder : MonoBehaviour
{
    //internal int refreshed;
    #region ### Enums & Delegates ###
    /// <summary>
    /// Determines Cap/Shape used to extrude the mesh
    /// </summary>
    public enum MeshCapShape
    {
        /// <summary>
        /// Single sided Line shape
        /// </summary>
        Line,
        /// <summary>
        /// Circle polygon with n segments
        /// </summary>
        NGon,
        /// <summary>
        /// Rectangle polygon
        /// </summary>
        Rectangle,
        /// <summary>
        /// Use this to provide your own mesh(es)
        /// </summary>
        Custom
    }

    /// <summary>
    /// Determines extrusion mode
    /// </summary>
    public enum MeshExtrusion
    {
        /// <summary>
        /// Create mesh segments by a fixed TF value
        /// </summary>
        /// <remarks>Use SplinePathMeshBuilder.ExtrusionParameter to set step width</remarks>
        FixedF = 0,
        /// <summary>
        /// Create mesh segments by a fixed distance (in world units)
        /// </summary>
        /// <remarks>Use SplinePathMeshBuilder.ExtrusionParameter to set step width</remarks>
        FixedDistance = 1,
        /// <summary>
        /// Create mesh segments when the curvation reached an angle-treshold (in degrees)
        /// </summary>
        /// <remarks>Use SplinePathMeshBuilder.ExtrusionParameter to set angle</remarks>
        Adaptive = 2
    }

    /// <summary>
    /// Determines UV mapping mode
    /// </summary>
    public enum MeshUV
    {
        /// <summary>
        /// Stretch V over the total mesh
        /// </summary>
        /// <remarks>Use CurvyMeshBuilder.UVParameter for tiling</remarks>
        StretchV = 0,
        /// <summary>
        /// Stretch V over each segment
        /// </summary>
        /// <remarks>Use CurvyMeshBuilder.UVParameter for tiling</remarks>
        StretchVSegment = 1,
        /// <summary>
        /// Adjust V to keep a certain distance aspect
        /// </summary>
        /// <remarks>Use CurvyMeshBuilder.UVParameter to set distance in world units</remarks>
        Absolute = 2
    }

    /// <summary>
    /// Determines how to alter the scale when extruding the mesh
    /// </summary>
    public enum MeshScaleModifier
    {
        /// <summary>
        /// Don't modify scale
        /// </summary>
        None,
        /// <summary>
        /// Use Control Point's scale
        /// </summary>
        ControlPoint,
        /// <summary>
        /// Use a UserValue to determine scale
        /// </summary>
        UserValue,
        /// <summary>
        /// Call a user method to determine scale
        /// </summary>
        Delegate,
        /// <summary>
        /// Use an animation curve to determine scale
        /// </summary>
        AnimationCurve
    }

    public delegate Vector3 OnGetScaleEvent(SplinePathMeshBuilder sender, float tf);

    #endregion

    #region ### Public Fields & Properties ###
    
    /// <summary>
    /// The Spline to use
    /// </summary>
    public CurvySpline Spline;
    /// <summary>
    /// Start TF
    /// </summary>
    public float FromTF = 0;
    /// <summary>
    /// End TF
    /// </summary>
    public float ToTF = 1;
    /// <summary>
    /// Use a fast linear approximation?
    /// </summary>
    public bool FastInterpolation = false;
    /// <summary>
    /// Base2 coordinates off spline's position?
    /// </summary>
    public bool UseWorldPosition = false;
    /// <summary>
    /// Extrusion mode
    /// </summary>
    public MeshExtrusion Extrusion = MeshExtrusion.Adaptive;
    /// <summary>
    /// Parameter usage depends on Extrusion mode.
    /// </summary>
    public float ExtrusionParameter = 1;
    /// <summary>
    /// Extrusion Shape generation mode
    /// </summary>
    public MeshCapShape CapShape = MeshCapShape.NGon;
    /// <summary>
    /// Width/Radius of Shape
    /// </summary>
    public float CapWidth=1;
    /// <summary>
    /// Heigh of Shape (only Rectangle shape)
    /// </summary>
    public float CapHeight=0.5f;
    /// <summary>
    /// Hollowness of shape
    /// </summary>
    public float CapHollow = 0;
    /// <summary>
    /// Shape Segments (only NGon shape)
    /// </summary>
    public int CapSegments=9;
    /// <summary>
    /// Should the mesh has a start cap?
    /// </summary>
    public bool StartCap = true;
    /// <summary>
    /// Start Cap / Extrusion shape
    /// </summary>
    public Mesh StartMesh;
    /// <summary>
    /// Shouöd the mesh has an end cap?
    /// </summary>
    public bool EndCap = true;
    /// <summary>
    /// End Cap Mesh
    /// </summary>
    public Mesh EndMesh;
    /// <summary>
    /// UV mapping mode
    /// </summary>
    public MeshUV UV = MeshUV.StretchV;
    /// <summary>
    /// UV tiling or step width (for mode 'Absolute')
    /// </summary>
    public float UVParameter = 1;
    /// <summary>
    /// How to modify scale
    /// </summary>
    public MeshScaleModifier ScaleModifier=MeshScaleModifier.None;
    /// <summary>
    /// Slot to use when ScaleModifier==MeshScaleModifier.UserValue
    /// </summary>
    public int ScaleModifierUserValueSlot;
    /// <summary>
    /// Animation curve to use when ScaleModifier==MeshScaleModifier.AnimationCurve
    /// </summary>
    public AnimationCurve ScaleModifierCurve;
    // General
    /// <summary>
    /// Whether the mesh should automatically adapt to spline changes
    /// </summary>
    public bool AutoRefresh = true;
    /// <summary>
    /// The refreshing speed
    /// </summary>
    public float AutoRefreshSpeed = 0;
    /// <summary>
    /// Number of mesh vertices
    /// </summary>
    public int VertexCount { get { return (mMesh) ? mMesh.vertexCount : 0; } }
    /// <summary>
    /// Number of mesh triangles
    /// </summary>
    public int TriangleCount { get { return (mTris!=null) ? mTris.Length/3 : 0; } }
    /// <summary>
    /// The generated spline mesh
    /// </summary>
    public Mesh Mesh { get { return mMesh; } }

    /// <summary>
    /// Gets the transform
    /// </summary>
    public Transform Transform
    {
        get
        {
            if (!mTransform)
                mTransform = transform;
            return mTransform;
        }
    }

    
    /// <summary>
    /// If ScaleModifier==MeshScaleModifier.Delegate, this event is called when the Mesh Builder needs a scale 
    /// </summary>
    public event OnGetScaleEvent OnGetScale;

#if UNITY_EDITOR
    public double DebugPerfPrepare;
    public double DebugPerfExtrude;
    public double DebugPerfTime;
    public double DebugPerfBuildTris;

    System.Diagnostics.Stopwatch mPerfWatch = new System.Diagnostics.Stopwatch();
#endif

    #endregion

    public Mesh mMesh;
    MeshFilter mMeshFilter;
    Transform mTransform;
    // resulting mesh data
    Vector3[] mVerts;
    Vector2[] mUV;
    int[] mTris;
    float mLastRefresh;
    
    List<CurvyMeshSegmentInfo> mSegmentInfo = new List<CurvyMeshSegmentInfo>();

    MeshInfo StartMeshInfo;
    MeshInfo EndMeshInfo;
    int SegmentSteps
    {
        get
        {
            return mSegmentInfo.Count;
        }
    }

    #region ### Unity Callbacks ###

    void OnEnable()
    {
        mMeshFilter = GetComponent<MeshFilter>();
        if (mMeshFilter == null)
        {
            mMeshFilter = gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();
        }
        if (Application.isPlaying) 
            mMesh = mMeshFilter.mesh;
        else 
            mMesh = mMeshFilter.sharedMesh;

        mMesh = new Mesh();
        mMesh.name = "CurvyMesh";
        mMeshFilter.sharedMesh = mMesh;
        if (!Application.isPlaying)
            Refresh();
        if(Spline!=null)
        Spline.OnRefresh += OnSplineRefresh;
    }

    void OnDisable()
    {
        if (Spline)
            Spline.OnRefresh -= OnSplineRefresh;
    }

    void OnDestroy()
    {
        
        if (Application.isPlaying)
            Destroy(mMeshFilter.sharedMesh);
        else
            DestroyImmediate(mMeshFilter.sharedMesh);
    }

    IEnumerator Start()
    {
        if (Spline) {
            while (!Spline.IsInitialized) 
                yield return null;
            Refresh();
        }
        Spline.OnRefresh += OnSplineRefresh;
    }
    
    //void Update()
    //{
    //    if (!Spline) return;
    //    if (AutoRefresh)
    //    {
    //        Spline.OnRefresh -= OnSplineRefresh;
    //        Spline.OnRefresh += OnSplineRefresh;
    //    }
    //    else
    //    {
    //        enabled = false;
    //        Spline.OnRefresh -= OnSplineRefresh;
    //    }
    //}

    #endregion

    #region ### Public Methods ###

    /// <summary>
    /// Creates a GameObject with a SplinePathMeshBuilder script attached
    /// </summary>
    /// <returns></returns>
    public static SplinePathMeshBuilder Create()
    {
        SplinePathMeshBuilder o = new GameObject("CurvyMeshPath", typeof(SplinePathMeshBuilder)).GetComponent<SplinePathMeshBuilder>();
        return o;
    }

    /// <summary>
    /// Create a new GameObject containing the mesh
    /// </summary>
    /// <returns>the new Transform</returns>
    public Transform Detach()
    {
        var go = new GameObject();
        go.transform.position = transform.position;
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();

        go.name = "CurvyMeshPath_detached";
        mf.sharedMesh = UnityEngine.Mesh.Instantiate(Mesh) as Mesh;        
        mf.sharedMesh.name = "CurvyMesh";
        mf.sharedMesh.RecalculateBounds();
        mr.sharedMaterials = GetComponent<MeshRenderer>().sharedMaterials;
        return go.transform;
    }

    /// <summary>
    /// Rebuilds the mesh
    /// </summary>
    public void Refresh()
    {
        StartMeshInfo = null;
        EndMeshInfo = null;
        mMesh.Clear();
        //Debug.Log("Refresh Mesh at " + Time.realtimeSinceStartup);
        if (!Spline || !Spline.IsInitialized) return;
        
        BuildCaps();
        Prepare();
        
        if (StartMesh && StartMeshInfo!=null && ToTF - FromTF != 0) {
            mVerts = new Vector3[getTotalVertexCount()];
            mUV = new Vector2[mVerts.Length];
            mTris = new int[getTotalTriIndexCount()];
            Extrude();

            mMesh.vertices = mVerts;
            mMesh.uv = mUV;
            mMesh.triangles = mTris;
            mMesh.RecalculateNormals();
        }
    }

    #endregion

    #region ### Privates & internal Publics ###

    /*! \cond PRIVATE */
    /*! @name Internal Public
     *  Don't use them unless you know what you're doing!
     */
    //@{

    void BuildCaps() 
    {
        switch (CapShape) {
            case MeshCapShape.Line:
                StartMesh = MeshHelper.CreateLineMesh(CapWidth);
                break;
            case MeshCapShape.Rectangle:
                StartMesh = MeshHelper.CreateRectangleMesh(CapWidth, CapHeight,CapHollow);
                break;
            case MeshCapShape.NGon:
                StartMesh = MeshHelper.CreateNgonMesh(CapSegments, CapWidth,CapHollow);
                break;
        }
    }

    void Prepare()
    {
#if UNITY_EDITOR
        mPerfWatch.Reset();
        mPerfWatch.Start();
#endif
        if (Spline && StartMesh && ExtrusionParameter > 0) {
            StartMeshInfo = new MeshInfo(StartMesh, true, false);
            if (EndMesh)
                EndMeshInfo = new MeshInfo(EndMesh, false, true);
            else
                EndMeshInfo = new MeshInfo(StartMesh, false, true);

            

            // Calculate Steps
            float tf = FromTF;
            mSegmentInfo.Clear();
            FromTF = Mathf.Clamp01(FromTF);
            ToTF = Mathf.Max(FromTF, Mathf.Clamp01(ToTF));
            Vector3 scale;
            if (FromTF != ToTF) {
                switch (Extrusion) {
                    case MeshExtrusion.FixedF:
                        while (tf < ToTF) {
                            scale = getScale(tf);
                            mSegmentInfo.Add(new CurvyMeshSegmentInfo(this, tf,scale));
                            tf += ExtrusionParameter;
                        }
                        break;
                    case MeshExtrusion.FixedDistance:
                        float d = Spline.TFToDistance(FromTF);
                        tf = Spline.DistanceToTF(d);
                        while (tf < ToTF) {
                            scale = getScale(tf);
                            mSegmentInfo.Add(new CurvyMeshSegmentInfo(this, tf, d, scale));
                            d += ExtrusionParameter;
                            tf = Spline.DistanceToTF(d);
                        }
                        break;
                    case MeshExtrusion.Adaptive:
                        while (tf < ToTF) {
                            scale = getScale(tf);
                            mSegmentInfo.Add(new CurvyMeshSegmentInfo(this, tf, scale));
                            int dir = 1;
                            Spline.MoveByAngle(ref tf, ref dir, ExtrusionParameter, CurvyClamping.Clamp, 0.001f);
                        }
                        break;
                }
                if (!Mathf.Approximately(tf, ToTF))
                    tf = ToTF;
                scale = getScale(tf);
                mSegmentInfo.Add(new CurvyMeshSegmentInfo(this, tf, scale));
            }
        }
#if UNITY_EDITOR
        mPerfWatch.Stop();
        DebugPerfPrepare = mPerfWatch.ElapsedTicks / (double)System.TimeSpan.TicksPerMillisecond;
        mPerfWatch.Reset();
#endif
    }

    void OnSplineRefresh(CurvySpline sender)
    {
        if (Time.realtimeSinceStartup - mLastRefresh > AutoRefreshSpeed) {
            mLastRefresh = Time.realtimeSinceStartup;
            Refresh();
        }
    }

    int getTotalVertexCount()
    {
        int t = 0;
        
        if (StartMesh) {
            // Caps
            if (StartCap)
                t += StartMesh.vertexCount;
            if (EndCap) 
                t+= (EndMesh) ? EndMesh.vertexCount : StartMesh.vertexCount;

            // Extrusions
            if (StartMeshInfo.LoopVertexCount > 0)
                t += SegmentSteps * StartMeshInfo.LoopVertexCount;
            else
                t += SegmentSteps * StartMeshInfo.VertexCount;
        }
        return t;
    }

    int getTotalTriIndexCount()
    {
        int t = 0;

        if (StartMesh) {
            if (StartCap)
                t += StartMesh.triangles.Length;
            if (EndCap)
                t += (EndMesh) ? EndMesh.triangles.Length : StartMesh.triangles.Length;

            if (StartMeshInfo.LoopVertexCount == 0)
                t += (SegmentSteps - 1) * Mathf.Max(2,(StartMeshInfo.VertexCount-1)) *2 * 3;
            else
                t += (SegmentSteps - 1) * StartMeshInfo.LoopTriIndexCount;
        }
        return t;
    }

    float getV(CurvyMeshSegmentInfo info, int step, int stepsTotal)
    {
        switch (UV) {
            case MeshUV.StretchVSegment:
                return step*UVParameter;
            case MeshUV.Absolute:
                return info.Distance / UVParameter;
            default: // StretchV
                return step / (float)stepsTotal * UVParameter;
        }
    }

    Vector3 getScale(float tf)
    {
        switch (ScaleModifier) {
            case MeshScaleModifier.ControlPoint:
                return Spline.InterpolateScale(tf);
            case MeshScaleModifier.UserValue:
                return Spline.InterpolateUserValue(tf,ScaleModifierUserValueSlot);    
            case MeshScaleModifier.Delegate:
                return (OnGetScale != null) ? OnGetScale(this, tf) : Vector3.one;
            case MeshScaleModifier.AnimationCurve:
                Vector3 v=Vector3.one;
                if (ScaleModifierCurve != null) {
                    return v * ScaleModifierCurve.Evaluate(tf);
                }
                else
                    return v;
            default:
                return Vector3.one;
        }
    }

    void Extrude()
    {
#if UNITY_EDITOR
        mPerfWatch.Reset();
        mPerfWatch.Start();
#endif
        int segSteps = SegmentSteps;
        int triangulationSteps = SegmentSteps - 1;
        int vIndex = 0;
        int triIndex = 0;
        int vStartCapIndex=0;
        int vEndCapIndex = 0;

        #region ### Build Vertices ###

        // ### Add Segment Vertices ###
        if (StartMeshInfo.LoopVertexCount == 0) {
            // Unclosed Mesh (i.e. line like)
            for (int seg = 0; seg < segSteps; seg++) { // each segment
                float vcoord = getV(mSegmentInfo[seg], seg, segSteps);
                for (int v = 0; v < StartMeshInfo.VertexCount; v++) {
                    mVerts[vIndex + v] = mSegmentInfo[seg].Matrix.MultiplyPoint3x4(StartMeshInfo.Vertices[v]);
                    mUV[vIndex + v] = new Vector2(StartMeshInfo.UVs[v].x, vcoord);
                }
                vIndex += StartMeshInfo.VertexCount;
            }
        }
        else {
            // Closed Mesh
            bool wallTexture = false;
            bool rotateTexture = false;
            if (curvySpline2 != null)
            {
                wallTexture = curvySpline2.wallTexture;
                rotateTexture = curvySpline2.rotateTexture;
            }

            for (int seg = 0; seg < segSteps; seg++)
            { // each segment
                float vcoord = getV(mSegmentInfo[seg], seg, segSteps)*.5f;
                for (int l = 0; l < StartMeshInfo.EdgeLoops.Length; l++) { // each EdgeLoop
                    EdgeLoop E = StartMeshInfo.EdgeLoops[l];
                    for (int ev = 0; ev < E.vertexCount; ev++) { // each EdgeLoop vertex
                        mVerts[vIndex + ev] = mSegmentInfo[seg].Matrix.MultiplyPoint3x4(StartMeshInfo.EdgeVertices[E.vertexIndex[ev]]);
                        Vector2 vector2 = StartMeshInfo.EdgeUV[E.vertexIndex[ev]];

                        var vector3 = new Vector2(wallTexture ? vcoord : vector2.x, wallTexture ? vector2.y : vcoord);
                        if (rotateTexture)
                        {
                            var a = vector3.y;
                            vector3.y = vector3.x;
                            vector3.x = a;
                        }
                        mUV[vIndex + ev] = vector3;
                    }
                    vIndex += E.vertexCount;
                }
            }
        }
        
        
        // ### Handle Caps ###
        if (StartCap) {
            StartMeshInfo.TRSVertices(mSegmentInfo[0].Matrix).CopyTo(mVerts, vIndex);
            StartMeshInfo.UVs.CopyTo(mUV, vIndex);
            vStartCapIndex = vIndex;
            vIndex += StartMeshInfo.VertexCount;
        }
        if (EndCap) {
            EndMeshInfo.TRSVertices(mSegmentInfo[segSteps-1].Matrix).CopyTo(mVerts, vIndex);
            EndMeshInfo.UVs.CopyTo(mUV, vIndex);
            vEndCapIndex = vIndex;
            vIndex += EndMeshInfo.VertexCount;
        }

        #endregion
        
        #region ### Build Triangles ###

#if UNITY_EDITOR
        mPerfWatch.Stop();
        DebugPerfExtrude = mPerfWatch.ElapsedTicks / (double)System.TimeSpan.TicksPerMillisecond;
        mPerfWatch.Reset();
        mPerfWatch.Start();
#endif
        if (StartMeshInfo.LoopVertexCount == 0) {
            // Unclosed Mesh (i.e. line like)
            int vertsPerStep = StartMeshInfo.VertexCount;
            for (int seg = 0; seg < triangulationSteps; seg++) { // each segment
                int vtSegN = vertsPerStep * seg;
                int vtSegN1 = vertsPerStep * (seg + 1);
                
                for (int ev = 0; ev < StartMeshInfo.VertexCount-1; ev++) {
                    mTris[triIndex] = vtSegN+ev;
                    mTris[triIndex + 1] = vtSegN1+ev;
                    mTris[triIndex + 2] = vtSegN1 +1+ev;
                    mTris[triIndex + 3] = vtSegN1 + 1+ev;
                    mTris[triIndex + 4] = vtSegN + 1+ev;
                    mTris[triIndex + 5] = vtSegN+ev;
                    triIndex += 6;
                }
                
            }
        }
        else {
            // Closed Mesh
            int vertsPerStep = StartMeshInfo.LoopVertexCount;

            for (int seg = 0; seg < triangulationSteps; seg++) { // each segment
                int vtSegN = vertsPerStep * seg;
                int vtSegN1 = vertsPerStep * (seg + 1);
                for (int l = 0; l < StartMeshInfo.EdgeLoops.Length; l++) { // each loop
                    EdgeLoop E = StartMeshInfo.EdgeLoops[l];

                    for (int ev = 0; ev < E.vertexCount; ev++) { // E.vertexIndex[] is E.vertexCount+1
                        mTris[triIndex] = vtSegN + E.vertexIndex[ev];
                        mTris[triIndex + 1] = vtSegN1 + E.vertexIndex[ev];
                        mTris[triIndex + 2] = vtSegN + E.vertexIndex[ev + 1];
                        mTris[triIndex + 3] = vtSegN1 + E.vertexIndex[ev];
                        mTris[triIndex + 4] = vtSegN1 + E.vertexIndex[ev + 1];
                        mTris[triIndex + 5] = vtSegN + E.vertexIndex[ev + 1];
                        triIndex += 6;
                    }
                }
            }
        }
        // ### Handle extruded segments ###
        

        // ### Handle Caps ###
        if (StartCap) {
            for (int t = 0; t<StartMeshInfo.Triangles.Length; t++) {
                mTris[triIndex + t] = StartMeshInfo.Triangles[t] + vStartCapIndex;
            }
            triIndex += StartMeshInfo.Triangles.Length;
        }
        if (EndCap) {
            for (int t = 0; t < EndMeshInfo.Triangles.Length; t++) {
                mTris[triIndex + t] = EndMeshInfo.Triangles[t] + vEndCapIndex;
            }
        }

        #endregion

#if UNITY_EDITOR
        mPerfWatch.Stop();
        DebugPerfBuildTris = mPerfWatch.ElapsedTicks / (double)System.TimeSpan.TicksPerMillisecond;
        DebugPerfExtrude += DebugPerfBuildTris;
        mPerfWatch.Reset();
        DebugPerfTime = DebugPerfPrepare + DebugPerfExtrude;
#endif
    }
    //@}
    /*! \endcond */
    #endregion
}



