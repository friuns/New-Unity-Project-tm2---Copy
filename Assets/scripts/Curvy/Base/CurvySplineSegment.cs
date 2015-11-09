// =====================================================================
// Copyright 2013 FluffyUnderware
// All rights reserved
// =====================================================================

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class covering a Curvy Spline Segment / ControlPoint
/// </summary>
public partial class CurvySplineSegment : MonoBehaviour
{
    #region ### Public fields and properties ###

    /// <summary>
    /// Keep Start/End-TCB synchronized
    /// </summary>
    /// <remarks>Applies only to TCB Interpolation</remarks>
    public bool SynchronizeTCB = true;
    /// <summary>
    /// Whether the tangent should be smoothed at the edge to the next segment
    /// </summary>
    /// <remarks>This is most useful for linear and TCB splines</remarks>
    public bool SmoothEdgeTangent;
    /// <summary>
    /// Whether local Tension should be used
    /// </summary>
    /// <remarks>This only applies to interpolation methods using Tension</remarks>
    public bool OverrideGlobalTension;
    /// <summary>
    /// Whether local Continuity should be used
    /// </summary>
    /// <remarks>This only applies to interpolation methods using Continuity</remarks>
    public bool OverrideGlobalContinuity;
    /// <summary>
    /// Whether local Bias should be used
    /// </summary>
    /// <remarks>This only applies to interpolation methods using Bias</remarks>
    public bool OverrideGlobalBias;
    /// <summary>
    /// Start Tension
    /// </summary>
    /// <remarks>This only applies to interpolation methods using Tension</remarks>
    public float StartTension;
    /// <summary>
    /// Start Continuity
    /// </summary>
    /// <remarks>This only applies to interpolation methods using Continuity</remarks>
    public float StartContinuity;
    /// <summary>
    /// Start Bias
    /// </summary>
    /// <remarks>This only applies to interpolation methods using Bias</remarks>
    public float StartBias;
    /// <summary>
    /// End Tension
    /// </summary>
    /// <remarks>This only applies to interpolation methods using Tension</remarks>
    public float EndTension;
    /// <summary>
    /// End Continuity
    /// </summary>
    /// <remarks>This only applies to interpolation methods using Continuity</remarks>
    public float EndContinuity;
    /// <summary>
    /// End Bias
    /// </summary>
    ///<remarks>This only applies to interpolation methods using Bias</remarks>
    public float EndBias;

    /// <summary>
    /// User Values array that can be lerped by calling CurvySplineSegment.InterpolateUserValue()
    /// </summary>
    /// <remarks>Use CurvySpline.UserValueSize to set define the size of this array</remarks>
    public Vector3[] UserValues = new Vector3[0];

    /// <summary>
    /// List of precalculated interpolations
    /// </summary>
    /// <remarks>Based on a spline's Granulartiy</remarks>
    [HideInInspector] public Vector3[] Approximation = new Vector3[0];

    /// <summary>
    /// List of precalculated distances
    /// </summary>
    /// <remarks>Based on a spline's Granularity</remarks>
    [HideInInspector] public float[] ApproximationDistances = new float[0];

    /// <summary>
    /// List of precalculated Up-Vectors
    /// </summary>
    /// <remarks>Based on a spline's Granularity</remarks>
    [HideInInspector] public Vector3[] ApproximationUp = new Vector3[0];
    /// <summary>
    /// List of precalculated Tangent-Normals
    /// </summary>
    /// <remarks>Based on a spline's Granularity</remarks>
    [HideInInspector] public Vector3[] ApproximationT = new Vector3[0];

    /// <summary>
    /// Gets transform
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
    /// Gets the length of this spline segment
    /// </summary>
    public float Length { get; private set; }

    /// <summary>
    /// Gets the distance from spline start to the first control point (localF=0) 
    /// </summary>
    public float Distance { get; private set; }

    /// <summary>
    /// Gets or sets tranform.position
    /// </summary>
    public Vector3 Position
    {
        get { return Transform.position; }
        set { Transform.position = value; }
    }

    /// <summary>
    /// Gets whether this Control Point reflects a valid segment or just an Control Point
    /// </summary>
    public bool IsValidSegment
    {
        get
        {
            //return Spline.CatmulRom(PreviousTransform.position, Transform.position, NextTransform.position, NextControlPoint.NextTransform.position, localF);
            switch (Spline.Interpolation)
            {
                case CurvyInterpolation.Linear:
                    return (Transform && NextTransform);
                case CurvyInterpolation.CatmulRom:
                case CurvyInterpolation.TCB:
                    return (Transform && PreviousTransform && NextControlPoint && NextControlPoint.NextTransform);
            }
            return false;
        }
    }

    /// <summary>
    /// Gets whether this segment is the first IGNORING closed splines
    /// </summary>
    public bool IsFirstSegment
    {
        get { return (!PreviousSegment || (Spline.Closed && PreviousSegment == Spline[Spline.Count - 1])); }
    }

    /// <summary>
    /// Gets whether this segment is the last IGNORING closed splines
    /// </summary>
    public bool IsLastSegment
    {
        get { return (!NextSegment || (Spline.Closed && NextSegment == Spline[0])); }
    }

    /// <summary>
    /// Gets the next Control Point
    /// </summary>
    /// <returns>a CurvySplineSegment or Null</returns>
    public CurvySplineSegment NextControlPoint
    {
        get { return Spline.NextControlPoint(this); }
    }

    /// <summary>
    /// Gets the previous Control Point
    /// </summary>
    /// <returns>a CurvySplineSegment or Null</returns>
    public CurvySplineSegment PreviousControlPoint
    {
        get { return Spline.PreviousControlPoint(this); }
    }

    /// <summary>
    /// Gets the next Transform
    /// </summary>
    /// <returns>a Transform or Null</returns>
    public Transform NextTransform
    {
        get { return Spline.NextTransform(this); }
    }

    /// <summary>
    /// Gets the previous Transform
    /// </summary>
    /// <returns>a Transform or Null</returns>
    public Transform PreviousTransform
    {
        get { return Spline.PreviousTransform(this); }
    }


    /// <summary>
    /// Gets the next segment
    /// </summary>
    /// <returns>a segment or null</returns>
    public CurvySplineSegment NextSegment
    {
        get { return Spline.NextSegment(this); }
    }

    /// <summary>
    /// Gets the previous segment
    /// </summary>
    /// <returns>a segment or null</returns>
    public CurvySplineSegment PreviousSegment
    {
        get { return Spline.PreviousSegment(this); }
    }

    /// <summary>
    /// Gets the Index of this segment
    /// </summary>
    /// <returns>an index to be used with CurvySpline.Segments or -1 if this Control Point doesn't form an segment</returns>
    public int SegmentIndex
    {
        get { return mSegmentIndex; }
    }
    /// <summary>
    /// Gets the Index of this Control Point
    /// </summary>
    /// <returns>an index to be used with CurvySpline.ControlPoints</returns>
    public int ControlPointIndex
    {
        get { return mControlPointIndex; }
    }

    /// <summary>
    /// Gets the parent spline
    /// </summary>
    
    public CurvySpline Spline
    {
        get
        {
            if (!mSpline)
                mSpline = transform.parent.GetComponent<CurvySpline>();
            return mSpline;
        }
    }

    #endregion

    private Transform mTransform;
    private Vector3 mPosition;
    private Quaternion mRotation;
    public CurvySpline mSpline;
    private float mStepSize;
    private int mControlPointIndex;
    private int mSegmentIndex;

    //CurvySplineSegment mNextControlPoint;

    #region ### Unity Callbacks ###

    private void OnDrawGizmos()
    {
        DoGizmos(false);
    }

    private void OnDrawGizmosSelected()
    {
        DoGizmos(true);
    }

    private void DoGizmos(bool selected)
    {
        //if (!Spline.ShowGizmos) return;
        //Gizmos.color = (selected) ? CurvySpline.GizmoSelectionColor : CurvySpline.GizmoColor;
        //Gizmos.DrawSphere(Transform.position, CurvySpline.GizmoControlPointSize);

        //if (!IsValidSegment) return;

        //Vector3 p = Transform.position;
        ////Gizmos.color = (selected) ? Spline.GizmoSelectionColor : Spline.GizmoColor;
        //for (float f = 0.05f; f < 1; f += 0.05f)
        //{
        //    Vector3 p1 = Interpolate(f);
        //    Gizmos.DrawLine(p, p1);
        //    p = p1;
        //}
        //Gizmos.DrawLine(p, NextTransform.position);

        //if (Spline.ShowApproximation && Approximation.Length > 0)
        //{
        //    Gizmos.color = Color.gray;
        //    foreach (Vector3 pa in Approximation)
        //        Gizmos.DrawCube(pa, new Vector3(0.1f, 0.1f, 0.1f));
        //    Vector3 p3 = Approximation[0];
        //    for (int i = 1; i < Approximation.Length; i++)
        //    {
        //        Vector3 p1 = Approximation[i];
        //        Gizmos.DrawLine(p3, p1);
        //        p3 = p1;
        //    }
        //}
        //if (Spline.ShowOrientation && Spline.Orientation != CurvyOrientation.None && ApproximationUp.Length > 0)
        //{
        //    Gizmos.color = Color.yellow;
        //    for (int i = 0; i < ApproximationUp.Length; i++)
        //        Gizmos.DrawLine(Approximation[i],
        //                        Approximation[i] + ApproximationUp[i]*CurvySpline.GizmoOrientationLength);
        //}

        //if (Spline.ShowTangents && ApproximationT.Length > 0)
        //{
        //    Gizmos.color = new Color(0, 0.7f, 0);
        //    for (int i = 0; i < ApproximationT.Length; i++)
        //        Gizmos.DrawLine(Approximation[i], Approximation[i] + ApproximationT[i]);
        //}

    }

    private void OnEnable()
    {
        mPosition = Transform.position;
        mRotation = Transform.rotation;
    }

    #endregion

    #region ### Public Methods ###

    /// <summary>
    /// Deletes this Control Point
    /// </summary>
    public void Delete()
    {
        Spline.Delete(this);
    }

    /// <summary>
    /// Interpolates position for a local F
    /// </summary>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <returns>the interpolated position</returns>
    public Vector3 Interpolate(float localF)
    {
        localF = Mathf.Clamp01(localF);
        switch (Spline.Interpolation)
        {
            case CurvyInterpolation.CatmulRom:
                return Spline.CatmulRom(PreviousTransform.position, Transform.position, NextTransform.position,
                                        NextControlPoint.NextTransform.position, localF);
            case CurvyInterpolation.TCB:
                float t0 = StartTension;
                float t1 = EndTension;
                float c0 = StartContinuity;
                float c1 = EndContinuity;
                float b0 = StartBias;
                float b1 = EndBias;

                if (!OverrideGlobalTension)
                    t0 = t1 = Spline.Tension;
                if (!OverrideGlobalContinuity)
                    c0 = c1 = Spline.Continuity;
                if (!OverrideGlobalBias)
                    b0 = b1 = Spline.Bias;
                return Spline.TCB(PreviousTransform.position, Transform.position, NextTransform.position,
                                  NextControlPoint.NextTransform.position, localF, t0, c0, b0, t1, c1, b1);
            default: // LINEAR
                return Vector3.Lerp(Transform.position, NextTransform.position, localF);
        }
    }

    /// <summary>
    /// Interpolates position for a local F using a linear approximation
    /// </summary>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <returns>the interpolated position</returns>
    public Vector3 InterpolateFast(float localF)
    {
        float frag;
        int idx = getApproximationIndex(localF, out frag);
        return (Vector3.Lerp(Approximation[idx], Approximation[idx + 1], frag));
    }

    /// <summary>
    /// Gets an interpolated User Value for a local F
    /// </summary>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <param name="index">the UserValue array index</param>
    /// <returns>the interpolated value</returns>
    public Vector3 InterpolateUserValue(float localF, int index)
    {
        if (index >= Spline.UserValueSize || NextControlPoint == null)
            return Vector3.zero;

        return Vector3.Lerp(UserValues[index], NextControlPoint.UserValues[index], localF);
    }

    /// <summary>
    /// Gets the interpolated Scale
    /// </summary>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <returns>the interpolated value</returns>
    public Vector3 InterpolateScale(float localF)
    {
        Transform T = NextTransform;
        return (T) ? Vector3.Lerp(Transform.lossyScale, T.lossyScale, localF) : Transform.lossyScale;
    }

    /// <summary>
    /// Gets the tangent for a local F
    /// </summary>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <returns>the tangent/direction</returns>
    public Vector3 GetTangent(float localF)
    {
        localF = Mathf.Clamp01(localF);
        Vector3 p = Interpolate(localF);
        return GetTangent(localF, ref p);
    }

    /// <summary>
    /// Gets the normalized tangent for a local F with the interpolated position for f known
    /// </summary>
    /// <remarks>This saves one interpolation if you already know the position</remarks>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <param name="position">the result of Interpolate(localF)</param>
    /// <returns></returns>
    public Vector3 GetTangent(float localF, ref Vector3 position)
    {
        float f2 = localF + 0.01f;
        if (f2 > 1)
        {
            if (NextSegment)
                return (NextSegment.Interpolate(f2 - 1) - position).normalized;
            else
            {
                f2 = localF - 0.01f;
                return (position - Interpolate(f2)).normalized;
            }
        }
        else
        {
            return (Interpolate(f2) - position).normalized;
        }
    }

    /// <summary>
    /// Gets the normalized tangent for a local F with the interpolated position for f known using a linear approximation
    /// </summary>
    /// <remarks>This saves one interpolation if you already know the position</remarks>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <returns></returns>
    public Vector3 GetTangentFast(float localF)
    {
        float frag;
        int idx = getApproximationIndex(localF, out frag);
        return (Vector3.Lerp(ApproximationT[idx], ApproximationT[idx + 1], frag));
    }

    /// <summary>
    /// Gets a rotation looking to Tangent with the head upwards along the Up-Vector
    /// </summary>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <returns>a rotation</returns>
    public Quaternion GetOrientationFast(float localF)
    {
        Vector3 view = GetTangentFast(localF);

        if (view != Vector3.zero)
            return Quaternion.LookRotation(GetTangentFast(localF), GetOrientationUpFast(localF));
        else
            return Quaternion.identity;
    }

    /// <summary>
    /// Gets the Up-Vector for a local F based on the splines' Orientation mode
    /// </summary>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <returns>the Up-Vector</returns>
    public Vector3 GetOrientationUpFast(float localF)
    {
        float frag;
        int idx = getApproximationIndex(localF, out frag);
        return (Vector3.Lerp(ApproximationUp[idx], ApproximationUp[idx + 1], frag));
    }

    /// <summary>
    /// Gets the local F by a distance within this line segment
    /// </summary>
    /// <param name="localDistance">local distance in the range 0..Length</param>
    /// <returns>a local F in the range 0..1</returns>
    public float DistanceToLocalF(float localDistance)
    {
        localDistance = Mathf.Clamp(localDistance, 0, Length);
        if (ApproximationDistances.Length == 0 || localDistance == 0) return 0;
        if (Mathf.Approximately(localDistance, Length)) return 1;

        int lidx = ApproximationDistances.Length - 1;
        while (ApproximationDistances[lidx] > localDistance)
            lidx--;

        float frag = (localDistance - ApproximationDistances[lidx])/
                     (ApproximationDistances[lidx + 1] - ApproximationDistances[lidx]);
        float lf = _getApproximationLocalF(lidx);
        float uf = _getApproximationLocalF(lidx + 1);
        return lf + (uf - lf)*frag;
    }

    /// <summary>
    /// Gets the local distance for a certain localF value
    /// </summary>
    /// <param name="localF">a local F value in the range 0..1</param>
    /// <returns>a distance in the range 0..Length</returns>
    public float LocalFToDistance(float localF)
    {
        localF = Mathf.Clamp01(localF);
        float frag;
        int idx = getApproximationIndex(localF, out frag);
        float d = ApproximationDistances[idx + 1] - ApproximationDistances[idx];
        return ApproximationDistances[idx] + d*frag;
    }

    /// <summary>
    /// Gets TF for a certain local F
    /// </summary>
    /// <param name="localF">a local F in the range 0..1</param>
    /// <returns>a TF value</returns>
    public float LocalFToTF(float localF)
    {
        return Spline.SegmentToTF(this, localF);
    }

    /// <summary>
    /// Moves the Control Point along it's Up-Vector to match a desired Spline length
    /// </summary>
    /// <remarks>When the desired length can't be achieved, the Control Point will stop moving at the nearest possible point</remarks>
    /// <param name="newSplineLength">the desired length of the spline</param>
    /// <param name="stepSize">stepSize used when moving</param>
    /// <returns>false if the length can't be achieved by moving this Control Point.</returns>
    public bool SnapToFitSplineLength(float newSplineLength, float stepSize)
    {
        if (stepSize == 0 || Mathf.Approximately(newSplineLength, Spline.Length)) return true;

        float curLength = Spline.Length;
        Vector3 oldPos = Transform.position;
        Vector3 upstep = Transform.up*stepSize;

        // Check if increasing by Up-Vector will increase the length
        Transform.position += upstep;
        Spline.Refresh(true, false, false);
        bool UpGrows = (Spline.Length > curLength);
        int loops = 30000;
        Transform.position = oldPos;

        // Need to grow?
        if (newSplineLength > curLength)
        {
            if (!UpGrows)
                upstep *= -1;
            while (Spline.Length < newSplineLength)
            {
                loops--;
                curLength = Spline.Length;
                Transform.position += upstep;
                Spline.Refresh(true, false, false);
                if (curLength > Spline.Length)
                {
                    return false;
                }
                if (loops == 0)
                {
                    Debug.LogError(
                        "CurvySplineSegment.SnapToFitSplineLength exceeds 30000 loops, considering this a dead loop! This shouldn't happen, please report this as an error!");
                    return false;
                }
            }
        }
        else
        {
            // otherwise shrink
            if (UpGrows)
                upstep *= -1;
            while (Spline.Length > newSplineLength)
            {
                loops--;
                curLength = Spline.Length;
                Transform.position += upstep;
                Spline.Refresh(true, false, false);
                if (curLength < Spline.Length)
                {
                    return false;
                }
                if (loops == 0)
                {
                    Debug.LogError(
                        "CurvySplineSegment.SnapToFitSplineLength exceeds 30000 loops, considering this a dead loop! This shouldn't happen, please report this as an error!");
                    return false;
                }
            }
        }
        return true;
    }



    #endregion

    #region ### Privates & internal Publics ###

    /*! \cond PRIVATE */
    /*! @name Internal Public
     *  Don't use them unless you know what you're doing!
     */
    //@{


    /// <summary>
    /// Internal, Gets localF by an index of mApproximation
    /// </summary>
    public float _getApproximationLocalF(int idx)
    {
        return idx*mStepSize;
    }

    /// <summary>
    /// Gets the index of mApproximation by F and the remaining fragment
    /// </summary>
    private int getApproximationIndex(float localF, out float frag)
    {
        localF = Mathf.Clamp01(localF);
        if (localF == 1)
        {
            frag = 1;
            return Approximation.Length - 2;
        }
        float f = localF/mStepSize;
        int idx = (int) f;
        frag = f - idx;
        return idx;
    }

    /// <summary>
    /// Internal, don't call directly
    /// </summary>
    public void _InitializeControlPoint()
    {
        //mNextControlPoint = NextControlPoint;
        mStepSize = 1f/Spline.Granularity;
        mControlPointIndex = Spline.ControlPoints.IndexOf(this);
        Approximation = new Vector3[0];
        ApproximationDistances = new float[0];
        ApproximationUp = new Vector3[0];
        ApproximationT = new Vector3[0];
        if (UserValues.Length != Spline.UserValueSize)
            CurvySplineSegment.Resize(ref UserValues, Spline.UserValueSize);
    }
    public static void Resize(ref Vector3[] userValues, int userValueSize)
    {
        Vector3[] ar = new Vector3[userValueSize];
        var length = Mathf.Min(userValueSize, userValues.Length);
        //System.Array.Copy(userValues, ar, length);
        for (int i = 0; i < length; i++)
        {
            ar[i] = userValues[i];
        }
        userValues = ar;
    }

    public static void Resize(ref Vector2[] userValues, int userValueSize)
    {
        Vector2[] ar = new Vector2[userValueSize];
        var length = Mathf.Min(userValueSize, userValues.Length);
        //System.Array.Copy(userValues, ar, length);
        for (int i = 0; i < length; i++)
        {
            ar[i] = userValues[i];
        }
        userValues = ar;
    }


    /// <summary>
    /// Internal, don't call directly
    /// </summary>
    public void _UpdateApproximation()
    {
        if (IsValidSegment)
        {
            mSegmentIndex = Spline.Segments.IndexOf(this);
            mStepSize = 1f/Spline.Granularity;
            Approximation = new Vector3[Spline.Granularity + 1];
            ApproximationUp = new Vector3[Spline.Granularity + 1];
            ApproximationT = new Vector3[Spline.Granularity + 1];
            Approximation[0] = Position;
            Approximation[Spline.Granularity] = NextTransform.position;

            int al = Approximation.Length - 1;
            for (int i = 1; i < al; i++)
                Approximation[i] = Interpolate(i*mStepSize);
        }
    }

    /// <summary>
    /// Internal, don't call directly
    /// </summary>
    /// <returns></returns>
    public float _UpdateLength()
    {
        CurvySplineSegment prev = PreviousSegment;
        Distance = (prev && prev.IsValidSegment && Spline[0] != this) ? prev.Distance + prev.Length : 0;
        Length = 0;
        if (IsValidSegment)
        {
            ApproximationDistances = new float[Approximation.Length];
            int al = ApproximationDistances.Length;
            for (int i = 1; i < al; i++)
            {
                ApproximationDistances[i] = ApproximationDistances[i - 1] +
                                            (Approximation[i] - Approximation[i - 1]).magnitude;
            }
            Length = ApproximationDistances[ApproximationDistances.Length - 1];
        }
        return Length;
    }

    public void _UpdateTangents()
    {
        if (IsValidSegment)
        {
            mStepSize = 1f/Spline.Granularity;
            int al = ApproximationT.Length;
            for (int i = 0; i < al; i++)
            {
                ApproximationT[i] = GetTangent(i*mStepSize, ref Approximation[i]);
            }
            // Smooth incoming edge tangent
            if (SmoothEdgeTangent)
            {
                if (!IsFirstSegment)
                {
                    PreviousSegment.ApproximationT[Spline.Granularity] =
                        Vector3.Lerp(PreviousSegment.ApproximationT[Spline.Granularity - 1], ApproximationT[0], 0.5f);
                    ApproximationT[0] = PreviousSegment.ApproximationT[Spline.Granularity];
                }
            }
            if (IsLastSegment && NextSegment && Spline[0].SmoothEdgeTangent)
            {
                // closed spline
                ApproximationT[Spline.Granularity] = Vector3.Lerp(ApproximationT[Spline.Granularity - 1],
                                                                  Spline[0].ApproximationT[0], 0.5f);
                Spline[0].ApproximationT[0] = ApproximationT[Spline.Granularity];
            }
        }
    }

    private Vector3 ParallelTransportFrame(ref Vector3 lastUp, ref Vector3 T1, ref Vector3 T2, float swirlangle)
    {
        Vector3 A = Vector3.Cross(T1, T2);
        float a = Mathf.Atan2(A.magnitude, Vector3.Dot(T1, T2));
        Quaternion Q = Quaternion.AngleAxis(Mathf.Rad2Deg*a, A.normalized);
        if (Spline.Swirl != CurvyOrientationSwirl.None)
            return Q*Quaternion.AngleAxis(swirlangle, T2)*lastUp;
        else
            return Q*lastUp;
    }

    public float swirl;
    public float scale
    {
        set { transform.localScale = new Vector3(value, 1, 1); }
        get { return transform.localScale.x; }
    }


// returns the last T
    public Vector3 _UpdateOrientation(Vector3 lastUpVector)
    {
        //if (NextControlPoint == null && PreviousControlPoint != null)
        //    transform.forward = Position - PreviousControlPoint.Position;
        //else
        //if(transform.forward!=Vector3.zero)
        //    transform.forward = GetTangent(0);
        //transform.Rotate(Vector3.forward, swirl, Space.Self);
        

        // Setup swirling
        float swirlangle=0;
        switch (Spline.Swirl) {
            case CurvyOrientationSwirl.Segment:
                swirlangle = Spline.SwirlTurns * 360 / (float)Spline.Granularity + swirl;
                break;
            case CurvyOrientationSwirl.Spline:
                swirlangle = (Spline.SwirlTurns * 360 / Spline.Count) / (float)Spline.Granularity + swirl;
                break;
        }

        int i;
        ApproximationUp[0] = lastUpVector;
        if (Spline.Orientation == CurvyOrientation.Tangent) {
            int al = Approximation.Length;
            for (i = 1; i < al; i++) {
                lastUpVector = ParallelTransportFrame(ref lastUpVector, ref ApproximationT[i-1], ref ApproximationT[i],swirlangle);
                ApproximationUp[i] = lastUpVector;
            }
        }
        else if (Spline.Orientation == CurvyOrientation.ControlPoints) {
            int al = Approximation.Length;
            for (i = 0; i < al; i++) {
                
                ApproximationUp[i] = Vector3.Lerp(Transform.up, NextTransform.up, i * mStepSize);
            }
            lastUpVector = ApproximationUp[Spline.Granularity];
        }

        return lastUpVector;
    }
    
    // returns the last T
    public Vector3 _SmoothOrientation(Vector3 lastUpVector,ref float angleaccu, float angle)
    {
        ApproximationUp[0] = lastUpVector;
        for (int i = 1; i < ApproximationUp.Length; i++) {
            ApproximationUp[i] = Quaternion.AngleAxis(angleaccu, ApproximationT[i]) * ApproximationUp[i];
            angleaccu += angle;
        }
        return ApproximationUp[ApproximationUp.Length-1];
    }
    
    public bool _PositionHasChanged()
    {
        if (!Transform)
            return true;
        bool res = Transform.position != mPosition;
        mPosition = Transform.position;
        return res;
    }

    public bool _RotationHasChanged()
    {
        if (!Transform)
            return true;

        bool res = Transform.rotation != mRotation;
        mRotation = Transform.rotation;
        return res;
    }
    //@}
    /*! \endcond */
    #endregion

}

