using UnityEngine;
using System.Collections;
/* Drop this script to a transform you'd like to move along a Curvy spline!
 * 
 * The difference to SplineWalker is that this script does work with absolute positions instead of relative
 * positions (TF). You usually want to work with relative positions (it's faster), but sometimes you'll need absolute
 * positions, e.g. when you alter the length of the spline (see the Dynamic Spline example scene).
 */


/// <summary>
/// Move a Transform along a spline purely based on distance
/// </summary>
[ExecuteInEditMode]
public class SplineWalkerDistance : MonoBehaviour
{
    public CurvySpline Spline; // Spline to use
    public CurvyClamping Clamping = CurvyClamping.Clamp; // what to do if we reach spline's end?
    public bool SetOrientation = true; // rotate to match orientation?
    public bool FastInterpolation; // use cached values?
    public float InitialDistance; // starting distance
    public float Speed; // speed in world units

    /// <summary>
    /// Absolute position on the spline (world units)
    /// </summary>
    public float Distance 
    {
        get { return mDistance; }
        set { mDistance = value; }
    }

    float mDistance;
    int mDir;
    Transform mTransform;

    // Use this for initialization
    IEnumerator Start()
    {
        mDistance = InitialDistance;
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
    void Update()
    {
        if (!Spline || !Spline.IsInitialized) return;
        // Runtime processing
        if (Application.isPlaying) {
            // get the TF of the current distance.
            // Note: It's recommended to use the TF based methods in consecutive calls, as the distance based
            // methods need to convert distance to TF internally each time!
            float tf = Spline.DistanceToTF(mDistance);

            // Move using cached values(slightly faster) or interpolate position now (more exact)
            // Note that we pass mTF and mDir by reference. These values will be changed by the Move methods
            mTransform.position = (FastInterpolation) ?
            Spline.MoveByFast(ref tf, ref mDir, Speed * Time.deltaTime, Clamping) :
            Spline.MoveBy(ref tf, ref mDir, Speed * Time.deltaTime, Clamping);
            mDistance = Spline.TFToDistance(tf);
            // Rotate the transform to match the spline's orientation
            if (SetOrientation) {
            transform.rotation = Spline.GetOrientationFast(tf);
            }
        }
        else  // Editor processing: continuously place the transform to reflect property changes in the editor
            InitPosAndRot();
    }

    void InitPosAndRot()
    {
        if (!Spline) return;
            // Get the TF for the current distance
            float tf = Spline.DistanceToTF(InitialDistance);
            // move Transform onto the spline
            mTransform.position = Spline.Interpolate(tf);
            // Rotate the transform to match the spline's orientation?
            if (SetOrientation)
                mTransform.rotation = Spline.GetOrientationFast(tf);
        
    }

}
