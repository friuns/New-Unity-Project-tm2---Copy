#pragma warning disable 618
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using print = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class Tools2 : Editor
{

    //[MenuItem("RTools/Reser UV")]
    //public static void ResetUv()
    //{
    //    var r = Selection.activeGameObject.GetComponent<MeshFilter>();
    //    Mesh m = r.mesh;
    //    m.uv = new Vector2[0];
    //    m.uv1 = new Vector2[0];
    //    m.uv2 = new Vector2[0];
    //}

    public static void MoveDirectory(string source, string target)
    {
        var stack = new Stack<Folders>();
        stack.Push(new Folders(source, target));

        while (stack.Count > 0)
        {
            var folders = stack.Pop();
            Directory.CreateDirectory(folders.Target);
            foreach (var file in Directory.GetFiles(folders.Source, "*.*"))
            {
                string targetFile = Path.Combine(folders.Target, Path.GetFileName(file));
                if (File.Exists(targetFile)) File.Delete(targetFile);
                File.Move(file, targetFile);
            }

            foreach (var folder in Directory.GetDirectories(folders.Source))
            {
                stack.Push(new Folders(folder, Path.Combine(folders.Target, Path.GetFileName(folder))));
            }
        }
        Directory.Delete(source, true);
    }
    public class Folders
    {
        public string Source { get; private set; }
        public string Target { get; private set; }

        public Folders(string source, string target)
        {
            Source = source;
            Target = target;
        }
    }

    public static void FileMove(string a, string b)
    {
        if (Directory.Exists(a))
            MoveDirectory(a, b);
        else
        {
            if (File.Exists(b))
                File.Delete(b);
            File.Move(a, b);
        }
    }
    [MenuItem("RTools/Lock Assets")]
    public static void Lockassets()
    {
        foreach (var a in Selection.objects)
        {
            var assetPath = AssetDatabase.GetAssetPath(a);
            if (!resEditor.lockassets.Contains(assetPath))
                resEditor.lockassets.Add(assetPath);
        }
    }



    [MenuItem("RTools/Move Assets")]
    public static void MoveAssets()
    {

        foreach (var a in Selection.objects)
        {
            var assetPath = AssetDatabase.GetAssetPath(a);
            //try
            //{
            Directory.CreateDirectory("backup/" + Path.GetDirectoryName(assetPath));
            FileMove(assetPath, "backup/" + assetPath);
            FileMove(assetPath + ".meta", "backup/" + assetPath + ".meta");
            //} catch (IOException e)
            //{
            //    print.Log("move failed " + assetPath);
            //}
        }
        foreach (var assetPath in resEditor.lockassets)
        {

            //try
            //{
            Directory.CreateDirectory(Path.GetDirectoryName(assetPath));
            FileMove("backup/" + assetPath, assetPath);
            FileMove("backup/" + assetPath + ".meta", assetPath + ".meta");
            //} catch (IOException e)
            //{
            //    print.Log("move failed "+assetPath);
            //}

        }
    }

    [MenuItem("RTools/Select Lods")]
    public static void LodSelect()
    {
        int j = 0;
        var gameObjects = new List<GameObject>(Selection.gameObjects);
        for (int i = 0; i < gameObjects.Count; i++)
        {
            var a = gameObjects[i];
            var path = AssetDatabase.GetAssetPath(a);

            string insert = path.Substring(0, path.Length - 4) + " LOD 0.prefab";

            if (File.Exists(insert))
            {
                if (new FileInfo(insert).Length < 10 * 1024)
                {
                    File.Delete(path.Substring(0, path.Length - 4) + " LOD 0.prefab");
                    File.Delete(path.Substring(0, path.Length - 4) + " LOD 1.prefab");
                }
                else
                {
                    j++;
                    gameObjects[i] = null;
                }
            }
        }

        print.Log("deselected: " + j);
        Selection.objects = gameObjects.Where(a => a != null).Cast<Object>().ToArray();
    }

    [MenuItem("RTools/Copy Animation")]
    public static void CopyAnim()
    {
        var animationClip = Selection.activeObject;
        string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(animationClip)) + "/" + animationClip.name + ((animationClip is AnimationClip) ? ".anim" : ".png");
        AssetDatabase.CreateAsset(Instantiate(animationClip), path);
    }

    //[MenuItem("RTools/AddToLibrary2")]
    //static void Start()
    //{
    //    var g = Selection.activeGameObject;
    //    MeshFilter[] meshFilters = g.GetComponentsInChildren<MeshFilter>();
    //    CombineInstance[] combine = new CombineInstance[meshFilters.Length];
    //    int i = 0;
    //    while (i < meshFilters.Length)
    //    {
    //        combine[i].mesh = meshFilters[i].sharedMesh;
    //        combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
    //        meshFilters[i].gameObject.active = false;
    //        i++;
    //    }
    //    g.AddComponent<MeshFilter>().mesh = new Mesh();
    //    g.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
    //    g.transform.gameObject.active = true;
    //    g.AddComponent<MeshRenderer>();
    //}
    //[MenuItem("RTools/CombineUtlility")]
    //public static void Test()
    //{
    //    //StaticBatchingUtility.Combine(Selection.activeGameObject);
    //    Selection.activeGameObject.AddComponent<MeshFilter>().sharedMesh= Selection.activeGameObject.GetComponentInChildren<MeshFilter>().sharedMesh;
    //}
    private static ModelLibrary modelLibrary;
    [MenuItem("RTools/AddToLibrary", false, -1)]
    public static void AddToLibrary()
    {
        //resEditor.ModelLibrary.RootItem.dirs.Clear();
        //resEditor.ModelLibrary.RootItem.files.Clear();
        //resEditor.ModelLibrary.models.Clear();
        modelLibrary = new ModelLibrary();

        foreach (var a in Selection.objects)
        {
            var modelItem = new ModelItem();
            modelLibrary.RootItem.dirs.Add(modelItem);
            var path = AssetDatabase.GetAssetPath(a);
            modelItem.Name = Path.GetFileName(path);
            GetDirs(path, modelItem);
        }
        File.WriteAllText("Assets/modelLibrary.json", LitJson.JsonMapper.ToJson(modelLibrary));
    }
    public static void GetDirs(string path, ModelItem modelItem)
    {
        var directories = Directory.GetDirectories(path);
        Debug.Log(directories.Length);

        foreach (var a in directories)
        {
            var m = new ModelItem();
            m.parent = modelItem;
            m.Name = Path.GetFileName(a);
            modelItem.dirs.Add(m);
            GetDirs(a, m);
        }
        var files = Directory.GetFiles(path, "*.fbx").Union(Directory.GetFiles(path, "*.prefab"));
        foreach (string a in files)
        {

            string p = "Assets/Res2/Resources/FileIcons/" + Path.GetFileNameWithoutExtension(a) + ".png";
            if (!File.Exists(p))
                if (!RenderObject(a, p))
                    continue;
            var modelFile = new ModelFile() { path = a.Substring(a.IndexOf("Resources/") + 10), name = Path.GetFileNameWithoutExtension(a) };
            modelItem.files.Add(modelFile);
            modelLibrary.models.Add(modelFile);
            if (modelItem.FolderTexture == null)
                modelItem.FolderTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(p, typeof(Texture2D));
        }
    }
    [MenuItem("RTools/CleanGrass")]
    public static void ClearTerrain()
    {
        var t = (Terrain)FindObjectOfType(typeof(Terrain));
        for (int i = 0; i < t.terrainData.detailPrototypes.Length; i++)
            t.terrainData.SetDetailLayer(0, 0, i, new int[t.terrainData.detailWidth, t.terrainData.detailHeight]);
    }
    [MenuItem("RTools/CleanTrees")]
    public static void ClearTrees()
    {
        var t = (Terrain)FindObjectOfType(typeof(Terrain));
        t.terrainData.treeInstances = new TreeInstance[0];
    }
    [MenuItem("RTools/CopyRenderSettings")]
    public static void CopyRenderSettings()
    {
        CustomRenderSettings r = new CustomRenderSettings();
        r.name = Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
        int i = resEditor.renderSettings.FindIndex(a => a.name == r.name);
        if (i == -1)
            resEditor.renderSettings.Add(r);
        else
            resEditor.renderSettings[i] = r;
        r.fog = RenderSettings.fog;
        r.ambientLight = RenderSettings.ambientLight;
        r.flareStrength = RenderSettings.flareStrength;
        r.fogColor = RenderSettings.fogColor;
        r.fogDensity = RenderSettings.fogDensity;
        r.fogEndDistance = RenderSettings.fogEndDistance;
        r.fogMode = RenderSettings.fogMode;
        r.fogStartDistance = RenderSettings.fogStartDistance;
        r.haloStrength = RenderSettings.haloStrength;
        r.skybox = RenderSettings.skybox;
        var monoBehaviours = Camera.main.GetComponents<MonoBehaviour>();
        for (int index = 0; index < monoBehaviours.Length; index++)
        {
            MonoBehaviour a = monoBehaviours[index];
            foreach (var f in a.GetType().GetFields())
            {
                var mp = new MyProperty().SetValue(f.Name, f.GetValue(a), a.GetType().Name);
                if (mp != null)
                    r.properties.Add(mp);
            }
        }
        EditorUtility.SetDirty(res);
    }
    [MenuItem("RTools/ResizeTerrain")]
    public static void ResizeTerrain2()
    {
        Terrain ter = Selection.activeGameObject.GetComponent<Terrain>();
        var td = ter.terrainData;
        float[,,] alph = td.GetAlphamaps(0, 0, td.alphamapWidth, td.alphamapHeight);
        float[,,] alph2 = new float[td.alphamapWidth / 2, td.alphamapWidth / 2, td.alphamapLayers];
        for (int x = 0; x < td.alphamapWidth; x += 2)
        {
            for (int y = 0; y < td.alphamapHeight; y += 2)
            {
                for (int i = 0; i < td.alphamapLayers; i++)
                {
                    alph2[x / 2, y / 2, i] = alph[x, y, i];
                }
            }
        }
        td.alphamapResolution /= 2;
        td.SetAlphamaps(0, 0, alph2);
        ResizeTerrain3();
        td.baseMapResolution /= 2;
        td.SetDetailResolution(td.detailResolution / 2, 8);
    }
    public static void ResizeTerrain3()
    {
        Terrain ter = Selection.activeGameObject.GetComponent<Terrain>();
        var td = ter.terrainData;
        float[,] alph = td.GetHeights(0, 0, td.heightmapWidth, td.heightmapHeight);
        float[,] alph2 = new float[td.heightmapWidth / 2 + 1, td.heightmapHeight / 2 + 1];
        for (int x = 0; x < td.heightmapWidth; x += 2)
        {
            for (int y = 0; y < td.heightmapHeight; y += 2)
            {
                try
                {
                    alph2[x / 2, y / 2] = alph[x, y];
                }
                catch (Exception) { }
            }
        }
        var old = td.size;
        td.heightmapResolution = (td.heightmapResolution /= 2);
        td.SetHeights(0, 0, alph2);
        td.size = old;
    }
    [MenuItem("RTools/Txts")]
    public static void Txts()
    {
        StringBuilder sb = new StringBuilder();
        foreach (Object a in Selection.objects.OrderBy(a => a.name, new AlphanumComparatorFast()))
            sb.AppendLine(a.name);
        Debug.Log(sb.ToString());
    }
    [MenuItem("RTools/Previews")]
    public static void AddMaps()
    {
        StringBuilder sb = new StringBuilder();
        foreach (Object a in Selection.objects.OrderBy(a => a.name, new AlphanumComparatorFast()))
        {
            var b = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(a), typeof(Object));
            Debug.Log(b);
            Texture2D assetPreview = AssetPreview.GetAssetPreview(b);
            sb.AppendLine(a.name);
            if (assetPreview != null)
            {
                string path = Loader.resourcesPath + a.name + ".png";
                File.WriteAllBytes(path, assetPreview.EncodeToPNG());
            }
            else
                Debug.LogWarning("no texture " + a.name);
        }
        Debug.Log(sb.ToString());
    }
    [MenuItem("RTools/build Release")]
    public static void Release()
    {
        Build2("");
    }
    public static void Build2(string s, BuildOptions buildOptions = BuildOptions.None)
    {
        //bool ios = activeBuildTarget == BuildTarget.iPhone;
        //bool android = activeBuildTarget == BuildTarget.Android;
        if (resEditor.autorun)
            buildOptions |= BuildOptions.AutoRunPlayer;
        //print.Log(buildOptions);
        Build(buildOptions, "tmRace" + (settings.version + 1) + s);
    }
    [MenuItem("RTools/build Debug %g")]
    public static void Build()
    {
        Build2("debug", BuildOptions.Development | BuildOptions.AllowDebugging);
        //BuildOptions buildOptions = BuildOptions.Development| BuildOptions.AllowDebugging;
        //Build(buildOptions, "tmRace" + (settings.version + 1) + "debug");
    }
    //private static string[] noPackages = new string[] { "Assets/!scenes/!loadingScreen.unity" };
    public static void Build(BuildOptions buildOptions, string name, bool includePackages = true)
    {

        var terrainData = resEditor.td;
        terrainData.alphamapResolution = 16;
        EditorUtility.SetDirty(terrainData);
        //DisableRes(!settings.includeRes);
        Time.timeScale = 1;
        settings.version++;
        settings.SetDirty();
        PlayerSettings.Android.bundleVersionCode = settings.version;
        PlayerSettings.bundleVersion = settings.version.ToString();
        settings.versionDate = DateTime.Now.ToShortDateString();
        var flash = activeBuildTarget == BuildTarget.FlashPlayer;
        var android = activeBuildTarget == BuildTarget.Android;
        var web = activeBuildTarget == BuildTarget.WebPlayer || activeBuildTarget == BuildTarget.WebPlayerStreamed;
        var linux = activeBuildTarget == BuildTarget.StandaloneLinux;
        PlayerSettings.bundleIdentifier = "com.tm.race";
        Debug.Log(activeBuildTarget);
        string outputFolder = ios ? "/Users/Admin/New Unity Project/builds" : android ? "builds/" + name + ".apk" : flash ? "builds/" + name + ".swf" : web ? "builds" : linux ? "builds/tmlinux/TrackRacing" : "builds/" + name + "PC" + "/" + name + ".exe";
        Debug.Log(outputFolder);
        Debug.Log(buildOptions);
        BuildPipeline.BuildPlayer(resEditor.includeMaps ? packages : packages.Take(5).ToArray(), outputFolder, activeBuildTarget, buildOptions);
#if UNITY_EDITOR
        if (web && name != "debug")
            File.Copy("builds/builds.unity3d", "builds/" + name + ".unity3d");
#endif
        //if (!settings.includeRes)
        //    DisableRes(false);
    }
    private static void DisableRes(bool bl)
    {
        PlayerSettings.firstStreamedLevelWithResources = 0;
        foreach (var b in resEditor.Resources)
        {
            var c = AssetDatabase.GetAssetPath(b);
            if (bl)
                Debug.Log(AssetDatabase.RenameAsset(c, "Res"));
            else
                Debug.Log(AssetDatabase.RenameAsset(c, "Resources"));
        }
        AssetDatabase.Refresh();
    }
    [MenuItem("RTools/build All")]
    public static void BuildAll()
    {
        var cur = activeBuildTarget;
        BuildPackage();
        Release();
        if (cur != BuildTarget.WebPlayer && cur != BuildTarget.WebPlayerStreamed)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WebPlayer);
            BuildPackage2();
            Release();
            BuildLevelsBuild();
        }
        if (cur != BuildTarget.FlashPlayer)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.FlashPlayer);
            BuildPackage2();
            Release();
            BuildLevelsBuild();
        }
        if (cur != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
            BuildPackage2();
            Release();
            BuildLevelsBuild();
        }
        Debug.Log("Done");
    }
    [MenuItem("RTools/build packages")]
    public static void BuildPackage()
    {
        BuildPackage2();
    }
    private static void BuildPackage2()
    {
        DisableRes(true);
        BuildTarget bt = activeBuildTarget;
        var f = "packages" + settings.packageVersion + ".unity3d" + bs.platformPrefix2;
        if (String.IsNullOrEmpty(BuildPipeline.BuildStreamedSceneAssetBundle(packages, outputFolder + f, bt)))
        {
        }
        DisableRes(false);
    }
    private static string[] packages
    {
        get { return bs.resEditor.packageScenes.Select(a => AssetDatabase.GetAssetPath(a)).ToArray(); }
    }
    [MenuItem("RTools/build all assets")]
    public static void BuildAllAssets()
    {
        BuildAssets();
        BuildAssets2();
    }
    [MenuItem("RTools/build assets")]
    public static void BuildAssets()
    {
        BuildAssets("resources" + settings.packageVersion, resEditor.Resources[0]);
    }
    [MenuItem("RTools/build assets2")]
    public static void BuildAssets2()
    {

        BuildAssets("resourcesa" + settings.packageVersion2, resEditor.Resources[1]);
    }
    public static void BuildAssets(string resources, Object assetObject)
    {
        //DisableRes(true);
        bs.setting = settings;
        BuildTarget bt = activeBuildTarget;
        var file = resources + ".unity3d" + bs.platformPrefix2;
        var path = AssetDatabase.GetAssetPath(assetObject);

        var asss = new List<Object>();
        var asss2 = new List<string>();
        foreach (var f in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
        {
            var asset = AssetDatabase.LoadMainAssetAtPath(f);
            if (asset != null)
            {
                asss.Add(asset);
                int startIndex = path.Length + 1;
                var str = f.Substring(startIndex, f.LastIndexOf('.') - startIndex).Replace('\\', '/');
                asss2.Add("@" + str);
            }
        }

        BuildPipeline.BuildAssetBundleExplicitAssetNames(asss.ToArray(), asss2.ToArray(), outputFolder + file, BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets, bt);
        //DisableRes(false);
    }

    private static bool oclude;
    [MenuItem("RTools/build levels With Occlusion")]
    public static void BuildLevelsBuildOcl()
    {
        oclude = true;
        dontBuild = false;
        BuildLevels();
    }
    [MenuItem("RTools/build levels")]
    public static void BuildLevelsBuild()
    {
        dontBuild = false;
        oclude = false;
        BuildLevels();
    }
    public static void BuildLevelsPreview()
    {
        dontBuild = true;
        BuildLevels();
    }
    public static void BuildLevels()
    {
        bs.setting = settings;
        EditorApplication.delayCall += delegate
        {
            DisableRes(true);
            var cur = EditorApplication.currentScene;
            BuildTarget bt = activeBuildTarget;
            Debug.Log(bt);
            string txt = "";
            var rand = Random.Range(0, 100);
            foreach (var scene in Selection.objects)
            {
                string assetPath = AssetDatabase.GetAssetPath(scene);
                EditorApplication.OpenScene(assetPath);
                FixMaterials();
                if (oclude)
                {
                    Debug.Log("ocluding");
                    StaticOcclusionCulling.Compute();
                }
                if (scene.name != Levels.game)
                {
                    clear(typeof(Game));
                    clear(typeof(Loader));
                    clear(typeof(Player));
                }
                foreach (Renderer renderer in FindObjectsOfType(typeof(Renderer)))
                {
                    int layer = renderer.transform.root.gameObject.layer;
                    if (layer == Layer.def || layer == Layer.level)
                        PrefabUtility.DisconnectPrefabInstance(renderer.transform.root);
                }
                var name = scene.name.ToLower();
                var path = "Assets/!scenes/tmp/" + name + ".unity";
                EditorApplication.SaveScene(path, true);
                var f = name + ".unity3d" + bs.platformPrefix2;
                File.Delete(f);
                txt += f + ":" + rand + "\n";
                if (bt != BuildTarget.iPhone && !dontBuild)
                {
                    Debug.Log(BuildPipeline.BuildStreamedSceneAssetBundle(new[] { path }, outputFolder + f, bt));
                }
            }
            EditorApplication.OpenScene(cur);
            Debug.Log(txt);
            DisableRes(false);
        };
    }
    static bool dontBuild;
    private static void clear(Type type)
    {
        MonoBehaviour f = (MonoBehaviour)FindObjectOfType(type);
        if (f != null)
        {
            if (f is Player && !GameObject.Find("Start"))
            {
                var g = new GameObject("Start");
                g.transform.position = f.transform.position;
                g.transform.rotation = f.transform.rotation;
            }
            DestroyImmediate(f.gameObject);
        }
    }
    [MenuItem("RTools/CreateTexture2D")]
    public static void CreateTextre()
    {
        var t = (Texture2D)Selection.activeObject;
        Material m = new Material(Shader.Find("Unlit/Transparent"));
        m.mainTexture = t;
        string path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(t)) + "/" + t.name + ".mat";
        AssetDatabase.CreateAsset(m, path);
        var g = (Transform)Instantiate(bs.resEditor.plane);
        Camera camera = bs._Player.hud.camera;
        g.transform.parent = camera.transform;
        g.transform.localPosition = Vector3.zero;
        g.transform.localEulerAngles = new Vector3(0, 180, 0);
        g.name = t.name;
        g.localScale = new Vector3(t.width, t.height, 1);
        g.renderer.sharedMaterial = m;
        g.gameObject.layer = Layer.nGui;
        g.gameObject.AddComponent<Archor>().camera = camera;
    }
    [MenuItem("RTools/Add Level Materials")]
    public static void AddMaterials()
    {
        LevelMaterial(true);
    }
    [MenuItem("RTools/Remove Level Materials")]
    public static void removeMaterials()
    {
        LevelMaterial(false);
    }
    private static void LevelMaterial(bool add)
    {
        foreach (var a in Selection.transforms)
        {
            if (a.renderer != null)
            {
                var levelMaterialsTxt = res.levelMaterialsTxt;
                if (!levelMaterialsTxt.Contains(a.renderer.sharedMaterial.name))
                {
                    if (add)
                    {
                        Debug.Log("added");
                        levelMaterialsTxt.Add(a.renderer.sharedMaterial.name);
                    }
                    else
                        Debug.Log("notFound");
                }
                else
                {
                    if (!add)
                    {
                        levelMaterialsTxt.Remove(a.renderer.sharedMaterial.name);
                        Debug.Log("removed");
                    }
                    else
                        Debug.Log("contais already");
                }
            }
        }
        EditorUtility.SetDirty(res);
    }
    [MenuItem("RTools/FixMaterials")]
    public static void FixMaterials()
    {
        Undo.RegisterSceneUndo("rtools");
        RenderSettings.fog = true;
        RenderSettings.fogDensity = 0.0002f;
        RenderSettings.skybox = bs.resEditor.skyboxes[Random.Range(0, bs.resEditor.skyboxes.Length)];
        foreach (Renderer r in FindObjectsOfType(typeof(Renderer)))
        {
            if (r.sharedMaterial != null && r.name.StartsWith("DrawCall"))
            {
                foreach (string b in res.levelMaterialsTxt)
                {
                    var isLevel = b == r.sharedMaterial.name;
                    r.gameObject.layer = isLevel ? Layer.level : Layer.cull;
                    if (isLevel) break;
                }
            }
        }
        BuildTools.FixMaterials();
    }
    [MenuItem("RTools/create cubemap")]
    public static void MakeCubemap()
    {
        Cubemap cubemap = new Cubemap(128, TextureFormat.RGB24, false);
        var go = new GameObject("CubemapCamera", typeof(Camera));
        var current = Camera.current;
        go.transform.position = current.transform.position;
        go.transform.rotation = Quaternion.identity;
        go.camera.farClipPlane = current.farClipPlane;
        go.camera.RenderToCubemap(cubemap, 63);
        AssetDatabase.CreateAsset(cubemap, "Assets/cubemap.cubemap");
    }
    [MenuItem("RTools/extract cubemap")]
    public static void SaveCubeMap()
    {
        var cubemap = (Cubemap)Selection.activeObject;
        var width = cubemap.width;
        var height = cubemap.height;
        foreach (CubemapFace asd in Enum.GetValues(typeof(CubemapFace)))
        {
            var pixels = cubemap.GetPixels(asd).Reverse().ToArray();
            var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.SetPixels(pixels);
            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(cubemap));
            File.WriteAllBytes(path + "/" + cubemap.name + asd + ".png", tex.EncodeToPNG());
        }
    }
    private static ResLoader settings { get { return (ResLoader)Resources.LoadAssetAtPath("Assets/!Prefabs/ResLoader.prefab", typeof(ResLoader)); } }
    public static Res res
    {
        get { return (Res)Resources.LoadAssetAtPath("Assets/!Prefabs/ResGame.prefab", typeof(Res)); }
    }
    public static ResEditor resEditor
    {
        get { return bs.resEditor; }
    }
    public static string outputFolder = ios ? "/Volumes/maps/" : "maps/";
    private static BuildTarget activeBuildTarget { get { return EditorUserBuildSettings.activeBuildTarget; } }
    private static bool ios
    {
        get { return activeBuildTarget == BuildTarget.iPhone; }
    }
    private static bool RenderObject(string a, string path)
    {
        var g = (GameObject)AssetDatabase.LoadAssetAtPath(a, typeof(GameObject));
        if (g == null) return false;
        EditorApplication.NewScene();
        g = (GameObject)Instantiate(g, Vector3.zero, g.transform.rotation);
        var camera = new GameObject().AddComponent<Camera>();
        var cam = camera.transform;
        var renderer = g.GetComponentInChildren<Renderer>();
        if (renderer == null)
        {
            Debug.Log("renderer not found " + g.name);
            return false;
        }
        var bounds = renderer.bounds;
        camera.orthographic = true;
        var vector3 = bounds.size;
        //vector3.y = 0;
        camera.orthographicSize = vector3.magnitude * .5f;
        cam.position = new Vector3(1, .5f, 1) * vector3.magnitude;
        var addComponent = cam.gameObject.AddComponent<Light>();
        addComponent.type = LightType.Directional;
        addComponent.intensity = .5f;
        cam.LookAt(bounds.center);

        var resWidth = 128;
        var resHeight = 128;
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        //DestroyImmediate(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        return true;
    }

}
[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        _property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
    }
}