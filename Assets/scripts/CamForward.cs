using UnityEngine;

public class CamForward:MonoBehaviour
{
    public Camera hudCamera;

    //public Camera[] cameras
    //{
    //    get { return new[] { hudCamera, camera }; }
    //}

    //public void OnPostRender()
    //{
    //    //print("Fuck");
    //    //if (reverceRender)
    //    hudCamera.Render();
    //    //if (reverceRender)
    //    //    GL.SetRevertBackfacing(false);
    //}

    //public Player pl;

    //public void OnPreCull()
    //{
    //    if (reverceRender)
    //    {
    //        foreach (var camera in cameras)
    //        {
    //            camera.ResetWorldToCameraMatrix();
    //            camera.ResetProjectionMatrix();
    //            camera.projectionMatrix = camera.projectionMatrix*Matrix4x4.Scale(new Vector3(1, -1, 1));
    //        }
    //    }
    //}

    //private bool reverceRender
    //{
    //    get { return pl.secondPlayer && false; }
    //}

    //public void OnPreRender()
    //{
    //    hudCamera.Render();
    //}


}