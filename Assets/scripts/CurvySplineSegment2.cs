using System;
using System.Collections.Generic;
using UnityEngine;

public partial class CurvySplineSegment
{
    
    public bool isEnd { get { return NextControlPoint == null || PreviousControlPoint == null; } }
    public CurvySpline2 Spline2 { get { return (CurvySpline2)Spline; } }
    public List<SplinePathMeshBuilder> sbs = new List<SplinePathMeshBuilder>();
    public List<CurvySpline2> spls = new List<CurvySpline2>();
    //public Bounds? m_bounds;
    //internal Vector2 bounds;
    internal Vector3 pivot;
    internal float dist = bs.MaxValue;
    internal float z;
    internal Vector3 point;
    public bool flying;
    //public static Vector3 dragOffset2;
    public Vector3 dragOffset;//{get{return dragOffset2;}set{dragOffset2=value;}}
    public Vector2 GetBounds()
    {
        var bounds = new Vector2(0, 0);
        foreach (CurvySpline2 a in spls)
            if (a)
            {
                Bounds b = a.GetBounds(true);
                bounds.y = Mathf.Max(bounds.y, b.extents.y - b.center.y);
                bounds.x = Math.Max(bounds.x, b.size.x);
            }
        if (bounds == Vector2.zero)
            bounds = new Vector2(25, -1);
            

        if (NextControlPoint != null || PreviousControlPoint != null)
        {
            float max = Mathf.Abs(swirl);
            if (NextControlPoint != null)
                max = Mathf.Max(max, Mathf.Abs(NextControlPoint.swirl));
            if (PreviousControlPoint != null)
                max = Mathf.Max(max, Mathf.Abs(PreviousControlPoint.swirl));

            var vector3 = Quaternion.Euler(0, 0, max) * bounds;
            vector3.x = Mathf.Abs(vector3.x);
            vector3.y = Mathf.Abs(vector3.y);
            //vector3.y = Mathf.Max(vector3.y, -(Quaternion.Euler(0, 0, max) * bounds).y);
            bounds = vector3;
        }
        bounds.x *= scale;
        return bounds;
    }
    //public Bounds bounds {get{return spls.Select(a=>a.GetBounds(false)).OrderBy(a=>a.size.magnitude;)}}
    public void OnDestroy()
    {
        foreach (var a in sbs)
            Destroy(a.gameObject);
        
    }


}