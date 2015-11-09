// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// Do test the code! You usually need to change a few small bits.

//using UnityEditor;
using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class CreateCubemap : MonoBehaviour
{
    // Attach this script to an object that uses a Reflective shader.
    // Realtime reflective cubemaps!



    //private int cubemapSize = 128;
    private bool oneFacePerFrame = false;
    public  Cubemap cubemap;
    
    void Awake()
    {
        // render all six faces at startup
    }
    public void OnEnable()
    {
        UpdateCubemap();
        
    }

    void Update()
    {
        if (oneFacePerFrame)
        {
            var faceToRender = Time.frameCount % 6;
            var faceMask = 1 << faceToRender;
            UpdateCubemap(faceMask);
        }
        else
        {
            UpdateCubemap(63); // all six faces
        }
    }

    public void UpdateCubemap(int faceMask = 63)
    {
        //if (!camera)
        //{
        //    //var go = new GameObject("CubemapCamera", typeof(Camera));
        //    //go.hideFlags = HideFlags.HideAndDontSave;
        //    //go.transform.position = transform.position;
        //    //go.transform.rotation = Quaternion.identity;
        //    //camera = go.camera;
        //    //cam.cullingMask = 1 << Layer.level;
        //    //cam.farClipPlane = 1000; // don't render very far into cubemap
        //    camera.enabled = false;
        //}

        if (!cubemap)
        {
            
            cubemap = new Cubemap(512, TextureFormat.RGB24, false);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(cubemap, "Assets/cubemap.cubemap");
#endif
            //rtex.isCubemap = true;
            //rtex.hideFlags = HideFlags.HideAndDontSave;
            //renderer.sharedMaterial.SetTexture("_Cube", rtex);
        }

        //cam.transform.position = transform.position+Vector3.up*40;//Camera.main.transform.position;
        camera.RenderToCubemap(cubemap);
    }

    void OnDisable()
    {
        //DestroyImmediate(cam.gameObject);
        //DestroyImmediate(rtex);
        
        
    }
}