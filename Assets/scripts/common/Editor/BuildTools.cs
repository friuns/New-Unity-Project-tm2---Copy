using System.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;
using print = UnityEngine.Debug;
public class BuildTools : Editor
{
    [MenuItem("Level/Prepare Level")]
    public static void FixMaterials()
    {
        if (!RenderSettings.fog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogDensity = 0.0002f;
        }

        //string assetPath = Selection.activeObject == null ? "Assets/!tracks/textures" : AssetDatabase.GetAssetPath(Selection.activeObject);
        string assetsPath = "Assets/!tracks/textures";
        Directory.CreateDirectory(assetsPath);



        foreach (Material m in Resources.FindObjectsOfTypeAll(typeof(Material)))
        {
            
            if (!m.shader.name.EndsWith("Diffuse")) continue;
            var mainTexture = m.mainTexture;
            //if (mainTexture == null) continue;
            var path = AssetDatabase.GetAssetPath(mainTexture);
            {
                Texture2D loadAssetAtPath = new[] { ".tif", ".png" }.Select(a => (Texture2D)AssetDatabase.LoadAssetAtPath(assetsPath + "/" + m.name + a, typeof(Texture2D))).FirstOrDefault(a => a != null);
                //print.Log(assetsPath + "/" + m.name + ".png");
                if (loadAssetAtPath != null)
                {
                    m.mainTexture = loadAssetAtPath;
                    EditorUtility.SetDirty(m);
                }
                else if (path.EndsWith(".dds"))
                {
                    print.Log("Creating png" + m.name);
                    
                    var t = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.ARGB32, true);
                    t.SetPixels32(((Texture2D)mainTexture).GetPixels32());
                    var png = ((Texture2D)t).EncodeToPNG();

                    var newPath = assetsPath + "/" + m.name + ".png";
                    File.WriteAllBytes(newPath, png);
                    AssetDatabase.Refresh();
                    m.mainTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(newPath, typeof(Texture2D));
                }
            }
        }
        AssetDatabase.Refresh();

        foreach (Renderer renderer in FindObjectsOfType(typeof(Renderer)))
        {
            if (renderer.gameObject.layer == Layer.def)
                renderer.gameObject.layer = Layer.level;
            if (renderer.gameObject.layer == Layer.level)
                renderer.gameObject.isStatic = true;
        }
    }

    [MenuItem("Level/Build level for PC")]
    public static void BuildWeb()
    {
        Build2(BuildTarget.WebPlayer);
    }
    [MenuItem("Level/Build level for Android")]
    public static void BuildAndroid()
    {
        Build2(BuildTarget.Android);
    }
    public static void Build2(BuildTarget bt)
    {

        if (GameObject.Find("Start") == null)
            EditorPopup.ShowPopup("Please Add Start Point");
        else if (GameObject.FindGameObjectWithTag(Tag.CheckPoint) == null)
            EditorPopup.ShowPopup("Please Add CheckPoint");
        else
        {
            var scene = EditorApplication.currentScene;
            if (Path.GetFileName(scene) != Path.GetFileName(scene).ToLower())
            {
                print.Log("Rename");
                File.Move(scene, scene.ToLower());
                scene = scene.ToLower();
                AssetDatabase.Refresh();
            }

            FixMaterials();
      

            EditorApplication.SaveScene();
            var f = Path.GetFileNameWithoutExtension(scene) + ".unity3d" +
                    (bt == BuildTarget.Android ? "android" : "web");
            File.Delete(f);
            Directory.CreateDirectory("maps");
            BuildPipeline.BuildStreamedSceneAssetBundle(new[] {scene}, "maps/" + f, bt);
        }
    }

}


public class EditorPopup : EditorWindow
{


    public static string te;
    public static void ShowPopup(string text)
    {
        te = text;
        //var window = new EditorPopup();
        //var window = (EditorPopup) ScriptableObject.CreateInstance(typeof (EditorPopup));
        var window = EditorWindow.GetWindow<EditorPopup>("Error");    
        //window.title = "Error";
        window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 350, 150);
        window.Show();
    }



    public void OnGUI()
    {
        GUILayout.Label(te);        
    }


}