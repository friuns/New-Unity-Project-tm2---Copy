
using System;
using System.Collections.Generic;
#if !UNITY_FLASH || UNITY_EDITOR
#endif
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
#endif
using System.Linq;
using UnityEngine;
using gui = UnityEngine.GUILayout;
using Object = UnityEngine.Object;

public class ModelLibrary:bs
{
    public ModelItem RootItem;

    public List<ModelFile> models = new List<ModelFile>();

    private Dictionary<string, ModelFile> m_dict;

    public Dictionary<string, ModelFile> dict
    {
        get
        {
            if (m_dict == null)
            {
                m_dict = new Dictionary<string, ModelFile>();
                foreach (var a in models)
                    if (!m_dict.ContainsKey(a.name))
                        m_dict.Add(a.name, a);
                    else
                        Debug.Log("Duplicate: " + a.name, a.gameObj);
            }
            return m_dict;
        }
    }

}

[Serializable]
public class ModelFile
{
    internal int usedCount;
    public float usedCountSqrt { get { return bs.setting.Popularity.TryGet(path, 0); } }
    public string name;
    public string path;
    public GameObject gameObj
    {
        get
        {
            var replace = path.Substring(0, path.LastIndexOf('.')).Replace('\\', '/');
            //Path.GetFileNameWithoutExtension(path).Replace('\\', '/');
            if (m_gameObj == null)
                if (bs._Loader.levelEditor || !bs.isDebug||true)
                {
                    m_gameObj = Instantiate(LoadPrefab(replace));
                    m_gameObj.transform.parent = bs._MapLoader.disable;
                }
                else
                {
                    GameObject prefab = null;
                    //if (bs.lowQuality || bs.isDebug)
                    //prefab = (GameObject)bs.LoadRes(replace + " LOD 0");

                    if (prefab == null)
                        prefab = LoadPrefab(replace);
                    if (prefab == null) return null;
                    m_gameObj = Instantiate(prefab);


                    m_gameObj.name = prefab.name;
                    var lodg = LoadPrefab(replace + " LOD 1");
                    if (!lodg)
                        lodg = prefab;
                    if (lodg)
                    {
                        var l = new LOD(.5f, m_gameObj.GetComponentsInChildren<Renderer>());
                        lodg = Instantiate(lodg);
                        lodg.transform.parent = m_gameObj.transform;
                        Collider[] componentsInChildren = lodg.GetComponentsInChildren<Collider>();
                        foreach (var a in componentsInChildren)
                            bs.DestroyImmediate(a);
                        var lod = m_gameObj.AddComponent<LODGroup>();
                        Renderer[] rs = lodg.GetComponentsInChildren<Renderer>();
                        foreach (var r in rs)
                        {
                            Material[] sharedMaterials = new Material[r.sharedMaterials.Length];
                            for (int i = 0; i < sharedMaterials.Length; i++)
                                sharedMaterials[i] = bs.res.diffuseMat;
                            r.castShadows = r.receiveShadows = false;
                            r.sharedMaterials = sharedMaterials;
                        }
                        var l2 = new LOD(0f, rs);
                        lod.SetLODS(new[] {l, l2});
                        lod.RecalculateBounds();
                    }
                    m_gameObj.transform.parent = bs._MapLoader.disable;
                }

            return m_gameObj;

        }
    }
    private static GameObject Instantiate(GameObject lodg)
    {
        var instantiate = (GameObject)bs.Instantiate(lodg);
        instantiate.name = lodg.name;
        //foreach (var a in loadPrefab.transform.GetTransforms().Select(a => a.light).Where(a => a != null).ToArray())
        if (instantiate)
            foreach (var a in instantiate.GetComponentsInChildren<Light>())
                GameObject.Destroy(a);
        return instantiate;
    }

    private static GameObject LoadPrefab(string replace)
    {
        GameObject loadPrefab = (GameObject)bs.LoadRes(replace);        
        return loadPrefab;
    }
    private GameObject m_gameObj;
    private Texture2D m_thumb;
    public Texture2D thumb
    {
        get
        {
            if (m_thumb == null)
                m_thumb = (Texture2D)bs.LoadRes("FileIcons/" + name);
            return m_thumb;
        }
    }
}
[Serializable]
public class ModelItem
{
    public string Name;
    public ModelItem parent;
    public List<ModelItem> dirs = new List<ModelItem>();
    public List<ModelFile> files = new List<ModelFile>();
    internal Vector2 scroll;
    //public List<GameObject> files = new List<GameObject>();
    //public List<Texture> thumbs = new List<Texture>();
    public Texture FolderTexture;
}