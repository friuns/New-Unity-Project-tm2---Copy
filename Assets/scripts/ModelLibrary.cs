using System.Collections.Generic;
using System.IO;
#if !UNITY_FLASH || UNITY_EDITOR
#endif
#if UNITY_EDITOR
//using Exception = System.AccessViolationException;
#endif
using System.Linq;
using UnityEngine;
using gui = UnityEngine.GUILayout;

public class ModelLibrary
{
    public ModelItem RootItem = new ModelItem();

    internal static IEnumerable<ModelFile> GetModels(ModelItem m)
    {
        foreach (var a in m.files)
            yield return a;
        foreach (ModelItem a in m.dirs)
            foreach (var b in GetModels(a))
                yield return b;
    }
    internal IEnumerable<ModelFile> models
    {
        get { return GetModels(RootItem); }
    }

    private Dictionary<string, ModelFile> m_dict;

    internal Dictionary<string, ModelFile> dict
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
    public static ModelLibrary Load(string s)
    {
        return LitJson.JsonMapper.ToObject<ModelLibrary>(s);
    }
}

public class ModelFile
{
    internal int usedCount;
    internal float usedCountSqrt { get { return bs.setting.Popularity.TryGet(path, 0); } }
    public string name;
    public string path;
    internal GameObject gameObj
    {
        get
        {
            //var replace = path.Substring(0, path.LastIndexOf('.')).Replace('\\', '/');

            var path2 = path.Substring(0, path.LastIndexOf('.')).Replace('\\', '/');
            if (m_gameObj == null)
                if (bs._Loader.levelEditor || !bs.isDebug || true)
                {
                    GameObject lodg = bs.LoadRes<GameObject>(path2);
                    var instantiate = (GameObject)bs.Instantiate(lodg);
                    instantiate.name = lodg.name;
                    //foreach (var a in loadPrefab.transform.GetTransforms().Select(a => a.light).Where(a => a != null).ToArray())
                    if (instantiate)
                        foreach (var a in instantiate.GetComponentsInChildren<Light>())
                            GameObject.Destroy(a);
                    m_gameObj = instantiate;
                    m_gameObj.transform.parent = bs._MapLoader.disable;
                }
            return m_gameObj;

        }
    }

    private GameObject m_gameObj;
    private Texture2D m_thumb;
    private bool inited;
    internal Texture2D thumb
    {
        get
        {
            if (!inited)
            {
                inited = true;
                m_thumb = bs.LoadRes<Texture2D>("FileIcons/" + name);
            }
            return m_thumb;
        }
    }

}
public class ModelItem
{
    public string Name;
    //public ModelItem parent;
    public List<ModelItem> dirs = new List<ModelItem>();
    public List<ModelFile> files = new List<ModelFile>();
    internal Vector2 scroll;
    //public List<GameObject> files = new List<GameObject>();
    //public List<Texture> thumbs = new List<Texture>();
    private bool inited;
    internal Texture FolderTexture
    {
        get
        {
            if (!inited)
            {
                var m = ModelLibrary.GetModels(this).FirstOrDefault();
                inited = true;
                if (m != null)
                    m_FolderTexture = bs.LoadRes<Texture2D>("FileIcons/" + Path.GetFileNameWithoutExtension(m.path));
            }
            return m_FolderTexture;
        }
    }
    private Texture m_FolderTexture;
}