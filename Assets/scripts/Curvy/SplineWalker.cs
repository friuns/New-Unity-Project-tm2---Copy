using UnityEngine;
using System.Collections;
/* Drop this script to a transform you'd like to move along a Curvy spline!
 * 
 * Basically you just need two methods (Spline.Move and Spline.GetOrientationFast) to align an object to a spline,
 * so you can easily incorporate spline control into your own scripts.
 */

/// <summary>
/// Move a Transform along a spline
/// </summary>
[ExecuteInEditMode]
public class SplineWalker : MonoBehaviour {
    public CurvySpline Spline; // The spline to use
    public CurvyClamping Clamping = CurvyClamping.Clamp; // What to do if we reach the spline's end?
    public bool SetOrientation = true; // Rotate to match orientation?
    public bool FastInterpolation; // use cached values?
    public bool MoveByWorldUnits = false; // move at a constant speed regardless of segment length?
    public float InitialF; // the starting position
    public float Speed; // the moving speed, either in F or world units (depending on MoveByWorldUnits)

    /// <summary>
    /// Relative position on the spline
    /// </summary>
    public float TF 
    {
        get { return mTF; }
        set { mTF = value; }
    }


    float mTF;
    int mDir;
    Transform mTransform;

	// Use this for initialization
	IEnumerator Start () {
        
        mTF = InitialF;
        mDir = (Speed >= 0) ? 1 : -1;
        Speed = Mathf.Abs(Speed);
        mTransform = transform;
        if (Spline) {
            // Wait until the spline is fully intialized before accessing it:
            while (!Spline.IsInitialized)
                yield return null;
            // now we're safe to use it
            InitPosAndRot();
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (!Spline) return;
        // Runtime processing
        if (Application.isPlaying) {
            // Move at a constant speed?
            if (MoveByWorldUnits) {
                // either used cached values(slightly faster) or interpolate position now (more exact)
                // Note that we pass mTF and mDir by reference. These values will be changed by the Move methods
                mTransform.position = (FastInterpolation) ?
                    Spline.MoveByFast(ref mTF, ref mDir, Speed * Time.deltaTime, Clamping) : // linear interpolate cached values
                    Spline.MoveBy(ref mTF, ref mDir, Speed * Time.deltaTime, Clamping); // interpolate now
            }
            else { // Move at constant F
                // either used cached values(slightly faster) or interpolate position now (more exact)
                // Note that we pass mTF and mDir by reference. These values will be changed by the Move methods
                mTransform.position = (FastInterpolation) ?
                    Spline.MoveFast(ref mTF, ref mDir, Speed * Time.deltaTime, Clamping) : // linear interpolate cached values
                    Spline.Move(ref mTF, ref mDir, Speed * Time.deltaTime, Clamping); // interpolate now
            }
            // Rotate the transform to match the spline's orientation
            if (SetOrientation) {
                transform.rotation = Spline.GetOrientationFast(mTF);
            }
        }
        else // Editor processing: continuously place the transform to reflect property changes in the editor
            InitPosAndRot();
	}

    void InitPosAndRot()
    {
        if (!Spline) return;
        // move the transform onto the spline
        mTransform.position = Spline.Interpolate(InitialF);
        // Rotate the transform to match the spline's orientation?
        if (SetOrientation)
            mTransform.rotation = Spline.GetOrientationFast(InitialF);
    }

}
